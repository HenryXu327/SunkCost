using UnityEngine;
using System.Collections.Generic;
using Player.GridInventorySystem;

namespace Dungeon
{
    public class TrophiesGenerator : MonoBehaviour
    {
        [SerializeField]
        private List<ItemData> itemsData = new List<ItemData>();

        [Range(0, 10)]
        [SerializeField]
        private int minTrophiesPerRoom = 1;

        [Range(0, 10)]
        [SerializeField]
        private int maxTrophiesPerRoom = 3;

        [SerializeField]
        private float spawnHeight = 3f;

        [SerializeField]
        private float roomBorderOffset = 1f;

        [SerializeField]
        private LayerMask groundLayerMask = -1;

        public void GenerateTrophies(List<Room> generatedRooms)
        {
            if (itemsData.Count == 0)
                return;

            foreach (var room in generatedRooms)
            {
                if (IsRoom(room.tile))
                {
                    GenerateTrophiesInRoom(room.tile);
                }
            }
        }

        private bool IsRoom(Transform tile)
        {
            string tileName = tile.name.ToLower();
            return tileName.Contains("room") && !tileName.Contains("hallway");
        }

        private void GenerateTrophiesInRoom(Transform roomTile)
        {
            BoxCollider roomCollider = roomTile.GetComponent<BoxCollider>();
            if (roomCollider == null)
            {
                roomCollider = roomTile.gameObject.AddComponent<BoxCollider>();
                roomCollider.isTrigger = true;
            }

            int spawnCount = Random.Range(minTrophiesPerRoom, maxTrophiesPerRoom + 1);

            for (int i = 0; i < spawnCount; i++)
            {
                // Debug.Log("Generating trophy in room " + roomTile.name);
                Vector3 spawnPosition = GetValidSpawnPosition(roomTile, roomCollider);
                
                if (spawnPosition != Vector3.zero)
                {
                    ItemData selected = ChooseItemByWeight();
                    if (selected == null || selected.Prefab == null)
                        continue;
                    
                    // Debug.Log("Spawning trophy " + selected.Prefab.name + " at " + spawnPosition);
                    
                    GameObject trophy = Instantiate(selected.Prefab, spawnPosition + Vector3.up * 0.5f, Quaternion.identity, roomTile);
                    trophy.name = string.IsNullOrEmpty(selected.Name) ? selected.Prefab.name : selected.Name;

                    float yRotation = Random.Range(0, 360f);
                    trophy.transform.Rotate(0, yRotation, 0);

                    BoxCollider box = trophy.GetComponent<BoxCollider>();
                    bool addedTempBox = false;
                    if (box == null)
                    {
                        box = trophy.AddComponent<BoxCollider>();
                        addedTempBox = true;
                    }

                    Bounds b = box.bounds;

                    // 仅与地面层做相交检测，并忽略触发器
                    Collider[] overlaps = Physics.OverlapBox(b.center, b.extents * 0.9f, Quaternion.identity, groundLayerMask, QueryTriggerInteraction.Ignore);

                    if (overlaps.Length > 0)
                    {
                        Destroy(trophy);
                        continue;
                    }

                    if (addedTempBox)
                    {
                        Destroy(box);
                    }
                }
            }
        }

        private ItemData ChooseItemByWeight()
        {
            List<ItemData> validItems = new List<ItemData>();
            float totalWeight = 0f;

            for (int i = 0; i < itemsData.Count; i++)
            {
                ItemData item = itemsData[i];
                if (item == null || item.Prefab == null)
                    continue;

                float weight = Mathf.Max(0f, item.GenerateProbability);
                totalWeight += weight;
                validItems.Add(item);
            }

            if (validItems.Count == 0)
                return null;

            if (totalWeight <= 0f)
            {
                int idx = Random.Range(0, validItems.Count);
                return validItems[idx];
            }

            float r = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            for (int i = 0; i < validItems.Count; i++)
            {
                float w = Mathf.Max(0f, validItems[i].GenerateProbability);
                cumulative += w;
                if (r <= cumulative)
                {
                    return validItems[i];
                }
            }

            return validItems[validItems.Count - 1];
        }

        private Vector3 GetValidSpawnPosition(Transform roomTile, BoxCollider roomCollider)
        {
            Vector3 roomSize = roomCollider.size;

            for (int attempts = 0; attempts < 10; attempts++)
            {
                float randomX = Random.Range(-roomSize.x / 2 + roomBorderOffset, roomSize.x / 2 - roomBorderOffset);
                float randomZ = Random.Range(-roomSize.z / 2 + roomBorderOffset, roomSize.z / 2 - roomBorderOffset);

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