using UnityEngine;

namespace Monster.Ability
{
    public class FindTarget : MonoBehaviour
    {
        public float detectionRange = 12f; 
        
        public LayerMask groundLayerMask = -1;
        public string targetTag = "Player"; 
        
        public bool isRobot = false;
        
        public Transform currentTarget;
        private void Update()
        {
            // 寻找最近目标
            Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRange);
            
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (Collider col in potentialTargets)
            {
                if (!isRobot)
                {
                    if (col.CompareTag(targetTag))
                    {
                        float distance = Vector3.Distance(transform.position, col.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestTarget = col.transform;
                        }
                    }
                }
                else
                {
                    if (col.CompareTag(targetTag) || col.CompareTag("Monster"))
                    {
                        float distance = Vector3.Distance(transform.position, col.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestTarget = col.transform;
                        }
                    }
                }
            }
            
            if (closestTarget == null)
            {
                currentTarget = null;
                return;
            }

            if (!isRobot && !Physics.Linecast(transform.position, closestTarget.position, groundLayerMask, QueryTriggerInteraction.Ignore))
            {
                currentTarget = closestTarget;
                return;
            }
            
            if (isRobot)
            {
                currentTarget = closestTarget;
                return;
            }

            currentTarget = null;

        }
    }
}