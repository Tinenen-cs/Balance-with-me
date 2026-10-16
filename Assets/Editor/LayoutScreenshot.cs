using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BalanceWithMe.Editor
{
    public static class LayoutScreenshot
    {
        static double started;
        [MenuItem("Balance with me/Capture Full Layout")]
        public static void Capture()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            // Disable domain reload for this automated capture, then restore it.
            SessionState.SetBool("BalanceCapture.OptionsEnabled",EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt("BalanceCapture.Options",(int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled=true;
            EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
            started=0;
            EditorApplication.update+=WaitForPlay;
            EditorApplication.isPlaying=true;
        }
        static void WaitForPlay()
        {
            if(!EditorApplication.isPlaying)return;
            if(started==0)started=EditorApplication.timeSinceStartup;
            if(EditorApplication.timeSinceStartup-started<2)return;
            EditorApplication.update-=WaitForPlay;
            try
            {
                var camera=Camera.main;
                var canvas=Object.FindFirstObjectByType<Canvas>();
                var target=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);
                target.Create();camera.targetTexture=target;camera.aspect=16f/9;
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=5;
                foreach(var label in Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None))
                {
                    label.font.RequestCharactersInTexture(label.text,label.fontSize,label.fontStyle);
                    label.font.material.mainTexture.filterMode=FilterMode.Point;
                }
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest { destination=target });
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest { destination=target });
                var previous=RenderTexture.active;RenderTexture.active=target;
                var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
                System.IO.Directory.CreateDirectory("Docs");System.IO.File.WriteAllBytes("Docs/gameplay_layout.png",pixels.EncodeToPNG());
                RenderTexture.active=previous;camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);
                Debug.Log("Full desktop game layout saved to Docs/gameplay_layout.png.");
            }
            catch(System.Exception exception) { Debug.LogException(exception);SessionState.SetBool("BalanceCapture.Failed",true); }
            EditorApplication.isPlaying=false;
            EditorApplication.update+=WaitForStop;
        }
        static void WaitForStop()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=WaitForStop;
            EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool("BalanceCapture.OptionsEnabled",false);
            EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt("BalanceCapture.Options",0);
            bool failed=SessionState.GetBool("BalanceCapture.Failed",false);SessionState.EraseBool("BalanceCapture.Failed");
            if(Application.isBatchMode)EditorApplication.Exit(failed?1:0);
        }
    }
}
