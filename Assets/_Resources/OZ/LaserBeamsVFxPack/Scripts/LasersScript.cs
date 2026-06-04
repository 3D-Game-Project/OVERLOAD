using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LaserEffects
{
    public class LasersScript : MonoBehaviour
    {
        [Header("Laser Prefabs")]
        public GameObject[] laserPrefabs;
        public int currentLaserIndex = 0;

        [Header("Hit Effect Prefabs")]
        public GameObject[] hitEffectPrefabs;
        public int currentHitEffectIndex = 0;

        [Header("References")]
        public Transform shootingPoint;
        public float maxDistance = 15f;

        GameObject spawnedLaser;
        GameObject spawnedHitEffect;

        void Start()
        {
            SpawnLaser();
        }

        void Update()
        {
            // Fire laser
            if (Input.GetMouseButtonDown(0))
                EnableLaser();

            if (Input.GetMouseButton(0))
                UpdateLaser();

            if (Input.GetMouseButtonUp(0))
                DisableLaser();

            // Cycle laser on right-click
            if (Input.GetMouseButtonDown(1))
                CycleLaser();
        }

        // --- Laser spawning ---
        void SpawnLaser()
        {
            if (laserPrefabs.Length == 0) return;

            if (spawnedLaser != null)
                Destroy(spawnedLaser);

            spawnedLaser = Instantiate(
                laserPrefabs[currentLaserIndex],
                shootingPoint
            );

            spawnedLaser.SetActive(false);
        }

        void EnableLaser()
        {
            if (spawnedLaser != null)
                spawnedLaser.SetActive(true);
        }

        void UpdateLaser()
        {
            if (spawnedLaser == null) return;

            RaycastHit hit;
            Vector3 origin = shootingPoint.position;
            Vector3 direction = shootingPoint.forward;

            spawnedLaser.transform.position = origin;
            spawnedLaser.transform.forward = direction;

            if (Physics.Raycast(origin, direction, out hit, maxDistance))
            {
                SetLaserLength(hit.distance);
                SpawnHitEffect(hit.point, hit.normal);
            }
            else
            {
                SetLaserLength(maxDistance);
                DisableHitEffect();
            }
        }

        void DisableLaser()
        {
            if (spawnedLaser != null)
                spawnedLaser.SetActive(false);

            DisableHitEffect();
        }

        void SetLaserLength(float length)
        {
            Vector3 scale = spawnedLaser.transform.localScale;
            scale.z = length;
            spawnedLaser.transform.localScale = scale;
        }

        void SpawnHitEffect(Vector3 position, Vector3 normal)
        {
            if (hitEffectPrefabs.Length == 0) return;

            if (spawnedHitEffect == null)
            {
                spawnedHitEffect = Instantiate(
                    hitEffectPrefabs[currentHitEffectIndex]
                );
            }

            spawnedHitEffect.SetActive(true);
            spawnedHitEffect.transform.position = position;
            spawnedHitEffect.transform.rotation = Quaternion.LookRotation(normal);
        }

        void DisableHitEffect()
        {
            if (spawnedHitEffect != null)
                spawnedHitEffect.SetActive(false);
        }

        // --- Cycle laser on right-click ---
        void CycleLaser()
        {
            if (laserPrefabs.Length == 0) return;

            // Switch laser index
            currentLaserIndex++;
            if (currentLaserIndex >= laserPrefabs.Length)
                currentLaserIndex = 0;

            // Spawn the new laser
            SpawnLaser();

            // Automatically switch hit effect to match laser
            if (hitEffectPrefabs.Length > 0)
            {
                currentHitEffectIndex = currentLaserIndex % hitEffectPrefabs.Length;

                if (spawnedHitEffect != null)
                {
                    Destroy(spawnedHitEffect);
                    spawnedHitEffect = null;
                }
            }
        }


        // Optional: switch hit effect manually
        public void SwitchHitEffect(int index)
        {
            if (index < 0 || index >= hitEffectPrefabs.Length) return;

            currentHitEffectIndex = index;

            if (spawnedHitEffect != null)
            {
                Destroy(spawnedHitEffect);
                spawnedHitEffect = null;
            }
        }
    }
}
