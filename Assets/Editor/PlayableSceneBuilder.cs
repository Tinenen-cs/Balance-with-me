using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BalanceWithMe.Editor
{
    public static class PlayableSceneBuilder
    {
        static Material material;
        static Sprite fistSprite;
        const string Generated = "Assets/Art/Characters/Generated";

        [MenuItem("Balance with me/Rebuild Playable Scene")]
        public static void Build()
        {
            System.IO.Directory.CreateDirectory(Generated);
            AssetDatabase.Refresh();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) throw new InvalidOperationException("URP 2D sprite shader missing.");
            material = AssetDatabase.LoadAssetAtPath<Material>(Generated + "/PixelUnlit.mat");
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, Generated + "/PixelUnlit.mat"); }
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10);
            var cam = camera.GetComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(.4f,.7f,1);
            Sprite bg = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Backgrounds/clouds_green_land.png").OfType<Sprite>().First();
            var background = Render("Clouds and Green Land", bg, new Vector2(0,0), -10);
            background.transform.localScale = new Vector3(19.2f / bg.bounds.size.x, 10.8f / bg.bounds.size.y, 1);
            var propTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Props/platform_ball.png");
            var plankSprite = Slice(propTexture, new Rect(225, 522, 1320, 76), "Plank", new Vector2(.5f,.5f));
            var ballSprite = Slice(propTexture, new Rect(724, 208, 326, 326), "Ball", new Vector2(.5f,.5f));
            var ball = Render("Large Ball", ballSprite, new Vector2(0, -1.7f), 0);
            ball.transform.localScale = Vector3.one * (3 / ballSprite.bounds.size.x);
            var board = new GameObject("Balancing Plank", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(HingeJoint2D));
            board.layer = 8;
            var boardBody = board.GetComponent<Rigidbody2D>(); boardBody.mass = 5; boardBody.angularDamping = 2;
            board.GetComponent<BoxCollider2D>().size = new Vector2(10, .4f);
            var hinge = board.GetComponent<HingeJoint2D>(); hinge.autoConfigureConnectedAnchor = false;
            hinge.connectedAnchor = Vector2.zero; hinge.useLimits = true; hinge.limits = new JointAngleLimits2D { min = -16, max = 16 };
            var boardArt = Render("Plank Artwork", plankSprite, Vector2.zero, 1);
            boardArt.transform.SetParent(board.transform, false);
            boardArt.transform.localScale = new Vector3(10/plankSprite.bounds.size.x, .4f/plankSprite.bounds.size.y, 1);
            fistSprite = CreateFist();
            var game = new GameObject("GameManager").AddComponent<BalanceGame>(); game.board = boardBody;
            game.boy = Player(false, game); game.girl = Player(true, game);
            game.boy.opponent = game.girl; game.girl.opponent = game.boy;
            BuildUI(game);
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue = "Ground"; tags.ApplyModifiedProperties();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Main.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Balance with me: playable push/punch scene generated.");
        }

        static PlayerActions Player(bool girl, BalanceGame game)
        {
            var root = new GameObject(girl ? "Girl" : "Boy", typeof(Rigidbody2D), typeof(CapsuleCollider2D));
            var body = root.GetComponent<Rigidbody2D>(); body.mass = 1; body.gravityScale = 2;
            body.constraints = RigidbodyConstraints2D.FreezeRotation; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = root.GetComponent<CapsuleCollider2D>(); collider.size = new Vector2(.55f, 1.25f); collider.offset = new Vector2(0,.65f);
            var controller = root.AddComponent<CharacterController2DP2>();
            var ground = new GameObject("Ground Check").transform; ground.SetParent(root.transform,false); ground.localPosition = new Vector3(0,.04f,0);
            var ceiling = new GameObject("Ceiling Check").transform; ceiling.SetParent(root.transform,false); ceiling.localPosition = new Vector3(0,1.3f,0);
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("m_GroundCheck").objectReferenceValue = ground;
            serialized.FindProperty("m_CeilingCheck").objectReferenceValue = ceiling;
            serialized.FindProperty("m_WhatIsGround").intValue = 1 << 8;
            serialized.FindProperty("m_AirControl").boolValue = true;
            serialized.FindProperty("m_JumpForce").floatValue = 330;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var actions = root.AddComponent<PlayerActions>(); actions.isGirl = girl; actions.game = game;
            actions.controller = controller; actions.body = body;
            actions.idle = Frames(girl,0); actions.walk = Frames(girl,1); actions.jump = Frames(girl,2); actions.hurt = Frames(girl,3);
            actions.visual = Render("Character", actions.idle[0], Vector2.zero, 5); actions.visual.transform.SetParent(root.transform,false);
            actions.fist = Render("Punch Fist", fistSprite, Vector2.zero, 8); actions.fist.transform.SetParent(root.transform,false);
            actions.fist.transform.localScale = new Vector3(.28f/fistSprite.bounds.size.x,.28f/fistSprite.bounds.size.y,1); actions.fist.color = new Color(1,.65f,.2f);
            actions.palm = Render("Push Palms", fistSprite, Vector2.zero, 8); actions.palm.transform.SetParent(root.transform,false);
            actions.palm.transform.localScale = new Vector3(.16f/fistSprite.bounds.size.x,.4f/fistSprite.bounds.size.y,1); actions.palm.color = new Color(.4f,1,1);
            actions.ResetPlayer(new Vector2(girl ? 2.6f : -2.6f,.24f));
            return actions;
        }

        static Sprite[] Frames(bool girl, int action)
        {
            const string path = "Assets/Art/Characters/two_players.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (!importer.isReadable) { importer.isReadable = true; importer.SaveAndReimport(); }
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int[] rows = {0,164,321,477,634,788,948,1105,1254};
            int[] columns = {0,350,630,910,1254};
            int row = (girl ? 4 : 0) + action;
            string[] names = { "idle", "walk", "jump", "hurt" };
            var pixels = texture.GetPixels32();
            return Enumerable.Range(0,4).Select(col => {
                int minX=texture.width, minY=texture.height, maxX=0, maxY=0;
                for (int y=texture.height-rows[row+1];y<texture.height-rows[row];y++)
                for (int x=columns[col];x<columns[col+1];x++)
                    if (pixels[y*texture.width+x].a > 32) { minX=Math.Min(minX,x); minY=Math.Min(minY,y); maxX=Math.Max(maxX,x); maxY=Math.Max(maxY,y); }
                if (minX>maxX) throw new InvalidOperationException("Empty sprite cell.");
                return Slice(texture,new Rect(minX,minY,maxX-minX+1,maxY-minY+1),(girl?"girl_":"boy_")+names[action]+"_"+(col+1),new Vector2(.5f,0));
            }).ToArray();
        }

        static Sprite Slice(Texture2D texture, Rect rect, string name, Vector2 pivot)
        {
            string path = Generated + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
            var sprite = Sprite.Create(texture,rect,pivot,100,0,SpriteMeshType.FullRect); sprite.name=name;
            AssetDatabase.CreateAsset(sprite,path); return sprite;
        }
        static Sprite CreateFist()
        {
            string path=Generated+"/ActionPixel.asset";
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(texture==null) {
                texture=new Texture2D(8,8,TextureFormat.RGBA32,false) { filterMode=FilterMode.Point };
                var colors=Enumerable.Repeat(Color.white,64).ToArray(); texture.SetPixels(colors); texture.Apply(); AssetDatabase.CreateAsset(texture,path);
            }
            return Slice(texture,new Rect(0,0,8,8),"ActionFist",new Vector2(.5f,.5f));
        }
        static SpriteRenderer Render(string name, Sprite sprite, Vector2 position, int order)
        {
            var render=new GameObject(name).AddComponent<SpriteRenderer>(); render.sprite=sprite; render.sharedMaterial=material;
            render.sortingOrder=order; render.transform.position=position; return render;
        }
        static void BuildUI(BalanceGame game)
        {
            var canvas=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080); scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            Text Label(string name,string content,int fontSize,float y,Vector2 size) {
                var text=new GameObject(name,typeof(RectTransform)).AddComponent<Text>(); text.transform.SetParent(canvas.transform,false);
                var rect=(RectTransform)text.transform; rect.anchoredPosition=new Vector2(0,y); rect.sizeDelta=size;
                text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=fontSize;text.alignment=TextAnchor.MiddleCenter;
                text.color=new Color(.09f,.15f,.2f);text.text=content;text.raycastTarget=false;return text;
            }
            Label("Title","Balance with me",64,440,new Vector2(1700,90));
            game.health=Label("Health","BOY 3/3 | GIRL 3/3",36,345,new Vector2(1600,60));
            game.status=Label("Status","Stay on the plank",30,280,new Vector2(1700,60));
            Label("Controls","BOY: A/D move • W jump • F punch • G push\nGIRL: ←/→ move • ↑ jump • K punch • L push",30,-365,new Vector2(1800,100));
            var restart=new GameObject("Restart",typeof(RectTransform),typeof(Image),typeof(Button));restart.transform.SetParent(canvas.transform,false);
            var rr=(RectTransform)restart.transform;rr.anchoredPosition=new Vector2(0,-470);rr.sizeDelta=new Vector2(300,70);
            restart.GetComponent<Image>().color=new Color(1,.9f,.65f);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(restart.GetComponent<Button>().onClick,game.Restart);
            var label=Label("Restart Label","RESTART (R)",28,0,new Vector2(300,70));label.transform.SetParent(restart.transform,false);
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        }
    }
}
