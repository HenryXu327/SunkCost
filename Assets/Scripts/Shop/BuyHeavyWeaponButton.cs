using UnityEngine;
using DefaultNamespace;
using SaveAndLoad;

namespace Shop
{
    public class BuyHeavyWeaponButton : MonoBehaviour
    {
        public int price = 1000;
        
        public void OnClickPurchase()
        {
            if (ShopUI.Instance != null && ShopUI.Instance.TryPurchase(price))
            {
                Player.Weapon.WeaponSwitch.HeavyWeaponUnlocked = true;

                var playerId = ShopUI.Instance.CurrentPlayerId;
                if (!string.IsNullOrEmpty(playerId))
                {
                    if (!SaveSystem.TryLoadPlayer(playerId, out var data))
                    {
                        data = new PlayerData();
                    }
                    data.HasHeavyWeapon = true;
                    SaveSystem.SavePlayer(playerId, data);
                }
            }
        }
    }
} 