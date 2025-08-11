using System;
using System.Collections;
using Monster;
using UnityEngine;
using Utility;

namespace Player.Weapon
{
    public class Gun : MonoBehaviour
    {
        [SerializeField]
        private LayerMask shootableLayers;
        
        [SerializeField]
        private float damage = 10f;
        
        [SerializeField]
        private float force = 100f;
        
        [SerializeField]
        private float range = 100f;
        
        [SerializeField]
        private float fireRate = 15f;
        
        [SerializeField]
        private int maxAmmo = 10;
        
        [SerializeField]
        public float reloadTime = 1f;
        
        [SerializeField]
        private Camera fpsCamera;
        
        [SerializeField]
        private ParticleSystem muzzleFlash;
        
        [SerializeField]
        private GameObject impactEffect;
        
        [SerializeField]
        private AudioSource audioSource;
        
        [SerializeField]
        private AudioClip shootSound;
        
        [SerializeField]
        private AudioClip reloadSound;
        
        [SerializeField]
        private WeaponSway weaponSway;
        [SerializeField]
        private float recoilAmount = 3f;
        
        // 摄像机后坐力上扬
        [SerializeField]
        private CameraController cameraController;

        private float nextTimeToFire = 0f;
        private int currentAmmo;
        private bool isReloading = false;
        private Coroutine reloadCoroutine;

        private void Start()
        {
            currentAmmo = maxAmmo;
        }

        private void Update()
        {
            if (GridInventorySystem.InventoryController.IsInventoryOpen)
                return;
            
            if (isReloading)
            {
                return;
            }
            
            if (currentAmmo <= 0 || Input.GetKeyDown(KeyCode.R))
            {
                StartReload();
                return;
            }
            
            if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
        }

        private void StartReload()
        {
            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
            }
            reloadCoroutine = StartCoroutine(Reload());
        }

        public void StopReload()
        {
            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
            }
            isReloading = false;
            
            if (weaponSway != null)
            {
                weaponSway.StopReloadAnimation();
            }
        }

        private IEnumerator Reload()
        {
            isReloading = true;
            
            if (weaponSway != null)
            {
                weaponSway.StartReloadAnimation();
            }
            
            audioSource.PlayOneShot(reloadSound);
            
            yield return new WaitForSeconds(reloadTime);
            
            if (isReloading) // 确保没有被中断
            {
                currentAmmo = maxAmmo;
            }
            
            isReloading = false;
            reloadCoroutine = null;
        }
        
        private void Shoot()
        {
            if (weaponSway != null)
            {
                weaponSway.AddRecoil(recoilAmount);
            }
            
            // 摄像机上扬
            if (cameraController != null)
            {
                cameraController.ApplyRecoil(recoilAmount * 0.5f);
            }
            
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play();
            
            audioSource.PlayOneShot(shootSound);
            
            currentAmmo--;
            
            RaycastHit hit;
            // 恢复使用摄像机正前方作为射线方向
            Vector3 shootDirection = fpsCamera.transform.forward;
            if (Physics.Raycast(fpsCamera.transform.position, shootDirection, out hit, range, shootableLayers, QueryTriggerInteraction.Ignore)) // 忽略是Trigger的Collider
            {
                Debug.Log("Hit: " + hit.transform.name);
                
                Health enemyHealth = hit.transform.GetComponent<Health>();
                if (enemyHealth!= null)
                {
                    enemyHealth.TakeDamage(damage);
                }

                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * force);
                }
                
                // 击中特效，使用对象池
                GameObject effectObject = ObjectPoolManager.SpawnObject(impactEffect, hit.point, Quaternion.LookRotation(hit.normal), ObjectPoolManager.PoolType.ParticleSystem);
                StartCoroutine(DestroyEffect(effectObject, 2f));

            }
        }

        private IEnumerator DestroyEffect(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            
            ObjectPoolManager.ReturnObjectToPool(obj, ObjectPoolManager.PoolType.ParticleSystem);
        }
    }
}