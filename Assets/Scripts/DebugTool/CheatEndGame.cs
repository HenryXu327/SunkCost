using Dungeon;
using UnityEngine;
using Player.GridInventorySystem;
using Shop;

namespace DebugTool
{
    public class CheatEndGame : MonoBehaviour
    {
        [SerializeField]
        private GameObject menuUI;
        
        [SerializeField]
        private KeyCode openKey = KeyCode.Z;
        
        private void Update()
        {
            if (Input.GetKeyDown(openKey))
            {
                EndGame();
            }
        }

        public void EndGame()
        {
            var inventory = FindObjectOfType<InventoryController>();
            var shop = ShopUI.Instance ?? ShopUI.FindExisting();
            Debug.Log("Inventory found: " + (inventory != null));
            Debug.Log("ShopUI found: " + (shop != null));
            if (inventory != null && shop != null)
            {
                Debug.Log("Selling all items from inventory");
                shop.SellAllFromInventory(inventory);
            }
                
            DungeonGenerator generator = FindObjectOfType<DungeonGenerator>();
            if (generator != null)
            {
                generator.ClearGenerated();
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
                
            if (menuUI != null)
            {
                menuUI.SetActive(true);
            }
                
            
        }
    }
} 