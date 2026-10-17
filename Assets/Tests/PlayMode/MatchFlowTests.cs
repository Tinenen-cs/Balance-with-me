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
            Assert.That(game.girlWinsText.text, Does.Contain("WINS 1"));
            Assert.That(game.boy.IsDefeated, Is.True);
            Assert.That(game.boyPortrait.sprite, Is.SameAs(game.boy.portraitDefeat));
            Assert.That(game.boy.defeat, Does.Contain(game.boy.visual.sprite));
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.Running, Is.True);
            Assert.That(game.RoundNumber, Is.EqualTo(2));
            Assert.That(game.GirlWins, Is.EqualTo(1));
            Assert.That(game.boy.HealthPercent, Is.EqualTo(100).Within(.01f));
            Assert.That(game.boy.body.position.x, Is.EqualTo(-2.6f).Within(.05f));
            Assert.That(game.board.simulated && game.girl.body.simulated, Is.True);
        }

        [UnityTest] public IEnumerator ThreeRoundsUseTotalScoreAndAutomaticallyBeginFreshMatch()
        {
            game.nextRoundDelay = .12f;
            game.matchResetDelay = .5f;
            for (int round = 1; round <= 3; round++)
            {
                // Boy wins the first two; Girl wins the last. The match winner must still be Boy.
                (round < 3 ? game.girl : game.boy).body.position = new Vector2(0, -4);
                yield return null;
                if (round < 3)
                {
                    Assert.That(game.MatchComplete, Is.False, "All three rounds must be played, even at 2-0.");
                    yield return new WaitForSecondsRealtime(.22f);
                    Assert.That(game.RoundNumber, Is.EqualTo(round + 1));
                }
            }
            Assert.That(game.MatchComplete, Is.True);
            Assert.That(game.Running, Is.False);
            Assert.That(game.RoundNumber, Is.EqualTo(3));
            Assert.That(game.status.text, Does.Contain("WINS THE MATCH"));
            Assert.That(game.Result, Is.EqualTo(BalanceGame.MatchResult.Boy));
            Assert.That(game.BoyWins, Is.EqualTo(2));
            Assert.That(game.GirlWins, Is.EqualTo(1));
            Assert.That(game.boy.HasWonRound && game.girl.IsDefeated, Is.True);
            Assert.That(game.boyPortrait.sprite, Is.SameAs(game.boy.portraitVictory));
            Assert.That(game.boy.TryAction(PlayerActions.ActionType.Punch), Is.False);
            yield return new WaitForSecondsRealtime(.6f);
            Assert.That(game.MatchComplete, Is.False);
            Assert.That(game.BoyWins + game.GirlWins + game.CompletedRounds, Is.Zero);
            Assert.That(game.RoundNumber, Is.EqualTo(1));
            Assert.That(game.Running, Is.True);
            Assert.That(game.boyPortrait.sprite, Is.SameAs(game.boy.portraitNormal));
            Assert.That(game.boy.HasWonRound || game.girl.IsDefeated, Is.False);
        }

        [UnityTest] public IEnumerator DrawAdvancesWithoutAwardingWinAndRestartCancelsCountdown()
        {
            game.nextRoundDelay = .12f;
            game.boy.body.position = game.girl.body.position = new Vector2(0, -4);
            yield return null;
            Assert.That(game.BoyWins + game.GirlWins, Is.Zero);
            Assert.That(game.CompletedRounds, Is.EqualTo(1));
            game.Restart();
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.RoundNumber, Is.EqualTo(2), "Restart after a counted result advances without duplicating the round.");
            Assert.That(game.Running, Is.True);
            game.girl.body.position = new Vector2(0, -4);
            yield return null;
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.RoundNumber, Is.EqualTo(3));
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
            Assert.That(game.girlLifeText.text, Does.Contain("LIFE 67%"));
            Assert.That(game.girlPortrait.sprite, Is.SameAs(game.girl.portraitHurt));
            Assert.That(game.girlLifeBar.fillAmount, Is.EqualTo(2f / 3).Within(.01f));
            Assert.That(game.boy.visual.sprite, Is.EqualTo(game.boy.punch[1]));
            var extended = game.boy.punch[2];
            game.boy.AdvanceAction(.06f);
            yield return null;
            Assert.That(game.boy.visual.sprite, Is.EqualTo(extended));
            Assert.That(game.boy.fist.enabled || game.boy.palm.enabled, Is.False);
            Assert.That(game.girl.Health, Is.EqualTo(2), "One punch cannot damage on multiple animation frames.");
        }

        [UnityTest] public IEnumerator ThreeDrawsEndAtThreeRoundsAndResetWithoutInput()
        {
            game.nextRoundDelay = .12f; game.matchResetDelay = .4f;
            for (int round = 1; round <= 3; round++)
            {
                game.boy.body.position = game.girl.body.position = new Vector2(0, -4);
                yield return null;
                if (round < 3) yield return new WaitForSecondsRealtime(.22f);
            }
            Assert.That(game.CompletedRounds, Is.EqualTo(3));
            Assert.That(game.Result, Is.EqualTo(BalanceGame.MatchResult.Draw));
            Assert.That(game.status.text, Does.Contain("MATCH DRAW"));
            yield return new WaitForSecondsRealtime(.5f);
            Assert.That(game.CompletedRounds, Is.Zero);
            Assert.That(game.RoundNumber, Is.EqualTo(1));
            Assert.That(game.Running, Is.True);
        }

        [UnityTest] public IEnumerator ZeroLifeShowsCryingBodyAndPortraitThenRestoresOnNextRound()
        {
            game.nextRoundDelay = 1;
            for (int hit = 0; hit < 3; hit++)
            {
                game.boy.body.position = new Vector2(-.4f, .1f);
                game.girl.body.position = new Vector2(.4f, .1f);
                game.boy.cooldown = 0;
                game.boy.TryAction(PlayerActions.ActionType.Punch);
                game.boy.AdvanceAction(.1f);
            }
            yield return null; yield return null;
            Assert.That(game.girl.HealthPercent, Is.Zero);
            Assert.That(game.girl.IsDefeated, Is.True);
            Assert.That(game.girl.defeat, Does.Contain(game.girl.visual.sprite));
            Assert.That(game.girlPortrait.sprite, Is.SameAs(game.girl.portraitDefeat));
            Assert.That(game.girlLifeText.text, Is.EqualTo("LIFE 0%"));
            Assert.That(game.girlLifeText.isActiveAndEnabled, Is.True, "Zero life must not hide the percentage label.");
            Assert.That(game.girlLifeText.transform.parent, Is.SameAs(game.girlLifeBar.transform.parent));
            Assert.That(game.boy.victory, Does.Contain(game.boy.visual.sprite));
            yield return new WaitForSeconds(.85f);
            Assert.That(game.sounds.LastCue, Is.EqualTo("win"));
            yield return new WaitForSeconds(.3f);
            Assert.That(game.girl.IsDefeated, Is.False);
            Assert.That(game.girlPortrait.sprite, Is.SameAs(game.girl.portraitNormal));
        }
    }
}
