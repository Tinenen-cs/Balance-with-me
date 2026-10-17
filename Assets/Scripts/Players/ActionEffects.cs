using System;
using UnityEngine;

namespace BalanceWithMe
{
    [RequireComponent(typeof(PlayerActions))]
    public class ActionEffects : MonoBehaviour
    {
        public enum Kind { None, Step, Jump, Land, Punch, Push, Hit, Shove, Happy, Sad, Victory, Fail, Spawn }
        public PlayerActions player;
        public Sprite pixelSprite;
        public Material material;
        [Range(32, 128)] public int capacity = 64;
        public Kind LastEffect { get; private set; }
        public event Action<Kind> EffectEmitted;
        struct Pixel
        {
            public SpriteRenderer renderer;
            public Vector2 position, velocity, size;
            public Color color;
            public float age, life, gravity, angle, spin;
        }
        Pixel[] pixels;
        Transform root;
        int next;
        public int ActiveParticleCount
        {
            get
            {
                int count = 0;
                if (pixels != null) foreach (var pixel in pixels) if (pixel.renderer != null && pixel.renderer.enabled) count++;
                return count;
            }
        }
        void EnsurePool()
        {
            if (pixels != null) return;
            root = new GameObject(name + " Action Pixels").transform;
            pixels = new Pixel[Mathf.Clamp(capacity, 32, 128)];
            for (int i = 0; i < pixels.Length; i++)
            {
                var renderer = new GameObject("Effect Pixel " + (i+1)).AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(root, false);
                renderer.sprite = pixelSprite;
                renderer.sharedMaterial = material;
                renderer.sortingOrder = 10;
                renderer.enabled = false;
                pixels[i].renderer = renderer;
            }
        }
        public void Play(Kind kind, float direction = 0)
        {
            if (!Application.isPlaying || !isActiveAndEnabled || kind == Kind.None || player == null || pixelSprite == null || material == null) return;
            if (kind == Kind.Punch || kind == Kind.Push)
            {
                LastEffect = kind;
                EffectEmitted?.Invoke(kind);
                return;
            }
            EnsurePool();
            if (direction == 0) direction = player.Direction;
            Vector2 origin = player.body.position;
            bool feet = kind == Kind.Step || kind == Kind.Jump || kind == Kind.Land || kind == Kind.Spawn;
            bool emote = kind == Kind.Happy || kind == Kind.Sad || kind == Kind.Victory || kind == Kind.Fail;
            origin += Vector2.up * (feet ? .08f : emote ? 1.5f : .95f);
            if (kind == Kind.Punch || kind == Kind.Push) origin.x += direction * .6f;
            int count = kind == Kind.Step ? 3 : kind == Kind.Victory ? 24 : kind == Kind.Hit ? 12 : kind == Kind.Land ? 10 : 8;
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new Vector2(UnityEngine.Random.Range(-.16f,.16f), UnityEngine.Random.Range(0,.12f));
                Vector2 velocity = new Vector2(UnityEngine.Random.Range(-.8f,.8f), UnityEngine.Random.Range(.3f,1));
                Vector2 size = Vector2.one * UnityEngine.Random.Range(.06f,.11f);
                Color color = new Color(.78f,.62f,.36f);
                float life = .35f, gravity = 1.8f, angle = 0, spin = 0;
                switch (kind)
                {
                    case Kind.Step:
                        velocity.x = -direction * UnityEngine.Random.Range(.2f,.7f); life = .25f; break;
                    case Kind.Jump:
                        velocity *= 1.4f; break;
                    case Kind.Land:
                        velocity.x *= 2; velocity.y *= .5f; break;
                    case Kind.Spawn:
                        color = new Color(.55f,.8f,.3f); break;
                    case Kind.Punch:
                        float arc = Mathf.Lerp(-65,65,i/(float)(count-1)) * Mathf.Deg2Rad;
                        offset = new Vector2(direction * Mathf.Cos(arc) * .28f, Mathf.Sin(arc) * .38f);
                        velocity = new Vector2(direction * 1.8f, Mathf.Sin(arc) * .5f);
                        size = new Vector2(.15f,.045f); angle = direction * arc * Mathf.Rad2Deg;
                        color = i%2 == 0 ? new Color(1,.69f,.22f) : new Color(1,.94f,.66f);
                        life = .18f; gravity = 0; break;
                    case Kind.Push:
                        offset.x = direction * i * .06f; offset.y = UnityEngine.Random.Range(-.25f,.25f);
                        velocity = new Vector2(direction * UnityEngine.Random.Range(1.6f,2.5f), .05f);
                        size = new Vector2(.18f,.04f); color = new Color(.65f,.95f,.77f); life = .25f; gravity = 0; break;
                    case Kind.Hit:
                        velocity = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(1.5f,2.6f);
                        offset = Vector2.zero; size = new Vector2(.12f,.045f); angle = Mathf.Atan2(velocity.y,velocity.x)*Mathf.Rad2Deg;
                        color = i%2 == 0 ? new Color(1,.86f,.32f) : new Color(1,.96f,.8f); life = .3f; gravity = .3f; break;
                    case Kind.Shove:
                        velocity = new Vector2(direction * UnityEngine.Random.Range(.7f,1.4f), .3f);
                        color = new Color(.68f,.88f,.62f); life = .25f; break;
                    case Kind.Happy:
                        offset.x *= 2; color = new Color(1,.87f,.32f); angle = 45; gravity = 0; life = .6f; break;
                    case Kind.Sad:
                        offset.x *= 2; color = new Color(.42f,.73f,1); size = new Vector2(.045f,.1f);
                        velocity *= .3f; velocity.y = -.15f; gravity = 2; life = .55f; break;
                    case Kind.Victory:
                        velocity = new Vector2(UnityEngine.Random.Range(-1.6f,1.6f), UnityEngine.Random.Range(1,2.5f));
                        color = i%3 == 0 ? new Color(1,.83f,.25f) : i%3 == 1 ? new Color(.5f,.85f,.35f) : new Color(1,.55f,.4f);
                        size = new Vector2(.06f,.11f); spin = UnityEngine.Random.Range(-240f,240f); life = .9f; gravity = 3; break;
                    case Kind.Fail:
                        color = new Color(.75f,.45f,.3f); life = .5f; break;
                }
                ref Pixel pixel = ref pixels[next]; next = (next+1)%pixels.Length;
                pixel.position = origin + offset; pixel.velocity = velocity; pixel.size = size; pixel.color = color;
                pixel.age = 0; pixel.life = life; pixel.gravity = gravity; pixel.angle = angle; pixel.spin = spin;
                pixel.renderer.enabled = true;
                Render(ref pixel);
            }
            LastEffect = kind;
            EffectEmitted?.Invoke(kind);
        }
        void Update()
        {
            if (pixels == null) return;
            float dt = Time.deltaTime;
            for (int i = 0; i < pixels.Length; i++)
            {
                ref Pixel pixel = ref pixels[i];
                if (!pixel.renderer.enabled) continue;
                pixel.age += dt;
                if (pixel.age >= pixel.life) { pixel.renderer.enabled = false; continue; }
                pixel.position += pixel.velocity * dt + Vector2.down * (.5f * pixel.gravity * dt * dt);
                pixel.velocity.y -= pixel.gravity * dt;
                pixel.angle += pixel.spin * dt;
                Render(ref pixel);
            }
        }
        void Render(ref Pixel pixel)
        {
            float progress = pixel.age / pixel.life;
            pixel.renderer.transform.position = pixel.position;
            pixel.renderer.transform.rotation = Quaternion.Euler(0,0,pixel.angle);
            pixel.renderer.transform.localScale = new Vector3(pixel.size.x/pixelSprite.bounds.size.x, pixel.size.y/pixelSprite.bounds.size.y,1) * (1-.3f*progress);
            pixel.renderer.color = new Color(pixel.color.r,pixel.color.g,pixel.color.b,1-progress);
        }
        public void Clear()
        {
            if (pixels != null) for (int i = 0; i < pixels.Length; i++) if (pixels[i].renderer != null) pixels[i].renderer.enabled = false;
            next = 0; LastEffect = Kind.None;
        }
        void OnDisable() => Clear();
        void OnDestroy() { if (root != null) Destroy(root.gameObject); }
    }
}
