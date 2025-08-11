using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;

namespace Dungeon
{
    public class ToggleDoor : MonoBehaviour
    {
        private AudioSource audioSource;
        
        [SerializeField]
        private AudioClip openClip;
        [SerializeField]
        private AudioClip closeClip;
        
        private Animator animator;

        private bool isInZone;
        
        private bool robotOpen = false;

        private void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }
        
        private void Update()
        {
            if (isInZone && Input.GetKeyDown(KeyCode.E))
            {
                bool isOpen = animator.GetBool("isOpen");
                animator.SetBool("isOpen", !isOpen);

                audioSource.pitch = (isOpen? openClip.length : closeClip.length) / 1f;
                audioSource.PlayOneShot(isOpen? openClip : closeClip);
            }
            else if (robotOpen)
            {
                bool isOpen = animator.GetBool("isOpen");
                if (!isOpen)
                {
                    animator.SetBool("isOpen", true);
                    audioSource.pitch = openClip.length / 1f;
                    audioSource.PlayOneShot(openClip);
                }
                robotOpen = false;
            }
        }
        
        
        private void OnTriggerEnter(Collider other)
        {
            // TODO: 可以在这里加某些会开门的怪物的tag，可以在这里加开门的额外条件
            if (other.CompareTag("Player"))
            {
                isInZone = true;
            }
            else if (other.CompareTag("Robot"))
            {
                robotOpen = true;
                
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isInZone = false;
            }
        }
    }
}