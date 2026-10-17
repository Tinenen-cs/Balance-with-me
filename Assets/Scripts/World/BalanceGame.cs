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
        [Min(0)] public float countdownStepDuration = 1;
        public const float RoundDuration = 60;
        public float RemainingSeconds => Mathf.Max(0, RoundDuration - elapsed);
        public bool IsCountingDown { get; private set; }
        public int CountdownPhase { get; private set; }
        public CanvasGroup readyPopup, timeAlertPopup;
        public Text readyNumber, readyCaption, timeAlert, winnerHeading;
        public Button playAgainButton;
        float readyElapsed, timeAlertRemaining;
        public enum DefeatCause { None, Fall, Stamina, Timeout }
        public DefeatCause BoyDefeat { get; private set; }
        public DefeatCause GirlDefeat { get; private set; }
        public Image boyLifeBar, girlLifeBar;
        public Text boyLifeText, girlLifeText;
        public Image[] boyWinStars, girlWinStars;
        public Image boyPortrait, girlPortrait;
        public CanvasGroup roundClockPopup, winnerPopup;
        public Image winnerHead;
        public Image[] winnerBackdropImages;
        public Text winnerName, nextRoundText, replayHint;
        MatchResult announcedResult;
        float winnerPopupTime;
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
            CaptureSharedInput();
            TickPopup(helpPopup, ref helpRemaining);
            TickPopup(statusPopup, ref statusRemaining);
            TickPopup(audioPopup, ref audioRemaining);
            TickPopup(timeAlertPopup, ref timeAlertRemaining);
            if (IsCountingDown) AdvanceReadyCountdown(Time.unscaledDeltaTime);
            else if (Running)
            {
                bool boyLost = boy.Health <= 0 || boy.body.position.y < -3;
                bool girlLost = girl.Health <= 0 || girl.body.position.y < -3;
                if (boyLost || girlLost)
                    EndRound(boyLost, girlLost,
                        boyLost ? (boy.body.position.y < -3 ? DefeatCause.Fall : DefeatCause.Stamina) : DefeatCause.None,
                        girlLost ? (girl.body.position.y < -3 ? DefeatCause.Fall : DefeatCause.Stamina) : DefeatCause.None);
                else AdvanceRoundTime(Time.deltaTime);
            }
            else if (!MatchComplete)
            {
                nextRoundRemaining = Mathf.Max(0, nextRoundRemaining - Time.unscaledDeltaTime);
                ShowCountdown();
                if (nextRoundRemaining <= 0) Restart();
            }
            RefreshHud();
            UpdateWinnerAnnouncement(Time.unscaledDeltaTime);
        }
        public void PlayAgain() { if (MatchComplete) Restart(); }
        public void CaptureSharedInput()
        {
            if (MatchComplete && Keyboard.current != null &&
                (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)) PlayAgain();
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) Restart();
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame) ToggleAudio();
            if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame) NextMusic();
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame) ShowHelp();
            if (inputs != null)
            {
                HandlePad(inputs.BoyPad); HandlePad(inputs.GirlPad);
            }
        }
        public void AdvanceReadyCountdown(float dt)
        {
            if (!IsCountingDown) return;
            readyElapsed += Mathf.Max(0, dt);
            if (readyElapsed >= countdownStepDuration * 3) { StartPlaying(); return; }
            int phase = Mathf.Min(3, (int)(readyElapsed / countdownStepDuration) + 1);
            if (phase != CountdownPhase) SetReadyPhase(phase);
        }
        void SetReadyPhase(int phase)
        {
            CountdownPhase = phase;
            readyNumber.text = phase.ToString();
            readyCaption.text = phase == 1 ? "READY" : phase == 2 ? "SET" : "START!";
            sounds?.Play(phase == 3 ? "start" : "ready_tick");
        }
        void StartPlaying()
        {
            IsCountingDown = false;
            if (readyPopup != null) readyPopup.alpha = 0;
            board.simulated = boy.body.simulated = girl.body.simulated = true;
            Running = true;
            if (countdownStepDuration <= 0) sounds?.Play("start");
            status.text = "KEEP YOUR FOOTING - FACE YOUR RIVAL";
            statusRemaining = 2.5f;
        }
        public void AdvanceRoundTime(float dt)
        {
            if (!Running) return;
            float before = RemainingSeconds;
            elapsed = Mathf.Min(RoundDuration, elapsed + Mathf.Max(0, dt));
            float remaining = RemainingSeconds;
            if (remaining <= 0)
            {
                bool tie = boy.Health == girl.Health;
                EndRound(tie || boy.Health < girl.Health, tie || girl.Health < boy.Health,
                    tie || boy.Health < girl.Health ? DefeatCause.Timeout : DefeatCause.None,
                    tie || girl.Health < boy.Health ? DefeatCause.Timeout : DefeatCause.None);
                return;
            }
            int seconds = Mathf.CeilToInt(remaining);
            if (before > 30 && remaining <= 30) ShowTimeAlert(30, "time_warning");
            if (seconds <= 10 && seconds < Mathf.CeilToInt(before)) ShowTimeAlert(seconds, "time_tick");
        }
        void ShowTimeAlert(int seconds, string cue)
        {
            timeAlert.text = seconds == 1 ? "1 SECOND LEFT!" : $"{seconds} SECONDS LEFT!";
            timeAlertRemaining = seconds <= 10 ? 1.2f : 2.5f;
            timeAlertPopup.alpha = 1;
            sounds?.Play(cue);
        }
        void EndRound(bool boyLost, bool girlLost, DefeatCause boyCause, DefeatCause girlCause)
        {
            Running = false;
            BoyDefeat = boyCause; GirlDefeat = girlCause;
            CompletedRounds++;
            if (boyLost && !girlLost) GirlWins++;
            if (girlLost && !boyLost) BoyWins++;
            MatchComplete = CompletedRounds >= RoundsPerMatch;
            nextRoundRemaining = Mathf.Max(.1f, nextRoundDelay);
            Vector2 boyPosition = ResultPosition(boy, boyCause, false);
            Vector2 girlPosition = ResultPosition(girl, girlCause, true);
            FreezeRound();
            bool boyWinner = MatchComplete ? BoyWins > GirlWins : !boyLost && girlLost;
            bool girlWinner = MatchComplete ? GirlWins > BoyWins : !girlLost && boyLost;
            announcedResult = boyWinner ? MatchResult.Boy : girlWinner ? MatchResult.Girl : MatchResult.Draw;
            if (MatchComplete) Result = boyWinner ? MatchResult.Boy : girlWinner ? MatchResult.Girl : MatchResult.Draw;
            sounds?.PlayResult(!boyWinner && !girlWinner);
            boy.PresentResult(boyWinner, boyPosition);
            girl.PresentResult(girlWinner, girlPosition);
            winnerPopupTime = 0;
            timeAlertRemaining = 0;
            timeAlertPopup.alpha = 0;
            statusRemaining = 0;
            statusPopup.alpha = 0;
            roundResult = !boyWinner && !girlWinner ? "DRAW" : (boyWinner ? boyName : girlName) + " WINS";
            if (MatchComplete) roundResult = Result == MatchResult.Draw ? "MATCH DRAW" : roundResult + " THE MATCH!";
            ShowCountdown();
        }
        Vector2 ResultPosition(PlayerActions player, DefeatCause cause, bool isGirl)
        {
            if (cause == DefeatCause.Fall)
            {
                float side = Mathf.Abs(player.body.position.x) > .1f ? Mathf.Sign(player.body.position.x) : (isGirl ? 1 : -1);
                return new Vector2(side * Mathf.Clamp(Mathf.Abs(player.body.position.x), 6.4f, 8.2f), -3.2f);
            }
            float spacing = 3.5f;
            return new Vector2(isGirl ? spacing : -spacing, boardOrigin.y + .24f);
        }
        public void RefreshHud()
        {
            health.text = $"BOY  {boy.Health}/3     |     GIRL  {girl.Health}/3";
            if (boyHud != null) boyHud.text = boyName;
            if (girlHud != null) girlHud.text = girlName;
            UpdateStars(boyWinStars, BoyWins);
            UpdateStars(girlWinStars, GirlWins);
            if (boyLifeText != null) boyLifeText.text = $"LIFE {boy.HealthPercent:0}%";
            if (girlLifeText != null) girlLifeText.text = $"LIFE {girl.HealthPercent:0}%";
            if (boyPortrait != null) boyPortrait.sprite = boy.CurrentPortrait;
            if (girlPortrait != null) girlPortrait.sprite = girl.CurrentPortrait;
            UpdateLifeBar(boyLifeBar, boy.HealthPercent);
            UpdateLifeBar(girlLifeBar, girl.HealthPercent);
            if (timer != null)
            {
                int seconds = Mathf.CeilToInt(RemainingSeconds);
                timer.text = $"ROUND {RoundNumber} / {RoundsPerMatch}\n{seconds / 60:00}:{seconds % 60:00}";
                timer.color = RemainingSeconds <= 10 ? new Color(.65f,.08f,.04f) : RemainingSeconds <= 30 ? new Color(.57f,.26f,.02f) : new Color(.25f,.14f,.07f);
            }
            if (roundClockPopup != null) roundClockPopup.alpha = !Application.isPlaying || Running || IsCountingDown ? 1 : 0;

        }
        void UpdateWinnerAnnouncement(float dt)
        {
            if (winnerPopup == null) return;
            bool visible = !Running && !IsCountingDown && CompletedRounds > 0;
            winnerPopup.blocksRaycasts = winnerPopup.interactable = visible && MatchComplete;
            if (!visible) { winnerPopup.alpha = 0; return; }
            winnerPopupTime += dt;
            float progress = Mathf.Clamp01(winnerPopupTime / .3f);
            float eased = 1 - Mathf.Pow(1 - progress, 3);
            winnerPopup.alpha = eased;
            winnerPopup.transform.localScale = Vector3.one * Mathf.Lerp(.86f, 1, eased);
            ((RectTransform)winnerPopup.transform).anchoredPosition = MatchComplete ? Vector2.zero : new Vector2(0,200);
            foreach (var backdrop in winnerBackdropImages) backdrop.enabled = MatchComplete;
            foreach (var label in new[] { winnerHeading, winnerName, nextRoundText })
            {
                label.color = MatchComplete ? new Color(.25f,.14f,.07f) : new Color(1,.96f,.79f);
                foreach (var effect in label.GetComponents<Shadow>()) effect.enabled = !MatchComplete;
            }
            winnerHeading.text = announcedResult == MatchResult.Draw ? (MatchComplete ? "MATCH DRAW" : "ROUND DRAW") : MatchComplete ? "MATCH WINNER" : "ROUND WINNER";
            playAgainButton.gameObject.SetActive(MatchComplete);
            replayHint.gameObject.SetActive(MatchComplete);
            nextRoundText.gameObject.SetActive(!MatchComplete);
            nextRoundText.text = $"NEXT ROUND IN {Mathf.CeilToInt(nextRoundRemaining)}\nROUND {CompletedRounds + 1} OF {RoundsPerMatch}";
            winnerHead.gameObject.SetActive(announcedResult != MatchResult.Draw);
            winnerHead.sprite = announcedResult == MatchResult.Boy ? boy.portraitVictory : girl.portraitVictory;
            winnerName.text = announcedResult == MatchResult.Draw ? (MatchComplete ? "EQUAL ROUND WINS" : "EQUAL RESULT") : announcedResult == MatchResult.Boy ? boyName : girlName;
        }
        void ShowCountdown()
        {
            status.text = MatchComplete
                ? roundResult
                : $"{roundResult}\nNEXT ROUND IN {Mathf.CeilToInt(nextRoundRemaining)}";
        }
        static void UpdateStars(Image[] stars, int wins)
        {
            if (stars == null) return;
            for (int i = 0; i < stars.Length; i++)
                stars[i].color = i < wins ? Color.white : new Color(.36f,.29f,.19f,.30f);
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
            if (MatchComplete && pad.buttonSouth.wasPressedThisFrame) PlayAgain();
            else if (pad.startButton.wasPressedThisFrame) Restart();
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
            winnerPopupTime = 0;
            if (winnerPopup != null) { winnerPopup.alpha = 0; winnerPopup.blocksRaycasts = winnerPopup.interactable = false; }
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
            Running = false;
            BoyDefeat = GirlDefeat = DefeatCause.None;
            timeAlertRemaining = statusRemaining = 0;
            timeAlertPopup.alpha = 0;
            sounds?.CancelResult();
            readyElapsed = 0;
            IsCountingDown = countdownStepDuration > 0;
            readyPopup.alpha = IsCountingDown ? 1 : 0;
            if (IsCountingDown) { FreezeRound(); SetReadyPhase(1); }
            else StartPlaying();
            Physics2D.SyncTransforms();
            RefreshHud();
        }
    }
}
