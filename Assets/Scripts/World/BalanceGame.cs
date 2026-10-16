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
        public CanvasGroup helpPopup, statusPopup, audioPopup;
        public float PopupDuration = 5;
        [Min(0)] public float centeringTorque = .75f;
        [Min(0)] public float edgeResistance = 4;
        [Range(0, 15)] public float softLimitAngle = 11;
        float elapsed, helpRemaining, statusRemaining, audioRemaining;
        void TickPopup(CanvasGroup popup, ref float remaining)
        {
            remaining = Mathf.Max(0, remaining - Time.unscaledDeltaTime);
            if (popup != null) { popup.alpha = remaining > 0 ? 1 : 0; popup.blocksRaycasts = false; popup.interactable = false; }
        }
        public void ShowHelp() { helpRemaining = PopupDuration; }
        public void ToggleAudio() { sounds?.ToggleMute(); ShowAudioNotice(); }
        public void NextMusic() { sounds?.NextTrack(); ShowAudioNotice(); }
        void ShowAudioNotice()
        {
            if (audioStatus != null && sounds != null)
                audioStatus.text = $"MUSIC {sounds.TrackIndex + 1}  -  {(sounds.Muted ? "SOUND OFF" : "SOUND ON")}";
            audioRemaining = 2.5f;
        }
        void Start() { Restart(); ShowHelp(); }
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) Restart();
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame) ToggleAudio();
            if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame) NextMusic();
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame) ShowHelp();
            if (inputs != null)
            {
                HandlePad(inputs.BoyPad); HandlePad(inputs.GirlPad);
            }
            TickPopup(helpPopup, ref helpRemaining);
            TickPopup(statusPopup, ref statusRemaining);
            TickPopup(audioPopup, ref audioRemaining);
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
                    sounds?.PlayResult(boyLost && girlLost);
                    if (boyLost) boy.effects?.Play(ActionEffects.Kind.Fail);
                    if (girlLost) girl.effects?.Play(ActionEffects.Kind.Fail);
                    if (boyLost && !girlLost) girl.CelebrateVictory();
                    if (girlLost && !boyLost) boy.CelebrateVictory();
                    statusRemaining = float.PositiveInfinity;
                    status.text = boyLost && girlLost ? "DRAW — R to restart" : (boyLost ? "GIRL WINS" : "BOY WINS") + " — R to restart";
                }
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
            if (pad.selectButton.wasPressedThisFrame) ToggleAudio();
            if (pad.rightShoulder.wasPressedThisFrame) NextMusic();
            if (pad.leftShoulder.wasPressedThisFrame) ShowHelp();
        }
        void FixedUpdate()
        {
            if (!Running) return;
            float angle = Mathf.DeltaAngle(0, board.rotation);
            float edge = Mathf.Max(0, Mathf.Abs(angle) - softLimitAngle);
            // Progressive resistance slows the swing before the hinge's hard limits.
            board.AddTorque(-angle * centeringTorque - Mathf.Sign(angle) * edge * edge * edgeResistance);
        }
        public void Restart()
        {
            board.rotation = 0;
            board.angularVelocity = 0;
            boy.ResetPlayer(new Vector2(-2.6f, board.position.y + .24f));
            girl.ResetPlayer(new Vector2(2.6f, board.position.y + .24f));
            elapsed = 0;
            Running = true;
            sounds?.CancelResult();
            sounds?.Play("start");
            status.text = "KEEP YOUR FOOTING - FACE YOUR RIVAL";
            statusRemaining = 2.5f;
            Physics2D.SyncTransforms();
        }
    }
}
