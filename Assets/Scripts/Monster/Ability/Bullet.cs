using System.Collections;
using UnityEngine;
using Utility;

namespace Monster.Ability
{
    public class Bullet : MonoBehaviour
    {
        private float damage = 5f;
        private float speed = 1f;
        public float lifeTime = 5f;
        public string targetTag = "Player";
        
        private bool hasHit = false;
        private GameObject shooter; // 发射者
        private Rigidbody bulletRigidbody;
        
        private void Awake()
        {
            bulletRigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            hasHit = false;
            StartCoroutine(DestroyBullet(lifeTime));

            if (bulletRigidbody != null)
            {
                bulletRigidbody.velocity = transform.forward * speed;
            }
        }
        
        // 设置发射者，忽略与发射者的碰撞
        public void SetParams(GameObject shooterObj, float bulletDamage, float bulletSpeed)
        {
            shooter = shooterObj;
            damage = bulletDamage;
            speed = bulletSpeed;
        }
        
        private IEnumerator DestroyBullet(float time)
        {
            yield return new WaitForSeconds(time);
            
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;
            
            // 忽略与发射者的碰撞
            if (shooter != null && (other.gameObject == shooter))
            {
                return;
            }
            
            hasHit = true;
                
            // 造成伤害
            var targetHealth = other.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"子弹击中{targetHealth.name}，造成 {damage} 点伤害");
            }
                
            
                
            ObjectPoolManager.ReturnObjectToPool(gameObject);
            
            // if (other.CompareTag(targetTag))
            // {
            //     hasHit = true;
            //     
            //     // 造成伤害
            //     var targetHealth = other.GetComponent<Health>();
            //     if (targetHealth != null)
            //     {
            //         targetHealth.TakeDamage(damage);
            //     }
            //     
            //     Debug.Log($"子弹击中目标，造成 {damage} 点伤害");
            //     
            //     ObjectPoolManager.ReturnObjectToPool(gameObject);
            // }
            // // 击中其他物体也销毁子弹（如墙壁等）
            // else 
            // {
            //     // Debug.Log($"子弹击中 {other.name}，销毁");
            //     hasHit = true;
            //     ObjectPoolManager.ReturnObjectToPool(gameObject);
            // }
        }
    }
} 