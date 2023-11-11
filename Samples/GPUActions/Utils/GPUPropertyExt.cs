using GPUParticleSystem.Samples.GPUActions;
using UnityEngine;

namespace GPUParticleSystem.Samples.GPUActions.Utils {

    public static class GPUPropertyExt {

        public static MaterialPropertyBlock SetParticles(this MaterialPropertyBlock matProps, GraphicsBuffer particles) {
            matProps.SetInt(GPUActionController.P_ParticlesCount, particles.count);
            matProps.SetBuffer(GPUActionController.P_Particles, particles);
            return matProps;
        }
        public static GraphicsBuffer SetGlobalParticles(this GraphicsBuffer particles) {
            Shader.SetGlobalInt(GPUActionController.P_ParticlesCount, particles.count);
            Shader.SetGlobalBuffer(GPUActionController.P_Particles, particles);
            return particles;
        }
    }
}
