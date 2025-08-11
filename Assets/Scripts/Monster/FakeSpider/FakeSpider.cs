using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Monster.FakeSpider
{
    public class FakeSpider : MonoBehaviour, IMove
    {
        [Tooltip("离地高度")]
        [Range(0.5f, 5f)]
        public float height = 0.8f;
        public LayerMask groundMask;
        
        public GameObject legPrefab;

        public int numberOfLegs = 8;
        public int CurrLegCount;
        public int MinLegCount = 4;

        public float minLegLifetime = 5f;
        public float maxLegLifetime = 10f;

        public Vector3 legOrigin = Vector3.zero;
        public float legPlaceRadius = 1f;

        public float minLegDistance = 2f;
        public float maxLegDistance = 5f;

        public float minGrowthSpeed = 4.5f;
        public float maxGrowthSpeed = 6.5f;

        public float newLegCooldown = 0.3f;
        
        public LayerMask legGroundLayerMask = -1;

        public bool canCreateLeg = true;

        private List<GameObject> availableLegPool = new List<GameObject>();

        public Vector3 velocity;

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

        void Start()
        {
            Reset();
        }

        private void OnValidate()
        {
            Reset();
        }

        public void Reset()
        {
            foreach (SpiderLeg g in GameObject.FindObjectsOfType<SpiderLeg>())
            {
                Destroy(g.gameObject);
            }

            CurrLegCount = 0;
            velocity = Vector3.zero;
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
            
            if (!canCreateLeg)
                return;
            
            legOrigin = transform.position + velocity.normalized * legPlaceRadius;

            if (CurrLegCount <= numberOfLegs)
            {
                Vector2 offset = Random.insideUnitCircle * legPlaceRadius;
                Vector3 newLegOrigin = legOrigin + new Vector3(offset.x, 0, offset.y);
                
                Vector3 newLegPosition = newLegOrigin + Random.onUnitSphere * Random.Range(minLegDistance, maxLegDistance);
                if (velocity.magnitude > 1f)
                {
                    float newLegAngle = Vector3.Angle(velocity, newLegPosition - transform.position);

                    if (Mathf.Abs(newLegAngle) > 90)
                    {
                        newLegPosition = transform.position - (newLegPosition - transform.position);
                        
                    }
                }

                RaycastHit hit;
                // if (Physics.Raycast(newLegPosition + Vector3.up * 1f, Vector3.down, out hit, 15f, groundLayerMask))
                // {
                //     Vector3 newLegPos = hit.point;
                //     if (Physics.Linecast(transform.position, newLegPos, out hit, groundLayerMask))
                //     {
                //         Debug.DrawLine(transform.position, newLegPos, Color.red, 1f);
                //         newLegPos = hit.point;
                //         
                //         float legLifetime = Random.Range(minLegLifetime, maxLegLifetime);
                //         float growthSpeed = Random.Range(minGrowthSpeed, maxGrowthSpeed);
                //
                //         StartCoroutine(CreateLegCooldown());
                //
                //         CreateLeg(newLegPos, growthSpeed, legLifetime);
                //     }
                // }
                // else 
                if (Physics.Linecast(transform.position, newLegPosition, out hit, legGroundLayerMask, QueryTriggerInteraction.Ignore))
                {
                    // Debug.DrawLine(transform.position, newLegPosition, Color.green, 1f);
                    newLegPosition = hit.point;
                        
                    float legLifetime = Random.Range(minLegLifetime, maxLegLifetime);
                    float growthSpeed = Random.Range(minGrowthSpeed, maxGrowthSpeed);

                    StartCoroutine(CreateLegCooldown());

                    CreateLeg(newLegPosition, growthSpeed, legLifetime);
                }

            }
        }

        private void CreateLeg(Vector3 position,float growthSpeed, float lifetime)
        {
            GameObject leg;
            if (availableLegPool.Count > 0)
            {
                leg = availableLegPool[0];
                availableLegPool.RemoveAt(0);
            }
            else
            {
                leg = Instantiate(legPrefab, transform.position, Quaternion.identity);
            }
            leg.SetActive(true);
            leg.GetComponent<SpiderLeg>().Initialize(this, position, growthSpeed, lifetime, legGroundLayerMask);
            leg.transform.SetParent(transform);

        }
        
        public void RecycleLeg(GameObject leg)
        {
            availableLegPool.Add(leg);
            leg.SetActive(false);
        }



        private IEnumerator CreateLegCooldown()
        {
            canCreateLeg = false;
            yield return new WaitForSeconds(newLegCooldown);
            canCreateLeg = true;
        }
    }
}