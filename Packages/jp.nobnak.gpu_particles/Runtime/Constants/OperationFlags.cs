using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUParticleSystem.Constants {

    [System.Flags]
    [GenerateHLSL]
    public enum OperationFlags {
        None = 0,
        ConsumeLife = 1 << 0,
    }

    public enum OperationMode {
        Skip = OperationFlags.None,
        Default = OperationFlags.ConsumeLife,
    }
}