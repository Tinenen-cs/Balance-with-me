using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BalanceWithMe.Editor
{
    public static class LayoutScreenshot
    {
        static double started, effectStarted;
        static bool actionPreview, staged;
        static bool hurtPreview, resultPreview, winnerPreview, readyPreview, warningPreview, menuPreview, captureSet;
        static int captureIndex;
        static bool hoverPreview, hoverStaged, quitPreview, closePreview;
        public static void CaptureAllPreviews()
        {
            captureSet = true; captureIndex = 0;
            RebuildAndCapture();
        }
        public static void RebuildAndCapture()
        {
            PlayableSceneBuilder.Build();
            Capture();
        }
        [MenuItem("Balance with me/Capture Full Layout")]
        public static void Capture() => BeginCapture(false);
        [MenuItem("Balance with me/Capture Action Effects")]
        public static void CaptureActionEffects() => BeginCapture(true);
        [MenuItem("Balance with me/Capture Hurt Portrait")]
        public static void CaptureHurtPortrait() { BeginCapture(false); hurtPreview = true; }
        [MenuItem("Balance with me/Capture Round Result")]
        public static void CaptureRoundResult() { BeginCapture(false); resultPreview = true; }
        [MenuItem("Balance with me/Capture Winner Announcement")]
        public static void CaptureWinnerAnnouncement() { BeginCapture(false); winnerPreview = true; }
        public static void CaptureReady() { BeginCapture(false); readyPreview = true; }
        public static void CaptureWarning() { BeginCapture(false); warningPreview = true; }
        public static void CaptureMenu() { BeginCapture(false); menuPreview=true; }
        public static void CaptureReplayHover() { BeginCapture(false);winnerPreview=true;hoverPreview=true; }
        public static void CaptureQuitConfirmation() { BeginCapture(false);menuPreview=true;quitPreview=true; }
        public static void CaptureCloseHover() { BeginCapture(false);menuPreview=true;closePreview=true; }
        static void BeginCapture(bool effects)
        {
            hoverPreview=hoverStaged=quitPreview=closePreview=false;
            actionPreview=effects;hurtPreview=resultPreview=winnerPreview=readyPreview=warningPreview=menuPreview=false;staged=false;effectStarted=0;
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
            if(EditorApplication.timeSinceStartup-started<(readyPreview ? .25 : 6))return;
            if(menuPreview && !staged)
            {
                var menu=Object.FindFirstObjectByType<GameMenu>();menu.SetOpen(true);
                if(quitPreview)menu.Quit();
                if(closePreview)
                {
                    var events=UnityEngine.EventSystems.EventSystem.current;events.SetSelectedGameObject(null);
                    var close=menu.panel.transform.Find("Controls Menu/Close Menu");
                    UnityEngine.EventSystems.ExecuteEvents.Execute(close.gameObject,new UnityEngine.EventSystems.PointerEventData(events),UnityEngine.EventSystems.ExecuteEvents.pointerEnterHandler);
                }
                staged=true;effectStarted=EditorApplication.timeSinceStartup;return;
            }
            if(menuPreview && EditorApplication.timeSinceStartup-effectStarted<.5)return;
            if (warningPreview && !staged)
            {
                var game = Object.FindFirstObjectByType<BalanceGame>();
                game.AdvanceRoundTime(game.RemainingSeconds - 30); game.RefreshHud(); staged = true;
            }
            if (winnerPreview)
            {
                var game = Object.FindFirstObjectByType<BalanceGame>();
                game.nextRoundDelay = .1f; game.countdownStepDuration = 0;
                if (!game.MatchComplete)
                {
                    if (game.Running) game.girl.body.position = new Vector2(0, -4);
                    return;
                }
                if (!staged) { staged = true; effectStarted = EditorApplication.timeSinceStartup; }
                if (EditorApplication.timeSinceStartup - effectStarted < .8) return;
                if(hoverPreview && !hoverStaged)
                {
                    var events=UnityEngine.EventSystems.EventSystem.current;
                    events.SetSelectedGameObject(null);
                    UnityEngine.EventSystems.ExecuteEvents.Execute(game.playAgainButton.gameObject,
                        new UnityEngine.EventSystems.PointerEventData(events),UnityEngine.EventSystems.ExecuteEvents.pointerEnterHandler);
                    hoverStaged=true;effectStarted=EditorApplication.timeSinceStartup;return;
                }
            }
            if(actionPreview || hurtPreview || resultPreview)
            {
                if(!staged)
                {
                    var game=Object.FindFirstObjectByType<BalanceGame>();
                    if (resultPreview) game.girl.body.position = new Vector2(0, -4);
                    else
                    {
                        // Separate attack poses for readability, or make contact to show the hurt portrait.
                        float spacing = hurtPreview ? .4f : .8f;
                        game.boy.body.position=new Vector2(-spacing,.1f);game.girl.body.position=new Vector2(spacing,.1f);
                        game.boy.body.linearVelocity=game.girl.body.linearVelocity=Vector2.zero;
                        Physics2D.SyncTransforms();
                        game.boy.TryAction(PlayerActions.ActionType.Punch);
                        if (hurtPreview) game.boy.AdvanceAction(.1f);
                        else game.girl.TryAction(PlayerActions.ActionType.Push);
                    }
                    staged=true;effectStarted=EditorApplication.timeSinceStartup;return;
                }
                if(EditorApplication.timeSinceStartup-effectStarted<(resultPreview ? .95 : hurtPreview ? .1 : .18))return;
            }
            EditorApplication.update-=WaitForPlay;
            try
            {
                var camera=Camera.main;
                var canvas=Object.FindFirstObjectByType<Canvas>();
                var target=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);
                target.Create();camera.targetTexture=target;camera.aspect=16f/9;
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=5;
                // The render target uses reference resolution; discard the batch Game View scale.
                canvas.GetComponent<UnityEngine.UI.CanvasScaler>().enabled=false;canvas.scaleFactor=1;
                Canvas.ForceUpdateCanvases();
                foreach (var player in Object.FindObjectsByType<PlayerActions>(FindObjectsSortMode.None)) player.RefreshEmoteLabel();
                foreach(var label in Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None))
                {
                    label.SetAllDirty();
                    label.font.RequestCharactersInTexture(label.text,label.fontSize,label.fontStyle);
                    label.font.material.mainTexture.filterMode=label.font.name=="Tiny5"?FilterMode.Point:FilterMode.Bilinear;
                }
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest { destination=target });
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest { destination=target });
                var previous=RenderTexture.active;RenderTexture.active=target;
                var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
                string path=quitPreview?"Docs/quit_confirmation.png":closePreview?"Docs/close_hover.png":hoverPreview?"Docs/replay_hover.png":menuPreview?"Docs/submission_controls.png":readyPreview?"Docs/round_start.png":warningPreview?"Docs/time_warning.png":winnerPreview?"Docs/winner_announcement.png":resultPreview?"Docs/round_result.png":hurtPreview?"Docs/hurt_portrait.png":actionPreview?"Docs/action_effects.png":"Docs/gameplay_layout.png";
                System.IO.Directory.CreateDirectory("Docs");System.IO.File.WriteAllBytes(path,pixels.EncodeToPNG());
                RenderTexture.active=previous;camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);
                Debug.Log("Full desktop game layout saved to "+path);
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
            if (captureSet && !failed && ++captureIndex < 11)
            {
                if (captureIndex == 1) CaptureActionEffects();
                else if (captureIndex == 2) CaptureHurtPortrait();
                else if (captureIndex == 3) CaptureRoundResult();
                else if (captureIndex == 4) CaptureWinnerAnnouncement();
                else if (captureIndex == 5) CaptureReady();
                else if (captureIndex == 6) CaptureWarning();
                else if(captureIndex==7) CaptureMenu();
                else if(captureIndex==8) CaptureReplayHover();
                else if(captureIndex==9) CaptureQuitConfirmation();
                else CaptureCloseHover();
                return;
            }
            captureSet = false;
            if(Application.isBatchMode)EditorApplication.Exit(failed?1:0);
        }
    }
}
