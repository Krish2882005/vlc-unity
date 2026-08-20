using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class VLCUnityPackageExporter
{
    public static void ValidateAndExport()
    {
        BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();
        BuildReport report;

        string dummyLocation = "CI_Build_Cache/build";
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

        if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
        {
            dummyLocation += ".exe";
        }
        else if (target == BuildTarget.StandaloneOSX)
        {
            dummyLocation += ".app";
        }
        else if (target == BuildTarget.StandaloneLinux64)
        {
            dummyLocation += ".x86_64";
        }

        if (activeProfile != null)
        {
            report = BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
            {
                buildProfile = activeProfile,
                locationPathName = dummyLocation,
                options = BuildOptions.Development
            });
        }
        else
        {
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

            // If build settings are empty, we find all the scenes
            if (scenes.Length == 0)
            {
                string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/VLCUnity" });
                scenes = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            }

            Debug.Log($"[CI] Found {scenes.Length} scenes to build.");

            if (scenes.Length == 0)
            {
                ExitCI(1);
                return;
            }

            report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = dummyLocation,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.Development
            });
        }

        if (report != null && report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[CI] Validation failed: {report.summary.totalErrors} errors.");
            ExitCI(1);
            return;
        }

        string exportPath = "VLCUnity.unitypackage";
        string assetDirectory = "Assets/VLCUnity";

        AssetDatabase.ExportPackage(assetDirectory, exportPath, ExportPackageOptions.Recurse);
        Debug.Log("[CI] Export successful.");

        ExitCI(0);
    }

    private static void ExitCI(int exitCode)
    {
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(exitCode);
        }
    }
}
