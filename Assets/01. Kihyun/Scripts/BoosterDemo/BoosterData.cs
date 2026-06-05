using UnityEngine;

public enum BoosterAirMode
{
    GlideOnly,
    FlyAndGlide
}

[CreateAssetMenu(menuName = "Parts/Booster Data")]
public class BoosterData : ScriptableObject
{
    [Header("Air Mode")]
    public BoosterAirMode airMode = BoosterAirMode.GlideOnly;

    [Header("Acceleration")]
    public float speedMultiplier = 1.5f;
    public float accelerationPowerCostPerSecond = 5f;

    [Header("Jump")]
    public float jumpVelocity = 8f;
    public float jumpPowerCost = 10f;

    [Header("Glide")]
    public float glideFallSpeed = -2f;
    public float glidePowerCostPerSecond = 3f;

    [Header("Fly")]
    public float flyUpVelocity = 3f;
    public float flyPowerCostPerSecond = 8f;
}