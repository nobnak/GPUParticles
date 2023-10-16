using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
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
            using var ps = new GPUParticles();

            ps.Init();
            ps.Update();

            var particles_add = new List<Particle>();
            for (var i = 0; i < count_add; i++) {
                var p = new Particle() {
                    activity = 1,
                    position = new float3(i, 0, 0),
                    velocity = new float3(0, 0, 0),
                    duration = 10f,
                };
                particles_add.Add(p);
            }
            ps.Add(particles_add.ToArray());

            var count_indexPool = ps.CountIndexPool();
            Assert.AreEqual(count_add, ps.Capacity - (int)count_indexPool);

            var allParticles = ps.GetParticles();
            var log_particleList = new StringBuilder("Particles:\n");
            for (var i = 0; i < allParticles.Length; i++) {
                var p = allParticles[i];
                if (p.activity == default) continue;

                log_particleList.AppendLine($"  {i}:\tact={p.activity}, pos={p.position}");

                var indexOfAddList = particles_add.FindIndex(v => v.position.Equals(p.position));
                Assert.AreNotEqual(-1, indexOfAddList);
                particles_add.RemoveAt(indexOfAddList);
            }
            Assert.AreEqual(0, particles_add.Count);
            Debug.Log(log_particleList);
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
