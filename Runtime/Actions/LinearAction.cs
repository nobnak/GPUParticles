using Gist2.Extensions.ComponentExt;
using GPUParticleSystem.Constants;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GPUParticleSystem.Actions {

    public class LinearAction : MonoBehaviour, IAction {

        protected ComputeShader cs;
        protected int k_Linear;
        protected uint g_Linear;

        #region unity
        void OnEnable() {
            cs = Resources.Load<ComputeShader>(CS_NAME);
            k_Linear = cs.FindKernel(K_Linear);
            cs.GetKernelThreadGroupSizes(k_Linear, out g_Linear, out _, out _);
        }
        #endregion

        #region interface
        public virtual void Next(float dt, Settings s) {
            if (!isActiveAndEnabled || s.particles == null) return;

            var gb_particle = s.particles.Particles;

            var forward = s.forwardDir;
#if UNITY_EDITOR
            var forward_lensq = math.lengthsq(forward);
            if (forward_lensq < 0.99f || forward_lensq > 1.01f) {
                Debug.LogWarning($"LinearAction: forward direction is not normalized: {forward}");
            }
#endif

            cs.SetVector(P_LinearDirection, new float4(forward * s.speed, 0f));
            cs.SetFloat(GPUParticles.P_DeltaTime, dt);
            cs.SetBuffer(k_Linear, GPUParticles.P_Particles, gb_particle);

            var dispatchCount = GPUParticles.DispatcCount(gb_particle.count, g_Linear);
            cs.SetInts(GPUParticles.P_ThreadCount, gb_particle.count);
            cs.Dispatch(k_Linear, dispatchCount, 1, 1);
        }
        #endregion

        #region declarations
        public const string CS_NAME = "GPUActions/Linear";
        public const string K_Linear = "Linear";

        public static readonly int P_LinearDirection = Shader.PropertyToID("_LinearDirection");

        public class Settings {
            public GPUParticles particles;
            public float3 forwardDir;
            public float speed;
        }
        #endregion
    }
}