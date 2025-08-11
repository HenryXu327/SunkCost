using UnityEngine;
using System.Collections.Generic;

namespace Dungeon
{
    public class PropsGenerator : MonoBehaviour
    {
        [SerializeField] 
        private GameObject[] propsPrefabs;
        
        [Range(0, 10)]
        [SerializeField] 
        private int minPropsPerRoom = 3;
        
        [Range(0, 10)]
        [SerializeField] 
        private int maxPropsPerRoom = 7;
        
        [SerializeField]
        private float spawnHeight = 3f;
        
        [SerializeField]
        private float roomBorderOffset = 1f;
        
        [SerializeField]
        private LayerMask groundLayerMask = -1;

        public void GenerateProps(List<Room> generatedRooms)
        {
            if (propsPrefabs == null || propsPrefabs.Length == 0)
                return;

            foreach (var room in generatedRooms)
            {
                if (IsRoom(room.tile))
                {
                    GeneratePropsInRoom(room.tile);
                }
            }
        }

        private bool IsRoom(Transform tile)
        {
            string tileName = tile.name.ToLower();
            return tileName.Contains("room") && !tileName.Contains("hallway");
        }

        private void GeneratePropsInRoom(Transform roomTile)
        {
            BoxCollider roomCollider = roomTile.GetComponent<BoxCollider>();
            if (roomCollider == null)
            {
                roomCollider = roomTile.gameObject.AddComponent<BoxCollider>();
                roomCollider.isTrigger = true;
            }

            int propsCount = Random.Range(minPropsPerRoom, maxPropsPerRoom + 1);
            
            for (int i = 0; i < propsCount; i++)
            {
                Vector3 spawnPosition = GetValidSpawnPosition(roomTile, roomCollider);
                if (spawnPosition != Vector3.zero)
                {
                    int propIndex = Random.Range(0, propsPrefabs.Length);
                    GameObject prop = Instantiate(propsPrefabs[propIndex], spawnPosition, Quaternion.identity, roomTile);
                    prop.name = propsPrefabs[propIndex].name;
                    
                    // float yRotation = Random.Range(0, 4) * 90f;
                    float yRotation = Random.Range(0, 360f);
                    prop.transform.Rotate(0, yRotation, 0);
                    
                    BoxCollider box = prop.GetComponent<BoxCollider>();
                    bool addedTempBox = false;
                    if (box == null)
                    {
                        box = prop.AddComponent<BoxCollider>();
                        addedTempBox = true;
                    }
                    
                    Bounds b = box.bounds;

                    // 仅与地面层做相交检测，并忽略触发器
                    Collider[] overlaps = Physics.OverlapBox(b.center, b.extents * 0.9f, Quaternion.identity, groundLayerMask, QueryTriggerInteraction.Ignore);
                    
                    if (overlaps.Length > 0)
                    {
                        Destroy(prop);
                        continue;
                    }

                    if (addedTempBox)
                    {
                        Destroy(box);
                    }
                }
            }
        }

        private Vector3 GetValidSpawnPosition(Transform roomTile, BoxCollider roomCollider)
        {
            Vector3 roomSize = roomCollider.size;
            
            // 尝试多次
            for (int attempts = 0; attempts < 10; attempts++)
            {
                float randomX = Random.Range(-roomSize.x/2 + roomBorderOffset, roomSize.x/2 - roomBorderOffset);
                float randomZ = Random.Range(-roomSize.z/2 + roomBorderOffset, roomSize.z/2 - roomBorderOffset);
                
                Vector3 localPosition = new Vector3(randomX, roomCollider.center.y, randomZ);
                Vector3 worldPosition = roomTile.TransformPoint(localPosition);
                
                Vector3 rayStart = worldPosition + Vector3.up * spawnHeight;
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, spawnHeight + 5f, groundLayerMask))
                {
                    return hit.point;
                }
            }
           
            return Vector3.zero;
        }
    }
}