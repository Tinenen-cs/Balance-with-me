using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BalanceWithMe
{
    public class BalanceGame : MonoBehaviour
    {
        public PlayerActions boy, girl;
        public Rigidbody2D board;
        public Text status, health;
        public Text boyHud, girlHud, timer;
        public string boyName = "FARMER BOY", girlName = "FARMER GIRL";
        public int BoyWins { get; private set; }
        public int GirlWins { get; private set; }
        public Text audioStatus;
        public LocalInputRouter inputs;
        public GameAudio sounds;
        public bool Running { get; private set; }
        float elapsed;
        void Start() => Restart();
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) Restart();
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame) sounds?.ToggleMute();
            if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame) sounds?.NextTrack();
            if (inputs != null)
            {
                HandlePad(inputs.BoyPad); HandlePad(inputs.GirlPad);
            }
            if (audioStatus != null && sounds != null) audioStatus.text = $"M: {(sounds.Muted ? "UNMUTE" : "MUTE")}  •  N: MUSIC {sounds.TrackIndex+1}";
            if (Running)
            {
                elapsed += Time.deltaTime;
                bool boyLost = boy.Health <= 0 || boy.body.position.y < -3;
                bool girlLost = girl.Health <= 0 || girl.body.position.y < -3;
                if (boyLost || girlLost)
                {
                    Running = false;
                    if (boyLost && !girlLost) GirlWins++;
                    if (girlLost && !boyLost) BoyWins++;
                    sounds?.Play(boyLost && girlLost ? "draw" : "win");
                    status.text = boyLost && girlLost ? "DRAW — R to restart" : (boyLost ? "GIRL WINS" : "BOY WINS") + " — R to restart";
                }
                else status.text = "KEEP YOUR FOOTING - FACE YOUR RIVAL";
            }
            health.text = $"BOY  {boy.Health}/3     |     GIRL  {girl.Health}/3";
            if (boyHud != null) boyHud.text = $"{boyName}\nHEALTH {boy.Health}/3   WINS {BoyWins}";
            if (girlHud != null) girlHud.text = $"{girlName}\nHEALTH {girl.Health}/3   WINS {GirlWins}";
            if (timer != null) timer.text = $"ROUND TIME\n{(int)elapsed/60:00}:{(int)elapsed%60:00}";
        }
        void HandlePad(Gamepad pad)
        {
            if (pad == null) return;
            if (pad.startButton.wasPressedThisFrame) Restart();
            if (pad.selectButton.wasPressedThisFrame) sounds?.ToggleMute();
            if (pad.rightShoulder.wasPressedThisFrame) sounds?.NextTrack();
        }
        void FixedUpdate()
        {
            // Gentle centering prevents the empty plank from remaining tilted forever.
            if (Running) board.AddTorque(-Mathf.DeltaAngle(0, board.rotation) * .12f);
        }
        public void Restart()
        {
            board.rotation = 0;
            board.angularVelocity = 0;
            boy.ResetPlayer(new Vector2(-2.6f, board.position.y + .24f));
            girl.ResetPlayer(new Vector2(2.6f, board.position.y + .24f));
            elapsed = 0;
            Running = true;
            sounds?.Play("start");
            Physics2D.SyncTransforms();
        }
    }
}
