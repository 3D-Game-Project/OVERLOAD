using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, GameInputAction.IPlayerInputMapActions
{
    public GameInputAction GameInput { get; private set;
    }
    public Vector2 MoveInput { get; private set; }

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
}
