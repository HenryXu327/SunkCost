using UnityEngine;

namespace Monster.FakeSnake
{
    public class FakeSnake : MonoBehaviour, IMove
    {
        [Tooltip("Body Height from ground")]
        [Range(0.5f, 5f)]
        public float height = 0.8f;
        public LayerMask groundMask;
        
        public Transform head;
        
        [SerializeField]
        public Vector3 velocity;
        public float rotationSpeed = 5f;
        
        private CharacterController characterController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }
        
        public void SetVelocity(Vector3 newVelocity)
        {
            velocity = newVelocity;
        }
        
        public void MoveTo(Vector3 newVelocity, Transform target)
        {
            characterController.Move(newVelocity * Time.deltaTime);
        }

        public void SetSpeed(float speed)
        {
            
        }
        
        private void Update()
        {
            RaycastHit heightHit;
            if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out heightHit, 10f, groundMask))
            {
                Debug.DrawLine(transform.position + Vector3.up * 2f, heightHit.point, Color.red);
                Vector3 destHeight = new Vector3(transform.position.x, heightHit.point.y + height, transform.position.z);
                Vector3 heightAdjustment = destHeight - transform.position;
                Vector3 smoothHeightAdjustment = Vector3.Lerp(Vector3.zero, heightAdjustment, 4f * Time.deltaTime);
                
                if (characterController != null)
                {
                    characterController.Move(smoothHeightAdjustment);
                }
            }
            
            // 蛇头转向移动方向
            if (velocity.magnitude > 0.1f && head != null)
            {
                Vector3 targetDirection = velocity.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                
                head.rotation = Quaternion.Slerp(head.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}