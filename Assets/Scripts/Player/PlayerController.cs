using System;
using Player.GridInventorySystem;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        public float maxWalkSpeed = 4f;
        public float maxRunSpeed = 7f;
        public float minWalkSpeed = 1f;
        public float minRunSpeed = 3f;
        public float jumpHeight = 2f;
        public float gravity = -20f;

        public LayerMask walkableLayer;
        public Transform groundCheck;
        public bool isGrounded;

        private CharacterController controller;
        private Vector3 velocity;
        
        private InventoryController inventoryController;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            inventoryController = GetComponent<InventoryController>();
        }

        private void Update()
        {
            if (GridInventorySystem.InventoryController.IsInventoryOpen)
                return;

            isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, walkableLayer);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -1f;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            float rate = inventoryController.SumWeight / inventoryController.MaxWeight;
            rate = 1 - Mathf.Clamp01(rate);
            
            float speed = Input.GetKey(KeyCode.LeftShift) ? Mathf.Lerp(minRunSpeed, maxRunSpeed, rate) : Mathf.Lerp(minWalkSpeed, maxWalkSpeed, rate) ;
            
            controller.Move(move * (speed * Time.deltaTime));

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(groundCheck.position, 0.2f);
        }
    }
}