using UnityEngine;
using System.Collections.Generic;

namespace Monster.Ability
{
    /// <summary>
    /// 挂载到怪物身上，赋予其"被观察时静止"的能力
    /// 当怪物在玩家视野内时会静止不动，离开视野后继续活动
    /// </summary>
    public class SawFreezeAbility : MonoBehaviour
    {
        [SerializeField]
        private float distance = 20f;
        
        [SerializeField]
        private float Angle = 40f;
        
        private Transform playerTransform;
        
        private MoveToTarget moveToTargetComponent;
        
        private bool isFreeze = false;

        private void Start()
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
            moveToTargetComponent = GetComponent<MoveToTarget>();

        }

        private void Update()
        {
            if (playerTransform == null)
                return;
            
            if (Vector3.Distance(playerTransform.position, transform.position) <= distance)
            {
                if (Vector3.Angle(transform.position - playerTransform.position, playerTransform.forward) <= Angle)
                {
                    isFreeze = true;
                    moveToTargetComponent.enabled = false;
                }
                else
                {
                    isFreeze = false;
                    moveToTargetComponent.enabled = true;
                }
            }
            else
            {
                isFreeze = false;
                moveToTargetComponent.enabled = true;
            }
            

        }
    }
}