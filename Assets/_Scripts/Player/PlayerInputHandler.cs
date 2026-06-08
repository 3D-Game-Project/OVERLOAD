using UnityEngine;
using UnityEngine.InputSystem;

// 플레이어 관련 모든 input은 여기서 관리
// 이 클래스를 받아서 public 변수들을 통해 그 인풋의 상태를 파악 가능
// inputaction의 인터페이스를 받아와서 그 인터페이스를 구현하는 방식
public class PlayerInputHandler : MonoBehaviour, GameInputAction.IPlayerInputMapActions
{
    public GameInputAction GameInput { get; private set;
    }
    public Vector2 MoveInput { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsJumpHeld { get; private set; }
    public bool IsBoostPressed { get; private set; }
    public bool IsBoostHeld { get; private set; }

    public bool InventoryTriggered { get; set; }
    public bool IsPickupPressed { get; set; }

    public bool MenuTriggered { get; set; }
    public bool InteractTriggered { get; set; }

    public bool IsFire { get; private set; }

    public bool IsPreview { get; set; } = false;

    public bool ReloadTriggered { get; set; }

    private void Awake()
    {
        GameInput = new GameInputAction();
        GameInput.PlayerInputMap.SetCallbacks(this);
    }

    private void OnEnable()
    {
        GameInput.PlayerInputMap.Enable();
    }

    private void OnDisable()
    {
        GameInput.PlayerInputMap.Disable();
    }
    private void OnDestroy()
    {
        GameInput?.Dispose();
    }

    private void LateUpdate()
    {
        IsJumpPressed = false;
        IsBoostPressed = false;

        ReloadTriggered = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsJumpPressed = true;
            IsJumpHeld = true;
        }

        if (context.canceled)
        {
            IsJumpHeld = false;
        }
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsBoostPressed = true;
            IsBoostHeld = true;
        }

        if (context.canceled)
        {
            IsBoostHeld = false;
        }
    }

    public void OnPickup(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsPickupPressed = true;
            Debug.Log("Pickup Pressed");
        }
        else if (context.canceled)
        {
            IsPickupPressed = false;
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InventoryTriggered = true;
        }
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        if (context.started) MenuTriggered = true;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started) InteractTriggered = true;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (IsPreview) { IsFire = false; return; }

        if (context.started || context.performed) IsFire = true;
        else if (context.canceled) IsFire = false;
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (IsPreview) { ReloadTriggered = false; return; }

        if (context.started) ReloadTriggered = true;
    }

}
