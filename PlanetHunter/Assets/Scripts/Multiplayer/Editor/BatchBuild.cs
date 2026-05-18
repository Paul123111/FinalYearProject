// Copyright 2019 Google LLC
// All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AgonesExample.Editor {
    public class BatchBuild {
        [MenuItem("Build Tool/Build Server")]
        public static void BuildServer() {
            string[] scenes = new[] { "Assets/Scenes/networking/MultiplayerMenu.unity",
                "Assets/Scenes/networking/GameNetworking.unity",
                "Assets/Scenes/networking/AstronautTest.unity",
                "Assets/Scenes/networking/ice.unity",
                "Assets/Scenes/networking/red.unity",
            };
            string dir = "Builds/Server";

            bool ok = SafeCleanDirectory(dir);
            if (!ok) {
                Debug.LogError($"Error: Directory could not be deleted: {dir}");
                return;
            }

            BuildPlayerOptions option = new BuildPlayerOptions {
                scenes = scenes,
                locationPathName = dir + "/UnitySimpleServer.x86_64",
                target = BuildTarget.StandaloneLinux64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(option);
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded) {
                Debug.Log($"Server Build Succeeded: {summary.totalSize / 1024 / 1024} MB");
            } else {
                Debug.LogError("Server Build Failed");
            }
        }

        [MenuItem("Build Tool/Build Client")]
        public static void BuildClient() {
            PlayerSettings.runInBackground = true;
            PlayerSettings.visibleInBackground = true;
            string[] scenes = new[] { "Assets/Scenes/networking/MultiplayerMenu.unity",
                "Assets/Scenes/networking/GameNetworking.unity",
                "Assets/Scenes/networking/AstronautTest.unity",
                "Assets/Scenes/networking/ice.unity",
                "Assets/Scenes/networking/red.unity",
            };

            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] {
                UnityEngine.Rendering.GraphicsDeviceType.Direct3D11,
                UnityEngine.Rendering.GraphicsDeviceType.Direct3D12
            });

            string dir = "Builds/PlanetHunter";

            Debug.Log("Building Client...");
            bool ok = SafeCleanDirectory(dir);
            if (!ok) {
                Debug.LogError($"Error: Directory could not be deleted: {dir}");
                return;
            }

            var target = BuildTarget.StandaloneWindows64;
#if UNITY_EDITOR_OSX
            target = BuildTarget.StandaloneOSX;
#elif !UNITY_EDITOR_WIN && !UNITY_EDITOR_OSX
            target = BuildTarget.StandaloneLinux64;
#endif
            BuildPlayerOptions option = new BuildPlayerOptions {
                scenes = scenes,
                locationPathName = dir + "/PlanetHunterClient.exe",
                target = target,
                subtarget = (int)StandaloneBuildSubtarget.Player,
                options = BuildOptions.None
            };
            BuildPipeline.BuildPlayer(option);
        }

        private static bool SafeCleanDirectory(string relativePath) {
            // convert to absolute path
            string absolutePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            Debug.Log("abspath: " + absolutePath + ", root: " + projectRoot);

            // block important Unity folders
            string folderName = Path.GetFileName(absolutePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)).ToLower();
            if (folderName == "assets" || folderName == "library" || folderName == "projectsettings" || absolutePath == projectRoot) {
                Debug.LogError($"SAFETY TRIGGERED: Blocked attempt to delete a critical project directory: {absolutePath}");
                return false;
            }

            // ensure the path is strictly nested inside a designated "Builds" subdirectory
            string buildsDirectory = Path.Combine(projectRoot, "Builds");
            if (!absolutePath.StartsWith(buildsDirectory, System.StringComparison.OrdinalIgnoreCase)) {
                Debug.LogError($"SAFETY TRIGGERED: Path is outside the designated Builds folder! Aborting wipe for: {absolutePath}");
                return false;
            }

            // if guards pass, execute the deletion safely
            if (Directory.Exists(absolutePath)) {
                try {
                    Directory.Delete(absolutePath, true);
                    Debug.Log($"Cleaned old build artifacts safely at: {relativePath}");
                } catch (IOException ex) {
                    Debug.LogWarning($"Could not wipe folder. A file might be locked by a running process: {ex.Message}");
                    return false;
                }
            }

            int retries = 0;
            while (Directory.Exists(absolutePath) && retries < 10) {
                // wait for OS to delete files
                Thread.Sleep(200);
                Debug.Log("Waiting for deletion");
                retries++;
            }

            if (Directory.Exists(absolutePath)) {
                Debug.LogError($"OS failed to release directory lock in time: {absolutePath}");
                return false;
            }

            // Always re-ensure the directory structure exists for the upcoming compilation step
            Directory.CreateDirectory(absolutePath);
            return true;
        }
    }
}
