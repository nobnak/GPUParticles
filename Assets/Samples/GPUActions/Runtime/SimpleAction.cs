using GPUParticleSystem.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUParticleSystem.Samples.GPUActions {
    public class SimpleAction : System.IDisposable { 
        public ComputeShader compute;
        public int kUpdate;
        public uint groupSize;

        public SimpleAction() {
            compute = Resources.Load<ComputeShader>(CS_RESOURCE_NAME);
            kUpdate = compute.FindKernel("Update");
            compute.GetKernelThreadGroupSizes(kUpdate, out groupSize, out _, out _);
        }

        public SimpleAction Update(GPUParticles gpart, float dt) {
            var particleCount = gpart.Particles.count;
            var dispatchSize = (particleCount - 1) / (int)groupSize + 1;
            gpart.SetParticles(compute, kUpdate);
            compute.SetInt(GPUParticles.P_ThreadCount, particleCount);
            compute.SetFloat(GPUParticles.P_DeltaTime, dt);
            compute.Dispatch(kUpdate, dispatchSize, 1, 1);
            return this;
        }

        public void Dispose() {
        }

        #region declarations
        public const string CS_RESOURCE_NAME = "SimpleAction";
        #endregion
    }
}