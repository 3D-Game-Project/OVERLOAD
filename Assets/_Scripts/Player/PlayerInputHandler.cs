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
    public bool IsPickupPressed { get; private set; }
    public bool InventoryTriggered { get; set; }

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

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            IsJumpPressed = true;
        else if (context.canceled)
            IsJumpPressed = false;
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


}
