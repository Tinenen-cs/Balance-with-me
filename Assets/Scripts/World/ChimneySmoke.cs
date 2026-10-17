using UnityEngine;

namespace BalanceWithMe
{
    public class ChimneySmoke : MonoBehaviour
    {
        public SpriteRenderer[] puffs;
        [Min(.1f)] public float cycleDuration = 4.5f;
        public float riseHeight = 1.05f, windDrift = .22f, phaseOffset;
        float elapsed;
        void Start() => Advance(0);
        void Update() => Advance(Time.deltaTime);
        public void Advance(float dt)
        {
            if (puffs == null || dt < 0) return;
            float duration = Mathf.Max(.1f, cycleDuration);
            elapsed = Mathf.Repeat(elapsed + dt, duration);
            for (int i = 0; i < puffs.Length; i++)
            {
                var puff = puffs[i];
                if (puff == null || puff.sprite == null) continue;
                float progress = Mathf.Repeat(elapsed / duration + i / (float)puffs.Length + phaseOffset, 1);
                puff.transform.localPosition = new Vector3(windDrift * progress + Mathf.Sin(progress * 8 + i) * .025f, .04f + progress * riseHeight, 0);
                float width = Mathf.Lerp(.13f, .43f, progress);
                puff.transform.localScale = Vector3.one * (width / puff.sprite.bounds.size.x);
                puff.color = new Color(.9f, .96f, 1, Mathf.Sin(progress * Mathf.PI) * .78f);
            }
        }
    }
}
