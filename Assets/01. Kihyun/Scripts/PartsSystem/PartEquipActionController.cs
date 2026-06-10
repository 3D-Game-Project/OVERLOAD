using System;
using System.Collections;
using UnityEngine;

public enum PartEquipActionType
{
    Attach,
    Detach,
    Replace
}

public class PartEquipActionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CorePartsController _corePartsController;

    [Header("Action Time")]
    [SerializeField] private float _attachTime = 2.0f;
    [SerializeField] private float _detachTime = 1.5f;
    [SerializeField] private float _replaceExtraTime = 0.5f;

    [Header("Time Option")]
    [SerializeField] private bool _useUnscaledTime = false;

    [Header("Test")]
    [SerializeField] private bool _enableTestInput = true;
    [SerializeField] private PartsData _testPartsData;
    [SerializeField] private string _testSlotId;
    [SerializeField] private KeyCode _replaceKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode _detachKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode _attachKey = KeyCode.Alpha3;

    private Coroutine _currentActionCoroutine;

    private PartEquipActionType _currentActionType;
    private PartsData _currentPartsData;
    private AttachmentSlot _currentSlot;

    private float _elapsedTime;
    private float _requiredTime;

    public bool IsBusy => _currentActionCoroutine != null;
    public float ProgressRatio => _requiredTime <= 0f ? 1f : Mathf.Clamp01(_elapsedTime / _requiredTime);
    public PartEquipActionType CurrentActionType => _currentActionType;
    public PartsData CurrentPartsData => _currentPartsData;
    public AttachmentSlot CurrentSlot => _currentSlot;

    public event Action<PartEquipActionType, PartsData, AttachmentSlot> OnActionStarted;
    public event Action<float> OnActionProgressChanged;
    public event Action<bool> OnActionFinished;

    private void Awake()
    {
        if (_corePartsController == null)
            _corePartsController = GetComponent<CorePartsController>();
    }

    private void Update()
    {
        if (!_enableTestInput)
            return;

        if (Input.GetKeyDown(_replaceKey))
        {
            TryStartReplace(_testPartsData, _testSlotId);
        }

        if (Input.GetKeyDown(_detachKey))
        {
            TryStartDetach(_testSlotId);
        }

        if (Input.GetKeyDown(_attachKey))
        {
            TryStartAttach(_testPartsData, _testSlotId);
        }
    }

    public bool TryStartAttach(PartsData partsData, string slotId)
    {
        AttachmentSlot slot = FindSlot(slotId);

        if (slot == null)
            return false;

        return TryStartAttach(partsData, slot);
    }

    public bool TryStartAttach(PartsData partsData, AttachmentSlot slot)
    {
        if (!CanStartCommonCheck())
            return false;

        if (partsData == null)
        {
            Debug.LogWarning("장착할 PartsData가 없습니다.");
            return false;
        }

        if (slot == null)
        {
            Debug.LogWarning("장착할 슬롯이 없습니다.");
            return false;
        }

        if (slot.HasPart)
        {
            Debug.LogWarning($"{slot.SlotId} 슬롯에는 이미 파츠가 있습니다. 교체하려면 Replace를 사용하세요.");
            return false;
        }

        if (!slot.CanAttach(partsData))
        {
            Debug.LogWarning($"{slot.SlotId} 슬롯에는 {partsData.PartsType} 파츠를 장착할 수 없습니다.");
            return false;
        }

        float actionTime = _attachTime;

        StartAction(
            PartEquipActionType.Attach,
            partsData,
            slot,
            actionTime
        );

        return true;
    }

    public bool TryStartDetach(string slotId)
    {
        AttachmentSlot slot = FindSlot(slotId);

        if (slot == null)
            return false;

        return TryStartDetach(slot);
    }

    public bool TryStartDetach(AttachmentSlot slot)
    {
        if (!CanStartCommonCheck())
            return false;

        if (slot == null)
        {
            Debug.LogWarning("해제할 슬롯이 없습니다.");
            return false;
        }

        // 여기서 slot.HasPart 검사로 막지 않는다.
        // CorePartsController.DetachPart()가
        // 등록된 파츠 / 등록되지 않은 자식 파츠를 모두 처리하게 맡긴다.
        float actionTime = _detachTime;

        StartAction(
            PartEquipActionType.Detach,
            null,
            slot,
            actionTime
        );

        return true;
    }

    public bool TryStartReplace(PartsData partsData, string slotId)
    {
        AttachmentSlot slot = FindSlot(slotId);

        if (slot == null)
            return false;

        return TryStartReplace(partsData, slot);
    }

    public bool TryStartReplace(PartsData partsData, AttachmentSlot slot)
    {
        if (!CanStartCommonCheck())
            return false;

        if (partsData == null)
        {
            Debug.LogWarning("교체할 PartsData가 없습니다.");
            return false;
        }

        if (slot == null)
        {
            Debug.LogWarning("교체할 슬롯이 없습니다.");
            return false;
        }

        if (!slot.CanAttach(partsData))
        {
            Debug.LogWarning($"{slot.SlotId} 슬롯에는 {partsData.PartsType} 파츠를 장착할 수 없습니다.");
            return false;
        }

        float actionTime = _attachTime;

        if (slot.HasPart)
            actionTime += _detachTime;

        actionTime += _replaceExtraTime;

        StartAction(
            PartEquipActionType.Replace,
            partsData,
            slot,
            actionTime
        );

        return true;
    }

    public void CancelCurrentAction()
    {
        if (_currentActionCoroutine == null)
            return;

        StopCoroutine(_currentActionCoroutine);
        _currentActionCoroutine = null;

        ClearCurrentAction();

        OnActionFinished?.Invoke(false);

        Debug.Log("파츠 탈부착 작업 취소");
    }

    private bool CanStartCommonCheck()
    {
        if (_corePartsController == null)
        {
            Debug.LogWarning("CorePartsController가 없습니다.");
            return false;
        }

        if (IsBusy)
        {
            Debug.LogWarning("이미 파츠 탈부착 작업이 진행 중입니다.");
            return false;
        }

        return true;
    }

    private AttachmentSlot FindSlot(string slotId)
    {
        if (_corePartsController == null)
        {
            Debug.LogWarning("CorePartsController가 없습니다.");
            return null;
        }

        AttachmentSlot slot = _corePartsController.FindSlotById(slotId);

        if (slot == null)
        {
            Debug.LogWarning($"{slotId} 슬롯을 찾을 수 없습니다.");
            return null;
        }

        return slot;
    }

    private void StartAction(
        PartEquipActionType actionType,
        PartsData partsData,
        AttachmentSlot slot,
        float requiredTime)
    {
        _currentActionType = actionType;
        _currentPartsData = partsData;
        _currentSlot = slot;

        _elapsedTime = 0f;
        _requiredTime = Mathf.Max(0.01f, requiredTime);

        _currentActionCoroutine = StartCoroutine(ActionRoutine());

        OnActionStarted?.Invoke(
            _currentActionType,
            _currentPartsData,
            _currentSlot
        );

        Debug.Log(
            $"파츠 작업 시작: {_currentActionType} / Slot: {_currentSlot.SlotId} / Time: {_requiredTime}"
        );
    }

    private IEnumerator ActionRoutine()
    {
        while (_elapsedTime < _requiredTime)
        {
            float deltaTime = _useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            _elapsedTime += deltaTime;

            OnActionProgressChanged?.Invoke(ProgressRatio);

            yield return null;
        }

        bool result = ExecuteCurrentAction();

        _currentActionCoroutine = null;

        ClearCurrentAction();

        OnActionFinished?.Invoke(result);
    }

    private bool ExecuteCurrentAction()
    {
        if (_corePartsController == null)
            return false;

        if (_currentSlot == null)
            return false;

        switch (_currentActionType)
        {
            case PartEquipActionType.Attach:
                return _corePartsController.AttachPart(
                    _currentPartsData,
                    _currentSlot
                );

            case PartEquipActionType.Detach:
                return _corePartsController.DetachPart(
                    _currentSlot
                );

            case PartEquipActionType.Replace:
                return _corePartsController.ReplacePart(
                    _currentPartsData,
                    _currentSlot
                );
        }

        return false;
    }

    private void ClearCurrentAction()
    {
        _currentActionType = default;
        _currentPartsData = null;
        _currentSlot = null;
        _elapsedTime = 0f;
        _requiredTime = 0f;
    }
}