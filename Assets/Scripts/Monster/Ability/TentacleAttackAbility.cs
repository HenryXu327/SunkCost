using System;
using UnityEngine;
using System.Collections;
using Utility;
using Random = UnityEngine.Random;

namespace Monster.Ability
{
    /// <summary>
    /// 挂载到怪物上，赋予其触手攻击能力
    /// </summary>
    public class TentacleAttackAbility : MonoBehaviour
    {
        public FindTarget findTarget;
        public GameObject tentaclePrefab; 
        public float attackCooldown = 3f; 
        public int maxSimultaneousTentacles = 3;
        
        public bool autoAttack = true;
        public float autoAttackInterval = 2f;
        
        private Transform currentTarget;
        private float lastAttackTime;
        private int activeTentacleCount = 0;
        
        private void Start()
        {
            // 开启自动攻击
            if (autoAttack)
            {
                StartCoroutine(AutoAttackCoroutine());
            }
        }

        private void Update()
        {
            currentTarget = findTarget.currentTarget;
        }
        
        public void TriggerAttack()
        {
            if (CanAttack())
            {
                PerformAttack();
            }
        }
        
        private bool CanAttack()
        {
            return currentTarget != null && 
                   Time.time - lastAttackTime >= attackCooldown &&
                   activeTentacleCount < maxSimultaneousTentacles &&
                   tentaclePrefab != null;
        }
        
        private void PerformAttack()
        {
            Vector3 attackOffset = Random.onUnitSphere.normalized * 0.5f;
            // GameObject tentacleObj = Instantiate(tentaclePrefab, transform.position + attackOffset, transform.rotation);
            GameObject tentacleObj = ObjectPoolManager.SpawnObject(tentaclePrefab, transform.position + attackOffset,
                transform.rotation, ObjectPoolManager.PoolType.GameObject);
            
            
            AttackTentacle tentacle = tentacleObj.GetComponent<AttackTentacle>();
            
            if (tentacle != null)
            {
                tentacle.Attack(currentTarget);
                
                activeTentacleCount++;
                
                StartCoroutine(MonitorTentacle(tentacleObj));
            }
            
            lastAttackTime = Time.time;
        }



        private IEnumerator MonitorTentacle(GameObject tentacleObj)
        {
            // 等待触手被销毁
            while (tentacleObj.activeSelf == true)
            {
                yield return null;
            }
            
            activeTentacleCount = Mathf.Max(0, activeTentacleCount - 1);
        }
        
        private IEnumerator AutoAttackCoroutine()
        {
            while (autoAttack)
            {
                yield return new WaitForSeconds(autoAttackInterval);
                
                if (CanAttack())
                {
                    PerformAttack();
                }
            }
        }
        
        public void StopAutoAttack()
        {
            autoAttack = false;
            StopCoroutine(AutoAttackCoroutine());
        }
        
        public void StartAutoAttack()
        {
            if (!autoAttack)
            {
                autoAttack = true;
                StartCoroutine(AutoAttackCoroutine());
            }
        }
        
        // private void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawWireSphere(transform.position, detectionRange);
        //     
        //     if (currentTarget != null)
        //     {
        //         Gizmos.color = Color.red;
        //         Gizmos.DrawLine(transform.position, currentTarget.position);
        //         Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
        //     }
        // }
    }
} 