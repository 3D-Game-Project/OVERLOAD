using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public enum KeyAction
{
    Forward,
    Backward,
    Left,
    Right,
    Jump_Fly,
    Dash,
    Fire,
    Pickup,
    Inventory,
    System_Setting
}

[System.Serializable]
public struct KeyBindingUI
{
    public KeyAction Action;
    public Button KeyButton;
    public TextMeshProUGUI KeyText;
    public KeyCode DefaultKeyCode;
}

public class SettingUI : MonoBehaviour
{
    [SerializeField] private Slider _volumeSlider;

    [SerializeField] private InputActionAsset _inputActionAsset;
    [SerializeField] private string _targetActionMapName = "PlayerInputMap";

    [SerializeField] private KeyBindingUI[] _keyBindingUI;
    [SerializeField] private Button _closeButton;

    private bool _isRebinding = false;
    private int _currentRebindIndex = -1;
    private bool _skipFirstFrame = false;

    private Dictionary<KeyAction, KeyCode> _keyBindings = new Dictionary<KeyAction, KeyCode>();

    private bool _ignoreMouseClick = false;

    private Dictionary<KeyAction, (string actionName, string bindingName)> _inputSystemMapping =
        new Dictionary<KeyAction, (string, string)>()
    {
        { KeyAction.Forward,        ("Move", "Up") },       
        { KeyAction.Backward,       ("Move", "Down") },     
        { KeyAction.Left,           ("Move", "Left") },     
        { KeyAction.Right,          ("Move", "Right") },    
        { KeyAction.Jump_Fly,       ("Jump", "") },         
        { KeyAction.Dash,           ("Boost", "") },        
        { KeyAction.Fire,           ("Fire", "") },
        { KeyAction.Pickup,         ("Pickup", "") },
        { KeyAction.Inventory,      ("Inventory", "") },
        { KeyAction.System_Setting, ("Menu", "") }          
    };

    private void Awake()
    {
        if (_closeButton != null) _closeButton.onClick.AddListener(OnClickClose);

        InitKeyBindings();
    }

    private void OnEnable()
    {
        _isRebinding = false;
        _currentRebindIndex = -1;
        _skipFirstFrame = false;

        UpdateKeyTexts();
    }

    private void Update()
    {
        if (!_isRebinding || _currentRebindIndex == -1) return;

        
        if (_skipFirstFrame)
        {
            _skipFirstFrame = false;
            return;
        }

        if (Input.anyKeyDown)
        {
            Debug.Log("[KeyBind 시스템] 유저의 키 입력 프레임 감지됨!");
            if (Input.GetKeyDown(KeyCode.Mouse0) && EventSystem.current != null)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    GameObject clickedUI = results[0].gameObject;
                    GameObject currentButtonObj = _keyBindingUI[_currentRebindIndex].KeyButton.gameObject;

                    if (clickedUI != currentButtonObj && !clickedUI.transform.IsChildOf(currentButtonObj.transform))
                    {
                        CancelBinding();
                        return;
                    }
                }
            }

            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode) || (keyCode == KeyCode.Mouse0 && Input.GetMouseButtonDown(0)))
                {
                    Debug.Log($"[KeyBind 시스템] 매칭된 KeyCode 확인됨 : {keyCode}");
                    if (keyCode == KeyCode.Escape)
                    {
                        Debug.Log("[KeyBind 시스템] ESC 입력으로 취소됨.");
                        CancelBinding();
                        return;
                    }

                    if (IsKeyDuplicate(keyCode, _keyBindingUI[_currentRebindIndex].Action))
                    {
                        _isRebinding = false;
                        int failedIndex = _currentRebindIndex;
                        _currentRebindIndex = -1;
                        UpdateKeyTexts();

                        Transform btnTransform = _keyBindingUI[failedIndex].KeyButton.transform;
                        btnTransform.DOKill();
                        btnTransform.DOShakePosition(0.3f, new Vector3(15f, 0f, 0f), vibrato: 20);

                        Debug.LogWarning($"[KeyBind] 중복된 키 입력 감지되어 차단됨: {keyCode}");
                        return;
                    }

                    if (keyCode == KeyCode.Mouse0)
                    {
                        _ignoreMouseClick = true;
                    }

                    ApplyNewKey(keyCode);
                    break;
                }
            }
        }
    }

    private void InitKeyBindings()
    {
        for (int i = 0; i < _keyBindingUI.Length; i++)
        {
            int index = i;
            KeyBindingUI item = _keyBindingUI[index];

            string savedKey = PlayerPrefs.GetString($"{item.Action}", item.DefaultKeyCode.ToString());
            KeyCode loadedKey = (KeyCode)Enum.Parse(typeof(KeyCode), savedKey);

            if (!_keyBindings.ContainsKey(item.Action))
                _keyBindings.Add(item.Action, loadedKey);
            else
                _keyBindings[item.Action] = loadedKey;

            if (loadedKey != item.DefaultKeyCode)
            {
                ApplyOverrideToInputSystem(item.Action, loadedKey);
            }

            if (item.KeyButton != null)
            {
                item.KeyButton.onClick.AddListener(() => RebindingKey(index));
            }
        }
    }

    private void UpdateKeyTexts()
    {
        for (int i = 0; i < _keyBindingUI.Length; i++)
        {
            KeyBindingUI item = _keyBindingUI[i];
            if (item.KeyText != null && _keyBindings.ContainsKey(item.Action))
            {
                item.KeyText.text = ChangeKeyName(_keyBindings[item.Action]);
            }
        }
    }

    private void RebindingKey(int index)
    {
        if (_ignoreMouseClick)
        {
            _ignoreMouseClick = false;
            return;
        }
        if (_isRebinding) return;

        _isRebinding = true;
        _currentRebindIndex = index;
        _skipFirstFrame = true; 

        if (_keyBindingUI[index].KeyText != null)
        {
            _keyBindingUI[index].KeyText.text = " ";
        }
    }

    private void ApplyNewKey(KeyCode newKey)
    {
        KeyBindingUI currentItem = _keyBindingUI[_currentRebindIndex];

        _keyBindings[currentItem.Action] = newKey;

        ApplyOverrideToInputSystem(currentItem.Action, newKey);

        PlayerPrefs.SetString($"{currentItem.Action}", newKey.ToString());
        PlayerPrefs.Save(); 

        _isRebinding = false;
        _currentRebindIndex = -1;
        UpdateKeyTexts();

        if (_inputActionAsset == null)
        {
            Debug.LogError("[검증 실패] 인스펙터 창에 _inputActionAsset 에셋 파일이 연결되지 않았습니다! (빈칸 상태)");
            return;
        }

        if (!_inputSystemMapping.TryGetValue(currentItem.Action, out var mapInfo))
        {
            Debug.LogError($"[검증 실패] 딕셔너리에 {currentItem.Action}에 대한 매핑 정보가 등록되어 있지 않습니다.");
            return;
        }

        var actionMap = _inputActionAsset.FindActionMap(_targetActionMapName);
        if (actionMap == null)
        {
            Debug.LogError($"[검증 실패] 인풋 에셋에서 '{_targetActionMapName}' 이름의 Action Map을 찾을 수 없습니다! 인스펙터 창의 Map Name을 실제 에셋 왼쪽 탭 이름과 똑같이 맞춰주세요.");
            return;
        }

        var action = actionMap.FindAction(mapInfo.actionName);
        if (action == null)
        {
            Debug.LogError($"[검증 실패] '{_targetActionMapName}' 맵 하위에서 '{mapInfo.actionName}' 이라는 이름의 Action을 찾을 수 없습니다. 대소문자를 확인해 주세요.");
            return;
        }

        Debug.Log($"<Color=Cyan>[실시간 검증 성공]</Color> {currentItem.Action} 액션의 런타임 바인딩이 성공적으로 변경됨!");

        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!string.IsNullOrEmpty(action.bindings[i].overridePath))
            {
                Debug.Log($" -> [바인딩 인덱스 {i}번] 경로: {action.bindings[i].overridePath}");
            }
        }
    }

    private void ApplyOverrideToInputSystem(KeyAction action, KeyCode newKeyCode)
    {
        if (_inputActionAsset == null) return;
        if (!_inputSystemMapping.TryGetValue(action, out var mapInfo)) return;

        InputActionMap map = _inputActionAsset.FindActionMap(_targetActionMapName);
        if (map == null) return;

        InputAction inputAction = map.FindAction(mapInfo.actionName);
        if (inputAction == null) return;

        string inputSystemPath = ConvertKeyCodeToInputSystemPath(newKeyCode);
        if (string.IsNullOrEmpty(inputSystemPath)) return;

        inputAction.Disable();

        if (!string.IsNullOrEmpty(mapInfo.bindingName))
        {
            for (int i = 0; i < inputAction.bindings.Count; i++)
            {
                if (inputAction.bindings[i].name.Equals(mapInfo.bindingName, StringComparison.OrdinalIgnoreCase))
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
    }

    private string ConvertKeyCodeToInputSystemPath(KeyCode keyCode)
    {
        string name = keyCode.ToString().ToLower();

        // 예외 문자열 필터 하드웨어 보정 가공
        if (keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9)
            return $"<Keyboard>/{name.Replace("alpha", "")}";
        if (keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6)
            return $"<Mouse>/{name.Replace("mouse", "button")}";
        if (keyCode == KeyCode.LeftShift || keyCode == KeyCode.RightShift)
            return $"<Keyboard>/{name.Replace("left", "left ").Replace("right", "right ")}";
        if (keyCode == KeyCode.LeftControl || keyCode == KeyCode.RightControl)
            return $"<Keyboard>/{name.Replace("leftcontrol", "leftCtrl").Replace("rightcontrol", "rightCtrl")}";
        if (keyCode == KeyCode.LeftAlt || keyCode == KeyCode.RightAlt)
            return $"<Keyboard>/{name.Replace("leftalt", "leftAlt").Replace("rightalt", "rightAlt")}";

        return $"<Keyboard>/{name}";
    }

    private bool IsKeyDuplicate(KeyCode checkingCode, KeyAction currentAction)
    {
        foreach (KeyValuePair<KeyAction, KeyCode> keyBinding in _keyBindings)
        {
            if (keyBinding.Key != currentAction && keyBinding.Value == checkingCode)
            {
                return true;
            }
        }
        return false;
    }

    private void CancelBinding()
    {
        _isRebinding = false;
        _currentRebindIndex = -1;
        UpdateKeyTexts();
        Debug.Log("[KeyBind] 다른 UI 요소를 클릭하여 키 변경이 취소되었습니다.");
    }

    private void OnClickClose()
    {
        gameObject.SetActive(false);
    }

    private string ChangeKeyName(KeyCode keyCode)
    {
        if (keyCode == KeyCode.Mouse0) return "Mouse-L";
        if (keyCode == KeyCode.Mouse1) return "Mouse-R";
        if (keyCode == KeyCode.Mouse2) return "Mouse-M";
        if (keyCode == KeyCode.LeftShift) return "L-Shift";
        if (keyCode == KeyCode.RightShift) return "R-Shift";
        if (keyCode == KeyCode.LeftControl) return "L-Ctrl";
        if (keyCode == KeyCode.LeftAlt) return "L-Alt";
        for (int i = 0; i <= 9; i++)
        {
            if (keyCode == (KeyCode)Enum.Parse(typeof(KeyCode), "Alpha" + i)) return i.ToString();
        }

        return keyCode.ToString();
    }

    //private void InitSoundSettings()
    //{
    //    if (_volumeSlider != null)
    //    {
    //        float savedVolume = PlayerPrefs.GetFloat("MainVolume", 1.0f);
    //        _volumeSlider.value = savedVolume;

    //        _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    //    }
    //}

    //private void OnVolumeChanged(float value)
    //{
    //    PlayerPrefs.SetFloat("MainVolume", value);
    //    PlayerPrefs.Save();
    //}


}
