using System.Collections;
using UnityEngine;

namespace Monster.BipedRobot
{
    public class BipedController : MonoBehaviour, IMove
    {
        [SerializeField] 
        private Transform target;
        
        [SerializeField] 
        private Transform headBone;
        [SerializeField] 
        private float headMaxTurnAngle = 70f;
        [SerializeField] 
        private float headTrackingSpeed = 10f;
        
        [SerializeField] 
        private float turnSpeed = 100f;
        [SerializeField] 
        private float moveSpeed = 2f;
        [SerializeField] 
        private float turnAcceleration = 5f;
        [SerializeField] 
        private float moveAcceleration = 5f;
        [SerializeField] 
        private float minDistToTarget = 2f;
        [SerializeField] 
        private float maxDistToTarget = 3f;
        [SerializeField] 
        private float maxAngToTarget = 25f;

        private Vector3 currentVelocity;
        private float currentAngularVelocity;
        
        // 添加重力相关变量
        [SerializeField]
        private float gravity = 9.81f;
        private float verticalVelocity = 0f;
        
        [SerializeField] 
        private LegStepper leftLegStepper;
        [SerializeField] 
        private LegStepper rightLegStepper;
        
        private CharacterController characterController;
        
        public void SetVelocity(Vector3 velocity)
        {
            
        }

        public void MoveTo(Vector3 velocity, Transform newTarget)
        {
            target = newTarget;
            
            // 合并水平移动和垂直移动（重力）
            Vector3 finalVelocity = currentVelocity;
            finalVelocity.y = verticalVelocity;
            
            if (characterController != null)
            {
                characterController.Move(finalVelocity * Time.deltaTime);
            }
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        void Awake()
        {
            StartCoroutine(LegUpdateCoroutine());
            
            characterController = GetComponent<CharacterController>();
        }

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }
            
            // 头部依赖于身体，先更新身体，然后更新头部。
            RootMotionUpdate();
            HeadTrackingUpdate();
        }

        void HeadTrackingUpdate()
        {
            Quaternion currentLocalRotation = headBone.localRotation;
            headBone.localRotation = Quaternion.identity;

            Vector3 targetWorldLookDir = target.position - headBone.position;
            Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);
            
            // Local的X轴向下，xRotation为左右移动
            float xRotation = Mathf.Atan2(targetLocalLookDir.z, -targetLocalLookDir.y) * Mathf.Rad2Deg;
            
            // Local的Z轴向右，zRotation为上下移动  
            float zRotation = Mathf.Atan2(targetLocalLookDir.x, -targetLocalLookDir.y) * Mathf.Rad2Deg;
            
            xRotation = Mathf.Clamp(xRotation, -headMaxTurnAngle, headMaxTurnAngle);
            zRotation = Mathf.Clamp(zRotation, -headMaxTurnAngle, headMaxTurnAngle);
            
            Quaternion targetLocalRotation = Quaternion.Euler(-xRotation, 0, zRotation);

            headBone.localRotation = Quaternion.Slerp(
                currentLocalRotation,
                targetLocalRotation,
                1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
            );
        }

        void RootMotionUpdate()
        {
            Vector3 towardTarget = target.position - transform.position;
            Vector3 towardTargetProjected = Vector3.ProjectOnPlane(towardTarget, transform.up);
            float angToTarget = Vector3.SignedAngle(transform.forward, towardTargetProjected, transform.up);

            float targetAngularVelocity = 0;

            if (Mathf.Abs(angToTarget) > maxAngToTarget)
            {
                if (angToTarget > 0)
                {
                    targetAngularVelocity = turnSpeed;
                }
                else
                {
                    targetAngularVelocity = -turnSpeed;
                }
            }

            currentAngularVelocity = Mathf.Lerp(
                currentAngularVelocity,
                targetAngularVelocity,
                1 - Mathf.Exp(-turnAcceleration * Time.deltaTime)
            );

            transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);

            Vector3 targetVelocity = Vector3.zero;

            if (Mathf.Abs(angToTarget) < 90)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);

                if (distToTarget > maxDistToTarget)
                {
                    targetVelocity = moveSpeed * towardTargetProjected.normalized;
                }
                else if (distToTarget < minDistToTarget)
                {
                    targetVelocity = moveSpeed * -towardTargetProjected.normalized;
                }
            }

            currentVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                1 - Mathf.Exp(-moveAcceleration * Time.deltaTime)
            );
            
            // transform.position += currentVelocity * Time.deltaTime;

            // 应用重力
            if (characterController != null && characterController.isGrounded)
            {
                verticalVelocity = 0f;
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }
        }
        
        IEnumerator LegUpdateCoroutine()
        {
            while (true)
            {
                // 移动左腿，等待完成
                do
                {
                    leftLegStepper.TryMove();
                    yield return null; 
                } while (leftLegStepper.Moving);

                // 移动右腿，等待完成
                do
                {
                    rightLegStepper.TryMove();
                    yield return null; 
                } while (rightLegStepper.Moving);
            }
        }


    }
}