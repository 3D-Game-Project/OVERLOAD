using System.Collections.Generic;
using UnityEngine;

public class CorePartsController : MonoBehaviour
{
    [Header("Test Attach")]
    [SerializeField] private PartsData _testPartsData;
    [SerializeField] private AttachmentSlot _testSlot;
    [SerializeField] private bool _attachOnStart = true;

    [Header("Core Controllers")]
    [SerializeField] private CoreLoadController _coreLoadController;
    [SerializeField] private CoreEnergyController _coreEnergyController;

    [Header("Movement References")]
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private PlayerLocomotionMotor _locomotionMotor;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _coreYawRoot;
    [SerializeField] private Transform _legYawRoot;

    private CorePartContext _context;

    private readonly List<GameObject> _attachedPartObjects = new();

    private void Awake()
    {
        _context = new CorePartContext(
            transform,
            _inputHandler,
            _locomotionMotor,
            _cameraTransform,
            _coreYawRoot,
            _legYawRoot,
            _coreEnergyController
        );
    }

    private void Start()
    {
        if (_attachOnStart)
        {
            AttachPart(_testPartsData, _testSlot);
        }
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

        slot.SetAttachedObject(partObject);
        _attachedPartObjects.Add(partObject);

        if (_coreLoadController != null)
        {
            _coreLoadController.AddLoad(partsData.RequiredLoad);
        }

        InitializePart(partsData, partObject);

        Debug.Log($"{partsData.PartsName} 파츠 장착 완료");
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

    private void InitializePart(PartsData partsData, GameObject partObject)
    {
        if (partsData is LegPartsData legPartsData)
        {
            LegPartController legPartController =
                partObject.GetComponentInChildren<LegPartController>(true);

            if (legPartController == null)
            {
                Debug.LogWarning($"{partObject.name}에 LegPartController가 없습니다.");
                return;
            }

            legPartController.Initialize(legPartsData, _context);
        }
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

        PartsData partsData = null;

        LegPartController legPartController =
            attachedObject.GetComponentInChildren<LegPartController>(true);

        if (legPartController != null)
        {
            partsData = legPartController.Data;
        }

        if (_coreLoadController != null && partsData != null)
        {
            _coreLoadController.RemoveLoad(partsData.RequiredLoad);
        }

        _attachedPartObjects.Remove(attachedObject);

        slot.Clear();

        Destroy(attachedObject);
    }
}