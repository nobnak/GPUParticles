using Gist2.Extensions.ComponentExt;
using GPUParticleSystem.Constants;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GPUParticleSystem.Actions {

    public class RotateAction : MonoBehaviour, IAction {

        [SerializeField]
        protected Links links = new();
        [SerializeField]
        protected Tuner tuner = new();

        protected ComputeShader cs;
        protected int k_Rotate;
        protected uint g_Rotate;

        #region properties
        public Tuner CurrTuner {
            get => tuner.DeepCopy();
            set => tuner = value.DeepCopy();
        }
        public Links CurrLinks {
            get => links;
            set => links = value;
        }
        public GPUParticles particles { get; set; }
        #endregion

        #region unity
        void OnEnable() {
            cs = Resources.Load<ComputeShader>(CS_NAME);
            k_Rotate = cs.FindKernel(K_Linear);
            cs.GetKernelThreadGroupSizes(k_Rotate, out g_Rotate, out _, out _);
        }
        #endregion

        #region interface
        public virtual void Next(float dt) {
            if (!isActiveAndEnabled || particles == null) return;

            var axis = links.axis != null ? links.axis : transform;
            var gb_particle = particles.Particles;

            float3 rotation_center = axis.position;
            float3 rotation_axis = axis.TransformDirection(tuner.rotationAxis.Direction());
            var rotation_angle = tuner.speed * dt * TWO_PI;
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

        [System.Serializable]
        public class  Links {
            public Transform axis;
        }
        [System.Serializable]
        public class Tuner {
            public Axis rotationAxis = Axis.Y;
            public float speed = 1f;
        }
        #endregion
    }
}