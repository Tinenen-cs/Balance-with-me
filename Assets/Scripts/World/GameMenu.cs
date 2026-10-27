using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace BalanceWithMe
{
    public class GameMenu : MonoBehaviour
    {
        public BalanceGame game;
        public CanvasGroup panel, menuContent, quitDialog;
        public Button cancelQuitButton, quitButton;
        public bool IsConfirmingQuit { get; private set; }
        bool quitConfirmed;
        public Button resumeButton;
        public Text deviceStatus, soundLabel, screenLabel;
        public bool IsOpen { get; private set; }
        float nextDeviceRefresh;
        Joystick mappingStick; int mappingAction; bool waitForRelease;
        static readonly string[] actionNames={"Jump","Punch","Push","Emote","Menu"};

        void OnEnable() { Application.wantsToQuit+=HandleQuitRequest; }
        void OnDisable() { Application.wantsToQuit-=HandleQuitRequest; }
        bool HandleQuitRequest()
        {
            if(quitConfirmed)return true;
            Quit();return false;
        }
        void Start()
        {
            if (!Application.isEditor) SetOpen(true);
            else SetOpen(false);
        }
        void Update()
        {
            // Face buttons control the farmers during play, not a previously clicked HUD button.
            if(EventSystem.current!=null)
            {
                EventSystem.current.sendNavigationEvents=IsOpen;
                if(!IsOpen && EventSystem.current.currentSelectedGameObject!=null)EventSystem.current.SetSelectedGameObject(null);
            }
            bool toggle = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            var boy=game.inputs.BoyPad;var girl=game.inputs.GirlPad;
            toggle |= boy != null && boy.rightStickButton.wasPressedThisFrame;
            toggle |= girl != null && girl.rightStickButton.wasPressedThisFrame;
            foreach(var stick in Joystick.all)
                if(stick==game.inputs.BoyJoystick || stick==game.inputs.GirlJoystick)
                {
                    var menuButton=game.inputs.ButtonsFor(stick)[4];
                    toggle |= mappingStick==null && menuButton!=null && menuButton.wasPressedThisFrame;
                }
            if(toggle && mappingStick!=null) { mappingStick=null; RefreshLabels(); }
            else if (toggle) Toggle();
            if(mappingStick!=null)
            {
                bool any=false;
                foreach(var control in mappingStick.allControls)
                    if(control is ButtonControl button && button.parent==mappingStick && !button.synthetic)
                    {
                        any |= button.isPressed;
                        if(!waitForRelease && button.wasPressedThisFrame)
                        {
                            game.inputs.BindJoystick(mappingStick,mappingAction,button);mappingAction++;waitForRelease=true;
                            if(mappingAction==5) { mappingStick=null;RefreshLabels();break; }
                            ShowMappingPrompt();break;
                        }
                    }
                if(!any)waitForRelease=false;
            }
            if (Keyboard.current != null && Keyboard.current.f11Key.wasPressedThisFrame) ToggleFullscreen();
            if (IsOpen && Time.unscaledTime >= nextDeviceRefresh)
            {
                RefreshLabels(); nextDeviceRefresh = Time.unscaledTime + .5f;
            }
        }
        void OnApplicationFocus(bool focused)
        {
            if (!focused && !Application.isEditor && game != null && !IsOpen) SetOpen(true);
        }
        public void MapBoyJoystick() => BeginMapping(false);
        public void MapGirlJoystick() => BeginMapping(true);
        void BeginMapping(bool girl)
        {
            mappingStick=game.inputs.ForJoystick(girl);mappingAction=0;waitForRelease=true;
            if(mappingStick==null) { deviceStatus.text="No USB joystick found for this player.\nGamepads use the controls above.";nextDeviceRefresh=Time.unscaledTime+3;return; }
            EventSystem.current.SetSelectedGameObject(null);ShowMappingPrompt();
        }
        void ShowMappingPrompt() { deviceStatus.text=$"USB setup ({mappingAction+1}/5): press a button for {actionNames[mappingAction]}.\nPress Esc to cancel."; }
        public void Toggle() { if(IsConfirmingQuit)CancelQuit();else SetOpen(!IsOpen); }
        public void Resume() => SetOpen(false);
        public void NewMatch() { game.NewMatch(); SetOpen(false); }
        public void ToggleSound() { game.ToggleAudio(); RefreshLabels(); }
        public void ToggleFullscreen()
        {
            Screen.fullScreenMode = Screen.fullScreen ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
            RefreshLabels();
        }
        public void Quit()
        {
            if(IsConfirmingQuit)return;
            if(!IsOpen)SetOpen(true);
            mappingStick=null;IsConfirmingQuit=true;
            quitDialog.alpha=1;quitDialog.blocksRaycasts=quitDialog.interactable=true;
            menuContent.interactable=menuContent.blocksRaycasts=false;
            cancelQuitButton.Select();
        }
        public void CancelQuit()
        {
            IsConfirmingQuit=false;quitDialog.alpha=0;
            quitDialog.interactable=quitDialog.blocksRaycasts=false;
            menuContent.interactable=menuContent.blocksRaycasts=true;
            if(IsOpen)quitButton.Select();
        }
        public void ConfirmQuit()
        {
            if(!IsConfirmingQuit)return;
            quitConfirmed=true;Application.Quit();
        }
        public void SetOpen(bool open)
        {
            // Esc/menu-close while confirming cancels the dialog and keeps the paused menu.
            if(IsConfirmingQuit) { CancelQuit();if(!open)return; }
            IsOpen = open; game.MenuPaused = open;
            if(!open)mappingStick=null;
            panel.alpha = open ? 1 : 0; panel.interactable = panel.blocksRaycasts = open;
            Time.timeScale = open ? 0 : 1;
            if (open)
            {
                game.boy.CancelAction(); game.girl.CancelAction();
                RefreshLabels(); resumeButton.Select();
            }
            else if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
        void RefreshLabels()
        {
            if(mappingStick!=null) { ShowMappingPrompt();return; }
            string boy = game.inputs.BoyPad != null ? game.inputs.BoyPad.displayName : game.inputs.BoyJoystick!=null?game.inputs.BoyJoystick.displayName:"Keyboard";
            string girl = game.inputs.GirlPad != null ? game.inputs.GirlPad.displayName : game.inputs.GirlJoystick!=null?game.inputs.GirlJoystick.displayName:"Keyboard";
            deviceStatus.text = "Boy: " + boy + "\nGirl: " + girl;
            soundLabel.text = game.sounds.Muted ? "Sound: Off" : "Sound: On";
            screenLabel.text = Screen.fullScreen ? "Display: Fullscreen" : "Display: Windowed";
        }
        void OnDestroy() { Time.timeScale = 1; }
    }
}
