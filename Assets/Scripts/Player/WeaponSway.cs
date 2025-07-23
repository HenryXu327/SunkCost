using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class WeaponSway : MonoBehaviour
    {
        [SerializeField]
        private float swaySpeed;

        [SerializeField] 
        private float smooth;
    
        void Update()
        {
            float x = Input.GetAxis("Mouse X") * swaySpeed;
            float y = Input.GetAxis("Mouse Y") * swaySpeed;

            Quaternion rotationX = Quaternion.AngleAxis(-x, Vector3.up);
            Quaternion rotationY = Quaternion.AngleAxis(y, Vector3.right);

            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotationX * rotationY, Time.deltaTime * smooth);
        }
    }
}

