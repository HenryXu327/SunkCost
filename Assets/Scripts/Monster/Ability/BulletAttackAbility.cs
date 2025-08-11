using UnityEngine;
using System.Collections;
using Utility;

namespace Monster.Ability
{
    /// <summary>
    /// 挂载到怪物身上，赋予其远程攻击能力
    /// </summary>
    public class BulletAttackAbility : MonoBehaviour
    {
        public FindTarget findTarget; 
        public GameObject bulletPrefab; 
        public float bulletDamage = 10f; 
        public float bulletSpeed = 10f; 
        public float attackCooldown = 3f; 
        public float detectionRange = 12f; 
        
        public Transform currentTarget;
        public float lastAttackTime;

        public virtual void Update()
        {
            currentTarget = findTarget.currentTarget;
            
            // 检测到目标直接攻击
            if (CanAttack())
            {
                PerformAttack();
            }
        }
        
        public void TriggerAttack()
        {
            if (CanAttack())
            {
                PerformAttack();
            }
        }
        
        public bool CanAttack()
        {
            return currentTarget != null && 
                   Time.time - lastAttackTime >= attackCooldown &&
                   bulletPrefab != null;
        }
        
        public virtual void PerformAttack()
        {
            Vector3 shootDirection = (currentTarget.position - transform.position).normalized;
            
            // 在发射者前方一点距离生成子弹，避免直接碰撞
            Vector3 spawnPosition = transform.position + shootDirection * 0.5f;
            
            GameObject bulletObj = ObjectPoolManager.SpawnObject(bulletPrefab, spawnPosition, Quaternion.LookRotation(shootDirection), ObjectPoolManager.PoolType.GameObject);
            
            // 设置发射者
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetParams(gameObject, bulletDamage, bulletSpeed);
            }
            
            lastAttackTime = Time.time;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.position);
                Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
            }
        }
    }
}