using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BalanceWithMe.Tests
{
    public class RoundClockTests
    {
        BalanceGame game;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Main"); yield return null;
            game = Object.FindFirstObjectByType<BalanceGame>();
            game.countdownStepDuration = 1; game.Restart();
        }
        [Test] public void ReadySetStartFreezesPhysicsAndInputsUntilCountdownEnds()
        {
            Assert.That(game.IsCountingDown, Is.True);
            Assert.That(game.readyNumber.text, Is.EqualTo("1"));
            Assert.That(game.readyCaption.text, Is.EqualTo("READY"));
            Assert.That(game.boy.body.simulated || game.board.simulated, Is.False);
            Assert.That(game.boy.TryAction(PlayerActions.ActionType.Punch), Is.False);
            game.AdvanceRoundTime(90);
            Assert.That(game.RemainingSeconds, Is.EqualTo(180));
            game.AdvanceReadyCountdown(1);
            Assert.That(game.readyNumber.text, Is.EqualTo("2"));
            Assert.That(game.readyCaption.text, Is.EqualTo("SET"));
            game.AdvanceReadyCountdown(1);
            Assert.That(game.readyNumber.text, Is.EqualTo("3"));
            Assert.That(game.readyCaption.text, Is.EqualTo("START!"));
            game.AdvanceReadyCountdown(1);
            Assert.That(game.Running, Is.True);
            Assert.That(game.readyPopup.alpha, Is.Zero);
            Assert.That(game.board.simulated && game.boy.body.simulated, Is.True);
            game.RefreshHud(); Assert.That(game.timer.text, Does.Contain("03:00"));
        }
        [Test] public void WarningsFireAtThirtyAndEveryFinalSecondAndClearOnRestart()
        {
            game.AdvanceReadyCountdown(3);
            int warnings = 0, ticks = 0;
            game.sounds.CuePlayed += cue => { if (cue == "time_warning") warnings++; if (cue == "time_tick") ticks++; };
            game.AdvanceRoundTime(150);
            Assert.That(warnings, Is.EqualTo(1));
            Assert.That(game.timeAlert.text, Is.EqualTo("30 SECONDS LEFT!"));
            game.AdvanceRoundTime(.1f); Assert.That(warnings, Is.EqualTo(1));
            game.AdvanceRoundTime(19.9f);
            for (int i = 9; i >= 1; i--) game.AdvanceRoundTime(1);
            Assert.That(ticks, Is.EqualTo(10));
            Assert.That(game.timeAlert.text, Is.EqualTo("1 SECOND LEFT!"));
            game.Restart();
            Assert.That(game.timeAlertPopup.alpha, Is.Zero);
            Assert.That(game.RemainingSeconds, Is.EqualTo(180));
        }
        [Test] public void TimeoutWithEqualLifeIsDrawAndCannotScoreTwice()
        {
            game.AdvanceReadyCountdown(3); game.AdvanceRoundTime(180);
            Assert.That(game.CompletedRounds, Is.EqualTo(1));
            Assert.That(game.BoyWins + game.GirlWins, Is.Zero);
            Assert.That(game.status.text, Does.Contain("DRAW"));
            game.AdvanceRoundTime(180);
            Assert.That(game.CompletedRounds, Is.EqualTo(1));
            Assert.That(game.girl.body.position.y, Is.GreaterThan(-1));
        }
        [Test] public void TimeoutAwardsHigherLifeForEitherPlayerWithoutMovingLoserToGround()
        {
            foreach (bool boyWins in new[] { true, false })
            {
                game.Restart(); game.AdvanceReadyCountdown(3);
                var attacker = boyWins ? game.boy : game.girl;
                var defender = boyWins ? game.girl : game.boy;
                game.boy.body.position = new Vector2(-.4f,.1f);
                game.girl.body.position = new Vector2(.4f,.1f);
                attacker.TryAction(PlayerActions.ActionType.Punch); attacker.AdvanceAction(.1f);
                Assert.That(defender.Health, Is.EqualTo(2));
                game.AdvanceRoundTime(180);
                Assert.That(attacker.HasWonRound, Is.True);
                Assert.That(defender.IsDefeated, Is.True);
                Assert.That(defender.body.position.y, Is.GreaterThan(-1));
            }
            Assert.That(game.BoyWins, Is.EqualTo(1)); Assert.That(game.GirlWins, Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator FinalResultWaitsForClickableReplayAndStartsReadyCountdown()
        {
            for (int round = 0; round < 3; round++)
            {
                if (round > 0) game.Restart();
                game.AdvanceReadyCountdown(3); game.AdvanceRoundTime(180);
            }
            yield return new WaitForSecondsRealtime(5.2f);
            Assert.That(game.MatchComplete, Is.True, "Final result must wait for input, beyond the old automatic reset delay.");
            Assert.That(game.playAgainButton.IsInteractable(), Is.True);
            Canvas.ForceUpdateCanvases();
            var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            pointer.position = RectTransformUtility.WorldToScreenPoint(null, game.playAgainButton.transform.position);
            var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Exists(hit => hit.gameObject == game.playAgainButton.gameObject), Is.True, "The button must receive real UI raycasts.");
            UnityEngine.EventSystems.ExecuteEvents.Execute(game.playAgainButton.gameObject, pointer,
                UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
            Assert.That(game.MatchComplete, Is.False);
            Assert.That(game.CompletedRounds + game.BoyWins + game.GirlWins, Is.Zero);
            Assert.That(game.IsCountingDown, Is.True);
            Assert.That(game.readyCaption.text, Is.EqualTo("READY"));
            Assert.That(game.RemainingSeconds, Is.EqualTo(180));
        }
        [UnityTest] public IEnumerator FallenLoserStaysOnGrassUntilNextRoundReadyPopup()
        {
            game.AdvanceReadyCountdown(3); game.nextRoundDelay = .2f;
            game.girl.body.position = new Vector2(6,-4);
            yield return null;
            Assert.That(game.girl.body.position.x, Is.GreaterThan(5));
            Assert.That(game.girl.body.position.y, Is.EqualTo(-3.2f));
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(game.girl.body.position.y, Is.EqualTo(-3.2f));
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(game.IsCountingDown, Is.True);
            Assert.That(game.RoundNumber, Is.EqualTo(2));
            Assert.That(game.girl.HealthPercent, Is.EqualTo(100));
        }
    }
}
