using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

    public class ServerBuild : MonoBehaviour
    {
        [MenuItem("Build/Build Server")]
        public static void BuildServer()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scene/ModelShowcase.unity" };

            var buildFolder = GetArg("-buildFolder");

            buildPlayerOptions.locationPathName = buildFolder;

            buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        private static string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
