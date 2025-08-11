using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Monster.FakeSpider
{
    public class SpiderLeg : MonoBehaviour
    {
        private FakeSpider fakeSpider;
        
        public bool canDie = false;
        public bool isDead = false;
        
        public LineRenderer legLineRenderer;
        public Vector3 finalFootPosition;
        public float finalFootOffset = 0.3f;
        public int legPartCount = 20;
        
        public float legMinHeight = 0.5f;
        public float legMaxHeight = 1.5f;
        private float legHeight;
        
        public float growthSpeed;
        public bool needToGrowth = true;
        [Range(0, 1)]
        public float growthProgress;
        
        public int bezierCurveCount = 8;
        public Vector3[] bezierCurvePoints;
        public Vector3[] bezierCurveOffsets;
        public float minBezierCurveOffset = 0.2f;
        public float maxBezierCurveOffset = 0.5f;
        
        private LayerMask groundLayerMask;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(finalFootPosition, 0.1f);
        }

        public void Initialize(FakeSpider fakeSpider, Vector3 finalFootPosition,float growthSpeed, float legLifeTime, LayerMask groundMask)
        {
            this.fakeSpider = fakeSpider;
            this.finalFootPosition = finalFootPosition;
            this.growthSpeed = growthSpeed;
            this.groundLayerMask = groundMask;
            
            legLineRenderer = GetComponent<LineRenderer>();
            
            bezierCurvePoints = new Vector3[bezierCurveCount];
            
            // RaycastHit hit;
            // Vector2 footOffset = Random.insideUnitCircle.normalized * finalFootOffset;
            // if (!Physics.Raycast(finalFootPosition + new Vector3(footOffset.x, 0, footOffset.y) + Vector3.up * 1f,
            //         Vector3.down, out hit, 10f, groundLayerMask))
            // {
            //     return;
            // }
            // bezierCurvePoints[bezierCurveCount - 1] = hit.point;
            bezierCurvePoints[bezierCurveCount - 1] = finalFootPosition;
            
            bezierCurveOffsets = new Vector3[bezierCurveCount - 2];
            for (int i = 0; i < bezierCurveCount - 2; i++)
            {
                bezierCurveOffsets[i] = Random.onUnitSphere * Random.Range(minBezierCurveOffset, maxBezierCurveOffset);
            }
            
            legHeight = Random.Range(legMinHeight, legMaxHeight);
            
            fakeSpider.CurrLegCount++;

            needToGrowth = true;

            canDie = false;
            isDead = false;
            
            StartCoroutine(WaitToDie());
            StartCoroutine(WaitAndDie(legLifeTime));
            
            SetBezierCurvePoints();
        }
        
        private IEnumerator WaitToDie()
        {
            yield return new WaitForSeconds(0.5f);
            canDie = true;
        }

        private IEnumerator WaitAndDie(float lifeTime)
        {
            yield return new WaitForSeconds(lifeTime);
            // 若蜘蛛腿数量不足，则等待直到蜘蛛腿数量足够
            while (fakeSpider.CurrLegCount < fakeSpider.MinLegCount)
                yield return null;
            needToGrowth = false;
        }

        private void Update()
        {
            float legLength  = Vector3.Distance(new Vector3(fakeSpider.legOrigin.x, 0, fakeSpider.legOrigin.z), new Vector3(finalFootPosition.x, 0, finalFootPosition.z));
            if (needToGrowth && legLength > fakeSpider.maxLegDistance && canDie &&
                fakeSpider.CurrLegCount > fakeSpider.MinLegCount)
            {
                needToGrowth = false;
            }
            else if (needToGrowth)
            {
                RaycastHit hit;
                if (Physics.Linecast(finalFootPosition, transform.position, out hit, groundLayerMask))
                {
                    needToGrowth = false;
                }
            }

            growthProgress = Mathf.Lerp(growthProgress, needToGrowth ? 1f : 0f, Time.deltaTime * growthSpeed);

            if (growthProgress < 0.5f && !needToGrowth)
            {
                if (!isDead)
                {
                    isDead = true;
                    fakeSpider.CurrLegCount--;
                }

                if (growthProgress < 0.1f)
                {
                    legLineRenderer.positionCount = 0;
                    fakeSpider.RecycleLeg(this.gameObject);
                    return;
                }
            }
            
            SetBezierCurvePoints();
            
            Vector3[] points = GetSamplePoints((Vector3[])bezierCurvePoints.Clone(), legPartCount, growthProgress);
            legLineRenderer.positionCount = points.Length;
            legLineRenderer.SetPositions(points);
        }

        public void SetBezierCurvePoints()
        {
            bezierCurvePoints[0] = transform.position;
            
            Vector3 startToEnd = bezierCurvePoints[bezierCurveCount - 1] - bezierCurvePoints[0];
            
            // 蜘蛛腿弯曲
            for (int i = 1; i < bezierCurveCount - 1; i++)
            {
                float t = (float)i / (bezierCurveCount - 1);
                Vector3 basePosition = bezierCurvePoints[0] + startToEnd * t;
                float heightCurve = Mathf.Sin(t * Mathf.PI) * legHeight;
                
                // 垂直偏移
                Vector3 heightOffset = Vector3.up * heightCurve;
                
                // 水平偏移
                Vector3 horizontalOffset = bezierCurveOffsets[i - 1];
                
                bezierCurvePoints[i] = basePosition + heightOffset + horizontalOffset;
            }
        }
        
        Vector3[] GetSamplePoints(Vector3[] curveHandles, int resolution, float t)
        {
            List<Vector3> segmentPos = new List<Vector3>();
            float segmentLength = 1f / (float)resolution;

            for (float _t = 0; _t <= t; _t += segmentLength)
                segmentPos.Add(GetPointOnCurve((Vector3[])curveHandles.Clone(), _t));
            segmentPos.Add(GetPointOnCurve(curveHandles, t));
            return segmentPos.ToArray();
        }

        Vector3 GetPointOnCurve(Vector3[] curveHandles, float t)
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


    }
}