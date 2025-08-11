using UnityEngine;
using DefaultNamespace;
using SaveAndLoad;

namespace Shop
{
    public class BuyMaxHealthButton : MonoBehaviour
    {
        [SerializeField]
        private int price = 500;
        
        [SerializeField]
        private float addMaxHealth = 20f;
        
        [SerializeField]
        private Player.PlayerHealth playerHealth;
        
        public void OnClickPurchase()
        {
            if (ShopUI.Instance == null)
            {
                return;
            }
            
            if (!ShopUI.Instance.TryPurchase(price))
            {
                return;
            }
            
            if (playerHealth == null)
            {
                playerHealth = FindObjectOfType<Player.PlayerHealth>();
            }
            
            if (playerHealth != null)
            {
                Debug.Log("Max health increased by " + addMaxHealth);
                playerHealth.maxHealth += addMaxHealth;

                var playerId = ShopUI.Instance.CurrentPlayerId;
                if (!string.IsNullOrEmpty(playerId))
                {
                    if (!SaveSystem.TryLoadPlayer(playerId, out var data))
                    {
                        data = new PlayerData();
                    }
                    data.HealthAddedAmount += Mathf.RoundToInt(addMaxHealth);
                    SaveSystem.SavePlayer(playerId, data);
                }
            }
        }
    }
} 