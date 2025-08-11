using System;
using System.Collections;
using UnityEngine;

namespace Monster.BipedRobot
{
    public class LegStepper : MonoBehaviour
    {
        [SerializeField] 
        private Transform homeTransform;

        [SerializeField] 
        private float wantStepAtDistance;

        [SerializeField] 
        private float moveDuration;

        [SerializeField] 
        private float stepOvershootFraction;

        public bool Moving;
        
        [SerializeField]
        private LayerMask groundLayer;

        public void Update()
        {
            Transform hipTransform = homeTransform.parent;
            
            RaycastHit hit;
            if (Physics.Raycast(hipTransform.position, Vector3.down, out hit, 10f,
                    groundLayer))
            {
                homeTransform.position = new Vector3(homeTransform.position.x, hit.point.y + 0.1f, homeTransform.position.z);
                // Debug.DrawLine(hipTransform.position, hit.point, Color.red);
            }
        }

        public void TryMove()
        {
            if (Moving) return;

            float distFromHome = Vector3.Distance(transform.position, homeTransform.position);
            
            if (distFromHome > wantStepAtDistance)
            {
                StartCoroutine(Move());
            }
        }

        private IEnumerator Move()
        {
            Moving = true;

            Vector3 startPoint = transform.position;
            Quaternion startRot = transform.rotation;

            Quaternion endRot = homeTransform.rotation;
            
            Vector3 towardHome = (homeTransform.position - transform.position);

            float overshootDistance = wantStepAtDistance * stepOvershootFraction;
            Vector3 overshootVector = towardHome * overshootDistance;
            
            overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);
            
            Vector3 endPoint = homeTransform.position + overshootVector;
            
            Vector3 centerPoint = (startPoint + endPoint) / 2;

            centerPoint += homeTransform.up * Vector3.Distance(startPoint, endPoint) / 2f;

            float timeElapsed = 0;
            do
            {
                timeElapsed += Time.deltaTime;
                float normalizedTime = timeElapsed / moveDuration;

                // normalizedTime = Easing.EaseInOutCubic(normalizedTime);
                
                transform.position =
                    Vector3.Lerp(
                        Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                        Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                        normalizedTime
                    );

                transform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

                yield return null;
            }
            while (timeElapsed < moveDuration);

            Moving = false;
        }
    }
}