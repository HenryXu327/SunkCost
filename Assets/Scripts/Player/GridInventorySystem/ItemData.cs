using UnityEngine;

namespace Player.GridInventorySystem
{
    [CreateAssetMenu(fileName = "Item Data", menuName = "Inventory/New Item Data")]
    public class ItemData : ScriptableObject
    {
        public string Name;
        
        public int Width;
        
        public int Height;
        
        public Sprite Icon;
        
        public GameObject Prefab;

        public int Value;
        
        public int Weight;
        
        public float GenerateProbability;

    }
}