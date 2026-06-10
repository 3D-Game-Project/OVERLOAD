using System;
using System.Collections.Generic;
using UnityEngine;

public class CorePartsController : MonoBehaviour
{
    [System.Serializable]
    public class InitialPartSetup
    {
        public PartsData partsData;
        public AttachmentSlot slot;
    }

    [Header("Initial Parts")]
    [SerializeField] private bool _attachInitialPartsOnStart = true;
    [SerializeField] private List<InitialPartSetup> _initialParts = new();

    [Header("Attachment Slots")]
    [SerializeField] private List<AttachmentSlot> _attachmentSlots = new();

    [Header("Core Controllers")]
    [SerializeField] private CoreLoadController _coreLoadController;
    [SerializeField] private CoreEnergyController _coreEnergyController;

    [Header("Movement References")]
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private PlayerLocomotionMotor _locomotionMotor;
    [SerializeField] private MovementCoordinator _movementCoordinator;
    [SerializeField] private CoreMovementController _coreMovementController;
    [SerializeField] private PlayerBodyShapeController _bodyShapeController;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _coreYawRoot;
    [SerializeField] private Transform _legYawRoot;


    [Header("Input Settings")]
    [SerializeField] private float _inputDeadZone = 0.05f;

    private CorePartContext _context;

    private readonly List<IPart> _allParts = new();
    private readonly List<ILocomotionPart> _locomotionParts = new();
    private readonly List<IBoosterPart> _boosterParts = new();
    private readonly List<IAttackPart> _attackParts = new();

    public bool HasLocomotionPart => _locomotionParts.Count > 0;
    public bool HasBoosterPart => _boosterParts.Count > 0;
    public bool HasAttackPart => _attackParts.Count > 0;
    public bool HasAnyPart => _allParts.Count > 0;

    private void Awake()
    {
        if (_coreLoadController == null)
            _coreLoadController = GetComponent<CoreLoadController>();

        if (_coreEnergyController == null)
            _coreEnergyController = GetComponent<CoreEnergyController>();

        if (_inputHandler == null)
            _inputHandler = GetComponent<PlayerInputHandler>();

        if (_locomotionMotor == null)
            _locomotionMotor = GetComponent<PlayerLocomotionMotor>();

        if (_movementCoordinator == null)
            _movementCoordinator = GetComponent<MovementCoordinator>();

        if (_coreMovementController == null)
            _coreMovementController = GetComponent<CoreMovementController>();

        if (_bodyShapeController == null)
            _bodyShapeController = GetComponent<PlayerBodyShapeController>();

        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;

        RebuildAttachmentSlotList();

        _context = new CorePartContext(
            transform,
            _inputHandler,
            _locomotionMotor,
            _movementCoordinator,
            _cameraTransform,
            _coreYawRoot,
            _legYawRoot,
            _coreEnergyController
        );
    }

    private void Start()
    {
        if (!_attachInitialPartsOnStart)
            return;

        foreach (InitialPartSetup setup in _initialParts)
        {
            if (setup == null)
                continue;

            AttachPartInternal(setup.partsData, setup.slot, false);
        }

        _bodyShapeController?.RebuildShapeFromVisuals();
    }

    private void Update()
    {
        HandleMovementParts();
        HandleAttackParts();
    }

    public bool AttachPart(PartsData partsData, AttachmentSlot slot)
    {
        return AttachPartInternal(partsData, slot, true);
    }

    public bool ReplacePart(PartsData newPartsData, AttachmentSlot targetSlot)
    {
        if (newPartsData == null)
        {
            Debug.LogWarning("교체할 PartsData가 없습니다.");
            return false;
        }

        if (targetSlot == null)
        {
            Debug.LogWarning("교체할 AttachmentSlot이 없습니다.");
            return false;
        }

        Debug.Log(
            $"Replace 요청 / Slot: {targetSlot.SlotId} / " +
            $"HasPart: {targetSlot.HasPart} / " +
            $"AttachedObject: {(targetSlot.AttachedObject != null ? targetSlot.AttachedObject.name : "None")}"
        );

        if (!targetSlot.CanAttach(newPartsData))
        {
            Debug.LogWarning($"{targetSlot.SlotId} 슬롯에는 {newPartsData.PartsType} 파츠를 장착할 수 없습니다.");
            return false;
        }

        if (!CanReplaceByLoad(newPartsData, targetSlot))
        {
            Debug.LogWarning("코어 장착 부하 한도를 초과해서 파츠를 교체할 수 없습니다.");
            return false;
        }

        if (targetSlot.HasPart)
        {
            Debug.Log($"기존 파츠 제거 시도: {targetSlot.AttachedObject.name}");
            DetachPartInternal(targetSlot, false);
        }
        else
        {
            RemoveUntrackedChildrenInSlot(targetSlot);
        }

        return AttachPartInternal(newPartsData, targetSlot, true);
    }

    public bool ReplacePartBySlotId(PartsData newPartsData, string slotId)
    {
        AttachmentSlot targetSlot = FindSlotById(slotId);

        if (targetSlot == null)
        {
            Debug.LogWarning($"{slotId} 슬롯을 찾을 수 없습니다.");
            return false;
        }

        return ReplacePart(newPartsData, targetSlot);
    }

    public bool DetachPart(AttachmentSlot slot)
    {
        return DetachPartInternal(slot, true);
    }

    public bool DetachPartBySlotId(string slotId)
    {
        AttachmentSlot targetSlot = FindSlotById(slotId);

        if (targetSlot == null)
        {
            Debug.LogWarning($"{slotId} 슬롯을 찾을 수 없습니다.");
            return false;
        }

        return DetachPart(targetSlot);
    }

    public AttachmentSlot FindSlotById(string slotId)
    {
        if (string.IsNullOrEmpty(slotId))
            return null;

        AttachmentSlot foundSlot = null;

        foreach (AttachmentSlot slot in _attachmentSlots)
        {
            if (slot == null)
                continue;

            if (slot.SlotId != slotId)
                continue;

            if (foundSlot != null)
            {
                Debug.LogWarning(
                    $"중복 SlotId 발견: {slotId}\n" +
                    $"첫 번째 슬롯: {foundSlot.name}\n" +
                    $"중복 슬롯: {slot.name}"
                );
            }

            if (foundSlot == null)
                foundSlot = slot;
        }

        return foundSlot;
    }

    private bool AttachPartInternal(
        PartsData partsData,
        AttachmentSlot slot,
        bool rebuildBodyShape)
    {
        if (partsData == null)
        {
            Debug.LogWarning("장착할 PartsData가 없습니다.");
            return false;
        }

        if (slot == null)
        {
            Debug.LogWarning("장착할 AttachmentSlot이 없습니다.");
            return false;
        }

        RegisterSlot(slot);

        if (slot.HasPart)
        {
            Debug.LogWarning($"{slot.SlotId} 슬롯에는 이미 파츠가 장착되어 있습니다. 교체하려면 ReplacePart를 사용하세요.");
            return false;
        }

        // 슬롯 상태는 비어있다고 되어 있는데 실제 자식 파츠가 남아있는 경우 방지
        RemoveUntrackedChildrenInSlot(slot);

        if (!slot.CanAttach(partsData))
        {
            Debug.LogWarning($"{slot.SlotId} 슬롯에는 {partsData.PartsType} 파츠를 장착할 수 없습니다.");
            return false;
        }

        if (partsData.PartsPrefab == null)
        {
            Debug.LogWarning($"{partsData.PartsName}에 PartsPrefab이 없습니다.");
            return false;
        }

        if (_coreLoadController != null)
        {
            if (!_coreLoadController.CanEquip(partsData.RequiredLoad))
            {
                Debug.LogWarning("코어 장착 부하 한도를 초과해서 파츠를 장착할 수 없습니다.");
                return false;
            }
        }

        GameObject partObject = Instantiate(partsData.PartsPrefab);

        AlignPartToSlot(partObject.transform, slot.AttachPoint);

        partObject.transform.SetParent(slot.AttachPoint, true);

        IPart part = FindPart(partObject);

        if (part == null)
        {
            Debug.LogWarning($"{partObject.name}에 IPart를 구현한 파츠 컨트롤러가 없습니다.");
            Destroy(partObject);
            return false;
        }

        part.Initialize(partsData, _context);
        part.OnAttached(slot);

        slot.SetAttachedObject(partObject);

        if (_coreLoadController != null)
        {
            _coreLoadController.AddLoad(partsData.RequiredLoad);
        }

        RegisterPart(part);
        RegisterSlotsInObject(partObject);

        if (rebuildBodyShape)
        {
            _bodyShapeController?.RebuildShapeFromVisuals();
        }

        Debug.Log($"{partsData.PartsName} 파츠 장착 완료");

        return true;
    }

    private bool DetachPartInternal(
        AttachmentSlot slot,
        bool rebuildBodyShape)
    {
        if (slot == null)
            return false;

        if (!slot.HasPart)
        {
            bool removedUntracked = RemoveUntrackedChildrenInSlot(slot);

            if (removedUntracked && rebuildBodyShape)
            {
                _bodyShapeController?.RebuildShapeFromVisuals();
            }

            return removedUntracked;
        }

        GameObject attachedObject = slot.AttachedObject;

        if (attachedObject == null)
        {
            slot.Clear();
            return false;
        }

        UnregisterPartsInObject(attachedObject);
        UnregisterSlotsInObject(attachedObject);

        slot.Clear();

        attachedObject.SetActive(false);
        Destroy(attachedObject);

        if (rebuildBodyShape)
        {
            _bodyShapeController?.RebuildShapeFromVisuals();
        }

        return true;
    }

    private bool RemoveUntrackedChildrenInSlot(AttachmentSlot slot)
    {
        if (slot == null)
            return false;

        if (slot.AttachPoint == null)
            return false;

        bool removedAny = false;

        for (int i = slot.AttachPoint.childCount - 1; i >= 0; i--)
        {
            Transform child = slot.AttachPoint.GetChild(i);

            if (child == null)
                continue;

            IPart childPart = FindPart(child.gameObject);

            if (childPart == null)
                continue;

            Debug.LogWarning(
                $"{slot.SlotId} 슬롯에 등록되지 않은 기존 파츠가 있어 제거합니다: {child.name}"
            );

            UnregisterPartsInObject(child.gameObject);
            UnregisterSlotsInObject(child.gameObject);

            child.gameObject.SetActive(false);
            Destroy(child.gameObject);

            removedAny = true;
        }

        return removedAny;
    }

    private bool CanReplaceByLoad(PartsData newPartsData, AttachmentSlot targetSlot)
    {
        if (_coreLoadController == null)
            return true;

        if (newPartsData == null)
            return false;

        if (targetSlot == null)
            return false;

        float currentSlotLoad = GetAttachedPartLoad(targetSlot);

        if (currentSlotLoad > 0f)
        {
            _coreLoadController.RemoveLoad(currentSlotLoad);
        }

        bool canEquip = _coreLoadController.CanEquip(newPartsData.RequiredLoad);

        if (currentSlotLoad > 0f)
        {
            _coreLoadController.AddLoad(currentSlotLoad);
        }

        return canEquip;
    }

    private float GetAttachedPartLoad(AttachmentSlot slot)
    {
        if (slot == null)
            return 0f;

        if (!slot.HasPart)
            return 0f;

        if (slot.AttachedObject == null)
            return 0f;

        IPart part = FindPart(slot.AttachedObject);

        if (part == null || part.Data == null)
            return 0f;

        return part.Data.RequiredLoad;
    }

    private IPart FindPart(GameObject partObject)
    {
        MonoBehaviour[] behaviours =
            partObject.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IPart part)
                return part;
        }

        return null;
    }

    private List<IPart> FindPartsInObject(GameObject partObject)
    {
        List<IPart> parts = new();

        if (partObject == null)
            return parts;

        MonoBehaviour[] behaviours =
            partObject.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IPart part)
            {
                if (!parts.Contains(part))
                    parts.Add(part);
            }
        }

        return parts;
    }

    private void RegisterPart(IPart part)
    {
        if (part == null)
            return;

        if (!_allParts.Contains(part))
            _allParts.Add(part);

        if (part is ILocomotionPart locomotionPart)
        {
            if (!_locomotionParts.Contains(locomotionPart))
                _locomotionParts.Add(locomotionPart);
        }

        if (part is IBoosterPart boosterPart)
        {
            if (!_boosterParts.Contains(boosterPart))
                _boosterParts.Add(boosterPart);
        }

        if (part is IAttackPart attackPart)
        {
            if (!_attackParts.Contains(attackPart))
                _attackParts.Add(attackPart);
        }
    }

    private void UnregisterPart(IPart part)
    {
        if (part == null)
            return;

        _allParts.Remove(part);

        if (part is ILocomotionPart locomotionPart)
            _locomotionParts.Remove(locomotionPart);

        if (part is IBoosterPart boosterPart)
            _boosterParts.Remove(boosterPart);

        if (part is IAttackPart attackPart)
            _attackParts.Remove(attackPart);
    }

    private void UnregisterPartsInObject(GameObject rootObject)
    {
        List<IPart> parts = FindPartsInObject(rootObject);

        for (int i = parts.Count - 1; i >= 0; i--)
        {
            IPart part = parts[i];

            part.OnDetached();
            UnregisterPart(part);

            if (_coreLoadController != null && part.Data != null)
            {
                _coreLoadController.RemoveLoad(part.Data.RequiredLoad);
            }
        }
    }

    private void RebuildAttachmentSlotList()
    {
        _attachmentSlots.Clear();
        RegisterSlotsInObject(gameObject);
    }

    private void RegisterSlot(AttachmentSlot slot)
    {
        if (slot == null)
            return;

        if (!_attachmentSlots.Contains(slot))
            _attachmentSlots.Add(slot);
    }

    private void RegisterSlotsInObject(GameObject rootObject)
    {
        if (rootObject == null)
            return;

        AttachmentSlot[] slots =
            rootObject.GetComponentsInChildren<AttachmentSlot>(true);

        foreach (AttachmentSlot slot in slots)
        {
            RegisterSlot(slot);
        }
    }

    private void UnregisterSlotsInObject(GameObject rootObject)
    {
        if (rootObject == null)
            return;

        AttachmentSlot[] slots =
            rootObject.GetComponentsInChildren<AttachmentSlot>(true);

        foreach (AttachmentSlot slot in slots)
        {
            _attachmentSlots.Remove(slot);
        }
    }

    private void AlignPartToSlot(Transform partRoot, Transform slotTransform)
    {
        if (partRoot == null || slotTransform == null)
            return;

        PartAttachAnchor anchor =
            partRoot.GetComponentInChildren<PartAttachAnchor>(true);

        if (anchor == null)
        {
            Debug.LogWarning($"{partRoot.name}에 PartAttachAnchor가 없습니다. 프리팹 루트를 기준으로 부착합니다.");

            partRoot.position = slotTransform.position;
            partRoot.rotation = slotTransform.rotation;
            return;
        }

        Quaternion rotationOffset =
            slotTransform.rotation * Quaternion.Inverse(anchor.transform.rotation);

        partRoot.rotation = rotationOffset * partRoot.rotation;

        Vector3 positionOffset =
            slotTransform.position - anchor.transform.position;

        partRoot.position += positionOffset;
    }

    private void HandleMovementParts()
    {
        if (_movementCoordinator == null)
            return;

        _movementCoordinator.BeginFrame();

        if (_inputHandler != null)
        {
            LocomotionCommand locomotionCommand = CreateLocomotionCommand();

            if (_coreMovementController != null)
            {
                _coreMovementController.HandleCoreMovement(
                    locomotionCommand,
                    HasLocomotionPart,
                    HasBoosterPart,
                    Time.deltaTime
                );
            }

            if (_locomotionParts.Count > 0)
            {
                foreach (ILocomotionPart locomotionPart in _locomotionParts)
                {
                    locomotionPart.HandleLocomotion(
                        locomotionCommand,
                        Time.deltaTime
                    );
                }
            }

            if (_boosterParts.Count > 0)
            {
                BoosterCommand boosterCommand = CreateBoosterCommand(locomotionCommand);

                foreach (IBoosterPart boosterPart in _boosterParts)
                {
                    boosterPart.HandleBooster(
                        boosterCommand,
                        Time.deltaTime
                    );
                }
            }
        }

        _movementCoordinator.ApplyFrame();
    }

    private LocomotionCommand CreateLocomotionCommand()
    {
        Vector2 moveInput = _inputHandler.MoveInput;

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (Mathf.Abs(moveInput.x) < _inputDeadZone)
            moveInput.x = 0f;

        if (Mathf.Abs(moveInput.y) < _inputDeadZone)
            moveInput.y = 0f;

        Vector3 cameraForward = GetCameraForwardOnPlane();
        Vector3 cameraRight = GetCameraRightOnPlane();

        Vector3 moveDirection =
            cameraForward * moveInput.y +
            cameraRight * moveInput.x;

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        bool hasMoveInput = moveDirection.sqrMagnitude > 0.001f;

        return new LocomotionCommand(
            moveDirection,
            cameraForward,
            hasMoveInput
        );
    }

    private BoosterCommand CreateBoosterCommand(LocomotionCommand locomotionCommand)
    {
        return new BoosterCommand(
            locomotionCommand.MoveDirection,
            locomotionCommand.LookDirection,
            _inputHandler.IsJumpPressed,
            _inputHandler.IsJumpHeld,
            _inputHandler.IsBoostHeld
        );
    }

    private void HandleAttackParts()
    {
        if (_inputHandler == null)
            return;

        if (_attackParts.Count <= 0)
            return;

        foreach (IAttackPart attackPart in _attackParts)
        {
            attackPart.HandleAttack(_inputHandler.IsFire);
        }

        if (_inputHandler.ReloadTriggered)
        {
            foreach (IAttackPart attackPart in _attackParts)
            {
                attackPart.HandleReload(true);
            }

            _inputHandler.ReloadTriggered = false;
        }
    }

    private Vector3 GetCameraForwardOnPlane()
    {
        if (_cameraTransform == null)
            return transform.forward;

        Vector3 forward = _cameraTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return transform.forward;

        return forward.normalized;
    }

    private Vector3 GetCameraRightOnPlane()
    {
        if (_cameraTransform == null)
            return transform.right;

        Vector3 right = _cameraTransform.right;
        right.y = 0f;

        if (right.sqrMagnitude < 0.001f)
            return transform.right;

        return right.normalized;
    }

    public void ResetYawRootsForPreview()
    {
        if (_coreYawRoot != null) _coreYawRoot.localRotation = Quaternion.identity;
        if (_legYawRoot != null) _legYawRoot.localRotation = Quaternion.identity;
    }
}