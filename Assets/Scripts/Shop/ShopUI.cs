using UnityEngine;
using Player.GridInventorySystem;
using UnityEngine.UI;
using SaveAndLoad;
using Player.Weapon;

namespace Shop
{
    public class ShopUI : MonoBehaviour
    {
        public static ShopUI Instance { get; private set; }
        
        [SerializeField]
        private int currency;
        
        public int Currency => currency;
        
        [SerializeField]
        private Text currencyText;
        
        private string currentPlayerId;
        public string CurrentPlayerId => currentPlayerId;

        private float initialMaxHealth = -1f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UpdateCurrencyText();
        }
        
        public void Deposit(int amount)
        {
            if (amount <= 0)
            {
                return;
            }
            currency += amount;
            UpdateCurrencyText();
            SaveCurrencyIfPossible();
        }
        
        public void SellAllFromInventory(InventoryController inventory)
        {
            if (inventory == null)
            {
                return;
            }
            Deposit(inventory.SumValue);
            inventory.ClearInventory();
        }
        
        public bool TryPurchase(int cost)
        {
            if (cost <= 0)
            {
                return true;
            }
            if (currency < cost)
            {
                return false;
            }
            currency -= cost;
            UpdateCurrencyText();
            SaveCurrencyIfPossible();
            return true;
        }

        private void UpdateCurrencyText()
        {
            if (currencyText != null)
            {
                currencyText.text = currency.ToString();
            }
        }

        public static ShopUI FindExisting()
        {
            if (Instance != null)
            {
                return Instance;
            }
            var all = Resources.FindObjectsOfTypeAll<ShopUI>();
            foreach (var ui in all)
            {
                if (ui != null && ui.gameObject.scene.IsValid())
                {
                    return ui;
                }
            }
            return null;
        }

        public void InitializeForPlayer(string playerId)
        {
            Debug.Log("Initializing shop for player " + playerId);
            bool isDifferentPlayer = currentPlayerId != playerId;
            currentPlayerId = playerId;
            if (isDifferentPlayer)
            {
                initialMaxHealth = -1f;
            }
            if (string.IsNullOrEmpty(currentPlayerId)) return;
            if (SaveSystem.TryLoadPlayer(currentPlayerId, out var loaded))
            {
                currency = loaded.Money;
                UpdateCurrencyText();

                WeaponSwitch.HeavyWeaponUnlocked = loaded.HasHeavyWeapon;
                Debug.Log("Loading heavy weapon unlock state: " + WeaponSwitch.HeavyWeaponUnlocked);

                Player.PlayerHealth playerHealth = null;
                var allHealth = Resources.FindObjectsOfTypeAll<Player.PlayerHealth>();
                foreach (var h in allHealth)
                {
                    if (h != null && h.gameObject.scene.IsValid())
                    {
                        playerHealth = h;
                        break;
                    }
                }
                if (playerHealth != null)
                {
                    if (initialMaxHealth < 0f)
                    {
                        initialMaxHealth = playerHealth.maxHealth;
                    }
                    playerHealth.maxHealth = initialMaxHealth + loaded.HealthAddedAmount;
                    Debug.Log("Loading health added amount: " + loaded.HealthAddedAmount);
                }
            }
            else
            {
                var data = new PlayerData { Money = currency };
                SaveSystem.SavePlayer(currentPlayerId, data);
                WeaponSwitch.HeavyWeaponUnlocked = false;
            }
        }

        private void SaveCurrencyIfPossible()
        {
            if (string.IsNullOrEmpty(currentPlayerId)) return;
            PlayerData data;
            if (SaveSystem.TryLoadPlayer(currentPlayerId, out var existing))
            {
                data = existing;
            }
            else
            {
                data = new PlayerData();
            }
            data.Money = currency;
            SaveSystem.SavePlayer(currentPlayerId, data);
        }

        private void OnApplicationQuit()
        {
            SaveCurrencyIfPossible();
        }
    }
}