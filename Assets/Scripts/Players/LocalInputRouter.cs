using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

namespace BalanceWithMe
{
    // Assign by connection, keeping the other player's pad stable on disconnect.
    public class LocalInputRouter : MonoBehaviour
    {
        public Gamepad BoyPad { get; private set; }
        public Gamepad GirlPad { get; private set; }
        public Joystick BoyJoystick { get; private set; }
        public Joystick GirlJoystick { get; private set; }
        readonly Dictionary<int, ButtonControl[]> joystickButtons = new Dictionary<int, ButtonControl[]>();
        void OnEnable() { InputSystem.onDeviceChange += DeviceChanged; Reconcile(); }
        void OnDisable() => InputSystem.onDeviceChange -= DeviceChanged;
        void DeviceChanged(InputDevice device, InputDeviceChange change)
        {
            if(change==InputDeviceChange.Removed)joystickButtons.Remove(device.deviceId);
            Reconcile();
        }
        public void Reconcile()
        {
            if (BoyPad != null && (!BoyPad.added || !BoyPad.enabled)) BoyPad = null;
            if (GirlPad != null && (!GirlPad.added || !GirlPad.enabled)) GirlPad = null;
            if (BoyJoystick != null && (!BoyJoystick.added || !BoyJoystick.enabled)) BoyJoystick = null;
            if (GirlJoystick != null && (!GirlJoystick.added || !GirlJoystick.enabled)) GirlJoystick = null;
            foreach (var pad in Gamepad.all)
            {
                if (!pad.enabled || pad == BoyPad || pad == GirlPad) continue;
                if (BoyPad == null && BoyJoystick == null) BoyPad = pad;
                else if (GirlPad == null && GirlJoystick == null) GirlPad = pad;
            }
            foreach(var stick in Joystick.all)
            {
                if(!stick.enabled || stick==BoyJoystick || stick==GirlJoystick)continue;
                if(BoyPad==null && BoyJoystick==null)BoyJoystick=stick;
                else if(GirlPad==null && GirlJoystick==null)GirlJoystick=stick;
            }
        }
        public Gamepad ForPlayer(bool girl) => girl ? GirlPad : BoyPad;
        public void Assign(bool girl, Gamepad pad)
        {
            if (girl) { if (BoyPad == pad) BoyPad = null; GirlPad = pad; GirlJoystick=null; }
            else { if (GirlPad == pad) GirlPad = null; BoyPad = pad; BoyJoystick=null; }
        }
        public void AssignJoystick(bool girl,Joystick stick)
        {
            if(girl) { if(BoyJoystick==stick)BoyJoystick=null;GirlJoystick=stick;GirlPad=null; }
            else { if(GirlJoystick==stick)GirlJoystick=null;BoyJoystick=stick;BoyPad=null; }
        }
        public Joystick ForJoystick(bool girl) => girl ? GirlJoystick : BoyJoystick;
        public ButtonControl[] ButtonsFor(Joystick stick)
        {
            if(!joystickButtons.TryGetValue(stick.deviceId,out var buttons))
            {
                var found=new List<ButtonControl>();
                if(stick.trigger!=null)found.Add(stick.trigger);
                foreach(var control in stick.allControls)
                    if(control is ButtonControl button && button.parent==stick && button!=stick.trigger && !button.synthetic)found.Add(button);
                buttons=new ButtonControl[5];
                for(int i=0;i<5;i++)
                {
                    string saved=PlayerPrefs.GetString("BalanceWithMe.Joystick."+stick.layout+"."+i,"");
                    buttons[i]=string.IsNullOrEmpty(saved)?(i<found.Count?found[i]:null):stick.TryGetChildControl<ButtonControl>(saved);
                }
                joystickButtons[stick.deviceId]=buttons;
            }
            return buttons;
        }
        public void BindJoystick(Joystick stick,int action,ButtonControl button)
        {
            ButtonsFor(stick)[action]=button;
            PlayerPrefs.SetString("BalanceWithMe.Joystick."+stick.layout+"."+action,button.path.Substring(stick.path.Length+1));
            PlayerPrefs.Save();
        }
    }
}
