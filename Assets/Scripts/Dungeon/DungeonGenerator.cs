using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Player.GridInventorySystem;
using UnityEngine;
using Random = UnityEngine.Random;

public enum GeneratingState
{
    None,
    GeneratingMain,
    GeneratingBranch,
    CleaningUp,
    Done
}

namespace Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] 
        KeyCode regenerateKey = KeyCode.Y;
        [SerializeField] 
        KeyCode smallMapKey = KeyCode.M;
        
        public GeneratingState GeneratingState = GeneratingState.None;
        
        [SerializeField] 
        GameObject[] startRoomPrefabs;
        [SerializeField] 
        GameObject[] tilePrefabs;
        [SerializeField] 
        GameObject[] exitRoomPrefabs;
        [SerializeField] 
        GameObject[] blockedPrefabs;
        [SerializeField] 
        GameObject[] doorPrefabs;
        
        [SerializeField]
        PropsGenerator propsGenerator;
        [SerializeField]
        TrophiesGenerator trophiesGenerator;
        [SerializeField]
        MonstersGenerator monstersGenerator;
        
        [Range(2, 100)] 
        [SerializeField] 
        int mainLength = 10;
        [Range(0, 50)] 
        [SerializeField] 
        int branchLength = 5;
        [Range(0, 25)] 
        [SerializeField] 
        int branchCount = 5;
        [Range(0, 100)] 
        [SerializeField] 
        int doorGeneratePercent = 50;
        [Range(0, 1f)] 
        [SerializeField] 
        float generateDelay = 0.15f;
        
        [SerializeField] 
        bool useLightsForDebugging;
        [SerializeField] 
        bool restoreLishtsAfterDebugging;
        private Color startColor = Color.white;
        
        public List<Room> generatedRooms = new List<Room>();
        
        private List<Connector> availableConnectors = new List<Connector>();

        private Transform rootRoom;
        private Transform previousRoom;
        private Transform currentRoom;

        private Transform container;
        
        private GameObject overviewCamera;
        
        private GameObject player;
        
        private int attemptsTimes = 0;
        private int maxAttemptsTimes = 100;

        public void StartAll()
        {
            ClearGenerated();
            StartCoroutine(GenerateDungeon());
        }
        
        void Start()
        {
            overviewCamera = GameObject.Find("Overview Camera");
            if (overviewCamera!= null)
            {
                Debug.Log("Overview Camera found!");
                overviewCamera.SetActive(true);
            }
            
            player = GameObject.FindGameObjectWithTag("Player");
            if (player!= null)
            {
                Debug.Log("Player found!");
                player.SetActive(false);
            }
        }

        public void ClearGenerated()
        {
            // 停止生成流程
            // StopAllCoroutines();
            // 恢复Player的各种数据，清空背包
            player.transform.position = new Vector3(0, 2, 0);
            player.transform.rotation = Quaternion.identity;
            player.GetComponent<PlayerHealth>().currentHealth = player.GetComponent<PlayerHealth>().maxHealth;
            player.GetComponent<InventoryController>().ClearInventory();
            player.SetActive(false);
            overviewCamera.SetActive(true);

            // 清理当前节点下的所有生成容器（"Main"、"Branch x"）
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                // DestroyImmediate(child.gameObject);
                Destroy(child.gameObject);
            }

            // 重置数据
            generatedRooms.Clear();
            availableConnectors.Clear();
            attemptsTimes = 0;

            rootRoom = null;
            previousRoom = null;
            currentRoom = null;
            container = null;

            GeneratingState = GeneratingState.None;
        }

        private void Update()
        {
            if (Input.GetKeyDown(regenerateKey))
            {
                ClearGenerated();
                // SceneManager.LoadScene("Game");
                // TODO: 清除已经生成的地图，清除
                StartCoroutine(GenerateDungeon());
            }

            if (Input.GetKeyDown(smallMapKey))
            {
                overviewCamera.SetActive(!overviewCamera.activeSelf);
                player.SetActive(!player.activeSelf);
            }
        }

        private IEnumerator GenerateDungeon()
        {
            // 生成主干
            GameObject containerGo = new GameObject("Main");
            container = containerGo.transform;
            container.SetParent(transform);
            
            rootRoom = GenerateStartRoom();
            ChangeRoomLightColor(rootRoom, Color.cyan);
            currentRoom = rootRoom;
            
            yield return new WaitForSeconds(generateDelay);
            
            GeneratingState = GeneratingState.GeneratingMain;
            while (generatedRooms.Count < mainLength)
            {
                yield return new WaitForSeconds(generateDelay);
                
                previousRoom = currentRoom;

                if (generatedRooms.Count == mainLength - 1)
                {
                    currentRoom = GenerateExitRoom();
                    ChangeRoomLightColor(currentRoom, Color.blue);
                }
                else
                {
                    currentRoom = GenerateTile();
                    ChangeRoomLightColor(currentRoom, Color.yellow);
                }
                
                ConnectRooms(previousRoom, currentRoom);
                CheckRoomCollisions();
                
            }
            
            availableConnectors = GetAvailableConnectors(container);
            
            // 生成分支
            GeneratingState = GeneratingState.GeneratingBranch;
            for (int branchIndex = 0; branchIndex < branchCount; branchIndex++)
            {
                yield return new WaitForSeconds(generateDelay);
                
                if (availableConnectors.Count == 0)
                {
                    break;
                }
                else
                {
                    containerGo = new GameObject("Branch " + (branchIndex + 1));
                    container = containerGo.transform;
                    container.SetParent(transform);
                    
                    // 随机选择一个连接点
                    int connectorIndex = Random.Range(0, availableConnectors.Count);
                    rootRoom = availableConnectors[connectorIndex].transform.parent.parent;
                    availableConnectors.RemoveAt(connectorIndex);
            
                    currentRoom = rootRoom;
            
                    for (int i = 0; i < branchLength - 1; i++)
                    {
                        yield return new WaitForSeconds(generateDelay);
                        
                        previousRoom = currentRoom;
                        currentRoom = GenerateTile();
                        ChangeRoomLightColor(currentRoom, Color.green);
                        ConnectRooms(previousRoom, currentRoom);
                        CheckRoomCollisions();
                        
                        if (attemptsTimes >= maxAttemptsTimes)
                        {
                            break;
                        }
                    }
                }
            }
            
            GeneratingState = GeneratingState.CleaningUp;
            RestoreRoomLightColor();
            CleanRoomBoxColliders();
            
            GenerateBlockeds();
            GenerateDoors();
            
            if (propsGenerator != null)
            {
                propsGenerator.GenerateProps(generatedRooms);
            }
            
            yield return new WaitForSeconds(1f);
            
            if (trophiesGenerator != null)
            {
                trophiesGenerator.GenerateTrophies(generatedRooms);
            }
            
            yield return new WaitForSeconds(1f);
           
            if (monstersGenerator != null)
            {
                monstersGenerator.GenerateMonsters(generatedRooms);
            }
            
            yield return new WaitForSeconds(3f);
            
            CleanRoomBoxColliders();
            
            yield return new WaitForSeconds(1f);
            
            GeneratingState = GeneratingState.Done;
            
            overviewCamera.SetActive(false);
            player.transform.position = new Vector3(0, 2, 0);
            player.transform.rotation = Quaternion.identity;
            player.SetActive(true);
        }

        
        private void CheckRoomCollisions()
        {
            BoxCollider boxCollider = currentRoom.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = currentRoom.gameObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
            }
            
            // Debug.Log(currentRoom.gameObject.name + boxCollider.size + " " + boxCollider.bounds + " " + boxCollider.extents + " " + boxCollider.bounds.extents + " " + boxCollider.bounds.size);
            
            // 计算BoxCollider的Global中心点
            Vector3 boxOffset = currentRoom.right * boxCollider.center.x + currentRoom.forward * boxCollider.center.z + currentRoom.up * boxCollider.center.y;
            
            // Debug.Log(currentRoom.gameObject.name + ' ' + (currentRoom.position + boxOffset) + " " + Quaternion.identity);
            // Debug.Log(currentRoom.gameObject.name + ' ' + (currentRoom.position + boxCollider.center) + " " + boxCollider.transform.rotation);
            
            // 检查是否与其他房间发生碰撞
            Collider[] colliders = Physics.OverlapBox(currentRoom.position + boxOffset, boxCollider.bounds.extents, Quaternion.identity, LayerMask.GetMask("Tile"));

            if (colliders.Length <= 0)
            {
                return;
            }
            
            bool hasCollision = false;
            foreach (var collider in colliders)
            {
                if (collider.transform != currentRoom && collider.transform != previousRoom)
                {
                    hasCollision = true;
                    break;
                }
            }
            
            if (hasCollision)
            {
                // Debug.Log("Room collision: " + currentRoom.gameObject.name + " " + collider.transform.gameObject.name + " " + collider.name + " " + collider.transform.name + " " + collider.gameObject.name);
                attemptsTimes++;
                
                // 从generatedRooms中移除currentRoom对应的Room对象
                int index = generatedRooms.FindIndex(x => x.tile == currentRoom);
                if (generatedRooms[index].connector != null)
                    generatedRooms[index].connector.isConnected = false;
                generatedRooms.RemoveAt(index);
                
                // 立即！！！销毁当前房间（防止下一帧再Destroy干扰后续碰撞检测）
                DestroyImmediate(currentRoom.gameObject);

                // 无论如何都无法在此处生成房间，尝试重新生成之前的房间
                if (attemptsTimes >= maxAttemptsTimes)
                {
                    int preIndex = generatedRooms.FindIndex(x => x.tile == previousRoom);
                    Room generatedPreviousRoom = generatedRooms[preIndex];
                
                    if (previousRoom != rootRoom)
                    {
                        if (generatedPreviousRoom.connector != null)
                            generatedPreviousRoom.connector.isConnected = false;
                
                        availableConnectors.RemoveAll(c => c.transform.parent.parent == previousRoom);
                        generatedRooms.RemoveAt(preIndex);
                        DestroyImmediate(previousRoom.gameObject);
                
                        if (generatedPreviousRoom.preRoom != rootRoom)
                        {
                            previousRoom = generatedPreviousRoom.preRoom;
                        }
                        // 在主干中
                        else if (container.name.Contains("Main"))
                        {
                            if (generatedPreviousRoom.preRoom != null)
                            {
                                rootRoom = generatedPreviousRoom.preRoom;
                                previousRoom = rootRoom;
                            }
                        }
                        // 在分支中
                        else if (availableConnectors.Count > 0)
                        {
                            int connectorIndex = Random.Range(0, availableConnectors.Count);
                            rootRoom = availableConnectors[connectorIndex].transform.parent.parent;
                            availableConnectors.RemoveAt(connectorIndex);
                            previousRoom = rootRoom;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (container.name.Contains("Main"))
                    {
                        if (generatedPreviousRoom.preRoom != null)
                        {
                            rootRoom = generatedPreviousRoom.preRoom;
                            previousRoom = rootRoom;
                        }
                    }
                    else if (availableConnectors.Count > 0)
                    {
                        int connectorIndex = Random.Range(0, availableConnectors.Count);
                        rootRoom = availableConnectors[connectorIndex].transform.parent.parent;
                        availableConnectors.RemoveAt(connectorIndex);
                        previousRoom = rootRoom;
                    }
                    else
                    {
                        return;
                    }
                }
                
                // 重新生成当前房间，递归判断是否有碰撞
                if (previousRoom != null)
                {
                    if (generatedRooms.Count == mainLength - 1)
                    {
                        currentRoom = GenerateExitRoom();
                        ChangeRoomLightColor(currentRoom, Color.blue);
                    }
                    else
                    {
                        currentRoom = GenerateTile();
                        Color retryColor = container.name.Contains("Branch") ? Color.green : Color.yellow;
                        ChangeRoomLightColor(currentRoom, retryColor);
                    }
                    
                    ConnectRooms(previousRoom, currentRoom);
                    CheckRoomCollisions();
                }
            }
            else
            {
                attemptsTimes = 0;
            }
            
        }

        private void ConnectRooms(Transform preRoom, Transform curRoom)
        {
            Transform preConnector = GetRandomConnector(preRoom);
            Transform curConnector = GetRandomConnector(curRoom);

            if (preConnector == null || curConnector == null)
            {
                Debug.Log("No connector found!");
                return;
            }
            
            // 设置Parent并调整位置以实现对齐
            curConnector.SetParent(preConnector);
            curRoom.SetParent(curConnector);
            
            curConnector.localPosition = Vector3.zero;
            curConnector.localRotation = Quaternion.identity;
            curConnector.Rotate(0, 180, 0);
            
            // 恢复原先的Parent
            curRoom.SetParent(container);
            curConnector.SetParent(curRoom.Find("Connectors"));
            
            // 记录连接关系
            if (generatedRooms.Count > 0)
                generatedRooms[generatedRooms.Count - 1].connector = preConnector.GetComponent<Connector>();
        }

        private List<Connector> GetAvailableConnectors(Transform curContainer)
        {
            List<Connector> availableConnectors = new List<Connector>();
            foreach (var connector in curContainer.GetComponentsInChildren<Connector>())
            {
                if (!connector.isConnected && !availableConnectors.Contains(connector))
                {
                    availableConnectors.Add(connector);
                }
            }
            
            return availableConnectors;
        }

        private Transform GetRandomConnector(Transform tile)
        {
            if (tile == null)
            {
                return null;
            }
            
            List<Connector> connectorList = new List<Connector>(8);

            // 获取当前tile的所有未连接的Connector组件
            foreach (var connector in tile.GetComponentsInChildren<Connector>())
            {
                if (connector.isConnected == false)
                    connectorList.Add(connector);
            }

            if (connectorList.Count == 0)
            {
                return null;
            }
            
            int index = Random.Range(0, connectorList.Count);
            connectorList[index].isConnected = true;
            return connectorList[index].transform;
        }

        private Transform GenerateStartRoom()
        {
            int index = Random.Range(0, startRoomPrefabs.Length);
            // Debug.Log("起始房间Prefab位置：" + transform.position);
            GameObject startRoomGo = Instantiate(startRoomPrefabs[index], transform.position, Quaternion.identity, container);
            startRoomGo.name = "Start Room";
            
            float yRotation = Random.Range(0, 4) * 90f;
            startRoomGo.transform.Rotate(0, yRotation, 0);
            
            player.transform.LookAt(-startRoomGo.transform.forward);
            
            generatedRooms.Add(new Room(startRoomGo.transform, null));
            
            BoxCollider boxCollider = startRoomGo.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = startRoomGo.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
            }

            return startRoomGo.transform;
        }

        private Transform GenerateTile()
        {
            int index = Random.Range(0, tilePrefabs.Length);
            GameObject roomGo = Instantiate(tilePrefabs[index], transform.position, Quaternion.identity, container);
            roomGo.name = tilePrefabs[index].name;

            // 找到此tile的前一个tile
            Transform preRoom = null;
            foreach (var generatedRoom in generatedRooms)
            {
                if (generatedRoom.tile == previousRoom)
                    preRoom = generatedRoom.tile;
            }
            
            generatedRooms.Add(new Room(roomGo.transform, preRoom));
            
            BoxCollider boxCollider = roomGo.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = roomGo.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
            }
            
            return roomGo.transform;
        }
        
        private Transform GenerateExitRoom()
        {
            int index = Random.Range(0, exitRoomPrefabs.Length);
            GameObject roomGo = Instantiate(exitRoomPrefabs[index], transform.position, Quaternion.identity, container);
            roomGo.name = "Exit Room";

            // 找到此tile的前一个tile
            Transform preRoom = null;
            foreach (var generatedRoom in generatedRooms)
            {
                if (generatedRoom.tile == previousRoom)
                    preRoom = generatedRoom.tile;
            }
            
            generatedRooms.Add(new Room(roomGo.transform, preRoom));
            
            BoxCollider boxCollider = roomGo.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = roomGo.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
            }
            
            return roomGo.transform;
        }

        private void CleanRoomBoxColliders()
        {
            foreach (var generatedRoom in generatedRooms)
            {
                BoxCollider boxCollider = generatedRoom.tile.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    Destroy(boxCollider);
                }
            }
        }

        private void ChangeRoomLightColor(Transform room, Color color)
        {
            if (!useLightsForDebugging || !Application.isEditor)
            {
                return;
            }
            
            Light[] lights = room.GetComponentsInChildren<Light>();
            foreach (var light in lights)
            {
                if (startColor == Color.white) 
                    startColor = light.color;
                
                light.color = color;
            }
        }

        /// <summary>
        /// 恢复所有房间的灯光颜色
        /// </summary>
        private void RestoreRoomLightColor()
        {
            if (useLightsForDebugging && Application.isEditor && restoreLishtsAfterDebugging)
            {
                // 获取所有房间的灯光
                Light[] lights = transform.GetComponentsInChildren<Light>();
                foreach (var light in lights)
                {
                    light.color = startColor;
                }
            }
        }
        
        private void GenerateBlockeds()
        {
            foreach (var connector in transform.GetComponentsInChildren<Connector>())
            {
                if (!connector.isConnected)
                {
                    DoorRoot targetDoorRoot = null;
                    // 获取此房间的距离此Connector最近的DoorRoot组件
                    foreach (var doorRoot in connector.transform.parent.parent.GetComponentsInChildren<DoorRoot>())
                    {
                        float distance = (connector.transform.position - doorRoot.transform.position).magnitude;
                        if (distance < 3f)
                        {
                            targetDoorRoot = doorRoot;
                            break;
                        }
                    }

                    if (targetDoorRoot != null)
                    {
                        if (!targetDoorRoot.isOccupied)
                        {
                            // 阻挡房间的门，生成在DoorRoot的位置
                            if (targetDoorRoot.isSmallDoor)
                            {
                                int blockedIndex = Random.Range(0, blockedPrefabs.Length);
                                GameObject blockedGo = Instantiate(blockedPrefabs[blockedIndex],
                                    targetDoorRoot.transform.position, targetDoorRoot.transform.rotation, targetDoorRoot.transform);
                                blockedGo.name = blockedPrefabs[blockedIndex].name;
                                targetDoorRoot.isOccupied = true;
                            }
                            // 阻挡走廊的尽头，生成在Connector的位置
                            else
                            {
                                int blockedIndex = Random.Range(0, blockedPrefabs.Length);
                                GameObject blockedGo = Instantiate(blockedPrefabs[blockedIndex],
                                    connector.transform.position, connector.transform.rotation, connector.transform);
                                // blockedGo.transform.localPosition = new Vector3(0, 0, 1f);
                                blockedGo.name = blockedPrefabs[blockedIndex].name;
                            }

                        }
                    }
                    else
                    {
                        int blockedIndex = Random.Range(0, blockedPrefabs.Length);
                        GameObject blockedGo = Instantiate(blockedPrefabs[blockedIndex],
                            connector.transform.position, connector.transform.rotation, connector.transform);
                        // blockedGo.transform.localPosition = new Vector3(0, 0, 1f);
                        blockedGo.name = blockedPrefabs[blockedIndex].name;
                    }
            
                }
            }
        }
        
        private void GenerateDoors()
        {
            foreach (var connector in transform.GetComponentsInChildren<Connector>())
            {
                if (connector.isConnected)
                {
                    // 获取此房间的所有DoorRoot组件
                    foreach (var doorRoot in connector.transform.parent.parent.GetComponentsInChildren<DoorRoot>())
                    {
                        if (!doorRoot.isOccupied)
                        {
                            int rollNum = Random.Range(0, 101);
                            if (rollNum < doorGeneratePercent)
                            {
                                Vector2 doorSize = doorRoot.isSmallDoor ? doorRoot.smallSize : doorRoot.bigSize;
                                
                                Collider[] colliders = Physics.OverlapBox(doorRoot.transform.position + doorRoot.transform.up * 0.5f, 
                                    new Vector3(doorSize.x, 1f, doorSize.x), 
                                    Quaternion.identity, 
                                    LayerMask.GetMask("Door"));

                                if (colliders.Length == 0)
                                {
                                    GameObject doorPrefab = doorPrefabs[1];
                                    if (doorRoot.isSmallDoor)
                                    {
                                        doorPrefab = doorPrefabs[0];
                                    }
                                    GameObject doorGo = Instantiate(doorPrefab, doorRoot.transform.position, doorRoot.transform.rotation, doorRoot.transform);
                                    doorGo.name = doorPrefab.name;
                                    doorRoot.isOccupied = true;
                                }
                            }
                        }
                    }
                    
                    
                }
            }
        }
        
    }
}