using System.Collections;
using System.Collections.Generic;
using Player.Weapon;
using UnityEngine;

namespace Player
{
    public class WeaponSway : MonoBehaviour
    {
        [Header("摇晃")]
        [SerializeField] private float swaySpeed = 2f;
        [SerializeField] private float swaySmooth = 8f;
        [Header("颠簸")]
        [SerializeField] private float bobAmount = 0.05f;
        [SerializeField] private float bobFrequency = 8f;
        [Header("后坐力")]
        [SerializeField] private float recoilRecovery = 10f;
        [SerializeField] private Transform crosshair;
        [SerializeField] private float crosshairRecoilMultiplier = 2f; // 后坐力倍率
        [Header("切枪动画")]
        [SerializeField] private float switchAnimationDuration = 0.3f;
        [Header("换弹动画")]
        [SerializeField] private float reloadAnimationDuration = 1f;

        private Quaternion initialLocalRotation;
        private Vector3 initialLocalPosition;
        private float bobTimer = 0f;
        private float currentRecoil = 0f;
        private float recoilVelocity = 0f;
        private bool isSwitchingWeapon = false;
        private float switchAnimationTimer = 0f;
        private Quaternion switchStartRotation;
        private bool isReloading = false;
        private float reloadAnimationTimer = 0f;
        private Quaternion reloadStartRotation;
        private WeaponSwitch weaponSwitch;
        // 准星初始位置
        private RectTransform crosshairRect;
        private Vector2 crosshairInitialAnchoredPosition;
        
        // 对外暴露当前后坐力值
        public float CurrentRecoil => currentRecoil;

        void Start()
        {
            initialLocalRotation = Quaternion.identity;
            initialLocalPosition = transform.localPosition;
            weaponSwitch = GetComponent<WeaponSwitch>();
            
            if (crosshair != null)
            {
                crosshairRect = crosshair as RectTransform;
                if (crosshairRect != null)
                {
                    crosshairInitialAnchoredPosition = crosshairRect.anchoredPosition;
                }

            }
        }

        void Update()
        {
            if (GridInventorySystem.InventoryController.IsInventoryOpen)
                return;
            
            if (isSwitchingWeapon)
            {
                UpdateWeaponSwitchAnimation();
            }
            else if (isReloading)
            {
                UpdateReloadAnimation();
            }
            else
            {
                UpdateNormalSway();
            }
        }

        private void UpdateReloadAnimation()
        {
            reloadAnimationTimer += Time.deltaTime;
            float progress = reloadAnimationTimer / reloadAnimationDuration;
            
            Quaternion reloadTargetRotation = Quaternion.Euler(-90f, initialLocalRotation.y, initialLocalRotation.z);
            if (progress >= 1f)
            {
                isReloading = false;
                transform.localRotation = initialLocalRotation;
            }
            else
            {
                if (progress < 0.33f)
                {
                    float t = progress / 0.33f;
                    transform.localRotation = Quaternion.Slerp(reloadStartRotation, reloadTargetRotation, t);
                }
                else if (progress < 0.66f)
                {
                    transform.localRotation = reloadTargetRotation;
                }
                else
                {
                    float t = (progress - 0.66f) / 0.34f;
                    transform.localRotation = Quaternion.Slerp(reloadTargetRotation, initialLocalRotation, t);
                }
            }
        }

        public void StartReloadAnimation()
        {
            isReloading = true;
            reloadAnimationTimer = 0f;
            reloadStartRotation = transform.localRotation;
            
            if (weaponSwitch != null && weaponSwitch.CurrentWeapon != null)
            {
                reloadAnimationDuration = weaponSwitch.CurrentWeapon.reloadTime;
            }
        }

        public void StopReloadAnimation()
        {
            isReloading = false;
            transform.localRotation = initialLocalRotation;
        }

        private void UpdateNormalSway()
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            float x = mouseDelta.x * swaySpeed;
            float y = mouseDelta.y * swaySpeed;
            Quaternion rotationX = Quaternion.AngleAxis(-x, Vector3.up);
            Quaternion rotationY = Quaternion.AngleAxis(y, Vector3.right);
            Quaternion swayRotation = initialLocalRotation * rotationX * rotationY;
            
            Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0);
            
            Quaternion targetRotation = swayRotation * recoilRotation;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * swaySmooth);

            ApplyBob();
            currentRecoil = Mathf.SmoothDamp(currentRecoil, 0f, ref recoilVelocity, 1f / recoilRecovery);

            // 更新准星受后坐力影响的偏移
            if (crosshair != null)
            {
                float offsetY = currentRecoil * crosshairRecoilMultiplier;
                if (crosshairRect != null)
                {
                    Vector2 target = crosshairInitialAnchoredPosition + Vector2.up * offsetY;
                    crosshairRect.anchoredPosition = Vector2.Lerp(crosshairRect.anchoredPosition, target, Time.deltaTime * swaySmooth);
                }
            }
        }

        private void UpdateWeaponSwitchAnimation()
        {
            switchAnimationTimer += Time.deltaTime;
            float progress = switchAnimationTimer / switchAnimationDuration;
            
            if (progress >= 1f)
            {
                isSwitchingWeapon = false;
                transform.localRotation = initialLocalRotation;
            }
            else
            {
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f); // 缓出效果
                transform.localRotation = Quaternion.Slerp(switchStartRotation, initialLocalRotation, easedProgress);
            }
        }

        public void StartWeaponSwitchAnimation()
        {
            isSwitchingWeapon = true;
            switchAnimationTimer = 0f;
            switchStartRotation = Quaternion.Euler(90f, initialLocalRotation.y, initialLocalRotation.z); // 枪口朝下
            transform.localRotation = switchStartRotation;
        }
        
        private void ApplyBob()
        {
            float move = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
            if (move > 0.1f)
            {
                bobTimer += Time.deltaTime * bobFrequency * (Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f);
            }
            else
            {
                bobTimer = 0f;
            }
            float bobOffset = Mathf.Sin(bobTimer) * bobAmount * (Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f);
            Vector3 targetPosition = initialLocalPosition + Vector3.up * bobOffset;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * swaySmooth);
        }

        public void AddRecoil(float amount)
        {
            currentRecoil += amount;
        }

       
    }
}

