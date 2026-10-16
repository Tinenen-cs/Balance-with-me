using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BalanceWithMe
{
    public class BalanceGame : MonoBehaviour
    {
        public PlayerActions boy, girl;
        public Rigidbody2D board;
        public Text status, health;
        public bool Running { get; private set; }
        float elapsed;
        void Start() => Restart();
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) Restart();
            if (Running)
            {
                elapsed += Time.deltaTime;
                bool boyLost = boy.Health <= 0 || boy.body.position.y < -3;
                bool girlLost = girl.Health <= 0 || girl.body.position.y < -3;
                if (boyLost || girlLost)
                {
                    Running = false;
                    status.text = boyLost && girlLost ? "DRAW — R to restart" : (boyLost ? "GIRL WINS" : "BOY WINS") + " — R to restart";
                }
                else status.text = $"Stay on the plank • Face your opponent • {elapsed:0.0}s";
            }
            health.text = $"BOY  {boy.Health}/3     |     GIRL  {girl.Health}/3";
        }
        void FixedUpdate()
        {
            // Gentle centering prevents the empty plank from remaining tilted forever.
            if (Running) board.AddTorque(-Mathf.DeltaAngle(0, board.rotation) * .12f);
        }
        public void Restart()
        {
            board.rotation = 0;
            board.angularVelocity = 0;
            boy.ResetPlayer(new Vector2(-2.6f, .24f));
            girl.ResetPlayer(new Vector2(2.6f, .24f));
            elapsed = 0;
            Running = true;
            Physics2D.SyncTransforms();
        }
    }
}
