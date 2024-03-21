using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager;
using UnityEngine;

namespace GPUParticleSystem.Editor {

    public static class PackGPUParticleData {

        public static DateTimeOffset Now => DateTimeOffset.Now;

        [DidReloadScripts]
        public static void OnScriptsReloaded() {
            if (Directory.Exists(OUTPUT_FOLDER)) {
                var dateOut = Directory.GetLastWriteTime(OUTPUT_FOLDER);
                var dateIn_runtime = Directory.GetLastWriteTime(Path.Combine(INPUT_FOLDER, "Runtime"));
                var dateIn_package = File.GetLastWriteTime(Path.Combine(INPUT_FOLDER, "package.json"));
                var dateIn = (dateIn_runtime > dateIn_package) ? dateIn_runtime : dateIn_package;
                if (dateIn < dateOut) {
                    return;
                }
                Debug.Log($"Date in={dateIn}, out={dateOut}");
            }

            var request = Client.Pack(INPUT_FOLDER, OUTPUT_FOLDER);
            var now = Now;
            var startTime = now;
            var took = 0f;
            while (!request.IsCompleted && took < 10) {
                Thread.Sleep(0);
                took = (float) ((now = Now) - startTime).TotalSeconds;
            }

            if (request.Status != StatusCode.Success) {
                Debug.LogError($"Packaging {PKG_PARTICLE_DATA} is failed.");
            }
        }

        #region declarations
        public const string PKG_PARTICLE = "jp.nobnak.gpu_particles";
        public const string PKG_PARTICLE_DATA = "jp.nobnak.gpu_particles.data";
        public const string INPUT_FOLDER = "Packages/" + PKG_PARTICLE_DATA;
        public const string OUTPUT_FOLDER = "Packages/" + PKG_PARTICLE + "/LocalPackages~";
        #endregion
    }

}