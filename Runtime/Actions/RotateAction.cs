using Gist2.Extensions.ComponentExt;
using GPUParticleSystem.Constants;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GPUParticleSystem.Actions {

    public class RotateAction : MonoBehaviour, IAction {

        protected ComputeShader cs;
        protected int k_Rotate;
        protected uint g_Rotate;

        #region unity
        void OnEnable() {
            cs = Resources.Load<ComputeShader>(CS_NAME);
            k_Rotate = cs.FindKernel(K_Linear);
            cs.GetKernelThreadGroupSizes(k_Rotate, out g_Rotate, out _, out _);
        }
        #endregion

        #region interface
        public virtual void Next(float dt, Settings settings) {
            if (!isActiveAndEnabled || settings.particles == null) return;

            var gb_particle = settings.particles.Particles;

            var rotation_axis = settings.axis;
            var rotation_center = settings.center;
#if UNITY_EDITOR
            var lensq_axis = math.lengthsq(rotation_axis);
            if (lensq_axis < 0.99f || 1.01f < lensq_axis)
                Debug.LogWarning($"Rotation axis is not normalized. {rotation_axis}");
#endif

            var rotation_angle = settings.speed * dt * TWO_PI;
            var rotation_matrix = float4x4.AxisAngle(rotation_axis, rotation_angle);
            rotation_matrix = math.mul(
                float4x4.Translate(rotation_center),
                rotation_matrix);
            rotation_matrix = math.mul(rotation_matrix,
                float4x4.Translate(-rotation_center));

            cs.SetMatrix(P_RotationMatrix, rotation_matrix);
            cs.SetFloat(GPUParticles.P_DeltaTime, dt);
            cs.SetBuffer(k_Rotate, GPUParticles.P_Particles, gb_particle);

            var dispatchCount = GPUParticles.DispatcCount(gb_particle.count, g_Rotate);
            cs.SetInts(GPUParticles.P_ThreadCount, gb_particle.count);
            cs.Dispatch(k_Rotate, dispatchCount, 1, 1);
        }
        #endregion

        #region declarations
        public const string CS_NAME = "GPUActions/Rotate";
        public const string K_Linear = "Rotate";

        public static readonly int P_RotationMatrix = Shader.PropertyToID("_RotationMatrix");

        public static readonly float TWO_PI = 2f * math.PI;

        public class Settings {
            public GPUParticles particles;
            public float3 center;
            public float3 axis;
            public float speed;
        }
        #endregion
    }
}