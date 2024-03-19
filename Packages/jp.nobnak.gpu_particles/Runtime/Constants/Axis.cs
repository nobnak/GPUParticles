using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GPUParticleSystem.Constants {

    public enum Axis {
        X = 0,
        Y = 1,
        Z = 2,
    }

    public static class AxisExtensions {

        public static float3 Direction(this Axis axis) {
            switch (axis) {
                case Axis.X: return new float3(1, 0, 0);
                case Axis.Y: return new float3(0, 1, 0);
                default: return new float3(0, 0, 1);
            }
        }
    }
}