using System;
using Dungeon;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public class LoadingUI : MonoBehaviour
    {
        [SerializeField]
        private DungeonGenerator dungeonGenerator;

        private void Update()
        {
            if (dungeonGenerator != null)
            {
                if (dungeonGenerator.GeneratingState == GeneratingState.Done)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    gameObject.SetActive(false);
                    
                }
            }
        }
    }
}