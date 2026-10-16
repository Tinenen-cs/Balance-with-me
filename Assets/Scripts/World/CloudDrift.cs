using UnityEngine;

namespace BalanceWithMe
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CloudDrift : MonoBehaviour
    {
        public Camera view;
        [Min(0)] public float speed = .08f;
        [Min(0)] public float edgePadding = .5f;
        SpriteRenderer cloud;

        void Awake() => cloud = GetComponent<SpriteRenderer>();
        void Update() => Advance(Time.deltaTime);

        public void Advance(float dt)
        {
            if (view == null || dt <= 0 || speed <= 0) return;
            if (cloud == null) cloud = GetComponent<SpriteRenderer>();
            float halfView = view.orthographicSize * view.aspect;
            float margin = cloud.bounds.extents.x + edgePadding;
            float left = view.transform.position.x - halfView - margin;
            float right = view.transform.position.x + halfView + margin;
            Vector3 position = transform.position;
            // Wrap only when the entire cloud has passed beyond the camera, retaining overshoot.
            position.x = left + Mathf.Repeat(position.x - left + speed * dt, right - left);
            transform.position = position;
        }
    }
}
