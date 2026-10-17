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
        public const int RoundsPerMatch = 3;
        [Min(.1f)] public float nextRoundDelay = 3.2f;
        [Min(.1f)] public float matchResetDelay = 5;
        public Image boyLifeBar, girlLifeBar;
        public Text boyLifeText, girlLifeText;
        public Text boyWinsText, girlWinsText;
        public Image boyPortrait, girlPortrait;
        public enum MatchResult { None, Boy, Girl, Draw }
        public MatchResult Result { get; private set; }
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
                    MatchComplete = CompletedRounds >= RoundsPerMatch;
                    nextRoundRemaining = Mathf.Max(.1f, MatchComplete ? matchResetDelay : nextRoundDelay);
                    FreezeRound();
                    bool boyWinner = MatchComplete ? BoyWins > GirlWins : !boyLost && girlLost;
                    bool girlWinner = MatchComplete ? GirlWins > BoyWins : !girlLost && boyLost;
                    if (MatchComplete) Result = boyWinner ? MatchResult.Boy : girlWinner ? MatchResult.Girl : MatchResult.Draw;
                    sounds?.PlayResult(!boyWinner && !girlWinner);
                    boy.PresentResult(boyWinner, new Vector2(-2.6f, boardOrigin.y + .24f));
                    girl.PresentResult(girlWinner, new Vector2(2.6f, boardOrigin.y + .24f));
                    statusRemaining = float.PositiveInfinity;
                    roundResult = !boyWinner && !girlWinner ? "DRAW" : (boyWinner ? boyName : girlName) + " WINS";
                    if (MatchComplete) roundResult = Result == MatchResult.Draw ? "MATCH DRAW" : roundResult + " THE MATCH!";
                    ShowCountdown();
                }
            }
            else
            {
                nextRoundRemaining = Mathf.Max(0, nextRoundRemaining - Time.unscaledDeltaTime);
                ShowCountdown();
                if (nextRoundRemaining <= 0)
                {
                    Restart();
                }
            }
            RefreshHud();
        }
        public void RefreshHud()
        {
            health.text = $"BOY  {boy.Health}/3     |     GIRL  {girl.Health}/3";
            if (boyHud != null) boyHud.text = boyName;
            if (girlHud != null) girlHud.text = girlName;
            if (boyWinsText != null) boyWinsText.text = $"ROUND WINS {BoyWins}";
            if (girlWinsText != null) girlWinsText.text = $"ROUND WINS {GirlWins}";
            if (boyLifeText != null) boyLifeText.text = $"LIFE {boy.HealthPercent:0}%";
            if (girlLifeText != null) girlLifeText.text = $"LIFE {girl.HealthPercent:0}%";
            if (boyPortrait != null) boyPortrait.sprite = boy.CurrentPortrait;
            if (girlPortrait != null) girlPortrait.sprite = girl.CurrentPortrait;
            UpdateLifeBar(boyLifeBar, boy.HealthPercent);
            UpdateLifeBar(girlLifeBar, girl.HealthPercent);
            if (timer != null) timer.text = $"ROUND {RoundNumber} / {RoundsPerMatch}\n{(int)elapsed/60:00}:{(int)elapsed%60:00}";
        }
        void ShowCountdown()
        {
            status.text = MatchComplete
                ? $"{roundResult}\nNEW MATCH IN {Mathf.CeilToInt(nextRoundRemaining)}"
                : $"{roundResult}\nNEXT ROUND IN {Mathf.CeilToInt(nextRoundRemaining)}";
        }
        static void UpdateLifeBar(Image bar, float percent)
        {
            if (bar == null) return;
            bar.fillAmount = percent / 100f;
            bar.color = percent > 67 ? new Color(.20f,.40f,.16f) : percent > 34 ? new Color(.56f,.29f,.06f) : new Color(.58f,.12f,.09f);
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
            board.position = boardOrigin;
            board.transform.position = boardOrigin;
            board.rotation = 0;
            board.transform.rotation = Quaternion.identity;
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
                Result = MatchResult.None;
            }
            nextRoundRemaining = 0;
            RoundNumber = CompletedRounds + 1;
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
