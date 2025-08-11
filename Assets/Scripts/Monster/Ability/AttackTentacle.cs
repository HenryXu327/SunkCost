using System.Collections;
using UnityEngine;
using Utility;

namespace Monster
{
    public class AttackTentacle : MonoBehaviour
    {
        public Transform target;
        
        public LineRenderer tentacleLineRenderer;
        public int tentaclePartCount = 20;
        
        public float growthSpeed = 20f;
        public float attackRange = 5f;
        [Range(0, 1)]
        public float growthProgress = 0f;
        
        public float tentacleMinHeight = 0.5f;
        public float tentacleMaxHeight = 2f;
        private float tentacleHeight;
        
        public int bezierCurveCount = 8;
        public Vector3[] bezierCurvePoints;
        public Vector3[] bezierCurveOffsets;
        public float minBezierCurveOffset = 0.2f;
        public float maxBezierCurveOffset = 0.8f;
        
        public float attackDamage = 25f;
        public float attackDuration = 0.1f; // 攻击持续时间
        
        public AudioSource audioSource;
        public AudioClip attackSound;
        
        private bool isAttacking = false;
        private bool hasAttacked = false;
        private bool needToGrow = false;
        private bool canAttack = false;
        
        private void Awake()
        {
            tentacleLineRenderer = GetComponent<LineRenderer>();
            
            audioSource = GetComponent<AudioSource>();
            
            if (tentacleLineRenderer != null)
            {
                tentacleLineRenderer.positionCount = 0;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(DestroyTentacle(3f));
        }

        private void OnDisable()
        {
            isAttacking = false;
            hasAttacked = false;
            needToGrow = false;
            canAttack = false;
        
            growthProgress = 0f;
            tentacleLineRenderer.positionCount = 0;
        
            bezierCurvePoints = new Vector3[bezierCurveCount];
            bezierCurveOffsets = new Vector3[bezierCurveCount - 2];
        }
        
        private IEnumerator DestroyTentacle(float delay)
        {
            yield return new WaitForSeconds(delay);

            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
        
        private void Update()
        {
            if (!isAttacking) return;
            
            growthProgress = Mathf.Lerp(growthProgress, needToGrow ? 1f : 0f, Time.deltaTime * growthSpeed);
            
            UpdateBezierCurvePoints();
            
            // 更新触手渲染
            Vector3[] points = GetSamplePoints((Vector3[])bezierCurvePoints.Clone(), tentaclePartCount, growthProgress);
            tentacleLineRenderer.positionCount = points.Length;
            tentacleLineRenderer.SetPositions(points);
        }
        
        public void Attack(Transform attackTarget)
        {
            if (isAttacking) return;
            
            target = attackTarget;
            if (target == null) return;
            
            // 检查攻击范围
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget > attackRange) return;
            
            isAttacking = true;
            hasAttacked = false;
            needToGrow = true;
            canAttack = false;
            growthProgress = 0f;
            
            // 初始化贝塞尔曲线
            InitializeBezierCurve();
            
            Debug.Log("触手开始攻击");
            StartCoroutine(AttackSequence());
        }
        
        
        private void InitializeBezierCurve()
        {
            bezierCurvePoints = new Vector3[bezierCurveCount];
            bezierCurvePoints[0] = transform.position; // 起始点
            bezierCurvePoints[bezierCurveCount - 1] = target.position; // 终点
            
            // 生成随机偏移
            bezierCurveOffsets = new Vector3[bezierCurveCount - 2];
            for (int i = 0; i < bezierCurveCount - 2; i++)
            {
                bezierCurveOffsets[i] = Random.onUnitSphere * Random.Range(minBezierCurveOffset, maxBezierCurveOffset);
            }
            
            tentacleHeight = Random.Range(tentacleMinHeight, tentacleMaxHeight);
        }
        
        private IEnumerator AttackSequence()
        {
            // 等待触手完全生长
            while (growthProgress < 0.95f)
            {
                yield return null;
            }
            
            Collider[] beAttackedColliders = Physics.OverlapSphere(transform.position, attackRange);
                
            canAttack = true;

            foreach (Collider collider in beAttackedColliders)
            {
                if (collider.CompareTag("Player"))
                {
                    var playerHealth = collider.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                    }
                    
                    Debug.Log($"触手攻击击中玩家，造成 {attackDamage} 点伤害");
                }
            }
            
            if (audioSource != null)
                audioSource.PlayOneShot(attackSound);
            
            // 攻击持续
            yield return new WaitForSeconds(attackDuration);
            
            
            // 收回
            needToGrow = false;
            
            // 等待触手完全收回
            while (growthProgress > 0.1f)
            {
                yield return null;
            }
            
            // 清理
            tentacleLineRenderer.positionCount = 0;
            
            ObjectPoolManager.ReturnObjectToPool(gameObject);
            // Destroy(gameObject);
            
            isAttacking = false;
        }
        
        private void UpdateBezierCurvePoints()
        {
            bezierCurvePoints[0] = transform.position;
            
            Vector3 startToEnd = bezierCurvePoints[bezierCurveCount - 1] - bezierCurvePoints[0];
            
            // 触手弯曲效果
            for (int i = 1; i < bezierCurveCount - 1; i++)
            {
                float t = (float)i / (bezierCurveCount - 1);
                Vector3 basePosition = bezierCurvePoints[0] + startToEnd * t;
                float heightCurve = Mathf.Sin(t * Mathf.PI) * tentacleHeight;
                
                // 垂直偏移
                Vector3 heightOffset = Vector3.up * heightCurve;
                
                // 水平偏移
                Vector3 horizontalOffset = bezierCurveOffsets[i - 1];
                
                bezierCurvePoints[i] = basePosition + heightOffset + horizontalOffset;
            }
        }
        
        private Vector3[] GetSamplePoints(Vector3[] curveHandles, int resolution, float t)
        {
            System.Collections.Generic.List<Vector3> segmentPos = new System.Collections.Generic.List<Vector3>();
            float segmentLength = 1f / (float)resolution;

            for (float _t = 0; _t <= t; _t += segmentLength)
                segmentPos.Add(GetPointOnCurve((Vector3[])curveHandles.Clone(), _t));
            segmentPos.Add(GetPointOnCurve(curveHandles, t));
            return segmentPos.ToArray();
        }

        private Vector3 GetPointOnCurve(Vector3[] curveHandles, float t)
        {
            int currentPoints = curveHandles.Length;

            while (currentPoints > 1)
            {
                for (int i = 0; i < currentPoints - 1; i++)
                    curveHandles[i] = Vector3.Lerp(curveHandles[i], curveHandles[i + 1], t);
                currentPoints--;
            }
            return curveHandles[0];
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            if (isAttacking)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(target.position, 0.2f);
            }
        }
    }
}