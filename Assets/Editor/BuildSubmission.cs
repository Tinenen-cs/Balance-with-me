using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace BalanceWithMe.Editor
{
    public static class BuildSubmission
    {
        [MenuItem("Balance with me/Build Windows Submission")]
        public static void Build()
        {
            PlayableSceneBuilder.Build();
            PlayerSettings.companyName="Tinenen-cs";PlayerSettings.productName="Balance with me";
            PlayerSettings.bundleVersion="1.0.0";
            PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;
            PlayerSettings.defaultIsNativeResolution=true;PlayerSettings.resizableWindow=true;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.runInBackground=false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
            QualitySettings.vSyncCount=1;
            string root=Environment.GetEnvironmentVariable("BALANCE_BUILD_OUTPUT");
            if(string.IsNullOrEmpty(root)) root=Path.GetFullPath("Builds/Windows");
            Directory.CreateDirectory(root);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/Scenes/Main.unity"},locationPathName=Path.Combine(root,"BalanceWithMe.exe"),
                target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4HC });
            string summary=$"Result: {report.summary.result}\nBytes: {report.summary.totalSize}\nDuration: {report.summary.totalTime}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\n";
            Directory.CreateDirectory("Logs");File.WriteAllText("Logs/submission-build-summary.txt",summary);Debug.Log(summary);
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Submission build failed; see build log.");
        }
    }
}
