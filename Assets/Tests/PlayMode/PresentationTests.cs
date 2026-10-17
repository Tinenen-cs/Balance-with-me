using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BalanceWithMe.Tests
{
    public class PresentationTests
    {
        BalanceGame game;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Main"); yield return null;
            game = Object.FindFirstObjectByType<BalanceGame>();
            game.countdownStepDuration = 0; game.Restart();
        }

        [Test] public void ReadyCountdownHasOnlyOutlinedTextAndNoPanelGraphics()
        {
            Assert.That(game.readyPopup.GetComponentsInChildren<Image>(true).Length, Is.Zero);
            foreach (var text in new[] { game.readyNumber, game.readyCaption })
            {
                Assert.That(text.GetComponent<Outline>().effectColor.a, Is.EqualTo(1));
                Assert.That(text.GetComponent<Shadow>(), Is.Not.Null);
                Assert.That(text.color.r, Is.GreaterThan(.9f));
            }
        }
        [UnityTest] public IEnumerator RoundPopupShowsCorrectHeadNameAndNextRoundThenHidesForReady()
        {
            game.nextRoundDelay = .8f;
            foreach (bool girlWins in new[] { false, true })
            {
                (girlWins ? game.boy : game.girl).body.position = new Vector2(0,-4);
                yield return new WaitForSecondsRealtime(.35f);
                Assert.That(game.statusPopup.alpha, Is.Zero);
                foreach (var backdrop in game.winnerBackdropImages) Assert.That(backdrop.enabled, Is.False);
                Assert.That(game.winnerPopup.alpha, Is.EqualTo(1));
                Assert.That(((RectTransform)game.winnerPopup.transform).anchoredPosition, Is.EqualTo(new Vector2(0,200)));
                Assert.That(game.winnerName.text, Is.EqualTo(girlWins ? game.girlName : game.boyName));
                Assert.That(game.winnerHead.sprite, Is.SameAs(girlWins ? game.girl.portraitVictory : game.boy.portraitVictory));
                Assert.That(game.nextRoundText.text, Does.Contain("NEXT ROUND IN"));
                Assert.That(game.nextRoundText.text, Does.Contain($"ROUND {game.CompletedRounds + 1} OF 3"));
                Assert.That(game.playAgainButton.gameObject.activeSelf, Is.False);
                game.countdownStepDuration = 1;
                yield return new WaitForSecondsRealtime(.55f);
                Assert.That(game.IsCountingDown, Is.True);
                Assert.That(game.winnerPopup.alpha, Is.Zero);
                game.AdvanceReadyCountdown(3);
            }
        }
        [UnityTest] public IEnumerator WinStarsAwardOnePerRoundDrawsAwardNoneAndReplayClearsThem()
        {
            game.nextRoundDelay = 10;
            foreach (var stars in new[] { game.boyWinStars, game.girlWinStars })
            {
                Assert.That(stars.Length, Is.EqualTo(3));
                foreach (var star in stars) { Assert.That(star.sprite, Is.Not.Null); Assert.That(star.color.a, Is.LessThan(.5f)); }
            }
            game.girl.body.position = new Vector2(0,-4); yield return null;
            Assert.That(game.boyWinStars[0].color, Is.EqualTo(Color.white));
            Assert.That(game.boyWinStars[1].color.a, Is.LessThan(.5f));
            game.Restart();
            game.boy.body.position = game.girl.body.position = new Vector2(0,-4); yield return null;
            Assert.That(game.BoyWins, Is.EqualTo(1));
            Assert.That(game.boyWinStars[1].color.a, Is.LessThan(.5f));
            game.Restart(); game.girl.body.position = new Vector2(0,-4); yield return null;
            Assert.That(game.boyWinStars[1].color, Is.EqualTo(Color.white));
            Assert.That(game.boyWinStars[2].color.a, Is.LessThan(.5f));
            foreach (var star in game.girlWinStars) Assert.That(star.color.a, Is.LessThan(.5f));
            game.PlayAgain();
            foreach (var star in game.boyWinStars) Assert.That(star.color.a, Is.LessThan(.5f));
        }
        [UnityTest] public IEnumerator CountdownAndRoundResultContentStayAbovePlankInBothCanvasModes()
        {
            game.countdownStepDuration = 1; game.Restart();
            var canvas = game.readyNumber.canvas;
            foreach (var mode in new[] { RenderMode.ScreenSpaceOverlay, RenderMode.ScreenSpaceCamera })
            {
                canvas.renderMode = mode; canvas.worldCamera = Camera.main; canvas.planeDistance = 5;
                Canvas.ForceUpdateCanvases();
                foreach (var label in new[] { game.readyNumber, game.readyCaption }) AssertAbovePlank(label.rectTransform, canvas);
            }
            game.AdvanceReadyCountdown(3); game.girl.body.position = new Vector2(0,-4);
            yield return new WaitForSecondsRealtime(.35f);
            foreach (var mode in new[] { RenderMode.ScreenSpaceOverlay, RenderMode.ScreenSpaceCamera })
            {
                canvas.renderMode = mode; Canvas.ForceUpdateCanvases();
                AssertAbovePlank(game.winnerHead.rectTransform, canvas);
                foreach (var text in new[] { game.winnerHeading, game.winnerName, game.nextRoundText }) AssertAbovePlank(text.rectTransform, canvas);
            }
        }
        void AssertAbovePlank(RectTransform rect, Canvas canvas)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            float bottom = RectTransformUtility.WorldToScreenPoint(camera,corners[0]).y;
            float plank = Camera.main.WorldToScreenPoint(game.board.transform.position + Vector3.up * .2f).y;
            Assert.That(bottom, Is.GreaterThan(plank + 8), rect.name + " must clear the plank.");
        }
        [UnityTest] public IEnumerator VillageMotionContinuesDuringResultsWithoutGrowingPools()
        {
            var rotor = Object.FindFirstObjectByType<WindmillRotor>();
            var smoke = Object.FindObjectsByType<ChimneySmoke>(FindObjectsSortMode.None);
            Assert.That(rotor, Is.Not.Null); Assert.That(smoke.Length, Is.EqualTo(2));
            float angle = rotor.transform.eulerAngles.z;
            rotor.Advance(.5f);
            Assert.That(Mathf.DeltaAngle(angle, rotor.transform.eulerAngles.z), Is.EqualTo(-6).Within(.001f));
            foreach (var chimney in smoke)
            {
                Assert.That(chimney.puffs.Length, Is.EqualTo(6));
                Vector3 before = chimney.puffs[0].transform.localPosition;
                chimney.Advance(.25f);
                Assert.That(chimney.puffs[0].transform.localPosition, Is.Not.EqualTo(before));
                for (int i = 0; i < 100; i++) chimney.Advance(.1f);
                Assert.That(chimney.GetComponentsInChildren<SpriteRenderer>().Length, Is.EqualTo(6));
                foreach (var puff in chimney.puffs) Assert.That(puff.color.a, Is.InRange(0, .78f));
            }
            game.girl.body.position = new Vector2(0, -4); yield return null;
            Assert.That(game.Running, Is.False);
            angle = rotor.transform.eulerAngles.z;
            Vector3 smokePosition = smoke[0].puffs[0].transform.position;
            yield return new WaitForSeconds(.2f);
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(angle, rotor.transform.eulerAngles.z)), Is.GreaterThan(1));
            Assert.That(smoke[0].puffs[0].transform.position, Is.Not.EqualTo(smokePosition));
        }

        [UnityTest] public IEnumerator HeadOnlyPortraitsAndAboveHeadReactionsWorkInBothCanvasModes()
        {
            Assert.That(game.boyPortrait.transform.parent.GetComponent<Image>(), Is.Null);
            Assert.That(game.girlPortrait.transform.parent.GetComponent<Image>(), Is.Null);
            game.girl.body.position = new Vector2(0, -4); yield return null; yield return null;
            var canvas = game.boy.emoteLabel.canvas;
            Assert.That(canvas.sortingOrder, Is.GreaterThan(10), "Winner UI must render above the plank and world particles.");
            foreach (var mode in new[] { RenderMode.ScreenSpaceOverlay, RenderMode.ScreenSpaceCamera })
            {
                canvas.renderMode = mode; canvas.worldCamera = Camera.main; canvas.planeDistance = 5;
                Canvas.ForceUpdateCanvases();
                foreach (var player in new[] { game.boy, game.girl })
                {
                    player.RefreshEmoteLabel(); Canvas.ForceUpdateCanvases();
                    var corners = new Vector3[4]; player.emoteLabel.rectTransform.GetWorldCorners(corners);
                    var uiCamera = mode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                    float labelBottom = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]).y;
                    float headTop = Camera.main.WorldToScreenPoint(new Vector3(player.transform.position.x, player.visual.bounds.max.y, 0)).y;
                    Assert.That(labelBottom, Is.GreaterThan(headTop), "The entire reaction label must be above the visible character.");
                    Assert.That(game.statusPopup.alpha, Is.Zero, "Round results must not leave the old top banner visible.");
                }
            }
        }

        [UnityTest] public IEnumerator EitherWinnerGetsNamedHeadAnnouncementAndReplayButton()
        {
            game.nextRoundDelay = .1f;
            foreach (bool girlWins in new[] { false, true })
            {
                for (int round = 1; round <= 3; round++)
                {
                    (girlWins ? game.boy : game.girl).body.position = new Vector2(0, -4);
                    yield return null;
                    if (round < 3)
                    {
                        Assert.That(game.winnerHeading.text, Is.EqualTo("ROUND WINNER"));
                        Assert.That(game.playAgainButton.gameObject.activeSelf, Is.False);
                        yield return new WaitForSecondsRealtime(.2f);
                    }
                }
                yield return new WaitForSecondsRealtime(.35f);
                Assert.That(game.winnerPopup.alpha, Is.EqualTo(1).Within(.01f));
                foreach (var backdrop in game.winnerBackdropImages) Assert.That(backdrop.enabled, Is.True);
                Assert.That(game.winnerName.text, Is.EqualTo(girlWins ? game.girlName : game.boyName));
                Assert.That(game.winnerHead.sprite, Is.SameAs(girlWins ? game.girl.portraitVictory : game.boy.portraitVictory));
                foreach (var star in girlWins ? game.girlWinStars : game.boyWinStars) Assert.That(star.color, Is.EqualTo(Color.white));
                Assert.That(game.roundClockPopup.alpha, Is.Zero);
                yield return new WaitForSecondsRealtime(.6f);
                Assert.That(game.MatchComplete, Is.True);
                Assert.That(game.playAgainButton.IsInteractable(), Is.True);
                game.playAgainButton.onClick.Invoke();
                Assert.That(game.winnerPopup.alpha, Is.Zero);
                Assert.That(game.Running, Is.True);
                Assert.That(game.CompletedRounds, Is.Zero);
            }
        }
    }
}
