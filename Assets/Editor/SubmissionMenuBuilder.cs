using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace BalanceWithMe.Editor
{
    public static class SubmissionMenuBuilder
    {
        public static void Add(BalanceGame game, Canvas canvas, Font font)
        {
            var menu = game.gameObject.AddComponent<GameMenu>(); menu.game = game;
            game.gameObject.AddComponent<DesktopDisplay>();
            game.gameObject.AddComponent<RuntimeDiagnostics>();
            RectTransform Box(string name, Transform parent, Vector2 position, Vector2 size, Color color)
            {
                var rect = (RectTransform)new GameObject(name,typeof(RectTransform),typeof(Image)).transform;
                rect.SetParent(parent,false); rect.anchoredPosition=position;rect.sizeDelta=size;
                rect.GetComponent<Image>().color=color;return rect;
            }
            Text Label(Transform parent, string name, string content, int size, Vector2 position, Vector2 dimensions)
            {
                var text=new GameObject(name,typeof(RectTransform)).AddComponent<Text>();text.transform.SetParent(parent,false);
                text.rectTransform.anchoredPosition=position;text.rectTransform.sizeDelta=dimensions;
                text.font=font;text.fontSize=size;text.text=content;text.color=new Color(.25f,.14f,.07f);
                text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
                text.horizontalOverflow=HorizontalWrapMode.Wrap;return text;
            }
            var shade=Box("Pause and Controls",canvas.transform,Vector2.zero,Vector2.zero,new Color(.08f,.12f,.12f,.84f));
            shade.anchorMin=Vector2.zero;shade.anchorMax=Vector2.one;shade.offsetMin=shade.offsetMax=Vector2.zero;
            menu.panel=shade.gameObject.AddComponent<CanvasGroup>();menu.panel.alpha=0;menu.panel.blocksRaycasts=menu.panel.interactable=false;
            var rim=Box("Controls Menu",shade,Vector2.zero,new Vector2(1400,900),new Color(.30f,.16f,.08f));
            var paper=Box("Parchment",rim,Vector2.zero,new Vector2(1384,884),new Color(1,.91f,.70f));paper.GetComponent<Image>().raycastTarget=false;
            Label(rim,"Menu Title","BALANCE WITH ME",54,new Vector2(0,375),new Vector2(1250,75));
            Label(rim,"Rules","TWO FARMERS. ONE PLANK. THREE ROUNDS.",30,new Vector2(0,310),new Vector2(1300,45));
            Label(rim,"Objectives","Push your rival off or land three punches. Higher life wins at 60 seconds.\nMove uphill to resist sliding. Each round win earns a gold star.",28,new Vector2(0,235),new Vector2(1300,100));
            Label(rim,"Keyboard Controls","SHARED KEYBOARD\n\nBOY: A / D MOVE   W JUMP\nF PUNCH   G PUSH   E EMOTE\n\nGIRL: ARROWS MOVE   UP JUMP\nK PUNCH   L PUSH   O EMOTE",29,new Vector2(-330,35),new Vector2(640,285));
            Label(rim,"Pad Controls","CONTROLLERS: XBOX / PS4 / GENERIC GAMEPAD\n\nLEFT STICK / D-PAD: MOVE\nA / CROSS: JUMP     X / SQUARE: PUNCH\nB / CIRCLE: PUSH     Y / TRIANGLE: EMOTE\nR3: MENU     START / OPTIONS: RESTART\nLB / L1: HELP   RB / R1: MUSIC   BACK / SHARE: MUTE",25,new Vector2(330,65),new Vector2(650,230));
            menu.deviceStatus=Label(rim,"Assigned Controllers","",26,new Vector2(0,-155),new Vector2(1300,55));
            Button Button(string name,string caption,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,out Text label)
            {
                var root=Box(name,rim,position,size,new Color(.40f,.24f,.10f));
                var inset=Box("Button Face",root,Vector2.zero,size-Vector2.one*8,new Color(.95f,.79f,.46f));inset.GetComponent<Image>().raycastTarget=false;
                var button=root.gameObject.AddComponent<Button>();button.targetGraphic=root.GetComponent<Image>();
                var colors=button.colors;colors.highlightedColor=new Color(1,.73f,.30f);colors.selectedColor=colors.highlightedColor;button.colors=colors;
                label=Label(root,"Caption",caption,29,Vector2.zero,size-Vector2.one*12);
                UnityEventTools.AddPersistentListener(button.onClick,action);return button;
            }
            Button("Map Boy USB","MAP BOY USB",new Vector2(160,-100),new Vector2(285,44),menu.MapBoyJoystick,out _);
            Button("Map Girl USB","MAP GIRL USB",new Vector2(490,-100),new Vector2(285,44),menu.MapGirlJoystick,out _);
            menu.resumeButton=Button("Play or Resume","PLAY / RESUME",new Vector2(-240,-220),new Vector2(420,70),menu.Resume,out _);
            Button("New Match","NEW MATCH",new Vector2(240,-220),new Vector2(420,70),menu.NewMatch,out _);
            Button("Sound","SOUND: ON",new Vector2(-440,-315),new Vector2(330,60),menu.ToggleSound,out menu.soundLabel);
            Button("Display","DISPLAY: FULLSCREEN",new Vector2(0,-315),new Vector2(440,60),menu.ToggleFullscreen,out menu.screenLabel);
            Button("Quit","QUIT GAME",new Vector2(440,-315),new Vector2(330,60),menu.Quit,out _);
            Label(rim,"Menu Shortcuts","ESC / R3 MENU   |   ENTER / A / CROSS SELECT   |   F11 DISPLAY   |   R RESTART   |   M MUTE\nFirst connected pad: Boy. Second: Girl. Keyboard and controller may be mixed.",24,new Vector2(0,-400),new Vector2(1320,75));
            var help=GameObject.Find("Help Farm Sign").GetComponent<Button>();
            help.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(help.onClick,menu.Toggle);
            help.GetComponentInChildren<Text>().text="MENU [ESC]";
        }
    }
}
