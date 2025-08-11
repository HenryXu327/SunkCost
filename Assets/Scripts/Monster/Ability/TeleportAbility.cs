using UnityEngine;
using System.Collections;

namespace Monster.Ability
{
    /// <summary>
    /// 挂载到怪物身上，赋予其传送攻击能力
    /// 在玩家前方待一段时间后，传送到玩家后方
    /// </summary>
    public class TeleportAbility : MonoBehaviour
    {
        public FindTarget findTarget;
        public float detectionRange = 10f; // 检测范围
        public float timeInFrontThreshold = 3f; // 在前方待多长时间后触发传送
        public float teleportCooldown = 8f; // 传送冷却时间
        public float teleportDistance = 7f; // 传送到玩家后方的距离
        
        public LayerMask targetLayerMask = -1;
        public string targetTag = "Player";
        
        public LayerMask groundLayerMask = 1;
        public float groundCheckDistance = 2f; // 地面检测距离
        public float groundCheckRadius = 0.5f; // 地面检测半径
        
        public GameObject teleportEffect; // 传送特效
        
        private Transform currentTarget;
        private float timeTargetInFront = 0f;
        private float lastTeleportTime = -999f;
        private bool isPlayerInFront = false;
        
        private CharacterController characterController;
        
        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            groundLayerMask = LayerMask.GetMask("Tile");
        }
        
        private void Update()
        {
            currentTarget = findTarget.currentTarget;

            UpdateTeleportLogic();
        }
        
        private void UpdateTeleportLogic()
        {
            if (currentTarget == null)
            {
                timeTargetInFront = 0f;
                isPlayerInFront = false;
                return;
            }
            
            // 检查是否在玩家前方180°范围内
            Vector3 directionToTarget = (transform.position - currentTarget.position).normalized;
            float dotProduct = Vector3.Dot(currentTarget.forward, directionToTarget);
            
            isPlayerInFront = dotProduct > 0f;
            
            if (isPlayerInFront)
            {
                timeTargetInFront += Time.deltaTime;
                
                if (CanTeleport())
                {
                    PerformTeleport();
                }
            }
            else
            {
                timeTargetInFront = 0f;
            }
        }
        
        private bool CanTeleport()
        {
            return timeTargetInFront >= timeInFrontThreshold && 
                   Time.time - lastTeleportTime >= teleportCooldown;
        }
        
        private void PerformTeleport()
        {
            Vector3 teleportPosition = FindValidTeleportPosition();
            
            if (teleportPosition != Vector3.zero)
            {
                // 播放传送特效
                if (teleportEffect != null)
                {
                    Instantiate(teleportEffect, transform.position, Quaternion.identity);
                }
                
                // Debug.Log("Teleporting to " + teleportPosition);
                // 执行传送
                // CharacterController，需要禁用再启用来实现瞬间传送！！！
                characterController.enabled = false;
                transform.position = teleportPosition;
                characterController.enabled = true;
                // Debug.Log("Teleported to " + transform.position);
                
                // 面向玩家
                Vector3 lookDirection = (currentTarget.position - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
                
                // 播放传送特效
                if (teleportEffect != null)
                {
                    Instantiate(teleportEffect, transform.position, Quaternion.identity);
                }
                
                timeTargetInFront = 0f;
                lastTeleportTime = Time.time;
            }
        }
        
        private Vector3 FindValidTeleportPosition()
        {
            if (currentTarget == null) return Vector3.zero;
            
            Vector3 playerForward = currentTarget.forward;
            
            // 尝试不同的角度偏移
            float[] angleOffsets = { 0f, 30f, -30f, 60f, -60f, 90f, -90f };
            
            foreach (float angleOffset in angleOffsets)
            {
                Vector3 direction = Quaternion.AngleAxis(angleOffset, Vector3.up) * (-playerForward); 
                Vector3 testPosition = currentTarget.position + direction * teleportDistance;
                
                // Debug.Log($"Testing teleport position: {testPosition} (angle offset: {angleOffset})");
                
                if (IsValidTeleportPosition(testPosition))
                {
                    // Debug.Log($"Valid teleport position found: {testPosition}");
                    return testPosition;
                }
            }
            
            // Debug.Log("No valid teleport position found");
            return Vector3.zero;
        }
        
        private bool IsValidTeleportPosition(Vector3 position)
        {
            Vector3 checkStart = position + Vector3.up * 0.1f;
            
            // 检测是否有地面
            if (Physics.SphereCast(checkStart, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayerMask))
            {
                // 检查传送位置上方是否有足够空间
                Vector3 finalPosition = hit.point;
                if (!Physics.CheckSphere(finalPosition + Vector3.up * 1f, 0.4f))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        // private void OnDrawGizmos()
        // {
        //     // 绘制检测范围
        //     Gizmos.color = Color.cyan;
        //     Gizmos.DrawWireSphere(transform.position, detectionRange);
        //     
        //     // 绘制前方180°检测区域
        //     Gizmos.color = isPlayerInFront ? Color.red : Color.yellow;
        //     Vector3 leftBoundary = Quaternion.AngleAxis(-90f, Vector3.up) * transform.forward * detectionRange;
        //     Vector3 rightBoundary = Quaternion.AngleAxis(90f, Vector3.up) * transform.forward * detectionRange;
        //     
        //     Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        //     Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        //     
        //     // 绘制到当前目标的连线
        //     if (currentTarget != null)
        //     {
        //         Gizmos.color = isPlayerInFront ? Color.red : Color.blue;
        //         Gizmos.DrawLine(transform.position, currentTarget.position);
        //         
        //         // 显示传送目标位置预览
        //         if (isPlayerInFront)
        //         {
        //             Vector3 teleportPos = FindValidTeleportPosition();
        //             if (teleportPos != Vector3.zero)
        //             {
        //                 Gizmos.color = Color.green;
        //                 Gizmos.DrawWireSphere(teleportPos, 0.5f);
        //                 Gizmos.DrawLine(transform.position, teleportPos);
        //             }
        //         }
        //     }
        //     
        //     // 显示计时进度
        //     if (isPlayerInFront && timeTargetInFront > 0)
        //     {
        //         Gizmos.color = Color.magenta;
        //         float progress = timeTargetInFront / timeInFrontThreshold;
        //         Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, progress * 0.5f);
        //     }
        // }
    }
}