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
    public class ActionEffectsTests
    {
        BalanceGame game;
        InputSettings.BackgroundBehavior background;
        InputSettings.EditorInputBehaviorInPlayMode editor;
        [UnitySetUp] public IEnumerator Load()
        {
            background=InputSystem.settings.backgroundBehavior;editor=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            game=Object.FindFirstObjectByType<BalanceGame>();game.Restart();
        }
        [TearDown] public void Restore()
        {
            InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editor;
        }
        [Test] public void AcceptedAttacksAndContactsEmitDifferentEffects()
        {
            game.boy.body.position=new Vector2(-.4f,.1f);game.girl.body.position=new Vector2(.4f,.1f);
            game.boy.effects.Clear();game.girl.effects.Clear();
            Assert.That(game.boy.TryAction(PlayerActions.ActionType.Punch),Is.True);
            Assert.That(game.boy.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Punch));
            int count=game.boy.effects.ActiveParticleCount;
            Assert.That(count,Is.GreaterThan(0));
            Assert.That(game.boy.TryAction(PlayerActions.ActionType.Punch),Is.False);
            Assert.That(game.boy.effects.ActiveParticleCount,Is.EqualTo(count));
            game.boy.AdvanceAction(.1f);
            Assert.That(game.girl.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Hit));
            Assert.That(game.girl.effects.ActiveParticleCount,Is.GreaterThan(0));
            game.Restart();
            game.boy.body.position=new Vector2(-.4f,.1f);game.girl.body.position=new Vector2(.4f,.1f);
            game.girl.TryAction(PlayerActions.ActionType.Push);
            Assert.That(game.girl.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Push));
            game.girl.AdvanceAction(.1f);
            Assert.That(game.boy.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Shove));
        }
        [UnityTest] public IEnumerator EmoteResultEffectsExpireAndThePoolStaysBounded()
        {
            Assert.That(game.boy.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Spawn));
            game.boy.Emote();Assert.That(game.boy.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Happy));
            yield return new WaitForSeconds(1.3f);
            game.boy.Emote();Assert.That(game.boy.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Sad));
            game.girl.body.position=new Vector2(0,-4);
            yield return null;yield return null;
            Assert.That(game.girl.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Fail));
            Assert.That(game.boy.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Victory));
            game.Restart();
            Assert.That(game.boy.effects.LastEffect,Is.EqualTo(ActionEffects.Kind.Spawn));
            Assert.That(game.boy.effects.ActiveParticleCount,Is.EqualTo(8));
            int rendererCount=Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length;
            for(int repeat=0;repeat<100;repeat++)game.boy.effects.Play(ActionEffects.Kind.Victory);
            Assert.That(game.boy.effects.ActiveParticleCount,Is.LessThanOrEqualTo(game.boy.effects.capacity));
            Assert.That(Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length,Is.EqualTo(rendererCount));
            yield return new WaitForSeconds(1.2f);
            Assert.That(game.boy.effects.ActiveParticleCount,Is.Zero);
            Assert.That(game.girl.effects.ActiveParticleCount,Is.Zero);
            game.boy.effects.enabled=false;game.boy.effects.Play(ActionEffects.Kind.Punch);
            Assert.That(game.boy.effects.ActiveParticleCount,Is.Zero);
        }
        [UnityTest] public IEnumerator BothPlayersEmitWalkJumpAndLandingEffects()
        {
            var boyEffects=new HashSet<ActionEffects.Kind>();var girlEffects=new HashSet<ActionEffects.Kind>();
            game.boy.effects.EffectEmitted+=kind=>boyEffects.Add(kind);game.girl.effects.EffectEmitted+=kind=>girlEffects.Add(kind);
            var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.EnableDevice(keyboard);
            try
            {
                yield return new WaitForSeconds(.2f);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D,Key.LeftArrow));InputSystem.Update();
                yield return new WaitForSeconds(.4f);
                Assert.That(boyEffects,Does.Contain(ActionEffects.Kind.Step));Assert.That(girlEffects,Does.Contain(ActionEffects.Kind.Step));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.UpArrow));InputSystem.Update();
                game.boy.CaptureKeyboardInput();game.girl.CaptureKeyboardInput();
                yield return new WaitForFixedUpdate();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
                yield return new WaitForSeconds(1.6f);
                foreach(var heard in new[]{boyEffects,girlEffects})
                {
                    Assert.That(heard,Does.Contain(ActionEffects.Kind.Jump));Assert.That(heard,Does.Contain(ActionEffects.Kind.Land));
                }
            }
            finally { InputSystem.RemoveDevice(keyboard); }
        }
    }
}
