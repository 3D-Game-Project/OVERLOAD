using UnityEngine;

public class BoosterPart : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BoosterData _boosterData;

    private PlayerLocomotionMotor _locomotionMotor;
    private CorePowerController _corePower;
    private PlayerInputHandler _inputHandler;

    public void Initialize(
        PlayerLocomotionMotor locomotionMotor,
        CorePowerController corePower,
        PlayerInputHandler inputHandler)
    {
        _locomotionMotor = locomotionMotor;
        _corePower = corePower;
        _inputHandler = inputHandler;
    }

    private void Update()
    {
        if (!CanOperate())
            return;

        HandleAcceleration();
        HandleJump();
        HandleAirBoost();
    }

    private bool CanOperate()
    {
        return _boosterData != null
            && _locomotionMotor != null
            && _corePower != null
            && _inputHandler != null;
    }

    private void HandleAcceleration()
    {
        if (!_inputHandler.IsBoostHeld)
            return;

        float outputRatio = _corePower.PowerRatio;
        float powerCost = _boosterData.accelerationPowerCostPerSecond * Time.deltaTime;

        if (!_corePower.TryUsePower(powerCost))
            return;

        float finalMultiplier = Mathf.Lerp(
            1f,
            _boosterData.speedMultiplier,
            outputRatio
        );

        _locomotionMotor.RequestHorizontalSpeedMultiplier(finalMultiplier);
    }

    private void HandleJump()
    {
        if (!_inputHandler.IsJumpPressed)
            return;

        if (!_locomotionMotor.IsGrounded)
            return;

        float outputRatio = _corePower.PowerRatio;

        if (!_corePower.TryUsePower(_boosterData.jumpPowerCost))
            return;

        float finalJumpVelocity = _boosterData.jumpVelocity * outputRatio;

        _locomotionMotor.ApplyJump(finalJumpVelocity);
    }

    private void HandleAirBoost()
    {
        if (_locomotionMotor.IsGrounded)
            return;

        if (!_inputHandler.IsJumpHeld)
            return;

        if (_boosterData.airMode == BoosterAirMode.FlyAndGlide)
        {
            HandleFlyOrGlide();
        }
        else
        {
            HandleGlide();
        }
    }

    private void HandleFlyOrGlide()
    {
        float outputRatio = _corePower.PowerRatio;
        float powerCost = _boosterData.flyPowerCostPerSecond * Time.deltaTime;

        if (_corePower.TryUsePower(powerCost))
        {
            float finalFlyVelocity = _boosterData.flyUpVelocity * outputRatio;

            _locomotionMotor.SetVerticalVelocity(finalFlyVelocity);
            _locomotionMotor.IgnoreGroundSnapThisFrame();
        }
        else
        {
            HandleGlide();
        }
    }

    private void HandleGlide()
    {
        float powerCost = _boosterData.glidePowerCostPerSecond * Time.deltaTime;

        if (!_corePower.TryUsePower(powerCost))
            return;

        _locomotionMotor.RequestFallSpeedLimit(_boosterData.glideFallSpeed);
    }
}