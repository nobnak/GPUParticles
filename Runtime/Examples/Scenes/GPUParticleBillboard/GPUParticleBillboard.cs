using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace GPUParticleSystem.Examples {

    public class GPUParticleBillboard : MonoBehaviour {

        public Links links = new();
        public Presets presets = new();

        protected Random rand;
        protected RenderParams renderParams;
        protected GPUParticles gpart;

        #region unity
        void OnEnable() {
            rand = new((uint)GetInstanceID());
            gpart = new GPUParticles();

            renderParams = new(links.material) {
                layer = gameObject.layer,
                camera = links.targetCamera,
                worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000),
            };
        }
        void Update() {
            var emitter = links.emitter;
            if (emitter != null && Input.GetMouseButton(0)) {
                var pos = emitter.TransformPoint(rand.NextFloat3(Emitter_Min, Emitter_Max));
                var dir = math.mul(rand.NextQuaternionRotation(), new float3(0, 0, 1));
                var p = new Particle() {
                    activity = 1,
                    position = pos,
                    velocity = presets.init_speed * dir,
                    duration = presets.duration,
                };
                gpart.Add(p);
            }

            gpart.Update(Time.deltaTime);
            Graphics.RenderPrimitives(renderParams, MeshTopology.Points, 1, gpart.Capacity);

            Debug.Log($"Active particles: count={gpart.CountActiveParticles()} pool={gpart.CountIndexPool()}");
        }
        void OnDisable() {
            if (gpart != null) {
                gpart.Dispose();
                gpart = null;
            }
        }
        #endregion

        #region declarations
        public static readonly float3 Emitter_Min = -0.5f * new float3(1, 1, 1);
        public static readonly float3 Emitter_Max = 0.5f * new float3(1, 1, 1);

        [System.Serializable]
        public class Links {
            public Camera targetCamera;
            public Material material;
            public Transform emitter;
        }
        [System.Serializable]
        public class Presets {
            public float duration = 60f;
            public float init_speed = 1f;
        }
        #endregion
    }
}
