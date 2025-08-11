using System.Collections.Generic;
using UnityEngine;

namespace Player.GridInventorySystem
{
    public class InventoryController : MonoBehaviour
    {
        public int SumValue;
        
        public int SumWeight;
        
        public int MaxWeight = 100;
        
        [SerializeField]
        private KeyCode toggleInventoryKey = KeyCode.B;
        
        [SerializeField]
        private KeyCode createItemKey = KeyCode.Alpha1;
        
        [SerializeField]
        private KeyCode insertItemKey = KeyCode.Alpha2;
        
        [SerializeField]
        private KeyCode rotateItemKey = KeyCode.Alpha3;
        
        [SerializeField]
        private List<ItemData> itemsData = new List<ItemData>();
        
        [SerializeField]
        private GameObject itemPrefab;

        [SerializeField] 
        private Transform inventoryCanvas;
        
        [SerializeField]
        private Transform playerTransform; // 玩家Transform，用于确定生成位置
        
        [SerializeField]
        private AudioSource uiAudioSource;
        
        [SerializeField]
        private List<AudioClip> inventorySounds = new List<AudioClip>();
        
        private InventoryGrids inventoryGrids;

        private InventoryHighlight inventoryHighlight;
        
        private Vector2Int oldHighlightPosition;
        
        private InventoryItem itemToHighlight;
        
        private InventoryItem selectedItem;

        private InventoryItem oldItem;
        
        private static bool isInventoryOpen = false;
        public static bool IsInventoryOpen => isInventoryOpen;
        
        private void Awake()
        {
            inventoryHighlight = GetComponent<InventoryHighlight>();
            // 先激活背包，确保Awake被调用
            bool wasActive = inventoryCanvas.gameObject.activeSelf;
            inventoryCanvas.gameObject.SetActive(true);

            inventoryGrids = inventoryCanvas.GetComponentInChildren<InventoryGrids>();
            if (inventoryGrids == null)
            {
                Debug.LogError("InventoryGrids not found under inventoryCanvas");
            }
            else
            {
                inventoryHighlight.SetParent(inventoryGrids);
            }

            // 恢复原状态
            inventoryCanvas.gameObject.SetActive(wasActive);
            
            // 如果没有设置playerTransform，尝试自动查找
            if (playerTransform == null)
            {
                playerTransform = FindObjectOfType<PlayerController>()?.transform;
            }
        }

        private void Start()
        {
            SumValue = 0;
            SumWeight = 0;
            MaxWeight = 100;
        }

        private void Update()
        {
            // B键开关背包
            if (Input.GetKeyDown(toggleInventoryKey))
            {
                bool wasOpen = isInventoryOpen;
                isInventoryOpen = !isInventoryOpen;
                inventoryCanvas.gameObject.SetActive(isInventoryOpen);
                Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = isInventoryOpen;
                
                // 如果背包正在关闭且有selectedItem，将其放置到场景中
                if (wasOpen && !isInventoryOpen && selectedItem != null)
                {
                    PlaceSceneItem();
                }
            }
            // 背包未开启时不处理背包逻辑
            if (!isInventoryOpen)
                return;

            DragItemIcon();

            if (Input.GetKeyDown(createItemKey))
            {
                if (selectedItem == null)
                {
                    CreateRandomItem();
                    oldHighlightPosition = Vector2Int.zero;
                }
            }

            if (Input.GetKeyDown(insertItemKey))
            {
                InsertRandomItem();
            }

            if (Input.GetKeyDown(rotateItemKey))
            {
                if (selectedItem != null)
                {
                    selectedItem.RotateItem();
                }
                oldHighlightPosition = Vector2Int.zero;
            }
            
            if (inventoryGrids == null)
            {
                inventoryHighlight.Show(false);
                return;
            }
            
            HandleHighlight();

            if (Input.GetMouseButtonDown(0))
            {
                LeftMouseButtonPress();
            }
        }

        private void CreateRandomItem()
        {
            InventoryItem item = Instantiate(itemPrefab).GetComponent<InventoryItem>();
            selectedItem = item;
            
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.SetParent(inventoryCanvas);
            itemRect.SetAsLastSibling();
            
            int randomIndex = Random.Range(0, itemsData.Count);
            item.SetItemData(itemsData[randomIndex]);
        }

        private void InsertRandomItem()
        {
            if (inventoryGrids == null || selectedItem != null)
            {
                return;
            }

            CreateRandomItem();
            InventoryItem item = selectedItem;
            selectedItem = null;
            
            Vector2Int? spacePosOnGrid = inventoryGrids.FindSpaceForItem(item);

            if (spacePosOnGrid == null)
            {
                Debug.LogWarning("No space found for item");
                Destroy(item.gameObject);
                return;
            }

            inventoryGrids.PlaceItem(item, spacePosOnGrid.Value.x, spacePosOnGrid.Value.y);
        }

        private void DragItemIcon()
        {
            if (selectedItem != null)
            {
                RectTransform itemRect = selectedItem.GetComponent<RectTransform>();
                itemRect.position = Input.mousePosition;
                itemRect.SetParent(inventoryCanvas);
            }
        }
        
        private void LeftMouseButtonPress()
        {
            Vector2Int tileGridPosition = MousePosition2TileGridPosition();

            if (selectedItem == null)
            {
                PickUpItem(tileGridPosition);
            }
            else
            {
                if (IsSelectedItemOutSideInventory())
                {
                    PlaceSceneItem();
                }
                else
                {
                    PlaceItem(tileGridPosition);
                }
            }
        }

        private void PickUpItem(Vector2Int tileGridPosition)
        {
            selectedItem = inventoryGrids.PickUpItem(tileGridPosition.x, tileGridPosition.y);
        }

        private void PlaceItem(Vector2Int tileGridPosition)
        {
            bool success = inventoryGrids.TryPlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, ref oldItem);
            if (success)
            {
                selectedItem = null;
                if (oldItem != null)
                {
                    selectedItem = oldItem;
                    oldItem = null;
                    RectTransform itemRect = selectedItem.GetComponent<RectTransform>();
                    itemRect.SetAsLastSibling();
                }
            }
        }
        
        private Vector2Int MousePosition2TileGridPosition()
        {
            Vector2 position = Input.mousePosition;

            if (selectedItem != null)
            {
                position.x -= (selectedItem.Width - 1) * InventoryGrids.TileWidth / 2;
                position.y += (selectedItem.Height - 1) * InventoryGrids.TileHeight / 2;
            }

            return inventoryGrids.GetTilePositionOnMouse(position);
        }

        private bool IsSelectedItemOutSideInventory()
        {
            if (selectedItem == null)
            {
                return false;
            }
            
            Vector2Int tileGridPosition = MousePosition2TileGridPosition();

            if (inventoryGrids.IsPositionAvailable(tileGridPosition.x, tileGridPosition.y))
            {
                return false;
            }
            
            return true;
        }

        private void HandleHighlight()
        {
            Vector2Int tileGridPosition = MousePosition2TileGridPosition();
            
            // Debug.Log("Tile grid position: " + tileGridPosition);

            if (oldHighlightPosition == tileGridPosition)
            {
                return;
            }
            
            oldHighlightPosition = tileGridPosition;

            if (selectedItem == null)
            {
                itemToHighlight = inventoryGrids.GetItemAtPosition(tileGridPosition.x, tileGridPosition.y);

                if (itemToHighlight != null)
                {
                    inventoryHighlight.Show(true);
                    inventoryHighlight.SetSize(itemToHighlight);
                    inventoryHighlight.SetPosition(itemToHighlight, inventoryGrids);
                }
                else
                {
                    inventoryHighlight.Show(false);
                }
            }
            else
            {
                bool show = inventoryGrids.IsItemPositionAvailable(tileGridPosition.x, tileGridPosition.y,
                    selectedItem.Width, selectedItem.Height);
                inventoryHighlight.Show(show);
                inventoryHighlight.SetSize(selectedItem);
                inventoryHighlight.SetPosition(selectedItem, inventoryGrids, tileGridPosition.x, tileGridPosition.y);
            }
            
        }

        public bool PickUpSceneItem(string itemName)
        {
            Debug.Log("Picking up scene item: " + itemName);
            if (inventoryGrids == null)
            {
                Debug.LogWarning("Inventory grid not set");
                return false;
            }
            
            InventoryItem item = Instantiate(itemPrefab).GetComponent<InventoryItem>();
            ItemData data = itemsData.Find(d => d.Name == itemName);
            if (data == null)
            {
                Debug.LogError("Item not found in inventory data: " + itemName);
                Destroy(item.gameObject);
                return false;
            }
            
            item.SetItemData(data);
            
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.SetParent(inventoryCanvas);
            itemRect.SetAsLastSibling();
            
            Vector2Int? spacePosOnGrid = inventoryGrids.FindSpaceForItem(item);

            if (spacePosOnGrid == null)
            {
                Debug.LogWarning("No space found for item: " + itemName);
                Destroy(item.gameObject);
                return false;
            }
            
            Debug.Log("Placing item at: " + spacePosOnGrid.Value);
            inventoryGrids.PlaceItem(item, spacePosOnGrid.Value.x, spacePosOnGrid.Value.y);
            
            SumValue += data.Value;
            SumWeight += data.Weight;
            
            uiAudioSource.PlayOneShot(inventorySounds[0]);
            
            return true;
        }

        public void PlaceSceneItem()
        {
            if (selectedItem == null)
                return;
            
            // 获取物品数据
            ItemData itemData = selectedItem.itemData;
            string itemName = itemData.Name;
            
            // 在玩家面前生成场景物品
            if (playerTransform != null)
            {
                // 计算生成位置：玩家前方2米处的地面
                Vector3 spawnPosition = playerTransform.position + playerTransform.forward * 2f;
                
                spawnPosition.y -= 1.0f;
                
                // TODO: 用对象池
                GameObject sceneItem = Instantiate(itemData.Prefab, spawnPosition, Quaternion.identity);
                sceneItem.name = itemName;
                
                Debug.Log("Placed scene item: " + itemName + " at position: " + spawnPosition);
                
                uiAudioSource.PlayOneShot(inventorySounds[0]);
            }
            else
            {
                Debug.LogWarning("Cannot place scene item: playerTransform or sceneItemPrefab is null");
            }
            
            // 删除背包中的物品
            Destroy(selectedItem.gameObject);
            selectedItem = null;
            
            SumValue -= itemData.Value;
            SumWeight -= itemData.Weight;
        }
        
        public void ClearInventory()
        {
            // 销毁背包 UI 下的所有物品（包括未放置与已放置的物品）
            if (inventoryCanvas != null)
            {
                InventoryItem[] items = inventoryCanvas.GetComponentsInChildren<InventoryItem>(true);
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] != null)
                    {
                        Destroy(items[i].gameObject);
                    }
                }
            }

            // 重置选择与高亮状态
            selectedItem = null;
            oldItem = null;
            itemToHighlight = null;
            oldHighlightPosition = Vector2Int.zero;
            if (inventoryHighlight != null)
            {
                inventoryHighlight.Show(false);
            }

            // 清零数值
            SumValue = 0;
            SumWeight = 0;
        }
        
    }
}