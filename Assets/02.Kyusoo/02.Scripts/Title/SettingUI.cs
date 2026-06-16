using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    
    [SerializeField] private KeyBindingUI[] _keyBindingUI;
    [SerializeField] private Button _closeButton;

    private bool _isRebinding = false;
    private int _currentRebindIndex = -1;
    private bool _skipFirstFrame = false;

    private Dictionary<KeyAction, KeyCode> _keyBindings = new Dictionary<KeyAction, KeyCode>();

    private bool _ignoreMouseClick = false;

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
                if (Input.GetKeyDown(keyCode))
                {
                    
                    if (keyCode == KeyCode.Escape)
                    {
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

        PlayerPrefs.SetString($"{currentItem.Action}", newKey.ToString());
        PlayerPrefs.Save(); 

        _isRebinding = false;
        _currentRebindIndex = -1;
        UpdateKeyTexts();

        Debug.Log($"[KeyBind] {currentItem.Action} 키가 {newKey}로 성공적으로 변경 및 저장되었습니다!");
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
        if (keyCode == KeyCode.Alpha0) return "0";
        if (keyCode == KeyCode.Alpha1) return "1";
        if (keyCode == KeyCode.Alpha2) return "2";
        if (keyCode == KeyCode.Alpha3) return "3";
        if (keyCode == KeyCode.Alpha4) return "4";
        if (keyCode == KeyCode.Alpha5) return "5";
        if (keyCode == KeyCode.Alpha6) return "6";
        if (keyCode == KeyCode.Alpha7) return "7";
        if (keyCode == KeyCode.Alpha8) return "8";
        if (keyCode == KeyCode.Alpha9) return "9";
        if (keyCode == KeyCode.Alpha0) return "0";

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
