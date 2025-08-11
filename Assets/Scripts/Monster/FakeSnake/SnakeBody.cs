using UnityEngine;

namespace Monster.FakeSnake
{
    public class SnakeBody : MonoBehaviour
    {
        public FakeSnake snake;
        
        public int length;
        
        public Vector3[] segments;
        private Vector3[] segmentsVelocity;

        public Transform targetDir;
        public float targetDistance;
        
        public float smoothTime;
        public float trailSpeed;
        
        public float wiggleSpeed;
        public float wiggleMagnitude;
        public Transform wiggleDir;
        
        public float groundOffset = 0.5f;
        public float maxGroundDistance = 1f;
        public LayerMask groundLayerMask = -1;
        
        public float segmentRadius = 0.3f;  // 每个segment的碰撞半径
        public LayerMask obstacleLayerMask = -1;
        
        private LineRenderer lineRenderer;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = length;
            segments = new Vector3[length];
            segmentsVelocity = new Vector3[length];
        }

        private void Update()
        {
            if (snake.velocity.magnitude > 5f)
            {
                wiggleSpeed = 10f;
            }
            else
            {
                wiggleSpeed = 3f;
            }
            
            wiggleDir.localRotation = Quaternion.Euler(0, Mathf.Sin(wiggleSpeed * Time.time) * wiggleMagnitude, 0);
            
            segments[0] = targetDir.position;

            for (int i = 1; i < segments.Length; i++)
            {
                Vector3 targetPosition = segments[i - 1] + (-targetDir.forward) * targetDistance;
                Vector3 newPosition = Vector3.SmoothDamp(segments[i], targetPosition,
                    ref segmentsVelocity[i], smoothTime + i / trailSpeed);
                
                // 检测并修正碰撞
                segments[i] = CheckAndResolveCollision(segments[i], newPosition, i);
            }
            
            // 调整每个段的高度
            AdjustSegmentsToGround();
            
            lineRenderer.SetPositions(segments);
        }
        
        private Vector3 CheckAndResolveCollision(Vector3 currentPos, Vector3 targetPos, int segmentIndex)
        {
            Vector3 finalPosition = targetPos;
            
            Vector3 direction = (targetPos - currentPos).normalized;
            float distance = Vector3.Distance(currentPos, targetPos);
            
            RaycastHit hit;
            if (Physics.SphereCast(currentPos, segmentRadius, direction, out hit, distance, obstacleLayerMask))
            {
                Vector3 collisionPoint = hit.point;
                Vector3 collisionNormal = hit.normal;
                
                // 计算沿着碰撞表面滑动的位置
                Vector3 slideDirection = Vector3.ProjectOnPlane(direction, collisionNormal).normalized;
                
                // 沿着表面滑动
                Vector3 slidePosition = currentPos + slideDirection * distance;
                
                // 再次检测滑动位置是否安全
                if (!Physics.CheckSphere(slidePosition, segmentRadius, obstacleLayerMask))
                {
                    finalPosition = slidePosition;
                }
            }
            
            return finalPosition;
        }
        
        private void AdjustSegmentsToGround()
        {
            for (int i = 1; i < segments.Length; i++)
            {
                Vector3 rayStart = new Vector3(segments[i].x, segments[i].y + maxGroundDistance, segments[i].z);
                RaycastHit hit;
                
                if (Physics.Raycast(rayStart, Vector3.down, out hit, maxGroundDistance * 2f, groundLayerMask))
                {
                    // 调整距离地面高度
                    Vector3 adjustedPosition = new Vector3(segments[i].x, hit.point.y + groundOffset, segments[i].z);
                    
                    // 调整高度后是否会碰撞
                    if (!Physics.CheckSphere(adjustedPosition, segmentRadius, obstacleLayerMask))
                    {
                        segments[i].y = adjustedPosition.y;
                    }
                }
            }
        }
    }
}