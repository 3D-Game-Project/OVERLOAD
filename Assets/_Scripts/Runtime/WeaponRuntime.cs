using System;
using UnityEngine;

public class WeaponRuntime
{
    public AttackPartsData AttackPartsData { get; private set; }

    private float _lastFireTime;


    public WeaponRuntime(AttackPartsData weaponData)
    {
        AttackPartsData = weaponData;
        _lastFireTime = -weaponData.FireCooldown;
    }

    public bool TryFire()
    {
        if (Time.time < _lastFireTime + AttackPartsData.FireCooldown) return false;

        return true;
    }

    public void RecordFire()
    {
        _lastFireTime = Time.time;
    }
}