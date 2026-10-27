using UnityEngine;

namespace BalanceWithMe
{
    public class DesktopDisplay : MonoBehaviour
    {
        int width, height;
        void Start()
        {
            if (Application.isEditor) { enabled = false; return; }
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            RefreshViewport();
        }
        void Update() { if (width != Screen.width || height != Screen.height) RefreshViewport(); }
        void RefreshViewport()
        {
            width = Screen.width; height = Screen.height;
            const float landscape = 16f / 9;
            float aspect = (float)width / Mathf.Max(1,height);
            var camera = Camera.main;
            camera.rect = aspect > landscape
                ? new Rect((1 - landscape / aspect) / 2, 0, landscape / aspect, 1)
                : new Rect(0, (1 - aspect / landscape) / 2, 1, aspect / landscape);
            camera.aspect = landscape;
            var canvas = FindFirstObjectByType<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera; canvas.planeDistance = 5;
        }
    }
}
