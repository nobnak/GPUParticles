using GPUParticleSystem.Data;
using GPUParticleSystem.Util;
using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace GPUParticleSystem.Tests {

    public class TestGPUParticle {

        // A Test behaves as an ordinary method
        [Test]
        public void TestCopyCountSimplePasses() {
            var cs = Resources.Load<ComputeShader>(PathToCS);
            Assert.IsNotNull(cs);

            var count = 5;
            var nPerMove = 1;

            using var gb_indices_append = new GraphicsBuffer(
                GraphicsBuffer.Target.Append, count, Marshal.SizeOf<uint>());
            using var gb_incides_consume = new GraphicsBuffer(
                GraphicsBuffer.Target.Append, count, Marshal.SizeOf<uint>());
            using var gb_counter = new GraphicsBuffer(
                GraphicsBuffer.Target.Raw, 1, Marshal.SizeOf<int>());

            gb_indices_append.SetCounterValue((uint)0);
            gb_incides_consume.SetCounterValue((uint)count);

            var kid_move = cs.FindKernel(K_Move);
            cs.GetKernelThreadGroupSizes(kid_move, out var cc_x, out var cc_y, out var cc_z);

            cs.SetBuffer(kid_move, P_IndicesA, gb_indices_append);
            cs.SetBuffer(kid_move, P_IndicesC, gb_incides_consume);

            var dispatchGroupSize_x = GetDispatchGroupSize_x(nPerMove, cc_x);
            Debug.Log($"Dispatch '{K_Move}': g_x={dispatchGroupSize_x}");

            var counterArray = new int[1];
            for (var moveTotal = 0; moveTotal < count;) {
                moveTotal += nPerMove;
                Debug.Log($"Move: total={moveTotal} per={nPerMove} count={count}");

                cs.SetInt(P_IndicesC_Count, nPerMove);
                cs.Dispatch(kid_move, dispatchGroupSize_x, 1, 1);

                GraphicsBuffer.CopyCount(gb_indices_append, gb_counter, 0);
                gb_counter.GetData(counterArray);
                Assert.AreEqual(moveTotal, counterArray[0]);
            }
        }
        [Test]
        public void TestGPUParticleClass() {
            var count_add = 10;
            var duration = 10f;
            using var ps = new GPUParticles();

            ps.Init();
            ps.Update(0f);

            var particles_allAdds = new List<Particle>();
            for (var j = 0; j < 10; j++) {
                var particles_add = new List<Particle>();
                for (var i = 0; i < count_add; i++) {
                    var p = new Particle() {
                        activity = Activity.Active,
                        position = new float3(i, j, 0),
                        life = new float4(duration, duration, 0f, 0f),
                    };
                    particles_add.Add(p);
                }

                ps.Add(particles_add.ToArray());
                particles_allAdds.AddRange(particles_add);
            }

            Assert.AreEqual(particles_allAdds.Count, ps.Capacity - ps.CountIndexPoolAsync().Sync<int>()[0]);
            Assert.AreEqual(particles_allAdds.Count, ps.CountActiveParticlesAsync().Sync<int>()[0]);

            ps.Update(duration * 0.1f);
            Assert.AreEqual(particles_allAdds.Count, ps.Capacity - ps.CountIndexPoolAsync().Sync<int>()[0]);
            Assert.AreEqual(particles_allAdds.Count, ps.CountActiveParticlesAsync().Sync<int>()[0]);

            var allParticles = ps.GetParticlesAsync().Sync<Particle>();
            var log_particleList = new StringBuilder("Particles:\n");
            for (var i = 0; i < allParticles.Length; i++) {
                var p = allParticles[i];
                if (p.activity == default) continue;

                log_particleList.AppendLine($"  {i}:\tact={p.activity}, pos={p.position}");

                var indexOfAddList = particles_allAdds.FindIndex(v => v.position.Equals(p.position));
                Assert.AreNotEqual(-1, indexOfAddList);
                particles_allAdds.RemoveAt(indexOfAddList);
            }
            Assert.AreEqual(0, particles_allAdds.Count);

            ps.Update(duration * 2f);
            Assert.AreEqual(ps.Capacity, ps.CountIndexPoolAsync().Sync<int>()[0]);
            Assert.AreEqual(0, ps.CountActiveParticlesAsync().Sync<int>()[0]);

            //Debug.Log(log_particleList);
        }

        private static int GetDispatchGroupSize_x(int count, uint cc_x) {
            return (int)((count - 1) / cc_x + 1);
        }

        #region declarations
        public const string PathToCS = "TestCopyCount";

        public const string K_Move = "Move";
        public static readonly int P_IndicesC_Count = Shader.PropertyToID("_IndicesC_Count");
        public static readonly int P_IndicesA = Shader.PropertyToID("_IndicesA");
        public static readonly int P_IndicesC = Shader.PropertyToID("_IndicesC");
        #endregion
    }
}
