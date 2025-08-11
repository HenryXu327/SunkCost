using UnityEngine;

namespace Player.Weapon
{
    public class WeaponSwitch : MonoBehaviour
    {
        [SerializeField]
        private WeaponSway weaponSway;
        
        [SerializeField]
        private AudioSource audioSource;
        
        private int currentWeapon = 0;
        private int weaponCount;
        public Gun CurrentWeapon { get; private set; }
        public static bool HeavyWeaponUnlocked = false;

        void Start()
        {
            weaponCount = transform.childCount;
            SelectWeapon(currentWeapon);
        }

        void Update()
        {
            if (GridInventorySystem.InventoryController.IsInventoryOpen)
                return;
            
            int previousWeapon = currentWeapon;

            // 鼠标滚轮切换
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                currentWeapon = (currentWeapon + 1) % weaponCount;
            }
            else if (scroll < 0f)
            {
                currentWeapon = (currentWeapon - 1 + weaponCount) % weaponCount;
            }

            // 数字键切换
            for (int i = 0; i < weaponCount && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    currentWeapon = i;
                }
            }

            if (previousWeapon != currentWeapon)
            {
                if (currentWeapon == 1 && !HeavyWeaponUnlocked)
                {
                    currentWeapon = previousWeapon;
                }
                else
                {
                    SelectWeapon(currentWeapon);
                }
            }
        }

        void SelectWeapon(int index)
        {
            // 停止当前武器的换弹
            for (int i = 0; i < weaponCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                {
                    Gun currentGun = transform.GetChild(i).GetComponent<Gun>();
                    if (currentGun != null)
                    {
                        currentGun.StopReload();
                    }
                    break;
                }
            }
            
            for (int i = 0; i < weaponCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == index);
            }
            
            // 更新当前武器引用
            CurrentWeapon = transform.GetChild(index).GetComponent<Gun>();
            
            audioSource.PlayOneShot(audioSource.clip);
            
            if (weaponSway != null)
            {
                weaponSway.StartWeaponSwitchAnimation();
            }
        }
    }
}