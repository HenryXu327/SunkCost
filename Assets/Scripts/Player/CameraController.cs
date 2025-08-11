using UnityEngine;

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        public float mouseSensitivity = 100f;
        public float minPitch = -70f;
        public float maxPitch = 90f;

        public Transform playerBody;
        private float xRotation = 0f;

        void Start()
        {
            // Cursor.lockState = CursorLockMode.Locked;
        }
        

        void Update()
        {
            if (GridInventorySystem.InventoryController.IsInventoryOpen)
                return;
            
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }

        // 供武器后坐力调用，叠加俯仰上扬
        public void ApplyRecoil(float degrees)
        {
            float targetRotation = Mathf.Clamp(xRotation - degrees, minPitch, maxPitch);
            xRotation = Mathf.SmoothDamp(xRotation, targetRotation, ref xRotation, 0.001f);
            // xRotation -= degrees;
            // xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);
        }
    }
}