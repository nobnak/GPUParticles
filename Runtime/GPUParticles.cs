using GPUParticleSystem.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUParticleSystem {

    public class GPUParticles : System.IDisposable {

        protected ComputeShader cs;

        protected int k_init, k_add, k_update, k_index;
        protected uint g_init, g_add, g_update, g_index;

        protected int capacity;
        protected GraphicsBuffer gb_particles;
        protected GraphicsBuffer gb_indexPool;
        protected GraphicsBuffer gb_add;
        protected GraphicsBuffer gb_count;
        protected GraphicsBuffer gb_activeIDs;

        public GPUParticles(int capacity = 1024) {
            this.capacity = capacity;
            gb_particles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, Marshal.SizeOf<Particle>());
            gb_indexPool = new GraphicsBuffer(GraphicsBuffer.Target.Append, capacity, Marshal.SizeOf<uint>());
            gb_add = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 256, Marshal.SizeOf<Particle>());
            gb_count = new GraphicsBuffer(GraphicsBuffer.Target.Raw, 8, Marshal.SizeOf<uint>());
            gb_activeIDs = new GraphicsBuffer(GraphicsBuffer.Target.Append, capacity, Marshal.SizeOf<uint>());

            cs = Resources.Load<ComputeShader>(PathToCS);
            if (cs == null) {
                Debug.LogError($"Failed to load compute shader '{PathToCS}'");
                return;
            }

            k_init = cs.FindKernel(K_Init);
            k_add = cs.FindKernel(K_Add);
            k_update = cs.FindKernel(K_Update);
            k_index = cs.FindKernel(k_Index);

            cs.GetKernelThreadGroupSizes(k_init, out g_init, out _, out _);
            cs.GetKernelThreadGroupSizes(k_add, out g_add, out _, out _);
            cs.GetKernelThreadGroupSizes(k_update, out g_update, out _, out _);
            cs.GetKernelThreadGroupSizes(k_index, out g_index, out _, out _);

            Init();
        }

        #region IDisposable
        public void Dispose() {
            if (gb_particles != null) {
                gb_particles.Dispose();
                gb_particles = null;
            }
            if (gb_indexPool != null) {
                gb_indexPool.Dispose();
                gb_indexPool = null;
            }
            if (gb_add != null) {
                gb_add.Dispose();
                gb_add = null;
            }
            if (gb_count != null) {
                gb_count.Dispose();
                gb_count = null;
            }
            if (gb_activeIDs != null) {
                gb_activeIDs.Dispose();
                gb_activeIDs = null;
            }
        }
        #endregion

        #region kernels
        public void Init() {
            gb_indexPool.SetCounterValue(0);

            if (cs == null) throw new System.Exception("Compute shader is not loaded");

            cs.SetBuffer(k_init, P_Particles, gb_particles);
            cs.SetBuffer(k_init, P_IndexPoolC, gb_indexPool);
            cs.SetBuffer(k_init, P_IndexPoolA, gb_indexPool);

            var count = gb_particles.count;
            var dispatchCount = DispatcCount(count, g_init);
            cs.SetInt(P_ThreadCount, count);
            cs.Dispatch(k_init, dispatchCount, 1, 1);
        }
        public void Add(IList<Particle> particles) {
            GraphicsBuffer.CopyCount(gb_indexPool, gb_count, (int)CounterByteOffset.IndexPool);

            if (particles is List<Particle>)
                gb_add.SetData((List<Particle>)particles);
            else
                gb_add.SetData(particles.ToArray());

            cs.SetBuffer(k_add, P_ParticlesAdd, gb_add);
            cs.SetBuffer(k_add, P_Particles, gb_particles);
            cs.SetBuffer(k_add, P_IndexPoolC, gb_indexPool);
            cs.SetBuffer(k_add, P_IndexPoolA, gb_indexPool);
            cs.SetBuffer(k_add, P_CounterBuffer, gb_count);

            var count = particles.Count;
            var dispatchCount = DispatcCount(count, g_add);
            cs.SetInt(P_ThreadCount, count);
            cs.Dispatch(k_add, dispatchCount, 1, 1);
        }
        public void Add(params Particle[] particles) { Add(particles); }
        public void Update(float dt) {
            cs.SetBuffer(k_update, P_Particles, gb_particles);
            cs.SetBuffer(k_update, P_IndexPoolC, gb_indexPool);
            cs.SetBuffer(k_update, P_IndexPoolA, gb_indexPool);

            cs.SetFloat(P_DeltaTime, dt);

            var count = gb_particles.count;
            var dispatchCount = DispatcCount(count, g_update);
            cs.SetInt(P_ThreadCount, count);
            cs.Dispatch(k_update, dispatchCount, 1, 1);
        }
        public void Index() {
            gb_activeIDs.SetCounterValue(0);
            cs.SetBuffer(k_index, P_Particles, gb_particles);
            cs.SetBuffer(k_index, P_ActiveIndexesA, gb_activeIDs);

            var count = gb_particles.count;
            var dispatchCount = DispatcCount(count, g_index);
            cs.SetInt(P_ThreadCount, count);
            cs.Dispatch(k_index, dispatchCount, 1, 1);
        }
        #endregion

        #region properties
        public int Capacity => capacity;
        public GraphicsBuffer Particles => gb_particles;
        #endregion

        public AsyncGPUReadbackRequest GetParticlesAsync() {
            var count = gb_particles.count;
            return AsyncGPUReadback.Request(gb_particles);
        }
        public AsyncGPUReadbackRequest CountIndexPoolAsync() {
            var offset = (int)CounterByteOffset.IndexPool;
            GraphicsBuffer.CopyCount(gb_indexPool, gb_count, offset);
            return AsyncGPUReadback.Request(gb_count, Marshal.SizeOf<uint>(), offset);
        }
        public AsyncGPUReadbackRequest CountActiveParticlesAsync() {
            Index();
            var offset = (int)CounterByteOffset.ActiveIDs;
            GraphicsBuffer.CopyCount(gb_activeIDs, gb_count, offset);
            return AsyncGPUReadback.Request(gb_count, Marshal.SizeOf<uint>(), offset);
        }
        #region methods
        public static int DispatcCount(int count, uint groupSize) {
            return (count - 1) / (int)groupSize + 1;
        }
        #endregion

        #region declarations
        public const string PathToCS = "GPUParticles";

        public const string K_Init = "Init";
        public const string K_Add = "Add";
        public const string K_Update = "Update";
        public const string k_Index = "Index";

        // Counter
        public static readonly int P_ThreadCount = Shader.PropertyToID("_ThreadCount");
        public static readonly int P_CounterBuffer = Shader.PropertyToID("_CounterBuffer");

        // Index Pool
        public static readonly int P_IndexPoolA = Shader.PropertyToID("_IndexPoolA");
        public static readonly int P_IndexPoolC = Shader.PropertyToID("_IndexPoolC");

        // Active IDs
        public static readonly int P_ActiveIndexesA = Shader.PropertyToID("_ActiveIndexesA");

        // Particles
        public static readonly int P_Particles = Shader.PropertyToID("_Particles");
        public static readonly int P_ParticlesAdd = Shader.PropertyToID("_ParticlesAdd");
        public static readonly int P_OperationMode = Shader.PropertyToID("_OperationMode");

        // Time
        public static readonly int P_DeltaTime = Shader.PropertyToID("_DeltaTime");

        public enum CounterByteOffset { IndexPool = 0, ActiveIDs = 4, }
        #endregion
    }
}