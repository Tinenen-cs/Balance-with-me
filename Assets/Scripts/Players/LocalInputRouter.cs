using UnityEngine;
using UnityEngine.InputSystem;

namespace BalanceWithMe
{
    // Assign by connection, keeping the other player's pad stable on disconnect.
    public class LocalInputRouter : MonoBehaviour
    {
        public Gamepad BoyPad { get; private set; }
        public Gamepad GirlPad { get; private set; }
        void OnEnable() { InputSystem.onDeviceChange += DeviceChanged; Reconcile(); }
        void OnDisable() => InputSystem.onDeviceChange -= DeviceChanged;
        void DeviceChanged(InputDevice device, InputDeviceChange change) => Reconcile();
        public void Reconcile()
        {
            if (BoyPad != null && (!BoyPad.added || !BoyPad.enabled)) BoyPad = null;
            if (GirlPad != null && (!GirlPad.added || !GirlPad.enabled)) GirlPad = null;
            foreach (var pad in Gamepad.all)
            {
                if (!pad.enabled || pad == BoyPad || pad == GirlPad) continue;
                if (BoyPad == null) BoyPad = pad;
                else if (GirlPad == null) GirlPad = pad;
            }
        }
        public Gamepad ForPlayer(bool girl) => girl ? GirlPad : BoyPad;
        public void Assign(bool girl, Gamepad pad)
        {
            if (girl) { if (BoyPad == pad) BoyPad = null; GirlPad = pad; }
            else { if (GirlPad == pad) GirlPad = null; BoyPad = pad; }
        }
    }
}
