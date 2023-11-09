using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUParticleSystem.GPUActions {

    public interface IAction {

        string name { get; }
        bool enabled { get; set; }

        void Next(float dt);

    }
}