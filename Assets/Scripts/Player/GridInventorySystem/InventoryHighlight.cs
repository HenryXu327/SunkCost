using UnityEngine;

namespace Player.GridInventorySystem
{
    public class InventoryHighlight : MonoBehaviour
    {
        [SerializeField]
        private RectTransform highlightRect;

        public void Show(bool show)
        {
            highlightRect.gameObject.SetActive(show);
        }

        public void SetSize(InventoryItem item)
        {
            highlightRect.sizeDelta = new Vector2(item.Width * InventoryGrids.TileWidth, item.Height * InventoryGrids.TileHeight);
        }

        public void SetPosition(InventoryItem item, InventoryGrids grid)
        {
            if (grid != null)
            {
                highlightRect.SetParent(grid.GetComponent<RectTransform>());
            }

            Vector2 position = grid.CalculatePositionOnGrid(item, item.PositionOnGridX, item.PositionOnGridY);
            highlightRect.localPosition = position;
        }
        
        public void SetPosition(InventoryItem item, InventoryGrids grid, int posX, int posY)
        {
            Vector2 position = grid.CalculatePositionOnGrid(item, posX, posY);
            highlightRect.localPosition = position;
        }

        public void SetParent(InventoryGrids grid)
        {
            if (grid != null)
            {
                highlightRect.SetParent(grid.GetComponent<RectTransform>());
            }
        }
    }
}