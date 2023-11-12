using GPUParticleSystem.Samples.GPUActions;
using UnityEngine;

namespace GPUParticleSystem.Samples.GPUActions.Utils {

    public static class GPUPropertyExt {

        public static MaterialPropertyBlock SetParticles(this MaterialPropertyBlock matProps, GraphicsBuffer particles) {
            if (particles == null) {
                matProps.SetInt(GPUActionController.P_ParticlesCount, 0);
            } else {
                matProps.SetInt(GPUActionController.P_ParticlesCount, particles.count);
                matProps.SetBuffer(GPUActionController.P_Particles, particles);
            }
            return matProps;
        }
        public static GraphicsBuffer SetGlobalParticles(this GraphicsBuffer particles) {
            particles.count.SetGlobalParticleCount();
            Shader.SetGlobalBuffer(GPUActionController.P_Particles, particles);
            return particles;
        }
        public static int SetGlobalParticleCount(this int count) {
            Shader.SetGlobalInt(GPUActionController.P_ParticlesCount, count);
            return count;
        }
    }
}
