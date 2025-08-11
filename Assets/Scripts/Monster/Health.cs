using UnityEngine;

namespace Monster
{
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        
        public bool isRobot = false;

        public virtual void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            if (isRobot)
            {
                Destroy(gameObject.transform.parent.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}