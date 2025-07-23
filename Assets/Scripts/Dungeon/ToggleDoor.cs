using UnityEngine;

namespace Dungeon
{
    public class ToggleDoor : MonoBehaviour
    {
        private Animator animator;

        private bool isInZone;

        private void Start()
        {
            animator = GetComponent<Animator>();
        }
        
        private void Update()
        {
            if (isInZone && Input.GetKeyDown(KeyCode.E))
            {
                bool isOpen = animator.GetBool("isOpen");
                animator.SetBool("isOpen", !isOpen);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // TODO: 可以在这里加某些会开门的怪物的tag，可以在这里加开门的额外条件
            if (other.CompareTag("Player"))
            {
                isInZone = true;
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