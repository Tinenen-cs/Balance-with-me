using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BalanceWithMe.Tests
{
    public class SubmissionTests
    {
        BalanceGame game; GameMenu menu;
        InputSettings.BackgroundBehavior background; InputSettings.EditorInputBehaviorInPlayMode editor;
        [UnitySetUp] public IEnumerator Load()
        {
            background=InputSystem.settings.backgroundBehavior;editor=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            game=Object.FindFirstObjectByType<BalanceGame>();menu=game.GetComponent<GameMenu>();
            game.countdownStepDuration=0;game.Restart();menu.SetOpen(false);
        }
        [UnityTearDown] public IEnumerator Restore() { menu.SetOpen(false);InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editor;yield return null; }
        [UnityTest] public IEnumerator MenuPausesClockPhysicsAndAttacksWithoutLosingScore()
        {
            menu.SetOpen(true);float remaining=game.RemainingSeconds;Vector2 position=game.boy.body.position;
            game.AdvanceRoundTime(10);
            Assert.That(game.boy.TryAction(PlayerActions.ActionType.Punch),Is.False);
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.RemainingSeconds,Is.EqualTo(remaining));
            Assert.That(game.boy.body.position,Is.EqualTo(position));
            Assert.That(menu.panel.blocksRaycasts,Is.True);
            menu.Resume();yield return new WaitForSeconds(.15f);
            Assert.That(game.RemainingSeconds,Is.LessThan(remaining));Assert.That(game.MenuPaused,Is.False);
        }
        [UnityTest] public IEnumerator NewMatchClearsPartialScoreAndPreparedCountdownCanPause()
        {
            game.girl.body.position=new Vector2(0,-4);yield return null;
            Assert.That(game.BoyWins,Is.EqualTo(1));
            menu.SetOpen(true);game.countdownStepDuration=1;menu.NewMatch();
            Assert.That(game.BoyWins+game.GirlWins+game.CompletedRounds,Is.Zero);
            Assert.That(game.IsCountingDown,Is.True);menu.SetOpen(true);
            game.AdvanceReadyCountdown(10);Assert.That(game.CountdownPhase,Is.EqualTo(1));
            menu.Resume();game.AdvanceReadyCountdown(3);Assert.That(game.CanControl,Is.True);
        }
        [Test] public void SceneAssetsFontsAnimationFramesAndControlsMenuAreComplete()
        {
            foreach(var player in new[]{game.boy,game.girl})
            {
                foreach(var frames in new[]{player.idle,player.walk,player.jump,player.hurt,player.punch,player.push,player.victory,player.defeat})
                { Assert.That(frames.Length,Is.GreaterThan(0));foreach(var frame in frames)Assert.That(frame,Is.Not.Null); }
                foreach(var portrait in new[]{player.portraitNormal,player.portraitHurt,player.portraitVictory,player.portraitDefeat})Assert.That(portrait,Is.Not.Null);
            }
            foreach(var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))Assert.That(text.font,Is.Not.Null,text.name);
            foreach(var cue in game.sounds.cues)Assert.That(cue.clip,Is.Not.Null,cue.name);
            Assert.That(menu.resumeButton,Is.Not.Null);Assert.That(menu.deviceStatus,Is.Not.Null);
        }
        [UnityTest] public IEnumerator XboxCompatiblePadControlsBothPlayersAndHotplugKeepsOtherAssignment()
        {
            var first=InputSystem.AddDevice<Gamepad>();var second=InputSystem.AddDevice<Gamepad>();
            try
            {
                game.inputs.Assign(false,first);game.inputs.Assign(true,second);
                InputSystem.QueueStateEvent(first,new GamepadState().WithButton(GamepadButton.West));
                InputSystem.QueueStateEvent(second,new GamepadState().WithButton(GamepadButton.East));InputSystem.Update();
                game.boy.CaptureKeyboardInput();game.girl.CaptureKeyboardInput();
                Assert.That(game.boy.CurrentAction,Is.EqualTo(PlayerActions.ActionType.Punch));
                Assert.That(game.girl.CurrentAction,Is.EqualTo(PlayerActions.ActionType.Push));
                InputSystem.RemoveDevice(first);game.inputs.Reconcile();Assert.That(game.inputs.GirlPad,Is.SameAs(second));
                yield return null;
            }
            finally { if(first.added)InputSystem.RemoveDevice(first);InputSystem.RemoveDevice(second); }
        }
        [UnityTest] public IEnumerator JumpDoesNotSubmitPreviouslySelectedRestartButton()
        {
            var pad=InputSystem.AddDevice<Gamepad>();
            try
            {
                game.inputs.Assign(false,pad);game.AdvanceRoundTime(5);
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(GameObject.Find("Restart Farm Sign"));
                yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();
                game.boy.CaptureKeyboardInput();yield return new WaitForSeconds(.1f);
                Assert.That(game.RemainingSeconds,Is.LessThan(55));
                Assert.That(UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents,Is.False);
                menu.SetOpen(true);yield return null;
                Assert.That(UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents,Is.True);
            }
            finally { InputSystem.RemoveDevice(pad); }
        }
        [UnityTest] public IEnumerator RawUsbJoystickCanBeMappedAndDisconnectedWithoutStealingOtherPlayer()
        {
            InputSystem.RegisterLayout("{\"name\":\"BalanceTestJoystick\",\"extend\":\"Joystick\",\"controls\":[{\"name\":\"button2\",\"layout\":\"Button\"},{\"name\":\"button3\",\"layout\":\"Button\"},{\"name\":\"button4\",\"layout\":\"Button\"},{\"name\":\"button5\",\"layout\":\"Button\"}]}");
            var stick=(Joystick)InputSystem.AddDevice("BalanceTestJoystick");
            try
            {
                game.inputs.AssignJoystick(false,stick);bool girl=false;
                Assert.That(game.inputs.ForJoystick(girl),Is.SameAs(stick));
                game.inputs.BindJoystick(stick,1,stick.trigger);
                using(StateEvent.From(stick,out var stateEvent))
                {
                    stick.trigger.WriteValueIntoEvent(1f,stateEvent);InputSystem.QueueEvent(stateEvent);
                }
                InputSystem.Update();
                var player=girl?game.girl:game.boy;player.CaptureKeyboardInput();
                Assert.That(player.CurrentAction,Is.EqualTo(PlayerActions.ActionType.Punch));
                Assert.That(game.inputs.ButtonsFor(stick).Length,Is.EqualTo(5));
                InputSystem.RemoveDevice(stick);game.inputs.Reconcile();Assert.That(game.inputs.ForJoystick(girl),Is.Null);
                yield return null;
            }
            finally
            {
                for(int i=0;i<5;i++)PlayerPrefs.DeleteKey("BalanceWithMe.Joystick.BalanceTestJoystick."+i);
                if(stick.added)InputSystem.RemoveDevice(stick);InputSystem.RemoveLayout("BalanceTestJoystick");
            }
        }
    }
}
