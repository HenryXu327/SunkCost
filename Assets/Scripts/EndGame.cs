using System;
using DebugTool;
using Dungeon;
using Shop;
using UnityEngine;

namespace DefaultNamespace
{
    public class EndGame : MonoBehaviour
    {
    
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player"))
            {
                return;
            }
            
            FindObjectOfType<CheatEndGame>().EndGame();
            
            // var inventory = other.GetComponent<Player.GridInventorySystem.InventoryController>();
            // if (inventory != null && ShopUI.Instance != null)
            // {
            //     ShopUI.Instance.SellAllFromInventory(inventory);
            // }
            // DungeonGenerator generator = FindObjectOfType<DungeonGenerator>();
            // if (generator != null)
            // {
            //     generator.ClearGenerated();
            // }
        }
    }
}