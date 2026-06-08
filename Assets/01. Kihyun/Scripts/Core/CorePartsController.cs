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

    [Header("Core Controllers")]
    [SerializeField] private CoreLoadController _coreLoadController;
    [SerializeField] private CoreEnergyController _coreEnergyController;

    [Header("Movement References")]
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private PlayerLocomotionMotor _locomotionMotor;
    [SerializeField] private MovementCoordinator _movementCoordinator;
    [SerializeField] private CoreMovementController _coreMovementController;
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

        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;

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

            AttachPart(setup.partsData, setup.slot);
        }
    }

    private void Update()
    {
        HandleMovementParts();
    }

    public void AttachPart(PartsData partsData, AttachmentSlot slot)
    {
        if (partsData == null)
        {
            Debug.LogWarning("장착할 PartsData가 없습니다.");
            return;
        }

        if (slot == null)
        {
            Debug.LogWarning("장착할 AttachmentSlot이 없습니다.");
            return;
        }

        if (!slot.CanAttach(partsData))
        {
            Debug.LogWarning($"{slot.SlotId} 슬롯에는 {partsData.PartsType} 파츠를 장착할 수 없습니다.");
            return;
        }

        if (partsData.PartsPrefab == null)
        {
            Debug.LogWarning($"{partsData.PartsName}에 PartsPrefab이 없습니다.");
            return;
        }

        if (_coreLoadController != null)
        {
            if (!_coreLoadController.CanEquip(partsData.RequiredLoad))
            {
                Debug.LogWarning("코어 장착 부하 한도를 초과해서 파츠를 장착할 수 없습니다.");
                return;
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
            return;
        }

        part.Initialize(partsData, _context);
        part.OnAttached(slot);

        slot.SetAttachedObject(partObject);

        if (_coreLoadController != null)
        {
            _coreLoadController.AddLoad(partsData.RequiredLoad);
        }

        RegisterPart(part);

        Debug.Log($"{partsData.PartsName} 파츠 장착 완료");
    }

    public void DetachPart(AttachmentSlot slot)
    {
        if (slot == null)
            return;

        if (!slot.HasPart)
            return;

        GameObject attachedObject = slot.AttachedObject;

        if (attachedObject == null)
        {
            slot.Clear();
            return;
        }

        IPart part = FindPart(attachedObject);

        if (part != null)
        {
            part.OnDetached();
            UnregisterPart(part);

            if (_coreLoadController != null && part.Data != null)
            {
                _coreLoadController.RemoveLoad(part.Data.RequiredLoad);
            }
        }

        slot.Clear();

        Destroy(attachedObject);
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
}