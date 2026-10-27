using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BalanceWithMe.Tests
{
    public class TypographyTests
    {
        GameMenu menu;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            menu=Object.FindFirstObjectByType<GameMenu>();menu.SetOpen(true);Canvas.ForceUpdateCanvases();
        }
        [TearDown] public void Restore() { menu.SetOpen(false); }
        [Test] public void PixelFontIsReservedForSelectedHeadings()
        {
            foreach(var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                bool heading=text.name=="Title" || text.name=="Menu Title" || text.name=="Countdown Number" || text.name=="Countdown Caption" || text.name=="Winner Heading" || text.name=="Boy Name" || text.name=="Girl Name" || text.name=="Winning Player Name";
                if(heading)Assert.That(text.font.name,Is.EqualTo("Tiny5"),text.name);
                else Assert.That(text.font.name,Does.StartWith("AtkinsonHyperlegible"),text.name);
            }
        }
        [Test] public void MouseCloseButtonResumesWithoutStartingANewMatch()
        {
            var game=menu.game;
            var close=menu.panel.transform.Find("Controls Menu/Close Menu").GetComponent<Button>();
            var rect=(RectTransform)close.transform;
            Assert.That(rect.anchoredPosition.x,Is.GreaterThan(600));
            Assert.That(rect.anchoredPosition.y,Is.GreaterThan(350));
            Assert.That(close.IsInteractable(),Is.True);
            float remaining=game.RemainingSeconds;
            ExecuteEvents.Execute(close.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerClickHandler);
            Assert.That(menu.IsOpen,Is.False);
            Assert.That(game.MenuPaused,Is.False);
            Assert.That(game.RemainingSeconds,Is.EqualTo(remaining));
        }
        [UnityTest] public IEnumerator CloseButtonHighlightsRed()
        {
            var close=menu.panel.transform.Find("Controls Menu/Close Menu").GetComponent<Button>();
            EventSystem.current.SetSelectedGameObject(null);
            ExecuteEvents.Execute(close.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerEnterHandler);
            yield return new WaitForSecondsRealtime(.2f);
            var tint=close.targetGraphic.canvasRenderer.GetColor();
            Assert.That(tint.r,Is.GreaterThan(tint.g*1.5f));
            Assert.That(tint.r,Is.GreaterThan(tint.b*1.5f));
        }
        [Test] public void QuitRequiresConfirmationAndNoOrEscapeReturnsToPausedMenu()
        {
            float remaining=menu.game.RemainingSeconds;
            menu.quitButton.onClick.Invoke();
            Assert.That(menu.IsConfirmingQuit,Is.True);
            Assert.That(menu.quitDialog.alpha,Is.EqualTo(1));
            Assert.That(menu.quitDialog.blocksRaycasts,Is.True);
            Assert.That(menu.resumeButton.IsInteractable(),Is.False);
            Assert.That(menu.cancelQuitButton.IsInteractable(),Is.True);
            Assert.That(EventSystem.current.currentSelectedGameObject,Is.SameAs(menu.cancelQuitButton.gameObject));
            menu.cancelQuitButton.onClick.Invoke();
            Assert.That(menu.IsConfirmingQuit,Is.False);
            Assert.That(menu.quitDialog.alpha,Is.Zero);
            Assert.That(menu.IsOpen,Is.True);
            Assert.That(menu.game.MenuPaused,Is.True);
            Assert.That(menu.resumeButton.IsInteractable(),Is.True);
            Assert.That(menu.game.RemainingSeconds,Is.EqualTo(remaining));
            menu.Quit();menu.Toggle();
            Assert.That(menu.IsConfirmingQuit,Is.False);
            Assert.That(menu.IsOpen,Is.True);
        }
        [Test] public void EveryRoundClockFitsInsideItsFrameWithPadding()
        {
            var timer=menu.game.timer;
            var frame=(RectTransform)timer.transform.parent;
            Assert.That(timer.verticalOverflow,Is.EqualTo(VerticalWrapMode.Truncate));
            foreach(var round in new[]{1,2,3})
            foreach(var clock in new[]{"00:25","00:20","00:10","00:00"})
            {
                timer.text=$"ROUND {round} / 3\n{clock}";
                Assert.That(timer.preferredHeight,Is.LessThanOrEqualTo(timer.rectTransform.rect.height));
                Assert.That(timer.preferredWidth,Is.LessThanOrEqualTo(timer.rectTransform.rect.width));
                Assert.That(frame.rect.height-timer.preferredHeight,Is.GreaterThanOrEqualTo(16));
                Assert.That(frame.rect.width-timer.preferredWidth,Is.GreaterThanOrEqualTo(16));
            }
        }
        [UnityTest] public IEnumerator ReplayFaceRespondsToHoverPressAndClick()
        {
            var game=menu.game;menu.Resume();game.countdownStepDuration=0;game.nextRoundDelay=.05f;game.NewMatch();
            for(int round=0;round<3;round++)
            {
                game.girl.body.position=new Vector2(0,-4);yield return null;
                yield return new WaitForSecondsRealtime(.15f);
            }
            Assert.That(game.MatchComplete,Is.True);
            var button=game.playAgainButton;
            var events=EventSystem.current;events.SetSelectedGameObject(null);
            var pointer=new PointerEventData(events){button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerExitHandler);
            yield return new WaitForSecondsRealtime(.2f);
            var normal=button.targetGraphic.canvasRenderer.GetColor();
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerEnterHandler);
            yield return new WaitForSecondsRealtime(.2f);
            var hover=button.targetGraphic.canvasRenderer.GetColor();
            Assert.That(hover,Is.Not.EqualTo(normal),"The visible button face must change on hover.");
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(button.targetGraphic.canvasRenderer.GetColor(),Is.Not.EqualTo(hover));
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
            Assert.That(game.CompletedRounds,Is.Zero);
            Assert.That(game.MatchComplete,Is.False);
            Assert.That(game.winnerPopup.alpha,Is.Zero);
        }
        [Test] public void MenuInstructionsAndButtonCaptionsFitTheirAllocatedSpace()
        {
            foreach(var text in menu.panel.GetComponentsInChildren<Text>(true))
            {
                Assert.That(text.preferredHeight,Is.LessThanOrEqualTo(text.rectTransform.rect.height+1),text.name+" is vertically clipped.");
                Assert.That(text.preferredWidth,Is.LessThanOrEqualTo(text.rectTransform.rect.width+1),text.name+" unexpectedly wraps.");
            }
            foreach(var button in menu.panel.GetComponentsInChildren<Button>(true))
            {
                var text=button.GetComponentInChildren<Text>();
                Assert.That(text.font.name,Does.StartWith("AtkinsonHyperlegible"));
                Assert.That(((RectTransform)button.transform).rect.height,Is.GreaterThanOrEqualTo(44));
            }
        }
    }
}
