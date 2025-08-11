using UnityEngine;
using Utility;

namespace Monster.Ability
{
    public class RobotBulletAttackAbility : BulletAttackAbility
    {
        public Transform fireSourcePoint;
        
        public AudioSource audioSource;

        public override void Update()
        {
            
            currentTarget = findTarget.currentTarget;
            
            // 检测到目标直接攻击
            if (CanAttack())
            {
                PerformAttack();
            }
        }

        public override void PerformAttack()
        {
            Vector3 shootDirection = fireSourcePoint.forward;
            
            // 在发射者前方一点距离生成子弹，避免直接碰撞
            Vector3 spawnPosition = fireSourcePoint.position + shootDirection * 0.2f;
            
            GameObject bulletObj = ObjectPoolManager.SpawnObject(bulletPrefab, spawnPosition, Quaternion.LookRotation(shootDirection), ObjectPoolManager.PoolType.GameObject);
            
            // 设置发射者
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetParams(gameObject, bulletDamage, bulletSpeed);
            }
            
            // 音效
            if (audioSource != null)
            {
                audioSource.PlayOneShot(audioSource.clip, 0.1f);
            }
            
            lastAttackTime = Time.time;
        }
    }
}