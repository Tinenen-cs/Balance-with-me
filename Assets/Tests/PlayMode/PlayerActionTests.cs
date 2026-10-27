using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BalanceWithMe.Tests
{
    public class PlayerActionTests
    {
        BalanceGame game;
        InputSettings.BackgroundBehavior backgroundBehavior;
        InputSettings.EditorInputBehaviorInPlayMode editorBehavior;
        [UnitySetUp] public IEnumerator Load()
        {
            backgroundBehavior=InputSystem.settings.backgroundBehavior;
            editorBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
            game=Object.FindFirstObjectByType<BalanceGame>();
            game.countdownStepDuration = 0; game.Restart();
            Assert.That(game,Is.Not.Null);
            game.boy.body.position=new Vector2(-.4f,.3f);
            game.girl.body.position=new Vector2(.4f,.3f);
            Physics2D.SyncTransforms();
        }
        [TearDown] public void RestoreInputSettings()
        {
            InputSystem.settings.backgroundBehavior=backgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode=editorBehavior;
        }
        void Tilt(float angle, float girlX = 3)
        {
            game.board.constraints = RigidbodyConstraints2D.FreezeRotation;
            game.board.rotation = angle;
            game.board.transform.rotation = Quaternion.Euler(0,0,angle);
            game.boy.body.position = game.board.transform.TransformPoint(new Vector3(-3,.24f,0));
            game.girl.body.position = game.board.transform.TransformPoint(new Vector3(girlX,.24f,0));
            game.boy.body.linearVelocity = game.girl.body.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
        }
        [UnityTest] public IEnumerator LowerSideSlideBuildsGraduallyAndFollowsTiltReversal()
        {
            Tilt(12);
            yield return new WaitForSeconds(.35f);
            Assert.That(game.boy.SlopeDrift, Is.LessThan(-.1f).And.GreaterThan(-.5f));
            Assert.That(game.girl.SlopeDrift, Is.Zero);
            float first = game.boy.SlopeDrift;
            float x = game.boy.body.position.x;
            yield return new WaitForSeconds(.4f);
            Assert.That(game.boy.SlopeDrift, Is.LessThan(first));
            Assert.That(game.boy.body.position.x, Is.LessThan(x));
            Tilt(-12);
            yield return new WaitForSeconds(.9f);
            Assert.That(game.girl.SlopeDrift, Is.GreaterThan(.2f));
            Assert.That(Mathf.Abs(game.boy.SlopeDrift), Is.LessThan(.05f));
            game.Restart(); Assert.That(game.boy.SlopeDrift + game.girl.SlopeDrift, Is.Zero);
        }
        [UnityTest] public IEnumerator UphillInputOvercomesSlideAndJumpClearsIt()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.EnableDevice(keyboard); Tilt(-12);
                yield return new WaitForSeconds(.7f);
                Assert.That(game.girl.SlopeDrift, Is.GreaterThan(.2f));
                float x = game.girl.body.position.x;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftArrow)); InputSystem.Update();
                game.girl.CaptureKeyboardInput();
                yield return new WaitForSeconds(.4f);
                Assert.That(game.girl.body.position.x, Is.LessThan(x - .2f));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.UpArrow)); InputSystem.Update();
                game.girl.CaptureKeyboardInput();
                yield return new WaitForSeconds(.12f);
                Assert.That(game.girl.controller.IsGrounded, Is.False);
                Assert.That(game.girl.SlopeDrift, Is.Zero);
            }
            finally { InputSystem.RemoveDevice(keyboard); }
        }
        [UnityTest] public IEnumerator UnattendedLowerEdgePlayerEventuallyFalls()
        {
            Tilt(-14, 3.9f);
            float deadline = Time.time + 6;
            while (game.Running && Time.time < deadline) yield return new WaitForFixedUpdate();
            Assert.That(game.Running, Is.False, "An idle lower-side player must eventually slide off the edge.");
            Assert.That(game.GirlDefeat, Is.EqualTo(BalanceGame.DefeatCause.Fall));
            Assert.That(game.BoyWins, Is.EqualTo(1));
            Assert.That(game.girl.SlopeDrift, Is.Zero);
        }
        [Test] public void PunchHitsOnceAndCannotBeSpammed()
        {
            Assert.That(game.boy.TryAction(PlayerActions.ActionType.Punch),Is.True);
            Assert.That(game.boy.TryAction(PlayerActions.ActionType.Punch),Is.False);
            game.boy.AdvanceAction(.1f);
            Assert.That(game.girl.Health,Is.EqualTo(2));
            Assert.That(game.girl.StunRemaining,Is.GreaterThan(0));
            Assert.That(game.girl.TryAction(PlayerActions.ActionType.Push),Is.False);
            game.boy.AdvanceAction(.02f);
            Assert.That(game.girl.Health,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator PushKnocksBackWithoutDamage()
        {
            game.girl.enabled=false;
            Assert.That(game.boy.TryAction(PlayerActions.ActionType.Push),Is.True);
            game.boy.AdvanceAction(.1f);
            yield return new WaitForFixedUpdate();
            Assert.That(game.girl.Health,Is.EqualTo(3));
            Assert.That(game.girl.LastReceivedAction,Is.EqualTo(PlayerActions.ActionType.Push));
            Assert.That(game.girl.body.linearVelocity.x,Is.GreaterThan(0));
        }
        [Test] public void PunchCannotHitBehindOrOutsideReach()
        {
            game.boy.controller.m_FacingRight=false;
            game.boy.TryAction(PlayerActions.ActionType.Punch);game.boy.AdvanceAction(.1f);
            Assert.That(game.girl.Health,Is.EqualTo(3));
            game.Restart();
            game.boy.TryAction(PlayerActions.ActionType.Punch);game.boy.AdvanceAction(.1f);
            Assert.That(game.girl.Health,Is.EqualTo(3));
        }
        [Test] public void RestartClearsHealthActionsAndKnockback()
        {
            game.boy.TryAction(PlayerActions.ActionType.Punch);game.boy.AdvanceAction(.1f);
            game.Restart();
            Assert.That(game.girl.Health,Is.EqualTo(3));
            Assert.That(game.girl.StunRemaining,Is.Zero);
            Assert.That(game.boy.CurrentAction,Is.EqualTo(PlayerActions.ActionType.None));
            Assert.That(game.girl.body.linearVelocity,Is.EqualTo(Vector2.zero));
            Assert.That(game.boy.body.position.x,Is.EqualTo(-2.6f));
            Assert.That(game.boy.controller.m_FacingRight,Is.True);
            Assert.That(game.girl.controller.m_FacingRight,Is.False);
        }
        [UnityTest] public IEnumerator FallingEndsRoundAndDisablesActions()
        {
            game.boy.body.position=new Vector2(0,-4);
            yield return null;
            Assert.That(game.Running,Is.False);
            Assert.That(game.girl.TryAction(PlayerActions.ActionType.Punch),Is.False);
            Assert.That(game.status.text,Does.Contain("GIRL WINS"));
        }
        [UnityTest] public IEnumerator WinUpdatesScoreAndRestartKeepsIt()
        {
            game.boy.body.position=new Vector2(0,-4);
            yield return null;
            Assert.That(game.GirlWins,Is.EqualTo(1));
            Assert.That(game.girlWinStars[0].color,Is.EqualTo(Color.white));
            game.Restart();yield return null;
            Assert.That(game.GirlWins,Is.EqualTo(1));
            Assert.That(game.timer.text,Does.Contain("00:25"));
            Assert.That(game.girl.Health,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator KeyboardStartsBoyPushAndGirlPunch()
        {
            game.Restart();
            var keyboard=InputSystem.AddDevice<Keyboard>();
            InputSystem.EnableDevice(keyboard);
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.G,Key.K));
                InputSystem.Update();
                game.boy.CaptureKeyboardInput();
                game.girl.CaptureKeyboardInput();
                Assert.That(game.boy.CurrentAction,Is.EqualTo(PlayerActions.ActionType.Push));
                Assert.That(game.girl.CurrentAction,Is.EqualTo(PlayerActions.ActionType.Punch));
                yield return null;
                Assert.That(game.boy.palm.enabled,Is.False);
                Assert.That(game.girl.fist.enabled,Is.False);
                Assert.That(game.boy.push,Does.Contain(game.boy.visual.sprite));
                Assert.That(game.girl.punch,Does.Contain(game.girl.visual.sprite));
            }
            finally { InputSystem.RemoveDevice(keyboard); }
        }
        [UnityTest] public IEnumerator MovementAcceleratesReversesAndSettlesWithoutOvershoot()
        {
            game.Restart();game.board.constraints=RigidbodyConstraints2D.FreezeRotation;
            yield return new WaitForSeconds(.2f);
            var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.EnableDevice(keyboard);
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));InputSystem.Update();game.boy.CaptureKeyboardInput();
                yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
                float initial=game.boy.body.linearVelocity.x;
                Assert.That(initial,Is.GreaterThan(0).And.LessThan(3.8f));
                yield return new WaitForSeconds(.2f);
                Assert.That(game.boy.body.linearVelocity.x,Is.GreaterThan(initial).And.LessThanOrEqualTo(3.9f));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.A));InputSystem.Update();game.boy.CaptureKeyboardInput();
                yield return new WaitForSeconds(.3f);
                Assert.That(game.boy.body.linearVelocity.x,Is.LessThan(-3).And.GreaterThanOrEqualTo(-3.9f));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();game.boy.CaptureKeyboardInput();
                yield return new WaitForSeconds(.3f);
                Assert.That(Mathf.Abs(game.boy.body.linearVelocity.x),Is.LessThan(.15f));
                game.Restart();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
                Assert.That(Mathf.Abs(game.boy.body.linearVelocity.x),Is.LessThan(.05f));
            }
            finally { InputSystem.RemoveDevice(keyboard); }
        }
        [UnityTest] public IEnumerator PlankSwingIsContinuousBoundedAndDamped()
        {
            game.Restart();
            Assert.That(game.board.interpolation,Is.EqualTo(RigidbodyInterpolation2D.Interpolate));
            Assert.That(game.boy.body.interpolation,Is.EqualTo(RigidbodyInterpolation2D.Interpolate));
            Assert.That(game.girl.body.interpolation,Is.EqualTo(RigidbodyInterpolation2D.Interpolate));
            // Isolate the plank to measure its response to a repeatable impulse.
            game.boy.body.simulated=false;game.girl.body.simulated=false;
            game.board.rotation=10;game.board.AddTorque(12,ForceMode2D.Impulse);
            float previous=game.board.rotation;
            for(int step=0;step<150;step++)
            {
                yield return new WaitForFixedUpdate();
                float current=game.board.rotation;
                Assert.That(Mathf.Abs(current),Is.LessThanOrEqualTo(16.5f));
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(previous,current)),Is.LessThan(1.5f));
                previous=current;
            }
            Assert.That(Mathf.Abs(game.board.rotation),Is.LessThan(10));
            Assert.That(Mathf.Abs(game.board.angularVelocity),Is.LessThan(5));
            game.boy.body.simulated=true;game.girl.body.simulated=true;game.Restart();
            game.boy.body.position=new Vector2(-3.8f,.04f);game.girl.body.position=new Vector2(.6f,.04f);
            previous=game.board.rotation;
            for(int step=0;step<50;step++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(Mathf.Abs(game.board.rotation),Is.LessThanOrEqualTo(16.5f));
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(previous,game.board.rotation)),Is.LessThan(1.5f));
                previous=game.board.rotation;
            }
            Assert.That(Mathf.Abs(game.board.rotation),Is.GreaterThan(1),"Uneven player weight should still tilt the plank.");
        }
        [UnityTest] public IEnumerator BothJumpKeysLiftGroundedPlayers()
        {
            game.Restart();
            yield return new WaitForSeconds(.15f);
            var keyboard=InputSystem.AddDevice<Keyboard>();
            InputSystem.EnableDevice(keyboard);
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.UpArrow));
                InputSystem.Update();
                game.boy.CaptureKeyboardInput();
                game.girl.CaptureKeyboardInput();
                yield return new WaitForSeconds(.15f);
                Assert.That(game.boy.body.position.y,Is.GreaterThan(.5f));
                Assert.That(game.girl.body.position.y,Is.GreaterThan(.5f));
            }
            finally { InputSystem.RemoveDevice(keyboard); }
        }
    }
}
