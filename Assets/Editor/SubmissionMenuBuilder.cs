using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
namespace BalanceWithMe.Editor
{
    public static class SubmissionMenuBuilder
    {
        public static void Add(BalanceGame game, Canvas canvas, Font titleFont)
        {
            var regular=ReadableFonts.Regular;var bold=ReadableFonts.Bold;
            var ink=new Color(.22f,.15f,.09f);var muted=new Color(.40f,.31f,.21f);
            var menu=game.gameObject.AddComponent<GameMenu>();menu.game=game;
            game.gameObject.AddComponent<DesktopDisplay>();game.gameObject.AddComponent<RuntimeDiagnostics>();
            RectTransform Box(string name,Transform parent,Vector2 position,Vector2 size,Color color,bool blocks=false)
            {
                var rect=(RectTransform)new GameObject(name,typeof(RectTransform),typeof(Image)).transform;
                rect.SetParent(parent,false);rect.anchoredPosition=position;rect.sizeDelta=size;
                rect.GetComponent<Image>().color=color;rect.GetComponent<Image>().raycastTarget=blocks;return rect;
            }
            Text Label(Transform parent,string name,string content,int size,Vector2 position,Vector2 dimensions,bool strong=false,TextAnchor alignment=TextAnchor.MiddleLeft)
            {
                var text=new GameObject(name,typeof(RectTransform)).AddComponent<Text>();text.transform.SetParent(parent,false);
                text.rectTransform.anchoredPosition=position;text.rectTransform.sizeDelta=dimensions;
                text.font=strong?bold:regular;text.fontStyle=strong?FontStyle.Bold:FontStyle.Normal;text.fontSize=size;text.text=content;text.color=ink;
                text.alignment=alignment;text.lineSpacing=1;text.raycastTarget=false;
                text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;
            }
            var shade=Box("Pause and Controls",canvas.transform,Vector2.zero,Vector2.zero,new Color(.08f,.12f,.12f,.84f),true);
            shade.anchorMin=Vector2.zero;shade.anchorMax=Vector2.one;shade.offsetMin=shade.offsetMax=Vector2.zero;
            menu.panel=shade.gameObject.AddComponent<CanvasGroup>();menu.panel.alpha=0;menu.panel.blocksRaycasts=menu.panel.interactable=false;
            var rim=Box("Controls Menu",shade,Vector2.zero,new Vector2(1480,940),new Color(.30f,.16f,.08f));
            menu.menuContent=rim.gameObject.AddComponent<CanvasGroup>();
            Box("Parchment",rim,Vector2.zero,new Vector2(1464,924),new Color(1,.91f,.70f));
            var title=Label(rim,"Menu Title","BALANCE WITH ME",52,new Vector2(0,402),new Vector2(1300,70),false,TextAnchor.MiddleCenter);title.font=titleFont;
            Label(rim,"Rules","Two players  /  Three rounds  /  60 seconds each",26,new Vector2(0,350),new Vector2(1300,40),false,TextAnchor.MiddleCenter).color=muted;
            var objective=Box("How to Play",rim,new Vector2(0,268),new Vector2(1360,128),new Color(1,.96f,.83f));
            Label(objective,"Objective Heading","How to win",24,new Vector2(0,38),new Vector2(1300,32),true);
            Label(objective,"Objective Line 1","Knock your rival off the plank or land three punches.",26,new Vector2(0,1),new Vector2(1300,38));
            Label(objective,"Objective Line 2","Higher life wins when time runs out. Move uphill to resist sliding.",26,new Vector2(0,-39),new Vector2(1300,38));
            var keyboard=Box("Keyboard Card",rim,new Vector2(-350,18),new Vector2(660,350),new Color(1,.95f,.81f));
            var gamepad=Box("Gamepad Card",rim,new Vector2(350,18),new Vector2(660,350),new Color(1,.95f,.81f));
            Label(keyboard,"Keyboard Heading","Keyboard",28,new Vector2(0,137),new Vector2(608,44),true);
            Label(keyboard,"Action Column","Action",21,new Vector2(-222,83),new Vector2(150,32),true).color=muted;
            Label(keyboard,"Boy Column","Farmer Boy",21,new Vector2(-35,83),new Vector2(160,32),true).color=muted;
            Label(keyboard,"Girl Column","Farmer Girl",21,new Vector2(178,83),new Vector2(200,32),true).color=muted;
            Box("Keyboard Divider",keyboard,new Vector2(0,62),new Vector2(608,2),new Color(.72f,.57f,.34f,.55f));
            string[] actions={"Move","Jump","Punch","Push","Emote"};
            string[] boyKeys={"A / D","W","F","G","E"};string[] girlKeys={"Left / Right","Up","K","L","O"};
            string[] padKeys={"Left stick / D-pad","A / Cross","X / Square","B / Circle","Y / Triangle"};
            for(int i=0;i<actions.Length;i++)
            {
                float y=33-i*42;
                Label(keyboard,"Keyboard Action "+i,actions[i],26,new Vector2(-222,y),new Vector2(150,38));
                Label(keyboard,"Boy Key "+i,boyKeys[i],26,new Vector2(-35,y),new Vector2(160,38),true);
                Label(keyboard,"Girl Key "+i,girlKeys[i],26,new Vector2(178,y),new Vector2(200,38),true);
                Label(gamepad,"Gamepad Action "+i,actions[i],26,new Vector2(-222,y),new Vector2(150,38));
                Label(gamepad,"Gamepad Key "+i,padKeys[i],26,new Vector2(75,y),new Vector2(390,38),true);
            }
            Label(gamepad,"Gamepad Heading","Controller",28,new Vector2(0,137),new Vector2(608,44),true);
            Label(gamepad,"Supported Controllers","Xbox, PS4 and standard USB gamepads",21,new Vector2(0,83),new Vector2(608,32)).color=muted;
            Box("Gamepad Divider",gamepad,new Vector2(0,62),new Vector2(608,2),new Color(.72f,.57f,.34f,.55f));
            var connection=Box("Connected Players",rim,new Vector2(0,-213),new Vector2(1360,96),new Color(.96f,.85f,.64f));
            menu.deviceStatus=Label(connection,"Assigned Controllers","",22,new Vector2(-280,0),new Vector2(740,78));
            menu.deviceStatus.lineSpacing=1.35f;
            Label(connection,"USB Setup Heading","USB joystick setup",20,new Vector2(420,27),new Vector2(440,26),true,TextAnchor.MiddleCenter);
            Button Button(string name,string caption,Transform parent,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,out Text label,bool primary=false,int fontSize=26)
            {
                var root=Box(name,parent,position,size,primary?new Color(.25f,.39f,.20f):new Color(.38f,.25f,.11f),true);
                var face=Box("Button Face",root,Vector2.zero,size-Vector2.one*6,primary?new Color(.29f,.44f,.23f):new Color(.98f,.85f,.60f));
                var button=root.gameObject.AddComponent<Button>();button.targetGraphic=face.GetComponent<Image>();
                var colors=button.colors;colors.highlightedColor=new Color(1,.86f,.53f);colors.selectedColor=colors.highlightedColor;
                colors.pressedColor=new Color(.78f,.65f,.39f);button.colors=colors;
                label=Label(root,"Caption",caption,fontSize,Vector2.zero,size-Vector2.one*16,true,TextAnchor.MiddleCenter);
                if(primary)label.color=new Color(1,.98f,.91f);
                UnityEventTools.AddPersistentListener(button.onClick,action);return button;
            }
            var close=Button("Close Menu","X",rim,new Vector2(674,402),new Vector2(60,60),menu.Resume,out _,false,28);
            close.targetGraphic.color=Color.white;
            var closeColors=close.colors;closeColors.normalColor=new Color(.98f,.85f,.60f);
            closeColors.highlightedColor=closeColors.selectedColor=new Color(1,.45f,.40f);
            closeColors.pressedColor=new Color(.90f,.30f,.25f);closeColors.fadeDuration=.12f;close.colors=closeColors;
            Button("Map Boy USB","Set up Boy USB",connection,new Vector2(300,-17),new Vector2(216,48),menu.MapBoyJoystick,out _,false,22);
            Button("Map Girl USB","Set up Girl USB",connection,new Vector2(538,-17),new Vector2(216,48),menu.MapGirlJoystick,out _,false,22);
            menu.resumeButton=Button("Play or Resume","Play / Resume",rim,new Vector2(-205,-315),new Vector2(450,72),menu.Resume,out _,true,28);
            Button("New Match","New match",rim,new Vector2(260,-315),new Vector2(330,72),menu.NewMatch,out _);
            Button("Sound","Sound: On",rim,new Vector2(-480,-400),new Vector2(285,56),menu.ToggleSound,out menu.soundLabel);
            Button("Display","Display: Fullscreen",rim,new Vector2(0,-400),new Vector2(470,56),menu.ToggleFullscreen,out menu.screenLabel);
            menu.quitButton=Button("Quit","Quit game",rim,new Vector2(480,-400),new Vector2(285,56),menu.Quit,out _);
            Label(rim,"Menu Shortcuts","Esc / R3: Menu    |    Enter / A / Cross: Select    |    F11: Display    |    R: Restart    |    M: Mute",20,new Vector2(0,-448),new Vector2(1360,30),false,TextAnchor.MiddleCenter).color=muted;
            // Modal sits above the menu and blocks its controls until Yes or No is chosen.
            var quitShade=Box("Quit Confirmation",shade,Vector2.zero,Vector2.zero,new Color(.08f,.05f,.03f,.70f),true);
            quitShade.anchorMin=Vector2.zero;quitShade.anchorMax=Vector2.one;quitShade.offsetMin=quitShade.offsetMax=Vector2.zero;
            menu.quitDialog=quitShade.gameObject.AddComponent<CanvasGroup>();
            menu.quitDialog.alpha=0;menu.quitDialog.interactable=menu.quitDialog.blocksRaycasts=false;
            var dialog=Box("Quit Dialog",quitShade,Vector2.zero,new Vector2(740,330),new Color(.30f,.16f,.08f));
            Box("Parchment",dialog,Vector2.zero,new Vector2(728,318),new Color(1,.91f,.70f));
            Label(dialog,"Quit Heading","Quit game?",34,new Vector2(0,111),new Vector2(660,48),true,TextAnchor.MiddleCenter);
            Label(dialog,"Quit Question","Do you really want to quit the game?",28,new Vector2(0,48),new Vector2(680,44),false,TextAnchor.MiddleCenter);
            Label(dialog,"Quit Hint","Choose No to return to the menu.",24,new Vector2(0,1),new Vector2(660,38),false,TextAnchor.MiddleCenter).color=muted;
            var yes=Button("Confirm Quit","Yes, quit",dialog,new Vector2(-160,-94),new Vector2(270,64),menu.ConfirmQuit,out var yesText);
            yesText.color=new Color(1,.98f,.91f);yes.targetGraphic.color=Color.white;
            var yesColors=yes.colors;yesColors.normalColor=new Color(.66f,.16f,.12f);
            yesColors.highlightedColor=yesColors.selectedColor=new Color(.76f,.19f,.14f);
            yesColors.pressedColor=new Color(.50f,.10f,.08f);yesColors.fadeDuration=.12f;yes.colors=yesColors;
            menu.cancelQuitButton=Button("Cancel Quit","No, stay",dialog,new Vector2(160,-94),new Vector2(270,64),menu.CancelQuit,out _,true);
            var help=GameObject.Find("Help Farm Sign").GetComponent<Button>();
            help.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(help.onClick,menu.Toggle);
            help.GetComponentInChildren<Text>().text="Menu [Esc]";
        }
    }
}
