using System.Collections.Generic;
using UnityEngine;
public class EnemyBehaviorBridge : MonoBehaviour
{
    [SerializeField] private EnemyData config;
    [SerializeField] private Transform[] patrolPoints;

    public EnemyData Config => config;
    public Transform[] PatrolPoints => patrolPoints;
    public bool HasConfig => config != null;
    public bool HasPatrolPoints => patrolPoints != null && patrolPoints.Length > 0;

    private List<FireManager> _equippedWeapons = new List<FireManager>();

    private void Awake()
    {
        TryPopulatePatrolPointsFromScene();
        UpdateEquippedWeapons();
    }

    private void TryPopulatePatrolPointsFromScene()
    {
        if (HasPatrolPoints)
        {
            return;
        }

        GameObject patrolRootObject = GameObject.Find("PatrolPoint");
        if (patrolRootObject == null) return;

        Transform patrolRoot = patrolRootObject.transform;
        if (patrolRoot.childCount <= 0) return;

        Transform[] discoveredPoints = new Transform[patrolRoot.childCount];
        for (int childIndex = 0; childIndex < patrolRoot.childCount; childIndex++)
        {
            discoveredPoints[childIndex] = patrolRoot.GetChild(childIndex);
        }

        patrolPoints = discoveredPoints;
    }

    public Vector3 GetPatrolPosition(int index)
    {
        if (!HasPatrolPoints)
            return transform.position;

        int safeIndex = Mathf.Abs(index) % patrolPoints.Length;
        Transform point = patrolPoints[safeIndex];
        return point != null ? point.position : transform.position;
    }

    // 현재 기체 하위에 부착된 무기 파츠 리스트 실시간 갱신
    // 기존 무기 목록 비운 후, 다시 할당
    public void UpdateEquippedWeapons()
    {
        _equippedWeapons.Clear();
        _equippedWeapons.AddRange(GetComponentsInChildren<FireManager>());
    }

    // 장착된 모든 무기 파츠중 가장 짧은 무기파츠의 공격사거리를 반환
    // => AI가 총을 최초 발사하는 시점을 가장 짧은 무기의 사거리 기준으로 처리하기 위해
    public float GetMinAttackRange()
    {
        if (_equippedWeapons == null || _equippedWeapons.Count <= 0)
        {
            UpdateEquippedWeapons();
        }

        if (_equippedWeapons.Count <= 0) return 0f;

        float minRange = float.MaxValue;
        bool hasValidWeapon = false;


        foreach (var weapon in _equippedWeapons)
        {
            if (weapon == null)
            {
                continue;
            }

            if (weapon.WeaponRuntime != null)
            {
                if (weapon.WeaponRuntime.AttackPartsData != null)
                {
                    float weaponRange = weapon.WeaponRuntime.AttackPartsData.Range;

                    if (weaponRange < minRange)
                    {
                        minRange = weaponRange;
                        hasValidWeapon = true;
                    }
                }
            }
        }
        return hasValidWeapon ? minRange : 2f;
    }

    // targetPoint를 기준으로 기체에 부착된 모든 무기파츠에 사격명령처리
    public void FireAllWeapons(Vector3 targetPoint)
    {
        Transform weaponBase = transform;

        Vector3 lookDirection = targetPoint - weaponBase.position;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            weaponBase.rotation = Quaternion.LookRotation(lookDirection);

            float angle = Vector3.Angle(weaponBase.forward, lookDirection);

            if (angle <= 15f)
            {
                foreach (var weapon in _equippedWeapons)
                {
                    weapon.TryFire(targetPoint);
                }
            }
        }
    }
}