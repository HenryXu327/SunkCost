using System.Collections;
using UnityEngine;

namespace Player
{
    public class CameraShake : MonoBehaviour
    {
        public IEnumerator Shake(float duration, float magnitude)
        {
            Vector3 originalPosition = transform.localPosition;
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                
                transform.localPosition = new Vector3(x, y, originalPosition.z) + originalPosition;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.localPosition = originalPosition;
        }
    }
}