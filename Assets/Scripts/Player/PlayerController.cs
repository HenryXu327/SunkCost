using System;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        public float walkSpeed = 5f;
        public float runSpeed = 10f;
        public float jumpHeight = 2f;
        public float gravity = -20f;

        public LayerMask walkableLayer;
        public Transform groundCheck;
        public bool isGrounded;

        private CharacterController controller;
        private Vector3 velocity;
        

        private void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, walkableLayer);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -1f;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
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