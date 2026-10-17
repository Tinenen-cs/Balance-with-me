using UnityEngine;

namespace BalanceWithMe
{
    public class WindmillRotor : MonoBehaviour
    {
        public float degreesPerSecond = 12;
        void Update() => Advance(Time.deltaTime);
        public void Advance(float dt)
        {
            if (dt <= 0) return;
            transform.Rotate(0, 0, -degreesPerSecond * dt);
        }
    }
}
