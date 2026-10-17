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
                    var labelBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, player.emoteLabel.transform);
                    var noticeBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, game.statusPopup.transform);
                    Assert.That(labelBounds.max.y, Is.LessThan(noticeBounds.min.y), "The result notice must leave room for the reaction text.");
                }
            }
        }

        [UnityTest] public IEnumerator EitherWinnerGetsNamedHeadAnnouncementBeforeAutomaticReset()
        {
            game.nextRoundDelay = .1f; game.matchResetDelay = .8f;
            foreach (bool girlWins in new[] { false, true })
            {
                for (int round = 1; round <= 3; round++)
                {
                    (girlWins ? game.boy : game.girl).body.position = new Vector2(0, -4);
                    yield return null;
                    if (round < 3)
                    {
                        Assert.That(game.winnerPopup.alpha, Is.Zero);
                        yield return new WaitForSecondsRealtime(.2f);
                    }
                }
                yield return new WaitForSecondsRealtime(.35f);
                Assert.That(game.winnerPopup.alpha, Is.EqualTo(1).Within(.01f));
                Assert.That(game.winnerName.text, Is.EqualTo(girlWins ? game.girlName : game.boyName));
                Assert.That(game.winnerHead.sprite, Is.SameAs(girlWins ? game.girl.portraitVictory : game.boy.portraitVictory));
                Assert.That(game.roundClockPopup.alpha, Is.Zero);
                yield return new WaitForSecondsRealtime(.6f);
                Assert.That(game.winnerPopup.alpha, Is.Zero);
                Assert.That(game.Running, Is.True);
                Assert.That(game.CompletedRounds, Is.Zero);
            }
        }
    }
}
