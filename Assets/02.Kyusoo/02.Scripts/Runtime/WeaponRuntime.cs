using System;
using UnityEngine;

public class WeaponRuntime
{
    public AttackPartsData AttackPartsData { get; private set; }

    public int CurrentMagazine { get; private set; }
    private float _lastFireTime;
    public bool IsReloading { get; private set; }

    public event Action<int, int> OnMagazineChanged;
    public event Action OnReloadStarted;

    public WeaponRuntime(AttackPartsData weaponData)
    {
        AttackPartsData = weaponData;
        CurrentMagazine = weaponData.MaxMagazineSize;
        _lastFireTime = -weaponData.FireCooldown;
        IsReloading = false;
    }

    public bool TryFire()
    {
        if (IsReloading) return false;
        if (Time.time < _lastFireTime + AttackPartsData.FireCooldown) return false;

        if (AttackPartsData.FireType == FireType.Hitscan && AttackPartsData.MaxMagazineSize == 0)
        {
            _lastFireTime = Time.time;
            return true;
        }

        if (CurrentMagazine <= 0)
        {
            StartReload();
            return false; 
        }

        CurrentMagazine--;
        _lastFireTime = Time.time;

        OnMagazineChanged?.Invoke(CurrentMagazine, AttackPartsData.MaxMagazineSize);

        return true;
    }

    public void StartReload()
    {
        if (IsReloading) return;
        if (AttackPartsData.FireType == FireType.Hitscan && AttackPartsData.MaxMagazineSize == 0) return;

        IsReloading = true;
        OnReloadStarted?.Invoke();
    }

    public void CompleteReload()
    {
        CurrentMagazine = AttackPartsData.MaxMagazineSize;
        IsReloading = false;
        OnMagazineChanged?.Invoke(CurrentMagazine, AttackPartsData.MaxMagazineSize);
    }
}