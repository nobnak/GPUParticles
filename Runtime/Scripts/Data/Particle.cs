using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;



[GenerateHLSL]
public struct Particle {
    public float3 position;
    public float3 velocity;
    public float duration;
    public int activity;
}
