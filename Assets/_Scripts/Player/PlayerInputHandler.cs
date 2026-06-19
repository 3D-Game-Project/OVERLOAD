using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 플레이어 관련 모든 input은 여기서 관리
// 이 클래스를 받아서 public 변수들을 통해 그 인풋의 상태를 파악 가능
// inputaction의 인터페이스를 받아와서 그 인터페이스를 구현하는 방식
public class PlayerInputHandler : MonoBehaviour, GameInputAction.IPlayerInputMapActions
{
    private PlayerInput _playerInput;
    private MenuController _menuController;

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
        _playerInput = GetComponent<PlayerInput>();
    }
    private void Start()
    {
        _menuController = FindFirstObjectByType<MenuController>();
        RestoreCustomKeyBindings();
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
        // 대표적인 설명 추가
        // 인벤토리가 열렸을 때, Move를 포함한 점프, 공격, 줍기 등의 모든 입력처리를 동작하지않도록 처리
        if (_menuController != null && _menuController.IsMenuOpen)
        {
            MoveInput = Vector2.zero;
            return;
        }
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (_menuController != null && _menuController.IsMenuOpen)
        {
            IsJumpPressed = false;
            IsJumpHeld = false;
            return;
        }

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
        if (_menuController != null && _menuController.IsMenuOpen)
        {
            IsBoostPressed = false;
            IsBoostHeld = false;
            return;
        }

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
        if (_menuController != null && _menuController.IsMenuOpen)
        {
            IsPickupPressed = false;
            return;
        }

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

        if (_menuController != null && _menuController.IsMenuOpen)
        {
            IsFire = false;
            return;
        }

        if (context.started || context.performed) IsFire = true;
        else if (context.canceled) IsFire = false;
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (IsPreview) { ReloadTriggered = false; return; }

        if (context.started) ReloadTriggered = true;
    }


    private void RestoreCustomKeyBindings()
    {
        if (GameInput == null || GameInput.asset == null) return;

        InputActionMap map = GameInput.asset.FindActionMap("PlayerInputMap");
        if (map == null) return;

        RestoreBinding(map, "Move", "Up", KeyAction.Forward);
        RestoreBinding(map, "Move", "Down", KeyAction.Backward);
        RestoreBinding(map, "Move", "Left", KeyAction.Left);
        RestoreBinding(map, "Move", "Right", KeyAction.Right);
        RestoreBinding(map, "Jump", "", KeyAction.Jump_Fly);
        RestoreBinding(map, "Boost", "", KeyAction.Dash);
        RestoreBinding(map, "Fire", "", KeyAction.Fire);
        RestoreBinding(map, "Pickup", "", KeyAction.Pickup);
        RestoreBinding(map, "Inventory", "", KeyAction.Inventory);
        RestoreBinding(map, "Menu", "", KeyAction.System_Setting);
    }

    private void RestoreBinding(InputActionMap map, string actionName, string bindingName, KeyAction actionEnum)
    {
        if (!PlayerPrefs.HasKey($"{actionEnum}")) return;

        InputAction inputAction = map.FindAction(actionName);
        if (inputAction == null) return;

        string savedKeyStr = PlayerPrefs.GetString($"{actionEnum}");
        if (!Enum.TryParse(savedKeyStr, out KeyCode keyCode)) return;

        string inputSystemPath = ConvertKeyCodeToInputSystemPath(keyCode);
        if (string.IsNullOrEmpty(inputSystemPath)) return;

        inputAction.Disable();

        if (!string.IsNullOrEmpty(bindingName))
        {
            for (int i = 0; i < inputAction.bindings.Count; i++)
            {
                if (inputAction.bindings[i].name.Equals(bindingName, StringComparison.OrdinalIgnoreCase))
                {
                    inputAction.ApplyBindingOverride(i, inputSystemPath);
                    break;
                }
            }
        }
        else
        {
            inputAction.ApplyBindingOverride(0, inputSystemPath);
        }

        inputAction.Enable();
        Debug.Log($"[인게임 세이브 로드] 씬 이동 복구 완료 : {actionName}({bindingName}) ➔ {inputSystemPath}");
    }

    // 경로 가공 툴 파일
    private string ConvertKeyCodeToInputSystemPath(KeyCode keyCode)
    {
        string name = keyCode.ToString().ToLower();

        if (keyCode == KeyCode.Mouse0) return "<Mouse>/leftButton";  // button0 에러 완벽 방어
        if (keyCode == KeyCode.Mouse1) return "<Mouse>/rightButton"; // button1 에러 완벽 방어
        if (keyCode == KeyCode.Mouse2) return "<Mouse>/middleButton";

        if (keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9)
            return $"<Keyboard>/{name.Replace("alpha", "")}";

        if (keyCode == KeyCode.LeftShift || keyCode == KeyCode.RightShift)
            return $"<Keyboard>/{name.Replace("left", "left ").Replace("right", "right ")}";

        if (keyCode == KeyCode.LeftControl || keyCode == KeyCode.RightControl)
            return $"<Keyboard>/{name.Replace("leftcontrol", "leftCtrl").Replace("rightcontrol", "rightCtrl")}";

        if (keyCode == KeyCode.LeftAlt || keyCode == KeyCode.RightAlt)
            return $"<Keyboard>/{name.Replace("leftalt", "leftAlt").Replace("rightalt", "rightAlt")}";

        if (keyCode == KeyCode.Space)
            return "<Keyboard>/space";

        return $"<Keyboard>/{name}";
    }
}
