using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEditor.U2D.Sprites;
using System.Collections.Generic;

namespace BalanceWithMe.Editor
{
    public static class PlayableSceneBuilder
    {
        static Material material;
        static Sprite fistSprite;
        static Sprite[] characterFrames;
        const string Generated = "Assets/Art/Characters/Generated";

        [MenuItem("Balance with me/Rebuild Playable Scene")]
        public static void Build()
        {
            System.IO.Directory.CreateDirectory(Generated);
            AssetDatabase.Refresh();
            PrepareCharacterSprites();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) throw new InvalidOperationException("URP 2D sprite shader missing.");
            material = AssetDatabase.LoadAssetAtPath<Material>(Generated + "/PixelUnlit.mat");
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, Generated + "/PixelUnlit.mat"); }
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10);
            var cam = camera.GetComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(.4f,.7f,1);
            const string backgroundPath = "Assets/Art/Backgrounds/farm_village_wood_ball.png";
            var backgroundImporter = (TextureImporter)AssetImporter.GetAtPath(backgroundPath);
            backgroundImporter.textureType = TextureImporterType.Sprite;
            backgroundImporter.spriteImportMode = SpriteImportMode.Single;
            backgroundImporter.filterMode = FilterMode.Point;
            backgroundImporter.textureCompression = TextureImporterCompression.Uncompressed;
            backgroundImporter.mipmapEnabled = false;
            backgroundImporter.maxTextureSize = 4096;
            backgroundImporter.SaveAndReimport();
            Sprite bg = AssetDatabase.LoadAllAssetsAtPath(backgroundPath).OfType<Sprite>().First();
            var background = Render("Farmer Village and Wooden Ball", bg, new Vector2(0,-.18f), -10);
            // Uniform overscan keeps the wooden fulcrum round and seats its top under the plank.
            background.transform.localScale = Vector3.one * (11.6f / bg.bounds.size.y);
            var plankSprite = ImportPlank();
            var board = new GameObject("Balancing Plank", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(HingeJoint2D));
            board.transform.position = new Vector3(0,-.2f,0);
            board.layer = 8;
            var boardBody = board.GetComponent<Rigidbody2D>(); boardBody.mass = 7; boardBody.angularDamping = 2.5f;
            boardBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            board.GetComponent<BoxCollider2D>().size = new Vector2(10, .4f);
            var hinge = board.GetComponent<HingeJoint2D>(); hinge.autoConfigureConnectedAnchor = false;
            hinge.connectedAnchor = new Vector2(0,-.2f); hinge.useLimits = true; hinge.limits = new JointAngleLimits2D { min = -16, max = 16 };
            var boardArt = Render("Plank Artwork", plankSprite, Vector2.zero, 1);
            boardArt.transform.SetParent(board.transform, false);
            boardArt.transform.localScale = new Vector3(10/plankSprite.bounds.size.x, .4f/plankSprite.bounds.size.y, 1);
            fistSprite = CreateFist();
            var game = new GameObject("GameManager").AddComponent<BalanceGame>(); game.board = boardBody;
            game.inputs = game.gameObject.AddComponent<LocalInputRouter>();
            game.sounds = BuildAudio();
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

        static Sprite ImportPlank()
        {
            const string path="Assets/Art/Props/farm_balance_plank.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.isReadable=true;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
            importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;
            importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=texture.GetPixels32();
            int minX=texture.width,minY=texture.height,maxX=0,maxY=0;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
                if(pixels[y*texture.width+x].a>32){minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}
            var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var id=new GUID(Hash128.Compute(path).ToString());
            provider.SetSpriteRects(new[]{new SpriteRect{name="Farm Plank",spriteID=id,rect=new Rect(minX,minY,maxX-minX+1,maxY-minY+1),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)}});
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[]{new SpriteNameFileIdPair("Farm Plank",id)});
            provider.Apply();importer.SaveAndReimport();return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
        }

        static GameAudio BuildAudio()
        {
            var root = new GameObject("Audio System");
            var audio = root.AddComponent<GameAudio>();
            audio.music = root.AddComponent<AudioSource>(); audio.music.playOnAwake = false; audio.music.volume = .18f;
            audio.effects = root.AddComponent<AudioSource>(); audio.effects.playOnAwake = false; audio.effects.volume = .55f;
            audio.tracks = new[] { "sky_breeze", "playful_balance" }.Select(name => AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/"+name+".wav")).ToArray();
            audio.cues = new[] { "jump", "punch", "push", "hit", "shove", "start", "win", "draw", "emote_happy", "emote_sad", "step", "land", "fail" }
                .Select(name => new GameAudio.Cue { name=name, clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/"+name+".wav") }).ToArray();
            if (audio.tracks.Any(clip => clip == null) || audio.cues.Any(cue => cue.clip == null))
                throw new InvalidOperationException("Audio missing. Run python3 Tools/make_audio.py before rebuilding.");
            return audio;
        }

        static PlayerActions Player(bool girl, BalanceGame game)
        {
            var root = new GameObject(girl ? "Girl" : "Boy", typeof(Rigidbody2D), typeof(CapsuleCollider2D));
            var body = root.GetComponent<Rigidbody2D>(); body.mass = 1; body.gravityScale = 2; body.interpolation = RigidbodyInterpolation2D.Interpolate;
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
            serialized.FindProperty("m_MovementSmoothing").floatValue = .08f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var actions = root.AddComponent<PlayerActions>(); actions.isGirl = girl; actions.game = game;
            actions.controller = controller; actions.body = body;
            actions.idle = Frames(girl,0); actions.walk = Frames(girl,1); actions.jump = Frames(girl,2); actions.hurt = Frames(girl,3);
            actions.visual = Render("Character", actions.idle[0], Vector2.zero, 5); actions.visual.transform.SetParent(root.transform,false);
            actions.fist = Render("Punch Fist", fistSprite, Vector2.zero, 8); actions.fist.transform.SetParent(root.transform,false);
            actions.fist.transform.localScale = new Vector3(.28f/fistSprite.bounds.size.x,.28f/fistSprite.bounds.size.y,1); actions.fist.color = new Color(1,.65f,.2f);
            actions.palm = Render("Push Palms", fistSprite, Vector2.zero, 8); actions.palm.transform.SetParent(root.transform,false);
            actions.palm.transform.localScale = new Vector3(.16f/fistSprite.bounds.size.x,.4f/fistSprite.bounds.size.y,1); actions.palm.color = new Color(.4f,1,1);
            actions.ResetPlayer(new Vector2(girl ? 2.6f : -2.6f,.04f));
            return actions;
        }

        static Sprite[] Frames(bool girl, int action)
        {
            string[] names = { "idle", "walk", "jump", "hurt" };
            string prefix=(girl?"girl_":"boy_")+names[action]+"_";
            return characterFrames.Where(frame=>frame.name.StartsWith(prefix)).OrderBy(frame=>frame.name).ToArray();
        }

        static void PrepareCharacterSprites()
        {
            const string path = "Assets/Art/Characters/two_players.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.isReadable=true;importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.filterMode=FilterMode.Point;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int[] rows = {0,164,321,477,634,788,948,1105,1254};
            int[] columns = {0,350,630,910,1254};
            string[] names = { "idle", "walk", "jump", "hurt" };
            var pixels = texture.GetPixels32();
            var rects=new List<SpriteRect>();
            for(int row=0;row<8;row++)
            for(int col=0;col<4;col++) {
                int minX=texture.width, minY=texture.height, maxX=0, maxY=0;
                for (int y=texture.height-rows[row+1];y<texture.height-rows[row];y++)
                for (int x=columns[col];x<columns[col+1];x++)
                    if (pixels[y*texture.width+x].a > 32) { minX=Math.Min(minX,x); minY=Math.Min(minY,y); maxX=Math.Max(maxX,x); maxY=Math.Max(maxY,y); }
                if (minX>maxX) throw new InvalidOperationException("Empty sprite cell.");
                string name=(row>=4?"girl_":"boy_")+names[row%4]+"_"+(col+1);
                rects.Add(new SpriteRect { name=name,spriteID=new GUID(Hash128.Compute(path+name).ToString()),
                    rect=new Rect(minX,minY,maxX-minX+1,maxY-minY+1),alignment=SpriteAlignment.BottomCenter,pivot=new Vector2(.5f,0) });
            }
            var factories=new SpriteDataProviderFactories();factories.Init();
            var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(rect=>new SpriteNameFileIdPair(rect.name,rect.spriteID)));
            provider.Apply();importer.SaveAndReimport();
            characterFrames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        }

        static Sprite CreateFist()
        {
            const string path="Assets/Art/UI/pixel_hand.png";
            if(!System.IO.File.Exists(path)) {
                var texture=new Texture2D(8,8,TextureFormat.RGBA32,false);
                var colors=new Color[64];
                for(int y=0;y<8;y++)for(int x=0;x<8;x++)
                    colors[y*8+x]=(x==0||x==7||y==0||y==7)?new Color(.24f,.16f,.1f,1):Color.white;
                colors[0]=colors[7]=colors[56]=colors[63]=Color.clear;
                texture.SetPixels(colors);texture.Apply();System.IO.File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.spritePixelsPerUnit=8;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
            scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Fonts/Tiny5.ttf");
            if(font==null)throw new InvalidOperationException("Tiny5 pixel font is missing.");
            Text Label(string name,string content,int fontSize,Vector2 position,Vector2 size) {
                var text=new GameObject(name,typeof(RectTransform)).AddComponent<Text>();text.transform.SetParent(canvas.transform,false);
                var rect=(RectTransform)text.transform;rect.anchoredPosition=position;rect.sizeDelta=size;
                text.font=font;text.fontSize=fontSize;text.alignment=TextAnchor.MiddleCenter;
                text.color=new Color(.25f,.14f,.07f);text.text=content;text.raycastTarget=false;
                text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;return text;
            }
            RectTransform Panel(string name,Vector2 position,Vector2 size) {
                var root=(RectTransform)new GameObject(name,typeof(RectTransform),typeof(Image)).transform;root.SetParent(canvas.transform,false);
                root.anchoredPosition=position;root.sizeDelta=size;root.GetComponent<Image>().color=new Color(.30f,.16f,.08f);root.GetComponent<Image>().raycastTarget=false;
                var trim=(RectTransform)new GameObject("Golden Timber Rim",typeof(RectTransform),typeof(Image)).transform;trim.SetParent(root,false);
                trim.anchorMin=Vector2.zero;trim.anchorMax=Vector2.one;trim.offsetMin=Vector2.one*4;trim.offsetMax=-Vector2.one*4;
                trim.GetComponent<Image>().color=new Color(.69f,.43f,.20f);trim.GetComponent<Image>().raycastTarget=false;
                var fill=(RectTransform)new GameObject("Parchment",typeof(RectTransform),typeof(Image)).transform;fill.SetParent(trim,false);
                fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=Vector2.one*4;fill.offsetMax=-Vector2.one*4;
                fill.GetComponent<Image>().color=new Color(1,.91f,.70f);fill.GetComponent<Image>().raycastTarget=false;
                return root;
            }
            Text PanelLabel(RectTransform panel,string name,string content,int fontSize,Vector2 position,Vector2 size) {
                var label=Label(name,content,fontSize,position,size);label.transform.SetParent(panel,false);return label;
            }
            CanvasGroup Popup(RectTransform panel) {
                var group=panel.gameObject.AddComponent<CanvasGroup>();group.alpha=0;group.blocksRaycasts=false;group.interactable=false;return group;
            }
            void SignButton(string name,string caption,Vector2 position,UnityEngine.Events.UnityAction action) {
                var panel=Panel(name,position,new Vector2(260,70));panel.GetComponent<Image>().raycastTarget=true;
                var button=panel.gameObject.AddComponent<Button>();button.targetGraphic=panel.GetComponent<Image>();
                var colors=button.colors;colors.highlightedColor=new Color(1,.9f,.72f);colors.pressedColor=new Color(.8f,.65f,.4f);button.colors=colors;
                UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick,action);
                PanelLabel(panel,name+" Label",caption,30,Vector2.zero,new Vector2(240,58));
            }
            Panel("Village Title Sign",new Vector2(0,460),new Vector2(620,72));
            Label("Title","BALANCE WITH ME",48,new Vector2(0,460),new Vector2(600,64));
            Text Scorecard(bool girl) {
                string player=girl?"Girl":"Boy";
                var panel=Panel(player+" Farm Scorecard",new Vector2(girl?650:-650,435),new Vector2(410,110));
                var badge=(RectTransform)new GameObject(player+" Icon Frame",typeof(RectTransform),typeof(Image)).transform;
                badge.SetParent(panel,false);badge.anchoredPosition=new Vector2(-151,0);badge.sizeDelta=new Vector2(88,90);
                badge.GetComponent<Image>().color=new Color(.69f,.43f,.20f);badge.GetComponent<Image>().raycastTarget=false;
                var icon=new GameObject(player+" Character Icon",typeof(RectTransform)).AddComponent<Image>();
                icon.transform.SetParent(badge,false);icon.rectTransform.sizeDelta=new Vector2(76,82);
                icon.sprite=girl?game.girl.idle[0]:game.boy.idle[0];icon.preserveAspect=true;icon.raycastTarget=false;
                return PanelLabel(panel,player+" Name Health and Score",girl?"FARMER GIRL\nHEALTH 3/3   WINS 0":"FARMER BOY\nHEALTH 3/3   WINS 0",27,new Vector2(46,0),new Vector2(292,96));
            }
            game.boyHud=Scorecard(false);
            game.girlHud=Scorecard(true);
            Panel("Round Clock",new Vector2(0,365),new Vector2(210,84));
            game.timer=Label("Timer","ROUND TIME\n00:00",30,new Vector2(0,365),new Vector2(190,78));
            var notice=Panel("Village Notice",new Vector2(0,260),new Vector2(760,54));
            game.statusPopup=Popup(notice);
            game.status=PanelLabel(notice,"Status","",26,Vector2.zero,new Vector2(730,44));
            game.health=Label("Health Data","",1,Vector2.zero,Vector2.one);game.health.gameObject.SetActive(false);
            game.boy.emoteLabel=Label("Boy Emote","",28,Vector2.zero,new Vector2(300,50));
            game.girl.emoteLabel=Label("Girl Emote","",28,Vector2.zero,new Vector2(300,50));
            foreach (var label in new[] { game.boy.emoteLabel, game.girl.emoteLabel }) {
                var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.18f,.09f,.03f,.9f);outline.effectDistance=new Vector2(1,-1);
            }
            var help=Panel("Farm Controls Popup",new Vector2(0,-300),new Vector2(1260,220));game.helpPopup=Popup(help);
            PanelLabel(help,"Boy Controls","BOY: A/D MOVE - W JUMP - F PUNCH - G PUSH - E EMOTE",25,new Vector2(0,75),new Vector2(1220,40));
            PanelLabel(help,"Girl Controls","GIRL: ARROWS MOVE - UP JUMP - K PUNCH - L PUSH - O EMOTE",25,new Vector2(0,30),new Vector2(1220,40));
            PanelLabel(help,"Gamepad Controls","PAD: STICK MOVE - SOUTH JUMP - WEST PUNCH - EAST PUSH - NORTH EMOTE",23,new Vector2(0,-15),new Vector2(1220,40));
            PanelLabel(help,"Shared Controls","M MUTE - N MUSIC - PAD: START RESTART / SELECT MUTE / RB MUSIC / LB HELP",23,new Vector2(0,-65),new Vector2(1220,40));
            SignButton("Help Farm Sign","HELP [H]",new Vector2(-780,-460),game.ShowHelp);
            SignButton("Restart Farm Sign","RESTART [R]",new Vector2(780,-460),game.Restart);
            var audio=Panel("Audio Notice",new Vector2(0,-465),new Vector2(520,48));game.audioPopup=Popup(audio);
            game.audioStatus=PanelLabel(audio,"Audio Status","",24,Vector2.zero,new Vector2(500,40));
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        }
    }
}
