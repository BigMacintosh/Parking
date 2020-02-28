using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ServerBuild {
    [MenuItem("Build/Build Server")]
    public static void BuildServer() {
        var buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] {"Assets/Scenes/ModelShowcase.unity"};


        const string buildFolder = "../builds/server/server";

        buildPlayerOptions.locationPathName = buildFolder;

        buildPlayerOptions.target  = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.options = BuildOptions.EnableHeadlessMode;

        var report  = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded) {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed) {
            Debug.Log("Build failed");
        }
    }

    [MenuItem("Build/Build Client")]
    public static void BuildClient() {
        var buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] {"Assets/Scenes/MainMenu.unity", "Assets/Scenes/ModelShowcase.unity"};


        const string buildFolder = "../builds/client/client";

        buildPlayerOptions.locationPathName = buildFolder;

        buildPlayerOptions.target  = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.options = BuildOptions.None;


        var report  = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded) {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed) {
            Debug.Log("Build failed");
        }
    }
}