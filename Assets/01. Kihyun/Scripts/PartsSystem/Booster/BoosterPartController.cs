using UnityEngine;

public class BoosterPartController : PartBehaviour, IBoosterPart
{
    private BoosterPartsData _boosterData;

    public override void Initialize(PartsData data, CorePartContext context)
    {
        base.Initialize(data, context);

        if (!TryGetData(out _boosterData))
        {
            Debug.LogError($"{gameObject.name}에는 BoosterPartsData가 필요합니다.");
            return;
        }

        Debug.Log(
            $"부스터 파츠 초기화 완료\n" +
            $"Parts Name: {_boosterData.PartsName}\n" +
            $"Function: {_boosterData.BoosterFunction}"
        );
    }

    public void HandleBooster(BoosterCommand command, float deltaTime)
    {
        if (!IsInitialized())
            return;

        if (_boosterData == null)
            return;

        if (_context.MovementCoordinator == null)
            return;

        HandleJump(command);
        HandleDash(command, deltaTime);
        HandleFlightOrGlide(command, deltaTime);
    }

    public void StopBooster()
    {
    }

    private void HandleJump(BoosterCommand command)
    {
        if (!command.JumpPressed)
            return;

        if (!_boosterData.HasFunction(BoosterFunction.Jump))
            return;

        if (_context.LocomotionMotor == null)
            return;

        if (!_context.LocomotionMotor.IsGrounded)
            return;

        if (!TryUseEnergy(_boosterData.JumpEnergyCost))
            return;

        _context.MovementCoordinator.RequestJump(
            _boosterData.JumpVelocity
        );
    }

    private void HandleDash(BoosterCommand command, float deltaTime)
    {
        if (!command.BoostHeld)
            return;

        if (!_boosterData.HasFunction(BoosterFunction.Dash))
            return;

        if (command.MoveDirection.sqrMagnitude < 0.001f)
            return;

        if (!TryUseEnergyPerSec(_boosterData.DashEnergyCostPerSec))
            return;

        Vector3 dashVelocity =
            command.MoveDirection.normalized * _boosterData.DashSpeed;

        _context.MovementCoordinator.SubmitAdditionalHorizontalVelocity(
            dashVelocity
        );
    }

    private void HandleFlightOrGlide(BoosterCommand command, float deltaTime)
    {
        if (!command.JumpHeld)
            return;

        if (_context.LocomotionMotor == null)
            return;

        if (_context.LocomotionMotor.IsGrounded)
            return;

        if (_boosterData.HasFunction(BoosterFunction.Flight))
        {
            HandleFlight(command, deltaTime);
            return;
        }

        if (_boosterData.HasFunction(BoosterFunction.Glide))
        {
            HandleGlide(deltaTime);
        }
    }

    private void HandleFlight(BoosterCommand command, float deltaTime)
    {
        if (!TryUseEnergyPerSec(_boosterData.FlightEnergyCostPerSec))
            return;

        _context.MovementCoordinator.SubmitVerticalAcceleration(
            _boosterData.FlightVerticalAcceleration,
            deltaTime
        );

        if (command.MoveDirection.sqrMagnitude > 0.001f)
        {
            Vector3 flightHorizontalVelocity =
                command.MoveDirection.normalized *
                _boosterData.FlightHorizontalSpeed;

            _context.MovementCoordinator.SubmitAdditionalHorizontalVelocity(
                flightHorizontalVelocity
            );
        }
    }

    private void HandleGlide(float deltaTime)
    {
        if (!TryUseEnergyPerSec(_boosterData.GlideEnergyCostPerSec))
            return;

        _context.MovementCoordinator.RequestFallSpeedLimit(
            _boosterData.GlideFallSpeedLimit
        );
    }

    private bool TryUseEnergy(float amount)
    {
        if (_context.EnergyController == null)
            return true;

        return _context.EnergyController.TryUseEnergy(amount);
    }

    private bool TryUseEnergyPerSec(float costPerSec)
    {
        if (_context.EnergyController == null)
            return true;

        return _context.EnergyController.TryUseEnergyPerSec(costPerSec);
    }

    public override void OnDetached()
    {
        StopBooster();
    }

    private void OnDisable()
    {
        StopBooster();
    }
}