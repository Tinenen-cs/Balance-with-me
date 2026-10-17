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
        public int CompletedRounds { get; private set; }
        public int RoundNumber { get; private set; } = 1;
        public bool MatchComplete { get; private set; }
        [Min(1)] public int winsToMatch = 3;
        [Min(.1f)] public float nextRoundDelay = 3.2f;
        public Image boyLifeBar, girlLifeBar;
        public float BoyWinRate => CompletedRounds == 0 ? 0 : 100f * BoyWins / CompletedRounds;
        public float GirlWinRate => CompletedRounds == 0 ? 0 : 100f * GirlWins / CompletedRounds;
        float nextRoundRemaining;
        string roundResult;
        Vector2 boardOrigin;
        void Awake() { boardOrigin = board.position; }
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
        void Start() { Restart(); }
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
                    CompletedRounds++;
                    if (boyLost && !girlLost) GirlWins++;
                    if (girlLost && !boyLost) BoyWins++;
                    MatchComplete = BoyWins >= Mathf.Max(1, winsToMatch) || GirlWins >= Mathf.Max(1, winsToMatch);
                    nextRoundRemaining = Mathf.Max(.1f, nextRoundDelay);
                    FreezeRound();
                    sounds?.PlayResult(boyLost && girlLost);
                    if (boyLost) boy.effects?.Play(ActionEffects.Kind.Fail);
                    if (girlLost) girl.effects?.Play(ActionEffects.Kind.Fail);
                    if (boyLost && !girlLost) girl.CelebrateVictory();
                    if (girlLost && !boyLost) boy.CelebrateVictory();
                    statusRemaining = float.PositiveInfinity;
                    roundResult = boyLost && girlLost ? "DRAW" : (boyLost ? girlName : boyName) + " WINS";
                    status.text = MatchComplete ? roundResult + " THE MATCH!  -  RESTART" : roundResult;
                }
            }
            else if (!MatchComplete)
            {
                nextRoundRemaining = Mathf.Max(0, nextRoundRemaining - Time.unscaledDeltaTime);
                status.text = $"{roundResult}  -  ROUND {RoundNumber + 1} IN {Mathf.CeilToInt(nextRoundRemaining)}";
                if (nextRoundRemaining <= 0) { RoundNumber++; Restart(); }
            }
            RefreshHud();
        }
        public void RefreshHud()
        {
            health.text = $"BOY  {boy.Health}/3     |     GIRL  {girl.Health}/3";
            if (boyHud != null) boyHud.text = $"{boyName}\nLIFE {boy.HealthPercent:0}%   WINS {BoyWins}/{winsToMatch}\nWIN RATE {BoyWinRate:0}%";
            if (girlHud != null) girlHud.text = $"{girlName}\nLIFE {girl.HealthPercent:0}%   WINS {GirlWins}/{winsToMatch}\nWIN RATE {GirlWinRate:0}%";
            UpdateLifeBar(boyLifeBar, boy.HealthPercent);
            UpdateLifeBar(girlLifeBar, girl.HealthPercent);
            if (timer != null) timer.text = $"ROUND {RoundNumber}\n{(int)elapsed/60:00}:{(int)elapsed%60:00}";
        }
        static void UpdateLifeBar(Image bar, float percent)
        {
            if (bar == null) return;
            bar.fillAmount = percent / 100f;
            bar.color = percent > 67 ? new Color(.29f,.52f,.22f) : percent > 34 ? new Color(.76f,.45f,.13f) : new Color(.7f,.22f,.15f);
        }
        void FreezeRound()
        {
            foreach (var player in new[] { boy, girl })
            {
                player.CancelAction();
                player.body.linearVelocity = Vector2.zero;
                player.body.angularVelocity = 0;
                player.body.simulated = false;
            }
            board.linearVelocity = Vector2.zero;
            board.angularVelocity = 0;
            board.simulated = false;
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
            if (MatchComplete)
            {
                BoyWins = GirlWins = CompletedRounds = 0;
                RoundNumber = 1;
                MatchComplete = false;
            }
            nextRoundRemaining = 0;
            board.simulated = boy.body.simulated = girl.body.simulated = true;
            board.position = boardOrigin;
            board.transform.position = boardOrigin;
            board.rotation = 0;
            board.transform.rotation = Quaternion.identity;
            board.linearVelocity = Vector2.zero;
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
            RefreshHud();
        }
    }
}
