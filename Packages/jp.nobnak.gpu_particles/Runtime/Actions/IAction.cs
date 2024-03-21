using Unity.Mathematics;

namespace GPUParticleSystem.Actions {

    public interface IAction<in T> where T : IAction<T>.ISettings {

        string name { get; }
        bool enabled { get; set; }

        // time: dt, time
        void Next(GPUParticles particles, float4 time, T s);

        public interface ISettings { }
    }
}