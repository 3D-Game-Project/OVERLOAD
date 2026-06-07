//using UnityEngine;

//public class BoosterPart : MonoBehaviour
//{
//    [Header("Data")]
//    [SerializeField] private BoosterData _boosterData;

//    private PlayerLocomotionMotor _motor;
//    private CoreEnergyController _corePower;
//    private PlayerInputHandler _playerInput;

//    public void Initialize(
//        PlayerLocomotionMotor locomotionMotor,
//        CoreEnergyController corePower,
//        PlayerInputHandler inputHandler)
//    {
//        _motor = locomotionMotor;
//        _corePower = corePower;
//        _playerInput = inputHandler;
//    }

//    private void Update()
//    {
//        if (!CanOperate())
//            return;

//        HandleAcceleration();
//        HandleJump();
//        HandleAirBoost();
//    }

//    private bool CanOperate()
//    {
//        return _boosterData != null
//            && _motor != null
//            && _corePower != null
//            && _playerInput != null;
//    }

//    private void HandleAcceleration()
//    {
//        if (!_playerInput.IsBoostHeld)
//            return;

//        float outputRatio = _corePower.PowerRatio;
//        float powerCost = _boosterData.accelerationPowerCostPerSecond * Time.deltaTime;

//        if (!_corePower.TryUsePower(powerCost))
//            return;

//        float finalMultiplier = Mathf.Lerp(
//            1f,
//            _boosterData.speedMultiplier,
//            outputRatio
//        );

//        _motor.RequestHorizontalSpeedMultiplier(finalMultiplier);
//    }

//    private void HandleJump()
//    {
//        if (!_playerInput.IsJumpPressed)
//            return;

//        if (!_motor.IsGrounded)
//            return;

//        float outputRatio = _corePower.PowerRatio;

//        if (!_corePower.TryUsePower(_boosterData.jumpPowerCost))
//            return;

//        float finalJumpVelocity = _boosterData.jumpVelocity * outputRatio;

//        _motor.ApplyJump(finalJumpVelocity);
//    }

//    private void HandleAirBoost()
//    {
//        if (_motor.IsGrounded)
//            return;

//        if (!_playerInput.IsJumpHeld)
//            return;

//        if (_boosterData.airMode == BoosterAirMode.FlyAndGlide)
//        {
//            HandleFlyOrGlide();
//        }
//        else
//        {
//            HandleGlide();
//        }
//    }

//    private void HandleFlyOrGlide()
//    {
//        float outputRatio = _corePower.PowerRatio;
//        float powerCost = _boosterData.flyPowerCostPerSecond * Time.deltaTime;

//        if (_corePower.TryUsePower(powerCost))
//        {
//            float finalFlyVelocity = _boosterData.flyUpVelocity * outputRatio;

//            _motor.SetVerticalVelocity(finalFlyVelocity);
//            _motor.IgnoreGroundSnapThisFrame();
//        }
//        else
//        {
//            HandleGlide();
//        }
//    }

//    private void HandleGlide()
//    {
//        float powerCost = _boosterData.glidePowerCostPerSecond * Time.deltaTime;

//        if (!_corePower.TryUsePower(powerCost))
//            return;

//        _motor.RequestFallSpeedLimit(_boosterData.glideFallSpeed);
//    }
//}