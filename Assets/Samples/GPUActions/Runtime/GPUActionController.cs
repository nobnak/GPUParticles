using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using UnityEngine.InputSystem;
using GPUParticleSystem.Data;
using GPUParticleSystem.Actions;
using GPUParticleSystem.Util;
using Gist2.Deferred;
using GPUParticleSystem.Constants;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GPUParticleSystem.Samples.GPUActions {

    public class GPUActionController : MonoBehaviour {

        [SerializeField]
        protected Events events = new();
        [SerializeField]
        protected Links links = new();
        [SerializeField]
        protected Presets presets = new();
        [SerializeField]
        protected Tuner tuner = new();

        protected Validator changed = new();

        protected Random rand;
        protected RenderParams renderParams;
        protected MaterialPropertyBlock matProps;
        protected GPUParticles gpart;

        protected ActionMode currAutomation = default;
        protected float readyToEmitCounter;
        protected List<Particle> readyToEmit = new();
        protected LinearAction linear;
        protected RotateAction rotate;
        protected List<Coroutine> actions = new();

        #region unity
        void OnEnable() {
            rand = new((uint)GetInstanceID());
            gpart = new(presets.Capacity);
            linear = new();
            rotate = new();

            matProps = new();
            renderParams = new(links.material) {
                layer = gameObject.layer,
                worldBounds = new(Vector3.zero, Vector3.one * 1000),
                matProps = matProps,
            };

            actions.Clear();
            readyToEmit.Clear();
            readyToEmitCounter = 0f;
            currAutomation = default;

            changed.OnValidate += () => {
                if (currAutomation != tuner.automation) {
                    currAutomation = tuner.automation;
                    StartAction(currAutomation);
                }
            };

            events.onGpuParticleSystemCreated?.Invoke(gpart);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            StartCoroutine(PeriodicReport());
#endif
        }
        void Update() {
            changed.Validate();

            var dt = Time.deltaTime;
            var emitter = links.emitter;
            var mouse = Mouse.current;
            if (emitter != null) {
                readyToEmitCounter += tuner.automation_emitsPerSec * dt;
                if (mouse.leftButton.isPressed)
                    readyToEmitCounter++;

                readyToEmit.Clear();
                for (; readyToEmitCounter >= 1f; readyToEmitCounter--) {
                    var pos = emitter.TransformPoint(rand.NextFloat3(Emitter_Min, Emitter_Max));
                    var p = new Particle() {
                        activity = Activity.Active,
                        position = pos,
                        life = new float4(tuner.particle_lifespan, tuner.particle_lifespan, 0, 0),
                        color = new(1, 1, 1, 1),
                    };
                    readyToEmit.Add(p);
                }

                if (readyToEmit.Count > 0) {
                    gpart.Add(readyToEmit);
                }
            }

            gpart.Update(Time.deltaTime, presets.mode);

            gpart.SetParticles(matProps);
            Graphics.RenderPrimitives(renderParams, MeshTopology.Points, 1, gpart.Capacity);
        }
        void OnDisable() {
            changed.Reset();
            StopAllActions();

            if (linear != null) {
                linear.Dispose();
                linear = null;
            }
            if (rotate != null) {
                rotate.Dispose();
                rotate = null;
            }
            if (gpart != null) {
                events.onGpuParticleSystemCreated?.Invoke(null);
                gpart.Dispose();
                gpart = null;
            }
        }
        void OnValidate() {
            changed.Invalidate();
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
        IEnumerator CoLinearAction() {
            yield return null;

            var s = new LinearAction.Settings();
            while (true) {
                var dt = Time.deltaTime;

                var linear_dir = links.linearDir;
                if (linear_dir != null) {
                    s.speed = tuner.linear_speed;
                    s.forwardDir = linear_dir.forward;
                    linear.Next(gpart, dt, s);
                }
                yield return null;
            }
        }
        IEnumerator CoRotateAction() {
            yield return null;

            var s = new RotateAction.Settings();
            while (true) {
                var dt = Time.deltaTime;

                var rotate_axis = links.rotationAxis;
                if (rotate_axis != null) {
                    s.speed = tuner.rotation_speed;
                    s.axis = rotate_axis.up;
                    s.center = rotate_axis.position;
                    rotate.Next(gpart, dt, s);
                }
                yield return null;
            }
        }
        IEnumerator CoScrewAction() {
            yield return null;

            var ls = new LinearAction.Settings();
            var rs = new RotateAction.Settings();
            while (true) {
                var dt = Time.deltaTime;
                var rotate_axis = links.rotationAxis;
                if (rotate_axis != null) {
                    var axis = rotate_axis.up;
                    var center = rotate_axis.position;

                    ls.speed = tuner.screw_linearSpeed;
                    ls.forwardDir = axis;
                    linear.Next(gpart, dt, ls);

                    rs.speed = tuner.screw_rotationSpeed;
                    rs.axis = axis;
                    rs.center = center;
                    rotate.Next(gpart, dt, rs);
                }
                yield return null;
            }
        }

        void StartAction(ActionMode autoMode) {
            StopAllActions();
            switch (autoMode) {
                case ActionMode.Linear: actions.Add(StartCoroutine(CoLinearAction())); break;
                case ActionMode.Rotate: actions.Add(StartCoroutine(CoRotateAction())); break;
                case ActionMode.Screw: actions.Add(StartCoroutine(CoScrewAction())); break;
                default: StopAllActions(); break;
            }
        }
        void StopAllActions() {
            foreach (var a in actions)
                StopCoroutine(a);
            actions.Clear();
        }
        #endregion

        #region declarations
        public static readonly float3 Emitter_Min = -0.5f * new float3(1, 1, 1);
        public static readonly float3 Emitter_Max = 0.5f * new float3(1, 1, 1);

        public enum ActionMode {
            None = 0,
            Linear,
            Rotate,
            Screw,
        }

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

            public Transform linearDir;
            public Transform rotationAxis;
        }
        [System.Serializable]
        public class Presets {
            [Range(1, 20)]
            public int po2capacity = 10;
            public OperationMode mode = OperationMode.Default;

            public int Capacity => math.clamp(1 << po2capacity, 1, 1 << 20);
        }
        [System.Serializable]
        public class Tuner {
            public float particle_lifespan = 60f;

            [Header("Automation")]
            public ActionMode automation = default;
            [Range(0f, 100f)]
            public float automation_emitsPerSec = 0f;

            [Header("Linear")]
            public float linear_speed = 1f;

            [Header("Rotation")]
            public float rotation_speed = 0.1f;

            [Header("Screw")]
            public float screw_rotationSpeed = 0.1f;
            public float screw_linearSpeed = 1f;
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

                using (new EditorGUILayout.HorizontalScope()) {
                    if (GUILayout.Button("Linear"))
                        t.StartAction(ActionMode.Linear);
                    if (GUILayout.Button("Rotate"))
                        t.StartAction(ActionMode.Rotate);
                    if (GUILayout.Button("Screw"))
                        t.StartAction(ActionMode.Screw);
                }
                if (GUILayout.Button("Stop All"))
                    t.StopAllActions();
            }
        }
#endif
        #endregion
    }
}
