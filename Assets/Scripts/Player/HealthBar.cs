using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class HealthBar : MonoBehaviour
    { 
        public PlayerHealth playerHealth;
        public int numOfHearts;
        
        public List<Image> hearts;
        public Sprite fullHeart;
        public Sprite emptyHeart;
        public GameObject heartPrefab;

        private void Update()
        {
            numOfHearts = (int)playerHealth.maxHealth / 10;
            int healthCount = (int)playerHealth.currentHealth / 10;

            if (numOfHearts > hearts.Count)
            {
                GameObject newHeart = Instantiate(heartPrefab, transform);
                hearts.Add(newHeart.GetComponent<Image>());
                return;
            }

            
            for (int i = 0; i < numOfHearts; i++)
            {
                if (i < healthCount)
                {
                    hearts[i].sprite = fullHeart;
                }
                else
                {
                    hearts[i].sprite = emptyHeart;
                }

                if (i < numOfHearts)
                {
                    hearts[i].enabled = true;
                }
                else
                {
                    hearts[i].enabled = false;
                }
            }
        }
    }
}