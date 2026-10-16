using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BalanceWithMe.Tests
{
    public class ControllerAudioTests
    {
        BalanceGame game;
        InputSettings.BackgroundBehavior background;
        InputSettings.EditorInputBehaviorInPlayMode editor;
        [UnitySetUp] public IEnumerator Load()
        {
            background=InputSystem.settings.backgroundBehavior;editor=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Main"); yield return null;
            game=Object.FindFirstObjectByType<BalanceGame>(); game.Restart();
        }
        [TearDown] public void Restore()
        {
            InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editor;
        }
        [UnityTest] public IEnumerator TwoGamepadsControlSeparatePlayers()
        {
            var first=InputSystem.AddDevice<Gamepad>();var second=InputSystem.AddDevice<Gamepad>();
            try
            {
                InputSystem.EnableDevice(first);InputSystem.EnableDevice(second);
                game.inputs.Assign(false,first);game.inputs.Assign(true,second);
                InputSystem.QueueStateEvent(first,new GamepadState().WithButton(GamepadButton.West));
                InputSystem.QueueStateEvent(second,new GamepadState().WithButton(GamepadButton.East));InputSystem.Update();
                game.boy.CaptureKeyboardInput();game.girl.CaptureKeyboardInput();
                Assert.That(game.boy.CurrentAction,Is.EqualTo(PlayerActions.ActionType.Punch));
                Assert.That(game.girl.CurrentAction,Is.EqualTo(PlayerActions.ActionType.Push));
                InputSystem.RemoveDevice(first);game.inputs.Reconcile();
                Assert.That(game.inputs.GirlPad,Is.SameAs(second));
                yield return null;
            }
            finally { if(first.added)InputSystem.RemoveDevice(first);InputSystem.RemoveDevice(second); }
        }
        [UnityTest] public IEnumerator GamepadStickAndJumpReachPhysicsMotor()
        {
            var pad=InputSystem.AddDevice<Gamepad>();
            try
            {
                InputSystem.EnableDevice(pad);game.inputs.Assign(false,pad);
                yield return new WaitForSeconds(.15f);
                InputSystem.QueueStateEvent(pad,new GamepadState { leftStick=new Vector2(.7f,0) }.WithButton(GamepadButton.South));InputSystem.Update();
                game.boy.CaptureKeyboardInput();yield return new WaitForSeconds(.15f);
                Assert.That(game.boy.body.position.y,Is.GreaterThan(.5f));
                Assert.That(game.boy.body.position.x,Is.GreaterThan(-2.6f));
            }
            finally { InputSystem.RemoveDevice(pad); }
        }
        [Test] public void AudioTracksCuesMuteAndEmotesAreWired()
        {
            Assert.That(game.sounds.tracks.Length,Is.EqualTo(2));
            foreach(var track in game.sounds.tracks)Assert.That(track.length,Is.GreaterThan(5));
            foreach(var cue in game.sounds.cues)Assert.That(cue.clip,Is.Not.Null);
            game.sounds.NextTrack();Assert.That(game.sounds.TrackIndex,Is.EqualTo(1));
            game.sounds.SetMuted(true,false);Assert.That(game.sounds.music.mute && game.sounds.effects.mute,Is.True);
            game.sounds.SetMuted(false,false);
            game.boy.Emote();Assert.That(game.sounds.LastCue,Is.EqualTo("emote_happy"));
            game.boy.TryAction(PlayerActions.ActionType.Punch);Assert.That(game.sounds.LastCue,Is.EqualTo("punch"));
        }
        [UnityTest] public IEnumerator BackgroundLoopsAndBothEmoteJinglesPlay()
        {
            Assert.That(game.sounds.music.loop,Is.True);
            Assert.That(game.sounds.music.isPlaying,Is.True);
            Assert.That(game.sounds.music.clip,Is.SameAs(game.sounds.tracks[0]));
            game.boy.Emote();Assert.That(game.sounds.LastCue,Is.EqualTo("emote_happy"));
            yield return new WaitForSeconds(1.3f);
            game.boy.Emote();Assert.That(game.sounds.LastCue,Is.EqualTo("emote_sad"));
            game.sounds.NextTrack();
            Assert.That(game.sounds.music.clip,Is.SameAs(game.sounds.tracks[1]));
            Assert.That(game.sounds.music.isPlaying,Is.True);
        }
        [UnityTest] public IEnumerator HitAndResultReactionsHaveAudio()
        {
            game.boy.body.position=new Vector2(-.4f,.1f);
            game.girl.body.position=new Vector2(.4f,.1f);
            game.boy.TryAction(PlayerActions.ActionType.Punch);game.boy.AdvanceAction(.1f);
            Assert.That(game.sounds.LastCue,Is.EqualTo("hit"));
            game.Restart();Assert.That(game.sounds.LastCue,Is.EqualTo("start"));
            game.boy.body.position=new Vector2(0,-4);
            yield return null;
            Assert.That(game.sounds.LastCue,Is.EqualTo("fail"));
            yield return new WaitForSeconds(.85f);
            Assert.That(game.sounds.LastCue,Is.EqualTo("win"));
        }
        [UnityTest] public IEnumerator HelpAndAudioNoticesExpireWithoutHidingEssentials()
        {
            yield return new WaitForSeconds(5.2f);
            Assert.That(game.helpPopup.alpha,Is.Zero);
            Assert.That(game.statusPopup.alpha,Is.Zero);
            Assert.That(game.audioPopup.alpha,Is.Zero);
            Assert.That(game.boyHud.isActiveAndEnabled && game.girlHud.isActiveAndEnabled && game.timer.isActiveAndEnabled,Is.True);
            game.PopupDuration=.1f;game.ShowHelp();game.NextMusic();
            yield return null;
            Assert.That(game.helpPopup.alpha,Is.EqualTo(1));
            Assert.That(game.audioPopup.alpha,Is.EqualTo(1));
            StringAssert.Contains("MUSIC 2",game.audioStatus.text);
            yield return new WaitForSeconds(2.7f);
            Assert.That(game.helpPopup.alpha,Is.Zero);
            Assert.That(game.audioPopup.alpha,Is.Zero);
        }
        [UnityTest] public IEnumerator BothPlayersHaveFootstepJumpAndLandingAudio()
        {
            var heard=new HashSet<string>();game.sounds.CuePlayed+=cue=>heard.Add(cue);
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.EnableDevice(keyboard);
                foreach(bool girl in new[]{false,true})
                {
                    game.Restart();heard.Clear();
                    yield return new WaitForSeconds(.25f);
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(girl?Key.LeftArrow:Key.D));InputSystem.Update();
                    yield return new WaitForSeconds(.4f);
                    Assert.That(heard,Does.Contain("step"));
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(girl?Key.UpArrow:Key.W));InputSystem.Update();
                    (girl?game.girl:game.boy).CaptureKeyboardInput();
                    yield return new WaitForFixedUpdate();
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
                    yield return new WaitForSeconds(1.6f);
                    Assert.That(heard,Does.Contain("jump"));
                    Assert.That(heard,Does.Contain("land"));
                }
            }
            finally { InputSystem.RemoveDevice(keyboard); }
        }
        [UnityTest] public IEnumerator RestartCancelsPendingResultJingle()
        {
            game.sounds.PlayResult(false);
            Assert.That(game.sounds.LastCue,Is.EqualTo("fail"));
            game.Restart();
            yield return new WaitForSeconds(.9f);
            Assert.That(game.sounds.LastCue,Is.EqualTo("start"));
            game.boy.body.position=new Vector2(0,-4);game.girl.body.position=new Vector2(1,-4);
            yield return null;
            Assert.That(game.sounds.LastCue,Is.EqualTo("fail"));
            yield return new WaitForSeconds(.85f);
            Assert.That(game.sounds.LastCue,Is.EqualTo("draw"));
        }
        [UnityTest] public IEnumerator GamepadStartRestartsAndNorthButtonEmotes()
        {
            var pad=InputSystem.AddDevice<Gamepad>();
            try
            {
                InputSystem.EnableDevice(pad);game.inputs.Assign(false,pad);
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.North));InputSystem.Update();
                game.boy.CaptureKeyboardInput();Assert.That(game.sounds.LastCue,Is.EqualTo("emote_happy"));
                InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();
                game.boy.body.position=new Vector2(-1,0);
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.Start));
                yield return null;yield return null;
                Assert.That(game.boy.body.position.x,Is.EqualTo(-2.6f).Within(.1f));
            }
            finally { InputSystem.RemoveDevice(pad); }
        }
    }
}
