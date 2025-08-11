using UnityEngine;

namespace Monster.Ability
{
    public class MoveToTarget : MonoBehaviour
    {
        public FindTarget findTarget;
        [SerializeField]
        public Transform target;
        
        public float minDistance = 2f;
        
        public float speed = 5f;
        Vector3 velocity = Vector3.zero;
        public float velocitySmooth = 4f;
        
        private IMove[] moveReceivers;

        private void Start()
        {
            moveReceivers = GetComponents<IMove>();
        }

        void Update()
        {
            target = findTarget?.currentTarget;
            
            if (target == null) 
                return;
            
            Vector3 move = target.position - transform.position;
            move.y = 0;

            if (Vector3.Distance(transform.position, target.position) < minDistance)
            {
                move = Vector3.zero;
            }
            
            velocity = Vector3.Lerp(velocity, move.normalized * speed, velocitySmooth * Time.deltaTime);
            
            // 通过接口设置速度，进行移动
            foreach (var receiver in moveReceivers)
            {
                receiver.SetVelocity(velocity);
                receiver.MoveTo(velocity, target);
                receiver.SetSpeed(speed);
            }
            
        }
    }
}