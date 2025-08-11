using UnityEngine;

namespace Player.GridInventorySystem
{
    public class PickableItem : MonoBehaviour
    {
        public string itemName;// 物品名，需与InventoryController的itemsData匹配

        [SerializeField]
        private float pickUpRange = 2f;
        
        [SerializeField]
        private bool playerInRange = false;
        
        [SerializeField]
        private InventoryController inventoryController;
        
        private SphereCollider sphereCollider;
        
        private Transform playerTransform;

        private void Start()
        {
            inventoryController = FindObjectOfType<InventoryController>();
            
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.radius = pickUpRange;
        }

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                if (inventoryController != null)
                {
                    bool picked = inventoryController.PickUpSceneItem(itemName);
                    if (picked)
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    inventoryController = playerTransform.GetComponent<InventoryController>();
                    bool picked = inventoryController.PickUpSceneItem(itemName);
                    if (picked)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerTransform = other.transform;
                playerInRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }
    }
} 