using System;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class LaunchUI: MonoBehaviour
    {
        public InputField idInputField;
        public Button launchButton;

        private void Start()
        {
            
        }
        
        public void LaunchGame()
        {
            string id = idInputField.text;
            if (string.IsNullOrEmpty(id))
            {
                Debug.Log("Please enter a valid ID");
                return;
            }
            Debug.Log("Launching game with ID: " + id);
            
            Shop.ShopUI.FindExisting()?.InitializeForPlayer(id);
        }
    }
}