using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUParticleSystem.Actions {

    public interface IAction {

        string name { get; }
        bool enabled { get; set; }

    }
}