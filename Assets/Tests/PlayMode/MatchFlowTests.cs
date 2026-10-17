using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BalanceWithMe.Tests
{
    public class MatchFlowTests
    {
        BalanceGame game;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
            game = Object.FindFirstObjectByType<BalanceGame>();
        }

        [UnityTest] public IEnumerator RoundAutomaticallyAdvancesAndPreservesScore()
        {
            game.nextRoundDelay = .15f;
            game.boy.body.position = new Vector2(0, -4);
            yield return null;
            Assert.That(game.Running, Is.False);
            Assert.That(game.board.simulated || game.girl.body.simulated, Is.False);
            Assert.That(game.CompletedRounds, Is.EqualTo(1));
            Assert.That(game.GirlWinRate, Is.EqualTo(100));
            Assert.That(game.girlHud.text, Does.Contain("WIN RATE 100%"));
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.Running, Is.True);
            Assert.That(game.RoundNumber, Is.EqualTo(2));
            Assert.That(game.GirlWins, Is.EqualTo(1));
            Assert.That(game.boy.HealthPercent, Is.EqualTo(100).Within(.01f));
            Assert.That(game.boy.body.position.x, Is.EqualTo(-2.6f).Within(.05f));
            Assert.That(game.board.simulated && game.girl.body.simulated, Is.True);
        }

        [UnityTest] public IEnumerator FirstToThreeEndsMatchAndRestartBeginsFreshMatch()
        {
            game.nextRoundDelay = .12f;
            for (int win = 1; win <= 3; win++)
            {
                game.girl.body.position = new Vector2(0, -4);
                yield return null;
                Assert.That(game.BoyWins, Is.EqualTo(win));
                yield return new WaitForSecondsRealtime(.22f);
            }
            Assert.That(game.MatchComplete, Is.True);
            Assert.That(game.Running, Is.False);
            Assert.That(game.RoundNumber, Is.EqualTo(3));
            Assert.That(game.status.text, Does.Contain("WINS THE MATCH"));
            Assert.That(game.boy.TryAction(PlayerActions.ActionType.Punch), Is.False);
            game.Restart();
            Assert.That(game.MatchComplete, Is.False);
            Assert.That(game.BoyWins + game.GirlWins + game.CompletedRounds, Is.Zero);
            Assert.That(game.RoundNumber, Is.EqualTo(1));
            Assert.That(game.Running, Is.True);
        }

        [UnityTest] public IEnumerator DrawAdvancesWithoutAwardingWinAndRestartCancelsCountdown()
        {
            game.nextRoundDelay = .12f;
            game.boy.body.position = game.girl.body.position = new Vector2(0, -4);
            yield return null;
            Assert.That(game.BoyWins + game.GirlWins, Is.Zero);
            Assert.That(game.CompletedRounds, Is.EqualTo(1));
            Assert.That(game.BoyWinRate + game.GirlWinRate, Is.Zero);
            game.Restart();
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.RoundNumber, Is.EqualTo(1), "A manual restart must cancel the pending transition.");
            Assert.That(game.Running, Is.True);
            game.girl.body.position = new Vector2(0, -4);
            yield return null;
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.RoundNumber, Is.EqualTo(2));
            Assert.That(game.BoyWinRate, Is.EqualTo(50));
        }

        [UnityTest] public IEnumerator DamageUpdatesLifePercentageAndAttackUsesDistinctFrames()
        {
            game.boy.body.position = new Vector2(-.4f, .1f);
            game.girl.body.position = new Vector2(.4f, .1f);
            Assert.That(game.boy.punch.Length, Is.EqualTo(4));
            Assert.That(game.girl.push.Length, Is.EqualTo(4));
            game.boy.TryAction(PlayerActions.ActionType.Punch);
            game.boy.AdvanceAction(.1f);
            yield return null;
            Assert.That(game.girl.HealthPercent, Is.EqualTo(200f / 3).Within(.01f));
            Assert.That(game.girlHud.text, Does.Contain("LIFE 67%"));
            Assert.That(game.girlLifeBar.fillAmount, Is.EqualTo(2f / 3).Within(.01f));
            Assert.That(game.boy.visual.sprite, Is.EqualTo(game.boy.punch[1]));
            var extended = game.boy.punch[2];
            game.boy.AdvanceAction(.06f);
            yield return null;
            Assert.That(game.boy.visual.sprite, Is.EqualTo(extended));
            Assert.That(game.boy.fist.enabled || game.boy.palm.enabled, Is.False);
            Assert.That(game.girl.Health, Is.EqualTo(2), "One punch cannot damage on multiple animation frames.");
        }
    }
}
