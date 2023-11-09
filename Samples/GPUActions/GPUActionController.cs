using GPUParticleSystem.GPUActions;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GPUParticleSystem.Samples.GPUActions {

    public class GPUActionController : MonoBehaviour {

        public Events events = new();
        public Links links = new();
        public Presets presets = new();

        protected Random rand;
        protected RenderParams renderParams;
        protected MaterialPropertyBlock matProps;
        protected GPUParticles gpart;

        protected Dictionary<string, IAction> actions = new();
        protected Coroutine coroutine;

        #region unity
        void OnEnable() {
            rand = new((uint)GetInstanceID());
            gpart = new GPUParticles();

            matProps = new();
            renderParams = new(links.material) {
                layer = gameObject.layer,
                worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000),
                matProps = matProps,
            };

            actions.Clear();
            if (links.actions != null ) {
                foreach (var action in links.actions.GetComponentsInChildren<IAction>()) {
                    if (action == null) continue;
                    var name = action.GetType().Name;
                    actions[name] = action;
                    Debug.Log($"Add action: {name}");
                }
            }

            events.onGpuParticleSystemCreated?.Invoke(gpart);

            StartCoroutine(PeriodicReport());
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
                    lifetime = presets.duration,
                    size = rand.NextFloat(0.8f, 1.2f),
                };
                gpart.Add(p);
            }

            gpart.Update(Time.deltaTime);
            matProps.SetBuffer(P_Particles, gpart.Particles);
            Graphics.RenderPrimitives(renderParams, MeshTopology.Points, 1, gpart.Capacity);
        }
        void OnDisable() {
            if (gpart != null) {
                events.onGpuParticleSystemCreated?.Invoke(null);
                gpart.Dispose();
                gpart = null;
            }
        }
        #endregion

        #region interface
        public void StartAction(IEnumerator enumerator) {
            coroutine = StartCoroutine(enumerator);
        }
        public void StopAction() {
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        #endregion

        #region methods
        IEnumerator PeriodicReport(float interval = 1f) {
            while (enabled) {
                yield return new WaitForSeconds(interval);
                var activeCountRequest = gpart.CountActiveParticlesAsync();
                var poolCountRequest = gpart.CountIndexPoolAsync();
                while (!activeCountRequest.done || !poolCountRequest.done)
                    yield return null;

                if (activeCountRequest.hasError || poolCountRequest.hasError) {
                    Debug.LogError("GPUParticles: error counting particles");
                    continue;
                }

                using var activeCountArray = activeCountRequest.GetData<uint>();
                using var poolCountArray = poolCountRequest.GetData<uint>();
                var activeCount = activeCountArray[0];
                var poolCount = poolCountArray[0];

                var capacity = gpart.Capacity;
                var activeRatio = (float)activeCount / capacity;
                var activeRatioStr = activeRatio.ToString("P2");
                Debug.Log($"Particles: usage={activeRatioStr} ({activeCount}/{capacity})");
            }
        }
        #endregion

        #region actions
        IEnumerator CoGenericAction<T>(float duration) where T : IAction {
            var typeName = typeof(T).Name;
            if (!actions.TryGetValue(typeName, out var action)) {
                Debug.LogError($"Action {typeName} not found");
                yield break;
            }
            yield return null;

            while (duration > 0f) {
                var dt = Time.deltaTime;
                action.Next(dt);

                duration -= dt;
                yield return null;
            }

        }
        IEnumerator CoLinearAction(float duration) {
            return CoGenericAction<LinearAction>(duration);
        }
        IEnumerator CoRotateAction(float duration) {
            return CoGenericAction<RotateAction>(duration);
        }
        #endregion

        #region declarations
        public static readonly float3 Emitter_Min = -0.5f * new float3(1, 1, 1);
        public static readonly float3 Emitter_Max = 0.5f * new float3(1, 1, 1);

        public static readonly int P_Particles = Shader.PropertyToID("_Particles");

        [System.Serializable]
        public class Events {

            public GPUParticlesEvent onGpuParticleSystemCreated = new();

            [System.Serializable]
            public class GPUParticlesEvent : UnityEngine.Events.UnityEvent<GPUParticles> { }
        }
        [System.Serializable]
        public class Links {
            public Material material;
            public Transform emitter;
            public Transform actions;
        }
        [System.Serializable]
        public class Presets {
            public float duration = 60f;
            public float init_speed = 1f;
        }
        #endregion

        #region inspector
#if UNITY_EDITOR
        [CustomEditor(typeof(GPUActionController))]
        public class Editor : UnityEditor.Editor {

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
                var t = target as GPUActionController;
                if (t == null) return;

                if (GUILayout.Button("Linear")) {
                    t.StopAction();
                    t.StartCoroutine(t.CoLinearAction(10f));
                }
                if (GUILayout.Button("Rotate")) {
                    t.StopAction();
                    t.StartCoroutine(t.CoRotateAction(10f));
                }
                if (GUILayout.Button("Stop All")) {
                    t.StopAction();
                }
            }
        }
#endif
        #endregion
    }
}
