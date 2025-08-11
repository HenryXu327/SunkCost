using UnityEngine;
using System.Collections.Generic;
using Monster.Ability;

namespace Dungeon
{
    public class MonstersGenerator : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] monsterPrefabs;
        
        [SerializeField]
        private GameObject robotPrefab;

        [Range(0, 10)]
        [SerializeField]
        private int minMonstersPerRoom = 1;

        [Range(0, 10)]
        [SerializeField]
        private int maxMonstersPerRoom = 3;

        [SerializeField]
        private float spawnHeight = 2f;

        [SerializeField]
        private float roomBorderOffset = 1f;

        [SerializeField]
        private LayerMask groundLayerMask = -1;

        [Range(0f, 1f)]
        [SerializeField]
        private float spawnRoomProbability = 0.3f;
        
        [Range(0f, 1f)]
        [SerializeField]
        private float abilityProbability = 0.5f;

        public void GenerateMonsters(List<Room> generatedRooms)
        {
            if (monsterPrefabs == null || monsterPrefabs.Length == 0)
                return;

            foreach (var room in generatedRooms)
            {
                if (IsRoom(room.tile))
                {
                    if (Random.value > spawnRoomProbability)
                        continue;
                    GenerateMonstersInRoom(room.tile);
                }
            }
        }

        private bool IsRoom(Transform tile)
        {
            string tileName = tile.name.ToLower();
            return tileName.Contains("room") && !tileName.Contains("hallway");
        }
        
        private bool IsHallway(Transform tile)
        {
            string tileName = tile.name.ToLower();
            return tileName.Contains("hallway");
        }

        private void GenerateMonstersInRoom(Transform roomTile)
        {
            BoxCollider roomCollider = roomTile.GetComponent<BoxCollider>();
            if (roomCollider == null)
            {
                roomCollider = roomTile.gameObject.AddComponent<BoxCollider>();
                roomCollider.isTrigger = true;
            }

            int spawnCount = Random.Range(minMonstersPerRoom, maxMonstersPerRoom + 1);

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPosition = GetValidSpawnPosition(roomTile, roomCollider);
                if (spawnPosition == Vector3.zero)
                    continue;

                int prefabIndex = Random.Range(0, monsterPrefabs.Length);
                GameObject prefab = monsterPrefabs[prefabIndex];
                if (prefab == null)
                    continue;

                GameObject monster = Instantiate(prefab, spawnPosition, Quaternion.identity, roomTile);
                monster.name = prefab.name;

                float yRotation = Random.Range(0, 360f);
                monster.transform.Rotate(0, yRotation, 0);
                
                BulletAttackAbility bulletAttackAbility = monster.GetComponent<BulletAttackAbility>();
                RobotBulletAttackAbility robotBulletAttackAbility = bulletAttackAbility as RobotBulletAttackAbility;
                if (robotBulletAttackAbility == null && bulletAttackAbility != null)
                {
                    if (Random.value < abilityProbability)
                    {
                        bulletAttackAbility.enabled = false;
                    }
                }
                
                TentacleAttackAbility tentacleAttackAbility = monster.GetComponent<TentacleAttackAbility>();
                if (tentacleAttackAbility != null)
                {
                    if (Random.value < abilityProbability)
                    {
                        tentacleAttackAbility.enabled = false;
                    }
                }

                CharacterController controller = monster.GetComponentInChildren<CharacterController>();
                if (controller == null)
                {
                    Destroy(monster);
                    continue;
                }
                
                Vector3 up = controller.transform.up;
                Vector3 worldCenter = controller.transform.TransformPoint(controller.center);
                float radius = controller.radius;
                float height = controller.height;
                float half = Mathf.Max(height * 0.5f - radius, 0f);
                Vector3 p1 = worldCenter + up * half;
                Vector3 p2 = worldCenter - up * half;
                
                bool overlapped = Physics.CheckCapsule(p1, p2, radius * 0.95f, groundLayerMask, QueryTriggerInteraction.Ignore);
                if (overlapped)
                {
                    // Debug.LogWarning("Monster " + monster.name + " overlaps with the ground.");
                    // Destroy(monster);
                    continue;
                }
            }
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