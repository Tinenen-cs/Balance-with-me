using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BalanceWithMe.Tests
{
    public class CloudDriftTests
    {
        CloudDrift cloud;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            cloud = Object.FindFirstObjectByType<CloudDrift>();
            Assert.That(cloud, Is.Not.Null);
        }
        [Test] public void DriftUsesElapsedTimeAndPreservesHeight()
        {
            Vector3 origin = cloud.transform.position;
            cloud.Advance(1);
            Vector3 singleStep = cloud.transform.position;
            Assert.That(singleStep.x-origin.x, Is.EqualTo(cloud.speed).Within(.0001f));
            Assert.That(singleStep.y, Is.EqualTo(origin.y));
            cloud.transform.position=origin;
            for(int step=0;step<20;step++)cloud.Advance(.05f);
            Assert.That(cloud.transform.position.x, Is.EqualTo(singleStep.x).Within(.0001f));
            cloud.Advance(0);
            Assert.That(cloud.transform.position.x, Is.EqualTo(singleStep.x).Within(.0001f));
        }
        [Test] public void WrapStaysOffscreenAndPreservesOvershoot()
        {
            float halfView=cloud.view.orthographicSize*cloud.view.aspect;
            float extent=cloud.GetComponent<SpriteRenderer>().bounds.extents.x;
            float right=cloud.view.transform.position.x+halfView+extent+cloud.edgePadding;
            float left=cloud.view.transform.position.x-halfView-extent-cloud.edgePadding;
            cloud.transform.position=new Vector3(right-.01f,cloud.transform.position.y,0);
            cloud.Advance(.02f/cloud.speed);
            Assert.That(cloud.transform.position.x,Is.EqualTo(left+.01f).Within(.0001f));
            Assert.That(cloud.GetComponent<SpriteRenderer>().bounds.max.x,Is.LessThan(cloud.view.transform.position.x-halfView));
            float position=cloud.transform.position.x;
            cloud.Advance(3*(right-left)/cloud.speed);
            Assert.That(cloud.transform.position.x,Is.EqualTo(position).Within(.0001f));
        }
        [UnityTest] public IEnumerator CloudsMoveSlowlyDuringPlayAndAfterTheRound()
        {
            var clouds=Object.FindObjectsByType<CloudDrift>(FindObjectsSortMode.None);
            Assert.That(clouds.Length,Is.EqualTo(5));
            foreach(var item in clouds)
            {
                var renderer=item.GetComponent<SpriteRenderer>();
                Assert.That(renderer.sprite,Is.Not.Null);
                Assert.That(renderer.sortingOrder,Is.GreaterThan(-10).And.LessThan(1));
                Assert.That(item.speed,Is.InRange(.05f,.12f));
            }
            float start=cloud.transform.position.x;
            yield return new WaitForSeconds(.25f);
            Assert.That(cloud.transform.position.x-start,Is.GreaterThan(0).And.LessThan(.05f));
            var game=Object.FindFirstObjectByType<BalanceGame>();game.boy.body.position=new Vector2(0,-4);
            yield return null;yield return null;
            Assert.That(game.Running,Is.False);
            start=cloud.transform.position.x;
            yield return new WaitForSeconds(.15f);
            Assert.That(cloud.transform.position.x,Is.GreaterThan(start));
        }
    }
}
