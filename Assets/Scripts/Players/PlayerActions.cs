using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BalanceWithMe
{
    [RequireComponent(typeof(CharacterController2DP2), typeof(Rigidbody2D))]
    public class PlayerActions : MonoBehaviour
    {
        public enum ActionType { None, Punch, Push }
        public bool isGirl;
        public CharacterController2DP2 controller;
        public Rigidbody2D body;
        public SpriteRenderer visual, fist, palm;
        public Sprite[] idle, walk, jump, hurt;
        public Sprite[] punch, push;
        public Sprite[] victory, defeat;
        public Sprite portraitNormal, portraitHurt, portraitVictory, portraitDefeat;
        public PlayerActions opponent;
        public BalanceGame game;
        public ActionEffects effects;
        public Text emoteLabel;
        public int Health { get; private set; } = 3;
        public float HealthPercent => Health * (100f / 3);
        public float StunRemaining { get; private set; }
        public ActionType CurrentAction { get; private set; }
        public ActionType LastReceivedAction { get; private set; }
        public float cooldown;
        float actionTime, move, flash, animationTime;
        float visualLean, leanVelocity, visualBob, bobVelocity;
        bool jumpQueued, punchConnected;
        float emoteTime, reactionCooldown, stepRemaining, airborneTime;
        bool sadEmote, victoryEmote;
        public bool IsCelebrating => victoryEmote && emoteTime > 0;
        public bool IsDefeated { get; private set; }
        public bool HasWonRound { get; private set; }
        public Sprite CurrentPortrait => IsDefeated ? portraitDefeat : HasWonRound ? portraitVictory : StunRemaining > 0 || flash > 0 ? portraitHurt : portraitNormal;
        string emoteText;
        public float Direction => controller.m_FacingRight ? 1 : -1;

        void Update()
        {
            float dt = Time.deltaTime;
            cooldown = Mathf.Max(0, cooldown - dt);
            StunRemaining = Mathf.Max(0, StunRemaining - dt);
            flash = Mathf.Max(0, flash - dt);
            emoteTime = Mathf.Max(0, emoteTime - dt);
            reactionCooldown = Mathf.Max(0, reactionCooldown - dt);
            CaptureKeyboardInput();
            Animate(dt);
        }

        public void CaptureKeyboardInput()
        {
            var keys = Keyboard.current;
            move = 0;
            if (game == null || !game.Running || StunRemaining > 0) return;
            if (keys != null)
            {
                move = isGirl
                    ? (keys.rightArrowKey.isPressed ? 1 : 0) - (keys.leftArrowKey.isPressed ? 1 : 0)
                    : (keys.dKey.isPressed ? 1 : 0) - (keys.aKey.isPressed ? 1 : 0);
                jumpQueued |= isGirl ? keys.upArrowKey.wasPressedThisFrame : keys.wKey.wasPressedThisFrame;
                if (isGirl ? keys.kKey.wasPressedThisFrame : keys.fKey.wasPressedThisFrame) TryAction(ActionType.Punch);
                if (isGirl ? keys.lKey.wasPressedThisFrame : keys.gKey.wasPressedThisFrame) TryAction(ActionType.Push);
                if (isGirl ? keys.oKey.wasPressedThisFrame : keys.eKey.wasPressedThisFrame) Emote();
            }
            var pad = game.inputs == null ? null : game.inputs.ForPlayer(isGirl);
            if (pad == null) return;
            float stick = pad.leftStick.x.ReadValue();
            float padMove = Mathf.Abs(stick) > .2f ? stick : pad.dpad.x.ReadValue();
            if (Mathf.Abs(move) < .01f) move = padMove;
            jumpQueued |= pad.buttonSouth.wasPressedThisFrame;
            if (pad.buttonWest.wasPressedThisFrame) TryAction(ActionType.Punch);
            if (pad.buttonEast.wasPressedThisFrame) TryAction(ActionType.Push);
            if (pad.buttonNorth.wasPressedThisFrame) Emote();
        }

        void FixedUpdate()
        {
            if (game == null || !game.Running) { jumpQueued = false; return; }
            float dt = Time.fixedDeltaTime;
            stepRemaining = Mathf.Max(0, stepRemaining - dt);
            if (!controller.IsGrounded) airborneTime += dt;
            else
            {
                if (airborneTime > .12f) { game.sounds?.Play("land"); effects?.Play(ActionEffects.Kind.Land); }
                airborneTime = 0;
                if (StunRemaining <= 0 && Mathf.Abs(move) > .1f && Mathf.Abs(body.linearVelocity.x) > .25f && stepRemaining <= 0)
                {
                    game.sounds?.Play("step");
                    effects?.Play(ActionEffects.Kind.Step);
                    stepRemaining = .32f;
                }
            }
            if (StunRemaining <= 0)
            {
                bool jumped = jumpQueued && controller.IsGrounded;
                controller.Move(move * .38f * (CurrentAction == ActionType.None ? 1 : .35f), false, jumpQueued);
                if (jumped) { game.sounds?.Play("jump"); effects?.Play(ActionEffects.Kind.Jump); }
            }
            jumpQueued = false;
            AdvanceAction(Time.fixedDeltaTime);
        }

        public bool TryAction(ActionType action)
        {
            if (action == ActionType.None || game == null || !game.Running || Health <= 0 || StunRemaining > 0 || cooldown > 0) return false;
            CurrentAction = action;
            actionTime = 0;
            punchConnected = false;
            cooldown = action == ActionType.Punch ? .65f : .9f;
            game.sounds?.Play(action == ActionType.Punch ? "punch" : "push");
            effects?.Play(action == ActionType.Punch ? ActionEffects.Kind.Punch : ActionEffects.Kind.Push);
            return true;
        }

        public void CancelAction()
        {
            CurrentAction = ActionType.None;
            move = 0;
            jumpQueued = false;
        }

        public void Emote()
        {
            if (game == null || !game.Running || emoteTime > 0 || StunRemaining > 0) return;
            sadEmote = !sadEmote;
            emoteText = sadEmote ? "OH NO!" : "LET'S GO!";
            emoteTime = 1.2f;
            game.sounds?.Play(sadEmote ? "emote_sad" : "emote_happy");
            effects?.Play(sadEmote ? ActionEffects.Kind.Sad : ActionEffects.Kind.Happy);
        }

        public void CelebrateVictory()
        {
            if (game == null || game.Running) return;
            CurrentAction = ActionType.None;
            StunRemaining = flash = move = 0;
            victoryEmote = true;
            sadEmote = false;
            emoteText = "* I DID IT! *";
            emoteTime = 2.4f;
            effects?.Play(ActionEffects.Kind.Victory);
        }

        public void PresentResult(bool winner, Vector2 position)
        {
            CancelAction();
            body.position = position;
            transform.position = position;
            controller.m_FacingRight = !isGirl;
            transform.localScale = new Vector3(isGirl ? -1 : 1, 1, 1);
            IsDefeated = !winner;
            HasWonRound = winner;
            animationTime = StunRemaining = flash = 0;
            if (winner) CelebrateVictory();
            else
            {
                victoryEmote = false;
                sadEmote = true;
                emoteText = "SO CLOSE...";
                emoteTime = 2.4f;
                effects?.Play(ActionEffects.Kind.Fail);
            }
            Animate(0);
        }

        void LateUpdate() => RefreshEmoteLabel();

        public void RefreshEmoteLabel()
        {
            if (emoteLabel == null) return;
            emoteLabel.text = emoteTime > 0 ? emoteText : "";
            emoteLabel.color = new Color(.22f, .12f, .05f);
            var camera = Camera.main;
            if (camera == null) return;
            var canvas = emoteLabel.canvas;
            var headAnchor = new Vector3(transform.position.x, visual.bounds.max.y + .32f, 0);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)emoteLabel.transform.parent,
                camera.WorldToScreenPoint(headAnchor), canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 point);
            emoteLabel.rectTransform.anchoredPosition = point;
        }

        public void AdvanceAction(float dt)
        {
            if (CurrentAction == ActionType.None) return;
            if (game == null || !game.Running) { CancelAction(); return; }
            actionTime += dt;
            if (StunRemaining > 0) { CurrentAction = ActionType.None; return; }
            float duration = CurrentAction == ActionType.Punch ? .28f : .45f;
            if (actionTime > duration) { CurrentAction = ActionType.None; return; }
            if (actionTime < .08f || opponent == null || opponent.Health <= 0) return;
            Vector2 offset = opponent.body.position - body.position;
            float ahead = offset.x * Direction;
            if (ahead < .05f || ahead > 1.2f || Mathf.Abs(offset.y) > 1.2f) return;
            if (CurrentAction == ActionType.Punch)
            {
                if (punchConnected) return;
                punchConnected = true;
                opponent.Receive(ActionType.Punch, new Vector2(Direction * 5.5f, 1.4f));
            }
            else opponent.Receive(ActionType.Push, new Vector2(Direction * 30 * dt, 0));
        }

        void Receive(ActionType action, Vector2 impulse)
        {
            LastReceivedAction = action;
            if (action == ActionType.Punch) Health = Mathf.Max(0, Health - 1);
            StunRemaining = Mathf.Max(StunRemaining, action == ActionType.Punch ? .35f : .09f);
            flash = .18f;
            CurrentAction = ActionType.None;
            body.AddForce(impulse, ForceMode2D.Impulse);
            if (reactionCooldown <= 0)
            {
                game.sounds?.Play(action == ActionType.Punch ? "hit" : "shove");
                effects?.Play(action == ActionType.Punch ? ActionEffects.Kind.Hit : ActionEffects.Kind.Shove, Mathf.Sign(impulse.x));
                reactionCooldown = .25f;
                sadEmote = true;
                emoteText = action == ActionType.Punch ? "OUCH!" : "WHOA!"; emoteTime = .5f;
            }
        }

        public void ResetPlayer(Vector2 position)
        {
            effects?.Clear();
            controller.ResetMotion();
            body.position = position;
            transform.position = position;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0;
            Health = 3;
            cooldown = StunRemaining = flash = actionTime = animationTime = move = 0;
            CurrentAction = LastReceivedAction = ActionType.None;
            jumpQueued = punchConnected = false;
            stepRemaining = .2f; airborneTime = 0;
            emoteTime = reactionCooldown = 0; emoteText = ""; sadEmote = true; victoryEmote = false;
            IsDefeated = HasWonRound = false;
            controller.m_FacingRight = !isGirl;
            transform.localScale = new Vector3(isGirl ? -1 : 1, 1, 1);
            Animate(0);
            effects?.Play(ActionEffects.Kind.Spawn);
        }

        void Animate(float dt)
        {
            animationTime += dt;
            bool punching = CurrentAction == ActionType.Punch;
            bool pushing = CurrentAction == ActionType.Push;
            Sprite[] frames = IsDefeated ? defeat : HasWonRound ? victory : StunRemaining > 0 ? hurt : punching ? punch : pushing ? push : !controller.IsGrounded ? jump : Mathf.Abs(body.linearVelocity.x) > .15f ? walk : idle;
            if (!Application.isPlaying || (game != null && game.IsCountingDown)) frames = idle;
            if (frames != null && frames.Length > 0)
            {
                int frame = punching || pushing
                    ? Mathf.Min(frames.Length - 1, Mathf.FloorToInt(actionTime / (punching ? .28f : .45f) * frames.Length))
                    : Mathf.FloorToInt(animationTime * 8) % frames.Length;
                visual.sprite = frames[frame];
            }
            // Attacks are full-body sprite animation; the old floating colored boxes stay hidden.
            fist.enabled = false;
            palm.enabled = false;
            float reach = punching ? Mathf.Sin(Mathf.Clamp01(actionTime / .28f) * Mathf.PI) : pushing ? .85f : 0;
            fist.transform.localPosition = new Vector3(.25f + reach * .6f, .85f, 0);
            palm.transform.localPosition = new Vector3(.75f, 1.05f, 0);
            float targetLean = IsDefeated ? Mathf.Sin(animationTime * 18) * 2 : HasWonRound ? Mathf.Sin(animationTime * 12) * 5 : punching ? -reach * 12 : pushing ? -8 : 0;
            float targetBob = HasWonRound ? Mathf.Abs(Mathf.Sin(animationTime * 8)) * .12f : emoteTime > 0 && !sadEmote ? Mathf.Abs(Mathf.Sin(animationTime * 12)) * .07f : 0;
            if (dt > 0)
            {
                visualLean = Mathf.SmoothDampAngle(visualLean, targetLean, ref leanVelocity, .06f, Mathf.Infinity, dt);
                visualBob = Mathf.SmoothDamp(visualBob, targetBob, ref bobVelocity, .045f, Mathf.Infinity, dt);
            }
            else { visualLean = visualBob = leanVelocity = bobVelocity = 0; }
            visual.transform.localRotation = Quaternion.Euler(0, 0, visualLean);
            visual.color = flash > 0 ? new Color(1, .45f, .4f) : Color.white;
            visual.transform.localPosition = new Vector3(0, visualBob, 0);
        }
    }
}
