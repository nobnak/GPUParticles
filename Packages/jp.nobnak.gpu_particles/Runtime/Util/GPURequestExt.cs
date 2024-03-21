using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUParticleSystem.Util {

    public static class GPURequestExt {

        #region static
        public static T[] Sync<T>(this AsyncGPUReadbackRequest req) where T : struct {
            req.WaitForCompletion();
            using var natives = req.GetData<T>();
            return natives.ToArray<T>();
        }
        #endregion
    }
}