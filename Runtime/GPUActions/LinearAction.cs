using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUParticleSystem.GPUActions {

    public class LinearAction : MonoBehaviour {

        public Links links = new();
        public Tuner tuner = new();

        protected ComputeShader cs;
        protected int k_Linear;
        protected uint g_Linear;

        #region properties
        public GPUParticles particles { get; set; }
        #endregion

        #region unity
        void OnEnable() {
            cs = Resources.Load<ComputeShader>(CS_NAME);
            k_Linear = cs.FindKernel(K_Linear);
            cs.GetKernelThreadGroupSizes(k_Linear, out g_Linear, out _, out _);
        }
        void Update() {
            if (particles == null) return;

            var forward = links.forward != null ? links.forward.forward : transform.forward;
            var gb_particle = particles.Particles;

            cs.SetVector(P_LinearDirection, forward * tuner.speed);
            cs.SetFloat(GPUParticles.P_DeltaTime, Time.deltaTime);
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

        [System.Serializable]
        public class  Links {
            public Transform forward;
        }
        [System.Serializable]
        public class Tuner {
            public float speed = 1f;
        }
        #endregion
    }
}