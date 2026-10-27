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
        static Sprite[] combatFrames;
        static Sprite[] portraitFrames, resultFrames;
        const string Generated = "Assets/Art/Characters/Generated";

        [MenuItem("Balance with me/Rebuild Playable Scene")]
        public static void Build()
        {
            System.IO.Directory.CreateDirectory(Generated);
            AssetDatabase.Refresh();
            PrepareCharacterSprites();
            PrepareCombatSprites();
            portraitFrames = PrepareExpressionSheet("Assets/Art/UI/character_portraits.png", 2, true);
            resultFrames = PrepareExpressionSheet("Assets/Art/Characters/result_players.png", 4, false);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) throw new InvalidOperationException("URP 2D sprite shader missing.");
            material = AssetDatabase.LoadAssetAtPath<Material>(Generated + "/PixelUnlit.mat");
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, Generated + "/PixelUnlit.mat"); }
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10);
            var cam = camera.GetComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(.4f,.7f,1);
            const string backgroundPath = "Assets/Art/Backgrounds/farm_village_animated_base.png";
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
            BuildClouds(cam);
            BuildVillageMotion(background);
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
            game.RefreshHud();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue = "Ground"; tags.ApplyModifiedProperties();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Main.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
            AssetDatabase.SaveAssets();
            ProjectStartup.ConfigureGameView();
            Debug.Log("Balance with me: playable push/punch scene generated.");
        }

        static void BuildVillageMotion(SpriteRenderer background)
        {
            const string rotorPath = "Assets/Art/Props/windmill_rotor.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(rotorPath);
            if (importer == null) throw new InvalidOperationException("Windmill rotor artwork is missing.");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            var rotorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(rotorPath);
            var bounds = background.bounds;
            Vector3 At(float x, float y) => new Vector3(bounds.min.x + bounds.size.x * x, bounds.max.y - bounds.size.y * y, 0);
            var rotor = Render("Turning Windmill Sails", rotorSprite, At(320f / 1672, 328f / 941), -8);
            rotor.transform.localScale = Vector3.one * (bounds.size.x * (120f / 1672) / rotorSprite.bounds.size.x);
            rotor.gameObject.AddComponent<WindmillRotor>();
            var smokeSprite = CreateSmokePuff();
            foreach (bool right in new[] { false, true })
            {
                var root = new GameObject(right ? "Right Chimney Smoke" : "Left Chimney Smoke");
                root.transform.position = At((right ? 1598f : 119f) / 1672, (right ? 326f : 320f) / 941);
                var smoke = root.AddComponent<ChimneySmoke>(); smoke.phaseOffset = right ? .37f : 0;
                smoke.puffs = new SpriteRenderer[6];
                for (int i = 0; i < smoke.puffs.Length; i++)
                {
                    var puff = Render("Rising Smoke Puff " + (i + 1), smokeSprite, Vector2.zero, -7);
                    puff.transform.SetParent(root.transform, false); smoke.puffs[i] = puff;
                }
                smoke.Advance(0);
            }
        }

        static void BuildClouds(Camera camera)
        {
            const string path = "Assets/Art/Backgrounds/moving_cloud.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("Moving cloud sprite is missing.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var root = new GameObject("Drifting Clouds").transform;
            Vector2[] positions = { new Vector2(-8,4.1f), new Vector2(7.4f,4.3f), new Vector2(-3.5f,3.5f), new Vector2(3.4f,3.6f), new Vector2(.8f,4.8f) };
            float[] widths = { 5.6f, 5.2f, 2.8f, 3.2f, 2.1f };
            for (int i = 0; i < positions.Length; i++)
            {
                var cloud = Render("Slow Cloud " + (i+1), sprite, positions[i], -9);
                cloud.transform.SetParent(root, true);
                cloud.transform.localScale = Vector3.one * (widths[i] / sprite.bounds.size.x);
                var drift = cloud.gameObject.AddComponent<CloudDrift>();
                drift.view = camera;
                drift.speed = .06f + i * .012f;
            }
        }

        static Sprite CreateSmokePuff()
        {
            const string path = "Assets/Art/Props/smoke_puff.png";
            if (!System.IO.File.Exists(path))
            {
                var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                var lobes = new[] { new Vector3(10, 13, 7), new Vector3(16, 21, 8), new Vector3(23, 14, 7), new Vector3(16, 11, 9) };
                for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    float coverage = 0;
                    foreach (var lobe in lobes) coverage = Mathf.Max(coverage, lobe.z - Vector2.Distance(new Vector2(x, y), new Vector2(lobe.x, lobe.y)));
                    texture.SetPixel(x, y, new Color(1, 1, 1, coverage > 1 ? 1 : coverage > 0 ? .55f : 0));
                }
                texture.Apply(); System.IO.File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
            audio.cues = new[] { "jump", "punch", "push", "hit", "shove", "start", "win", "draw", "emote_happy", "emote_sad", "step", "land", "fail", "ready_tick", "time_warning", "time_tick" }
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
            Sprite[] Combat(string action) => combatFrames.Where(frame => frame.name.StartsWith((girl ? "girl_" : "boy_") + action + "_")).OrderBy(frame => frame.name).ToArray();
            actions.idle = Combat("idle"); actions.punch = Combat("punch"); actions.push = Combat("push");
            string prefix = girl ? "girl_" : "boy_";
            actions.victory = resultFrames.Where(frame => frame.name.StartsWith(prefix + "victory_")).OrderBy(frame => frame.name).ToArray();
            actions.defeat = resultFrames.Where(frame => frame.name.StartsWith(prefix + "defeat_")).OrderBy(frame => frame.name).ToArray();
            Sprite Portrait(string expression) => portraitFrames.Single(frame => frame.name == prefix + expression);
            actions.portraitNormal = Portrait("normal"); actions.portraitHurt = Portrait("hurt");
            actions.portraitVictory = Portrait("victory"); actions.portraitDefeat = Portrait("defeat");
            actions.visual = Render("Character", actions.idle[0], Vector2.zero, 5); actions.visual.transform.SetParent(root.transform,false);
            actions.fist = Render("Punch Fist", fistSprite, Vector2.zero, 8); actions.fist.transform.SetParent(root.transform,false);
            actions.fist.transform.localScale = new Vector3(.28f/fistSprite.bounds.size.x,.28f/fistSprite.bounds.size.y,1); actions.fist.color = new Color(1,.65f,.2f);
            actions.palm = Render("Push Palms", fistSprite, Vector2.zero, 8); actions.palm.transform.SetParent(root.transform,false);
            actions.palm.transform.localScale = new Vector3(.16f/fistSprite.bounds.size.x,.4f/fistSprite.bounds.size.y,1); actions.palm.color = new Color(.4f,1,1);
            actions.effects = root.AddComponent<ActionEffects>();
            actions.effects.player = actions; actions.effects.pixelSprite = fistSprite; actions.effects.material = material;
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

        static void PrepareCombatSprites()
        {
            const string path = "Assets/Art/Characters/combat_players.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("The combat character sheet is missing.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var pixels = texture.GetPixels32();
            var occupiedX = new bool[texture.width];
            var occupiedY = new bool[texture.height];
            for (int y = 0; y < texture.height; y++)
            for (int x = 0; x < texture.width; x++)
                if (pixels[y * texture.width + x].a > 32) { occupiedX[x] = true; occupiedY[y] = true; }
            // Slice at the actual transparent gutters rather than assuming a perfectly uniform generated grid.
            List<Vector2Int> Bands(bool[] occupied)
            {
                var result = new List<Vector2Int>();
                int start = -1;
                for (int i = 0; i <= occupied.Length; i++)
                {
                    bool filled = i < occupied.Length && occupied[i];
                    if (filled && start < 0) start = i;
                    if (!filled && start >= 0) { result.Add(new Vector2Int(start, i)); start = -1; }
                }
                return result;
            }
            var columns = Bands(occupiedX);
            var rows = Bands(occupiedY); rows.Reverse();
            if (columns.Count != 4 || rows.Count != 6) throw new InvalidOperationException("Combat sheet must contain four columns and six separated rows.");
            importer.spritePixelsPerUnit = (rows[0].y - rows[0].x) / 1.45f;
            var rects = new List<SpriteRect>();
            string[] actions = { "idle", "punch", "push" };
            for (int row = 0; row < 6; row++)
            for (int col = 0; col < 4; col++)
            {
                int minX = texture.width, minY = texture.height, maxX = 0, maxY = 0;
                for (int y = rows[row].x; y < rows[row].y; y++)
                for (int x = columns[col].x; x < columns[col].y; x++)
                    if (pixels[y * texture.width + x].a > 32) { minX = Math.Min(minX, x); minY = Math.Min(minY, y); maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y); }
                int footMin = maxX, footMax = minX;
                for (int y = minY; y < minY + (maxY - minY + 1) / 10; y++)
                for (int x = minX; x <= maxX; x++)
                    if (pixels[y * texture.width + x].a > 32) { footMin = Math.Min(footMin, x); footMax = Math.Max(footMax, x); }
                string name = (row >= 3 ? "girl_" : "boy_") + actions[row % 3] + "_" + (col + 1);
                rects.Add(new SpriteRect { name = name, spriteID = new GUID(Hash128.Compute(path + name).ToString()),
                    rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1), alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(((footMin + footMax) * .5f - minX) / (maxX - minX + 1), 0) });
            }
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            combatFrames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        }

        static Sprite[] PrepareExpressionSheet(string path, int rowCount, bool portraits)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("Missing expression sheet: " + path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true; importer.alphaIsTransparency = true; importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096; importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false; importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var pixels = texture.GetPixels32();
            var occupiedX = new bool[texture.width]; var occupiedY = new bool[texture.height];
            for (int y = 0; y < texture.height; y++)
            for (int x = 0; x < texture.width; x++)
                if (pixels[y * texture.width + x].a > 32) { occupiedX[x] = true; occupiedY[y] = true; }
            List<Vector2Int> Bands(bool[] occupied)
            {
                var result = new List<Vector2Int>(); int start = -1;
                for (int i = 0; i <= occupied.Length; i++)
                {
                    bool filled = i < occupied.Length && occupied[i];
                    if (filled && start < 0) start = i;
                    if (!filled && start >= 0) { result.Add(new Vector2Int(start, i)); start = -1; }
                }
                return result;
            }
            var columns = Bands(occupiedX); var rows = Bands(occupiedY); rows.Reverse();
            if (columns.Count != 4 || rows.Count != rowCount) throw new InvalidOperationException("Expected separated 4 x " + rowCount + " expression sheet: " + path);
            importer.spritePixelsPerUnit = portraits ? 100 : (rows[0].y - rows[0].x) / 1.85f;
            var rects = new List<SpriteRect>();
            string[] expressions = { "normal", "hurt", "victory", "defeat" };
            for (int row = 0; row < rowCount; row++)
            for (int col = 0; col < 4; col++)
            {
                int minX = texture.width, minY = texture.height, maxX = 0, maxY = 0;
                for (int y = rows[row].x; y < rows[row].y; y++)
                for (int x = columns[col].x; x < columns[col].y; x++)
                    if (pixels[y * texture.width + x].a > 32) { minX = Math.Min(minX, x); minY = Math.Min(minY, y); maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y); }
                if (minX > maxX) throw new InvalidOperationException("Empty expression cell: " + path);
                float pivotX = .5f;
                if (!portraits)
                {
                    int footMin = maxX, footMax = minX;
                    for (int y = minY; y < minY + (maxY - minY + 1) / 10; y++)
                    for (int x = minX; x <= maxX; x++)
                        if (pixels[y * texture.width + x].a > 32) { footMin = Math.Min(footMin, x); footMax = Math.Max(footMax, x); }
                    pivotX = ((footMin + footMax) * .5f - minX) / (maxX - minX + 1);
                }
                string name = portraits ? (row == 0 ? "boy_" : "girl_") + expressions[col]
                    : (row < 2 ? "boy_" : "girl_") + (row % 2 == 0 ? "victory_" : "defeat_") + (col + 1);
                rects.Add(new SpriteRect { name = name, spriteID = new GUID(Hash128.Compute(path + name).ToString()),
                    rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1), alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(pivotX, portraits ? .5f : 0) });
            }
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
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
            const string lifeFillPath = "Assets/Art/UI/life_fill.png";
            if (!System.IO.File.Exists(lifeFillPath))
            {
                var pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                pixel.SetPixel(0, 0, Color.white); pixel.Apply();
                System.IO.File.WriteAllBytes(lifeFillPath, pixel.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(pixel);
            }
            AssetDatabase.ImportAsset(lifeFillPath, ImportAssetOptions.ForceSynchronousImport);
            var fillImporter = (TextureImporter)AssetImporter.GetAtPath(lifeFillPath);
            fillImporter.textureType = TextureImporterType.Sprite; fillImporter.spriteImportMode = SpriteImportMode.Single;
            fillImporter.filterMode = FilterMode.Point; fillImporter.mipmapEnabled = false;
            fillImporter.textureCompression = TextureImporterCompression.Uncompressed; fillImporter.SaveAndReimport();
            var lifeFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(lifeFillPath);
            const string starPath = "Assets/Art/UI/round_win_star.png";
            AssetDatabase.ImportAsset(starPath, ImportAssetOptions.ForceSynchronousImport);
            var starImporter = (TextureImporter)AssetImporter.GetAtPath(starPath);
            if (starImporter == null) throw new InvalidOperationException("Round win star sprite is missing.");
            starImporter.textureType=TextureImporterType.Sprite;starImporter.spriteImportMode=SpriteImportMode.Single;
            starImporter.filterMode=FilterMode.Point;starImporter.textureCompression=TextureImporterCompression.Uncompressed;
            starImporter.alphaIsTransparency=true;starImporter.mipmapEnabled=false;starImporter.SaveAndReimport();
            var starSprite=AssetDatabase.LoadAssetAtPath<Sprite>(starPath);
            var canvas=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<Canvas>().sortingOrder=100;
            var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var pixelFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Fonts/Tiny5.ttf");
            var font=ReadableFonts.Regular;
            if(pixelFont==null)throw new InvalidOperationException("Tiny5 pixel font is missing.");
            Text Label(string name,string content,int fontSize,Vector2 position,Vector2 size) {
                var text=new GameObject(name,typeof(RectTransform)).AddComponent<Text>();text.transform.SetParent(canvas.transform,false);
                var rect=(RectTransform)text.transform;rect.anchoredPosition=position;rect.sizeDelta=size;
                text.font=font;text.fontSize=fontSize;text.lineSpacing=1.25f;text.alignment=TextAnchor.MiddleCenter;
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
            Button SignButton(string name,string caption,Vector2 position,UnityEngine.Events.UnityAction action) {
                var panel=Panel(name,position,new Vector2(260,70));panel.GetComponent<Image>().raycastTarget=true;
                var button=panel.gameObject.AddComponent<Button>();
                // Tint the visible face, rather than only the thin outer border.
                var face=panel.Find("Golden Timber Rim/Parchment").GetComponent<Image>();
                face.color=Color.white;button.targetGraphic=face;
                var colors=button.colors;colors.normalColor=new Color(1,.91f,.70f);
                colors.highlightedColor=new Color(1,.78f,.37f);colors.selectedColor=colors.highlightedColor;
                colors.pressedColor=new Color(.84f,.60f,.28f);colors.fadeDuration=.12f;button.colors=colors;
                UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick,action);
                PanelLabel(panel,name+" Label",caption,30,Vector2.zero,new Vector2(240,58));
                return button;
            }
            Panel("Village Title Sign",new Vector2(0,460),new Vector2(620,72));
            Label("Title","BALANCE WITH ME",48,new Vector2(0,460),new Vector2(600,64)).font=pixelFont;
            Text Scorecard(bool girl) {
                string player=girl?"Girl":"Boy";
                var panel=Panel(player+" Farm Scorecard",new Vector2(girl?650:-650,425),new Vector2(440,166));
                var badge=(RectTransform)new GameObject(player+" Head Anchor",typeof(RectTransform)).transform;
                badge.SetParent(panel,false);badge.anchoredPosition=new Vector2(-155,0);badge.sizeDelta=new Vector2(108,132);
                var icon=new GameObject(player+" Character Icon",typeof(RectTransform)).AddComponent<Image>();
                icon.transform.SetParent(badge,false);icon.rectTransform.sizeDelta=new Vector2(100,116);
                icon.sprite=girl?game.girl.portraitNormal:game.boy.portraitNormal;icon.preserveAspect=true;icon.raycastTarget=false;
                if(girl)game.girlPortrait=icon;else game.boyPortrait=icon;
                var track = new GameObject(player+" Life Track", typeof(RectTransform)).AddComponent<Image>();
                track.transform.SetParent(panel,false);track.rectTransform.anchoredPosition=new Vector2(54,0);track.rectTransform.sizeDelta=new Vector2(284,42);
                track.color=new Color(.32f,.23f,.12f);track.raycastTarget=false;
                var bar = new GameObject(player+" Life Fill", typeof(RectTransform)).AddComponent<Image>();
                bar.transform.SetParent(track.transform,false);bar.rectTransform.anchorMin=Vector2.zero;bar.rectTransform.anchorMax=Vector2.one;
                bar.rectTransform.offsetMin=new Vector2(3,3);bar.rectTransform.offsetMax=new Vector2(-3,-3);
                bar.sprite=lifeFillSprite;bar.type=Image.Type.Filled;bar.fillMethod=Image.FillMethod.Horizontal;bar.fillAmount=1;bar.raycastTarget=false;
                if(girl)game.girlLifeBar=bar;else game.boyLifeBar=bar;
                var life = PanelLabel(track.rectTransform,player+" Life Percentage","LIFE 100%",30,Vector2.zero,new Vector2(278,38));
                life.color=new Color(1,.98f,.87f);
                var stroke=life.gameObject.AddComponent<Outline>();stroke.effectColor=new Color(.12f,.08f,.04f);stroke.effectDistance=new Vector2(1,-1);
                var stars=new Image[BalanceGame.RoundsPerMatch];
                for(int i=0;i<stars.Length;i++)
                {
                    var star=new GameObject(player+" Round Star "+(i+1),typeof(RectTransform)).AddComponent<Image>();
                    star.transform.SetParent(panel,false);star.rectTransform.anchoredPosition=new Vector2(54+(i-1)*56,-49);
                    star.rectTransform.sizeDelta=new Vector2(44,44);star.sprite=starSprite;star.preserveAspect=true;star.raycastTarget=false;
                    stars[i]=star;
                }
                if(girl){game.girlLifeText=life;game.girlWinStars=stars;}else{game.boyLifeText=life;game.boyWinStars=stars;}
                var name=PanelLabel(panel,player+" Name","",36,new Vector2(54,47),new Vector2(284,44));
                name.font=pixelFont;name.lineSpacing=1;name.verticalOverflow=VerticalWrapMode.Truncate;
                return name;
            }
            game.boyHud=Scorecard(false);
            game.girlHud=Scorecard(true);
            var clock=Panel("Round Clock",new Vector2(0,365),new Vector2(250,96));
            game.roundClockPopup=clock.gameObject.AddComponent<CanvasGroup>();game.roundClockPopup.blocksRaycasts=false;
            game.timer=PanelLabel(clock,"Timer","ROUND 1 / 3\n00:25",26,Vector2.zero,new Vector2(222,76));
            game.timer.lineSpacing=1.1f;game.timer.verticalOverflow=VerticalWrapMode.Truncate;
            game.timer.resizeTextForBestFit=true;game.timer.resizeTextMinSize=22;game.timer.resizeTextMaxSize=26;
            var notice=Panel("Village Notice",new Vector2(0,245),new Vector2(790,76));
            game.statusPopup=Popup(notice);
            game.status=PanelLabel(notice,"Status","",28,Vector2.zero,new Vector2(754,68));
            game.health=Label("Health Data","",1,Vector2.zero,Vector2.one);game.health.gameObject.SetActive(false);
            game.boy.emoteLabel=Label("Boy Emote","",28,Vector2.zero,new Vector2(300,50));
            game.girl.emoteLabel=Label("Girl Emote","",28,Vector2.zero,new Vector2(300,50));
            foreach (var label in new[] { game.boy.emoteLabel, game.girl.emoteLabel }) {
                var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=new Color(1,.94f,.77f,.95f);outline.effectDistance=new Vector2(1,-1);
            }
            var help=Panel("Farm Controls Popup",new Vector2(0,-300),new Vector2(1260,220));game.helpPopup=Popup(help);
            PanelLabel(help,"Boy Controls","Boy: A/D move   W jump   F punch   G push   E emote",25,new Vector2(0,75),new Vector2(1220,40));
            PanelLabel(help,"Girl Controls","Girl: Left/Right move   Up jump   K punch   L push   O emote",25,new Vector2(0,30),new Vector2(1220,40));
            PanelLabel(help,"Gamepad Controls","PS4: Stick/D-pad move   Cross jump   Square punch   Circle push   Triangle emote",23,new Vector2(0,-15),new Vector2(1220,40));
            PanelLabel(help,"Shared Controls","M: Mute   N: Music   PS4: Options restart   Share mute   R1 music   L1 help",23,new Vector2(0,-65),new Vector2(1220,40));
            SignButton("Help Farm Sign","Help [H]",new Vector2(-780,-460),game.ShowHelp);
            SignButton("Restart Farm Sign","Restart [R]",new Vector2(780,-460),game.Restart);
            var audio=Panel("Audio Notice",new Vector2(0,-465),new Vector2(520,48));game.audioPopup=Popup(audio);
            game.audioStatus=PanelLabel(audio,"Audio Status","",24,Vector2.zero,new Vector2(500,40));
            var announcement=Panel("Match Winner Announcement",Vector2.zero,new Vector2(500,340));
            game.winnerBackdropImages=announcement.GetComponentsInChildren<Image>(true);
            game.winnerPopup=Popup(announcement);
            game.winnerHeading=PanelLabel(announcement,"Winner Heading","MATCH WINNER",28,new Vector2(0,135),new Vector2(450,34));
            game.winnerHead=new GameObject("Winning Character Head",typeof(RectTransform)).AddComponent<Image>();
            game.winnerHead.transform.SetParent(announcement,false);game.winnerHead.rectTransform.anchoredPosition=new Vector2(0,55);
            game.winnerHead.rectTransform.sizeDelta=new Vector2(118,104);game.winnerHead.preserveAspect=true;game.winnerHead.raycastTarget=false;
            game.winnerHead.sprite=game.boy.portraitVictory;
            var headOutline=game.winnerHead.gameObject.AddComponent<Outline>();headOutline.effectColor=new Color(1,.96f,.79f,1);
            headOutline.effectDistance=new Vector2(2,-2);headOutline.useGraphicAlpha=false;
            var headShadow=game.winnerHead.gameObject.AddComponent<Shadow>();headShadow.effectColor=new Color(.18f,.08f,.03f,1);headShadow.effectDistance=new Vector2(1,-2);
            game.winnerName=PanelLabel(announcement,"Winning Player Name","",38,new Vector2(0,-25),new Vector2(454,46));
            game.winnerName.resizeTextForBestFit=true;game.winnerName.resizeTextMinSize=24;game.winnerName.resizeTextMaxSize=38;
            game.playAgainButton=SignButton("Play Again", "Play again", Vector2.zero, game.PlayAgain);
            var replay=(RectTransform)game.playAgainButton.transform;replay.SetParent(announcement,false);
            replay.anchoredPosition=new Vector2(0,-85);replay.sizeDelta=new Vector2(260,52);
            game.nextRoundText=PanelLabel(announcement,"Next Round Indicator","",28,new Vector2(0,-100),new Vector2(450,74));
            game.replayHint=PanelLabel(announcement,"Replay Shortcuts","Enter / PS4 Cross or Options",23,new Vector2(0,-137),new Vector2(470,32));
            var ready=(RectTransform)new GameObject("Round Start Countdown",typeof(RectTransform)).transform;
            ready.SetParent(canvas.transform,false);ready.anchoredPosition=new Vector2(0,150);ready.sizeDelta=new Vector2(420,260);
            game.readyPopup=Popup(ready);
            game.readyNumber=PanelLabel(ready,"Countdown Number","1",112,new Vector2(0,40),new Vector2(360,130));
            game.readyCaption=PanelLabel(ready,"Countdown Caption","READY",42,new Vector2(0,-65),new Vector2(360,60));
            game.readyNumber.font=game.readyCaption.font=pixelFont;
            game.winnerHeading.font=pixelFont;
            game.winnerName.font=pixelFont;game.winnerName.fontStyle=FontStyle.Normal;
            game.winnerName.fontSize=38;game.winnerName.resizeTextMaxSize=38;game.winnerName.lineSpacing=1;
            game.nextRoundText.fontSize=24;game.nextRoundText.lineSpacing=1.25f;
            foreach (var text in new[] { game.readyNumber, game.readyCaption, game.winnerHeading, game.winnerName, game.nextRoundText })
            {
                text.color=new Color(1,.96f,.79f);
                var outline=text.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.18f,.08f,.03f,1);
                outline.effectDistance=text == game.readyNumber || text == game.readyCaption ? new Vector2(4,-4) : new Vector2(2,-2);outline.useGraphicAlpha=false;
                var shadow=text.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(.10f,.04f,.02f,.85f);
                shadow.effectDistance=text == game.readyNumber || text == game.readyCaption ? new Vector2(2,-6) : new Vector2(0,-2);
            }
            var warning=Panel("Time Warning",new Vector2(0,280),new Vector2(450,58));game.timeAlertPopup=Popup(warning);
            game.timeAlert=PanelLabel(warning,"Time Warning Text","",30,Vector2.zero,new Vector2(426,50));
            game.timeAlert.color=new Color(.65f,.08f,.04f);
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            SubmissionMenuBuilder.Add(game,canvas.GetComponent<Canvas>(),pixelFont);
        }
    }
}
