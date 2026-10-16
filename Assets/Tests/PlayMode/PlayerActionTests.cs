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
                Assert.That(game.boy.palm.enabled,Is.True);
                Assert.That(game.girl.fist.enabled,Is.True);
                Assert.That(game.boy.palm.bounds.size.x,Is.GreaterThan(.1f));
                Assert.That(game.girl.fist.bounds.size.x,Is.GreaterThan(.2f));
            }
            finally { InputSystem.RemoveDevice(keyboard); }
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
