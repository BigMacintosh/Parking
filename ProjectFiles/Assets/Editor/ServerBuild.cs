using UnityEditor;
//using UnityEditor.Build.Reporting;
using UnityEngine;


public class ServerBuild
{
    
    [MenuItem("Build/Build Server")]
    public static void BuildServer()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/ModelShowcase.unity" };

        var buildFolder = GetArg("-buildFolder");

        buildPlayerOptions.locationPathName = buildFolder;

        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
        //BuildReport report = 
        //BuildSummary summary = report.summary;

        //if (summary.result == BuildResult.Succeeded)
        //{
        //    Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        //}

        //if (summary.result == BuildResult.Failed)
        //{
        //    Debug.Log("Build failed");
        //}
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
