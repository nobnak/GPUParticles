using Gist2.Extensions.ComponentExt;
using GPUParticleSystem.Constants;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GPUParticleSystem.Actions {

    public class LinearAction : System.IDisposable, IAction<LinearAction.Settings> {

        protected ComputeShader cs;
        protected int k_Linear;
        protected uint g_Linear;

        public LinearAction() {
            cs = Resources.Load<ComputeShader>(CS_NAME);
            k_Linear = cs.FindKernel(K_Linear);
            cs.GetKernelThreadGroupSizes(k_Linear, out g_Linear, out _, out _);
        }

        #region IAction
        public string name => this.GetType().Name;
        public bool enabled { get; set; } = true;

        public virtual void Next(GPUParticles particle, float4 time, Settings s) {
            if (!enabled || particle == null) return;

            var gb_particle = particle.Particles;

            var forward = s.forwardDir;
#if UNITY_EDITOR
            var forward_lensq = math.lengthsq(forward);
            if (forward_lensq < 0.99f || forward_lensq > 1.01f) {
                Debug.LogWarning($"LinearAction: forward direction is not normalized: {forward}");
            }
#endif

            cs.SetVector(P_LinearDirection, new float4(forward * s.speed, 0f));
            cs.SetFloat(GPUParticles.P_DeltaTime, time.x);
            cs.SetBuffer(k_Linear, GPUParticles.P_Particles, (GraphicsBuffer)gb_particle);

            var dispatchCount = GPUParticles.DispatcCount((int)gb_particle.count, g_Linear);
            cs.SetInts(GPUParticles.P_ThreadCount, (int)gb_particle.count);
            cs.Dispatch(k_Linear, dispatchCount, 1, 1);
        }
        #endregion

        #region IDisposable Support
        public void Dispose() {
        }
        #endregion

        #region declarations
        public const string CS_NAME = "GPUActions/Linear";
        public const string K_Linear = "Linear";

        public static readonly int P_LinearDirection = Shader.PropertyToID("_LinearDirection");

        public class Settings : IAction<Settings>.ISettings {
            public float3 forwardDir;
            public float speed;
        }
        #endregion
    }
}