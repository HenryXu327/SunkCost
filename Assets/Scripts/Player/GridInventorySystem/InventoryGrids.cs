using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Player.GridInventorySystem
{
    public class InventoryGrids : MonoBehaviour
    {
        public const float TileWidth = 32;
        public const float TileHeight = 32;
        
        private InventoryItem[,] inventoryItemsArray;

        [SerializeField] 
        private int GridsWidth = 20;
        
        [SerializeField] 
        private int GridsHeight = 10;
        
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            Init(GridsWidth, GridsHeight);
        }

        private void Init(int width, int height)
        {
            inventoryItemsArray = new InventoryItem[width, height];
            
            rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width * TileWidth, height * TileHeight);
        }

        public Vector2Int GetTilePositionOnMouse(Vector2 mousePosition)
        {
            float offsetX = mousePosition.x - rectTransform.position.x;
            float offsetY = rectTransform.position.y - mousePosition.y;
            
            int tileX = (int) (offsetX / TileWidth);
            int tileY = (int) (offsetY / TileHeight);

            return new Vector2Int(tileX, tileY);
        }

        public bool TryPlaceItem(InventoryItem item, int posX, int posY, ref InventoryItem oldItem)
        {
            if (!IsItemPositionAvailable(posX, posY, item.Width, item.Height))
            {
                return false;
            }

            // TODO: ???
            if (!OverlapCheck(posX, posY, item.Width, item.Height, ref oldItem))
            {
                oldItem = null;
                return false;
            }

            if (oldItem != null)
            {
                ClearItem(oldItem);
            }
            
            PlaceItem(item, posX, posY);
            
            return true;
        }
        

        public void PlaceItem(InventoryItem item, int posX, int posY)
        {
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.SetParent(this.rectTransform);

            for (int x = 0; x < item.Width; x++)
            {
                for (int y = 0; y < item.Height; y++)
                {
                    inventoryItemsArray[posX + x, posY + y] = item; // 这些格子里都是这个item
                }
            }
            
            item.PositionOnGridX = posX;
            item.PositionOnGridY = posY;
            
            itemRect.localPosition = CalculatePositionOnGrid(item, posX, posY);
        }

        public InventoryItem GetItemAtPosition(int posX, int posY)
        {
            if (!IsPositionAvailable(posX, posY))
            {
                return null;
            }
            return inventoryItemsArray[posX, posY];
        }

        private void ClearItem(InventoryItem item)
        {
            for (int x = 0; x < item.Width; x++)
            {
                for (int y = 0; y < item.Height; y++)
                {
                    inventoryItemsArray[item.PositionOnGridX + x, item.PositionOnGridY + y] = null;
                }
            }
        }

        public InventoryItem PickUpItem(int posX, int posY)
        {
            InventoryItem item = GetItemAtPosition(posX, posY);

            if (item != null)
            {
                ClearItem(item);
                return item;
            }
            
            return null;
        }

        public bool IsItemPositionAvailable(int posX, int posY, int itemWidth, int itemHeight)
        {
            if (!IsPositionAvailable(posX, posY) || !IsPositionAvailable(posX + itemWidth - 1, posY + itemHeight - 1))
            {
                return false;
            }
            
            return true;
        }

        public bool IsPositionAvailable(int posX, int posY)
        {
            if (posX < 0 || posX >= GridsWidth || posY < 0 || posY >= GridsHeight)
            {
                return false;
            }

            return true;
        }

        public Vector2 CalculatePositionOnGrid(InventoryItem item, int posX, int posY)
        {
            float x = posX * TileWidth + item.Width * TileWidth / 2;
            float y = -(posY * TileHeight + item.Height * TileHeight / 2);
            
            return new Vector2(x, y);
        }

        public Vector2Int? FindSpaceForItem(InventoryItem item)
        {
            int width = GridsWidth - item.Width + 1;
            int height = GridsHeight - item.Height + 1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (CanItemPlaceAtPosition(x, y, item.Width, item.Height))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            
            return null;
        }

        private bool CanItemPlaceAtPosition(int posX, int posY, int itemWidth, int itemHeight)
        {
            if (!IsItemPositionAvailable(posX, posY, itemWidth, itemHeight))
            {
                return false;
            }

            for (int x = 0; x < itemWidth; x++)
            {
                for (int y = 0; y < itemHeight; y++)
                {
                    if (inventoryItemsArray[posX + x, posY + y] != null)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        private bool OverlapCheck(int posX, int posY, int width, int height, ref InventoryItem overlapItem)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (inventoryItemsArray[posX + x, posY + y] != null)
                    {
                        if (overlapItem == null)
                        {
                            overlapItem = inventoryItemsArray[posX + x, posY + y];
                        }
                        else if (overlapItem != inventoryItemsArray[posX + x, posY + y])
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}