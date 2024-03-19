using GPUParticleSystem.Constants;
using UnityEngine;

namespace GPUParticleSystem.Util {

    public static class GPUPropertyExt {

        public static MaterialPropertyBlock SetParticles(this MaterialPropertyBlock matProps, GraphicsBuffer particles) {
            if (particles == null) {
                matProps.SetInt(ShaderIDs.P_ParticlesCount, 0);
            } else {
                matProps.SetInt(ShaderIDs.P_ParticlesCount, particles.count);
                matProps.SetBuffer(ShaderIDs.P_Particles, particles);
            }
            return matProps;
        }
        public static GraphicsBuffer SetGlobalParticles(this GraphicsBuffer particles) {
            particles.count.SetGlobalParticleCount();
            Shader.SetGlobalBuffer(ShaderIDs.P_Particles, particles);
            return particles;
        }
        public static int SetGlobalParticleCount(this int count) {
            Shader.SetGlobalInt(ShaderIDs.P_ParticlesCount, count);
            return count;
        }
    }
}
