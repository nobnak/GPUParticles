using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUParticleSystem.Actions {

    public interface IAction<in T> where T : IAction<T>.ISettings {

        string name { get; }
        bool enabled { get; set; }

        void Next(GPUParticles particles, float dt, T s);

        public interface ISettings { }
    }
}