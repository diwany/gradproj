using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Museum.EditorTools
{
    /// <summary>
    /// Production VR build pipeline. Produces a self-contained Windows folder
    /// at Build/MuseumVR-Production/ targeting Meta Quest 2 / 3 / Pro through
    /// PCVR Link, with the operator's OpenAI key co-located so the folder is
    /// drop-on-USB portable.
    ///
    /// Hardens the build with pre-flight checks:
    ///   - No leftover XR Device Simulator GameObjects in any built scene.
    ///   - OpenXR auto-loading is ON for Standalone (the production target).
    ///   - LobbyController.loadAdditively is OFF (single-mode scene transition).
    /// </summary>
    public static class PortablePackageBuilder
    {
        const string MenuPath = "Tools/Museum/Phase 7/Build Portable Production Package (VR)";
        const string OutputFolder = "Build/MuseumVR-Production";
        const string ExeName = "MuseumVR.exe";
        const string ConfigFileName = "config.json";
        const string SimulatorGameObjectName = "XR Device Simulator";

        [MenuItem(MenuPath)]
        public static void BuildProductionVR()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var outDir = Path.Combine(projectRoot, OutputFolder);
            var exePath = Path.Combine(outDir, ExeName);

            // 1. Collect enabled scenes from Build Settings.
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                Fail("No scenes enabled in Build Settings. Open File → Build Profiles and enable Lobby + Museum.");
                return;
            }

            // 2. Pre-flight: validate the project is in a production-ready state.
            var preflight = RunPreflightChecks();
            if (preflight.Count > 0)
            {
                var msg = "Production build refused. Fix these first:\n\n  • " + string.Join("\n  • ", preflight);
                Fail(msg);
                return;
            }

            // 3. Strip any leftover XR Device Simulator GameObjects from each scene.
            int simulatorsRemoved = StripSimulatorFromBuildScenes(scenes);

            // 4. Output folder: clear or refuse.
            if (Directory.Exists(outDir))
            {
                if (!EditorUtility.DisplayDialog("Build Portable Production Package",
                    $"Output folder already exists:\n{outDir}\n\nDelete and rebuild?", "Yes, rebuild", "Cancel"))
                {
                    return;
                }
                try { Directory.Delete(outDir, recursive: true); }
                catch (Exception e) { Fail($"Couldn't delete existing folder: {e.Message}"); return; }
            }
            Directory.CreateDirectory(outDir);

            // 5. Build the player. No development flag — this is the production target.
            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Fail($"Build failed: {report.summary.result} (errors: {report.summary.totalErrors}). See Console.");
                return;
            }

            // 6. Co-locate the OpenAI config so the build is portable.
            var portableMuseumDir = Path.Combine(outDir, "MuseumVR");
            Directory.CreateDirectory(portableMuseumDir);
            var srcConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MuseumVR", ConfigFileName);
            bool configCopied = false;
            if (File.Exists(srcConfig))
            {
                File.Copy(srcConfig, Path.Combine(portableMuseumDir, ConfigFileName), overwrite: true);
                configCopied = true;
            }

            // 7. Production README — operator-facing, no developer detail.
            File.WriteAllText(Path.Combine(outDir, "README.txt"), BuildProductionReadme(report, configCopied));

            // 8. Summary dialog.
            string message =
                $"PRODUCTION BUILD\n\n" +
                $"Output:\n{outDir}\n\n" +
                $"  • {ExeName}\n" +
                $"  • MuseumVR/{ConfigFileName}  {(configCopied ? "(copied from %APPDATA%/MuseumVR/)" : "*** MISSING — copy before shipping ***")}\n" +
                $"  • README.txt\n\n" +
                $"Build size: {FormatBytes(report.summary.totalSize)}\n" +
                $"Build time: {report.summary.totalTime:hh\\:mm\\:ss}\n" +
                $"Simulator residue stripped: {simulatorsRemoved} GameObject(s)\n\n" +
                "Drop the folder onto a USB. On the target PC: connect a Meta Quest via Quest Link, run MuseumVR.exe.";

            EditorUtility.DisplayDialog("Production Package Built", message, "Open Folder");
            EditorUtility.RevealInFinder(outDir);
        }

        // ---------------- pre-flight ----------------

        static List<string> RunPreflightChecks()
        {
            var failures = new List<string>();

            // OpenXR auto-loading must be ON for Standalone — this is a VR-only build.
            var manager = GetStandaloneXRManager();
            if (manager == null)
            {
                    failures.Add("XR Plug-in Management for Standalone is not configured.");
            }
            else if (manager.activeLoaders == null || manager.activeLoaders.Count == 0)
            {
                failures.Add("No XR loader is configured for Standalone.");
            }

            // Lobby must be in single-mode load (avoids the dual XR Origin collision).
            var lobbyPath = "Assets/Scenes/Lobby.unity";
            if (!File.Exists(lobbyPath))
            {
                failures.Add($"{lobbyPath} not found.");
            }
            else
            {
                var prev = EditorSceneManager.GetActiveScene().path;
                var lobbyScene = EditorSceneManager.OpenScene(lobbyPath, OpenSceneMode.Single);
                var lc = UnityEngine.Object.FindAnyObjectByType<Museum.Lobby.LobbyController>();
                if (lc == null)
                {
                    failures.Add("LobbyController not found in Lobby.unity. Run Tools > Museum > Phase 4 > Set Up Lobby Scene.");
                }
                else if (lc.loadAdditively)
                {
                    failures.Add("LobbyController.loadAdditively is TRUE. Single mode is required for production (additive briefly instantiates two XR Origins → controllers vanish, white render streak).");
                }
                if (!string.IsNullOrEmpty(prev)) EditorSceneManager.OpenScene(prev, OpenSceneMode.Single);
            }

            return failures;
        }

        static UnityEngine.XR.Management.XRManagerSettings GetStandaloneXRManager()
        {
            UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget perTarget = null;
            EditorBuildSettings.TryGetConfigObject(
                UnityEngine.XR.Management.XRGeneralSettings.k_SettingsKey, out perTarget);
            if (perTarget == null) return null;
            var settings = perTarget.SettingsForBuildTarget(BuildTargetGroup.Standalone);
            return settings?.Manager;
        }

        // ---------------- scene cleanup ----------------

        static int StripSimulatorFromBuildScenes(string[] scenePaths)
        {
            int totalRemoved = 0;
            foreach (var path in scenePaths)
            {
                if (!File.Exists(path)) continue;
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int removedHere = 0;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name == SimulatorGameObjectName || root.name.StartsWith(SimulatorGameObjectName))
                    {
                        UnityEngine.Object.DestroyImmediate(root);
                        removedHere++;
                    }
                }
                if (removedHere > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[PortablePackageBuilder] Stripped {removedHere} simulator GameObject(s) from {path}");
                }
                totalRemoved += removedHere;
            }
            return totalRemoved;
        }

        // ---------------- README ----------------

        static string BuildProductionReadme(BuildReport report, bool configCopied)
        {
            return string.Join("\n", new[]
            {
                "MuseumVR — Production Edition (VR)",
                "==================================",
                "",
                "Run on the target PC:",
                "",
                "  1. Connect a Meta Quest 2 / 3 / Pro via USB-C Link cable",
                "     (or pair via Air Link or Steam Link).",
                "  2. Launch the Meta Quest Link app on the PC.",
                "     In the headset, enable Quest Link from the universal menu.",
                "  3. Double-click MuseumVR.exe.",
                "",
                "Configuration:",
                "",
                $"  • OpenAI API key: MuseumVR/{ConfigFileName} next to the executable.",
                $"    {(configCopied ? "Already populated from the dev machine." : "MISSING — populate before running.")}",
                "    Format: { \"openai_api_key\": \"sk-...\" }",
                "",
                "  • Visitor records: written to MuseumVR/museum.db on first run.",
                "",
                "Visitor Controls (Meta Quest):",
                "",
                "  • Look at any artifact within 2 m for 1.5 s",
                "        → label appears + tour guide narrates.",
                "  • Hold Y (left) or B (right) controller button",
                "        → ask the guide a question (push-to-talk).",
                "  • Left thumbstick → continuous locomotion.",
                "  • Left thumbstick forward + release → teleport.",
                "  • Right thumbstick → 30° snap turn.",
                "",
                "Operator Notes:",
                "",
                "  • The OpenAI key is owned by the operator, never seen by the visitor.",
                "  • Visitor records are local to this installation — no cloud upload.",
                "  • Per-visit OpenAI API cost: approximately $0.05 – $0.15.",
                "  • Internet connection is required for the conversational guide.",
                "",
                "System Requirements:",
                "",
                "  • Windows 10 or 11 (x64).",
                "  • Meta Quest 2, Quest 3, or Quest Pro.",
                "  • Meta Quest Link app installed on the PC.",
                "  • USB-C cable rated for data + Quest Link, or Air Link / Steam Link.",
                "  • Microphone (PC mic, USB mic, or the Quest's built-in mic).",
                "  • Stable internet connection.",
                "",
                "Build Information:",
                "",
                "  Built: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                "  Size : " + FormatBytes(report.summary.totalSize),
                "  Build target: Windows Standalone x64, OpenXR (Meta Quest profiles).",
                "",
                "Support:",
                "",
                "  For configuration help or to rotate the OpenAI key,",
                "  open MuseumVR/" + ConfigFileName + " in any text editor and",
                "  replace the value of openai_api_key.",
            });
        }

        // ---------------- helpers ----------------

        static void Fail(string msg)
        {
            EditorUtility.DisplayDialog("Build Portable Production Package", msg, "OK");
            Debug.LogError("[PortablePackageBuilder] " + msg);
        }

        static string FormatBytes(ulong bytes)
        {
            if (bytes < 1024UL * 1024UL) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024UL * 1024UL * 1024UL) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }
}
