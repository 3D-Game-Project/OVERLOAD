using UnityEngine;

[System.Serializable]
public struct FireEffect
{
    public WeaponType WeaponType;
    public ParticleSystem FireParticle;
}

[System.Serializable]
public struct TakeDamageEffect
{
    public WeaponType WeaponType;
    public ParticleSystem TakeDamageParticle;
}

public enum WeaponType
{
    DoubleGun,
    Launcher,
    Shocker,
    Sniper,
    Shotgun,
    Minigun
}