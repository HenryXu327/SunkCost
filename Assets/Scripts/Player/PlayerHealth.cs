using DebugTool;
using UnityEngine;
using Monster;

namespace Player
{
    public class PlayerHealth : Health
    {
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private float damageThreshold = 20f;
        [SerializeField] private float smallShakeDuration = 0.05f;
        [SerializeField] private float smallShakeMagnitude = 0.1f;
        [SerializeField] private float bigShakeDuration = 0.15f;
        [SerializeField] private float bigShakeMagnitude = 0.2f;

        private bool isDead;

        private void Awake()
        {
            if (currentHealth <= 0f)
            {
                currentHealth = maxHealth;
            }
        }

        public override void TakeDamage(float damage)
        {
            if (isDead) return;

            if (cameraShake != null)
            {
                float shakeDuration = damage > damageThreshold? bigShakeDuration : smallShakeDuration;
                float shakeMagnitude = damage > damageThreshold? bigShakeMagnitude : smallShakeMagnitude;
                
                StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
            }
            
            Debug.Log("Player took damage: " + damage);

            base.TakeDamage(damage);
        }

        protected override void Die()
        {
            isDead = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            FindObjectOfType<CheatEndGame>().EndGame();
        }
    }
} 