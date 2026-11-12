using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PhoenicianJourney.Editor
{
    /// <summary>
    /// Builds the game. From the editor: Tools ▸ Phoenician Journey ▸ Build Windows (or Build Web).
    /// From a terminal (batch mode), see UNITY_MIGRATION.md.
    /// </summary>
    public static class BuildScript
    {
        public const string WindowsOutput = "Builds/Windows/APhoenicianJourney.exe";
        public const string StoryWindowsOutput = "Builds/WindowsStory/APhoenicianJourney.exe";
        public const string WebOutput = "Builds/Web";

        /// <summary>The illustrated 2D story, the team's main version since 12 November 2026.</summary>
        public const string StoryScenePath = "Assets/Scenes/Harbor2D.unity";

        /// <summary>
        /// Windows build of the 2D story (Harbor2D). The 3D harbor build below stays available.
        /// </summary>
        [MenuItem("Tools/Phoenician Journey/Build Windows (2D story)")]
        public static void BuildWindowsStory()
        {
            ApplyCommonSettings();
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            Build(BuildTarget.StandaloneWindows64, StoryWindowsOutput, "Windows 2D story", StoryScenePath);
        }

        /// <summary>
        /// Windows desktop build with the Mono backend, which ships with the editor, so it
        /// needs no extra module.
        /// </summary>
        [MenuItem("Tools/Phoenician Journey/Build Windows")]
        public static void BuildWindows()
        {
            ApplyCommonSettings();
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            Build(BuildTarget.StandaloneWindows64, WindowsOutput, "Windows");
        }

        /// <summary>
        /// Browser build. Needs the Web Build Support module (about 1 GB download, 5.6 GB on disk).
        /// </summary>
        [MenuItem("Tools/Phoenician Journey/Build Web")]
        public static void BuildWeb()
        {
            ApplyCommonSettings();
            // Gzip with a decompression fallback runs on any static server, including the
            // presentation laptop, without special response headers.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            Build(BuildTarget.WebGL, WebOutput, "Web");
        }

        private static void ApplyCommonSettings()
        {
            PlayerSettings.productName = "A Phoenician Journey";
            PlayerSettings.companyName = "A Phoenician Journey team";
        }

        private static void Build(BuildTarget target, string output, string label, string scene = HarborSceneBuilder.ScenePath)
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { scene },
                locationPathName = output,
                target = target,
                options = BuildOptions.None,
            });

            var summary = report.summary;
            Debug.Log($"{label} build {summary.result}: {summary.totalSize / (1024f * 1024f):0.0} MB in {summary.totalTime}, output {output}");
            if (summary.result != BuildResult.Succeeded)
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                throw new Exception($"{label} build {summary.result}");
            }
        }
    }
}
