using UnityEngine;
using UnityEngine.UI;

namespace Player.GridInventorySystem
{
    public class InventoryItem : MonoBehaviour
    {
        public ItemData itemData;

        public int Height
        {
            get
            {
                if (!hasRotated)
                {
                    return itemData.Height;
                }
                
                return itemData.Width;
            }
        }

        public int Width
        {
            get
            {
                if (!hasRotated)
                {
                    return itemData.Width;
                }
                
                return itemData.Height;
            }
        }
        
        public bool hasRotated = false;
        
        public int PositionOnGridX;
        
        public int PositionOnGridY;

        public void SetItemData(ItemData itemData)
        {
            this.itemData = itemData;

            GetComponent<Image>().sprite = itemData.Icon;
            GetComponent<RectTransform>().sizeDelta = new Vector2(itemData.Width * InventoryGrids.TileWidth, itemData.Height * InventoryGrids.TileHeight);
        }
        
        public void RotateItem()
        {
            hasRotated = !hasRotated;
            
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.rotation = Quaternion.Euler(0, 0, hasRotated? 90f : 0);
        }
    }
}