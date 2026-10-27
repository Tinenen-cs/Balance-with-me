using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

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
                bool heading=text.name=="Title" || text.name=="Menu Title" || text.name=="Countdown Number" || text.name=="Countdown Caption" || text.name=="Winner Heading";
                if(heading)Assert.That(text.font.name,Is.EqualTo("Tiny5"),text.name);
                else Assert.That(text.font.name,Does.StartWith("AtkinsonHyperlegible"),text.name);
            }
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
