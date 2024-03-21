using UnityEngine;

namespace GPUParticleSystem.Constants {

    public static class ShaderIDs {
        public static readonly int P_ParticlesCount = Shader.PropertyToID("_ParticlesCount");
        public static readonly int P_Particles = Shader.PropertyToID("_Particles");
    }
}
