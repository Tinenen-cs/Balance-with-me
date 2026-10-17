using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BalanceWithMe.Editor
{
    public static class ProjectStartup
    {
        const string MainScene = "Assets/Scenes/Main.unity";

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            if (Application.isBatchMode || SessionState.GetBool("BalanceWithMe.StartupChecked", false)) return;
            EditorApplication.delayCall += OpenOnFirstLaunch;
        }

        static void OpenOnFirstLaunch()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += OpenOnFirstLaunch;
                return;
            }
            SessionState.SetBool("BalanceWithMe.StartupChecked", true);
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = SceneManager.GetActiveScene();
            // Preserve saved scenes and unsaved work. Only replace Unity's untouched startup scene.
            if (SceneManager.sceneCount == 1 && string.IsNullOrEmpty(scene.path) && !scene.isDirty &&
                scene.GetRootGameObjects().All(root => root.name == "Main Camera" && root.GetComponent<Camera>() != null))
                EditorSceneManager.OpenScene(MainScene);
            if (SceneManager.GetActiveScene().path == MainScene)
                ConfigureGameView();
        }

        [MenuItem("Balance with me/Open Game Preview")]
        public static void OpenGamePreview()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(MainScene);
            ConfigureGameView();
        }

        public static void ConfigureGameView()
        {
            if (Application.isBatchMode) return;
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            // Unity has no public Game-view aspect API. Resolve the named 16:9 preset
            // defensively, so another editor version can still show the scene normally.
            try
            {
                var assembly = typeof(UnityEditor.Editor).Assembly;
                var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
                var sizes = sizesType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
                var group = sizesType.GetProperty("currentGroup").GetValue(sizes);
                var labels = (string[])group.GetType().GetMethod("GetDisplayTexts").Invoke(group, null);
                int index = System.Array.FindIndex(labels, label => label == "16:9" || label.StartsWith("16:9 Aspect"));
                if (index < 0) return;
                var gameViewType = assembly.GetType("UnityEditor.GameView");
                var view = EditorWindow.GetWindow(gameViewType);
                gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, index);
                view.Repaint();
            }
            catch (System.Exception)
            {
                Debug.Log("Balance with me: select 16:9 in the Game view to match the reference layout.");
            }
        }
    }
}
