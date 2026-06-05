using System.Collections.Generic;
using UnityEngine;

public class CorePartsController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private CorePowerController _powerController;
    [SerializeField] private PlayerInputHandler _inputHandler;

    private CorePartContext _context;

    private readonly List<IPart> _allParts = new();
    private readonly List<IWeaponPart> _weaponParts = new();
    private readonly List<ILocomotionPart> _locomotionParts = new();
    private readonly List<IBoosterPart> _boosterParts = new();

    private void Awake()
    {
        _context = new CorePartContext(
            transform,
            _powerController,
            _inputHandler
        );
    }

    public void AttachPart(PartsData data, Transform attachPoint)
    {
        if (data == null)
        {
            Debug.LogWarning("장착할 PartsData가 없습니다.");
            return;
        }

        if (data.PartsPrefab == null)
        {
            Debug.LogWarning($"{data.PartsName}의 프리팹이 없습니다.");
            return;
        }

        if (!_powerController.CanEquip(data.RequiredPower))
        {
            Debug.LogWarning("코어 출력이 부족해서 장착할 수 없습니다.");
            return;
        }

        GameObject partObject = Instantiate(data.PartsPrefab, attachPoint);
        partObject.transform.localPosition = Vector3.zero;
        partObject.transform.localRotation = Quaternion.identity;

        if (!partObject.TryGetComponent(out IPart part))
        {
            Debug.LogError($"{data.PartsPrefab.name}에 IPart를 구현한 스크립트가 없습니다.");
            Destroy(partObject);
            return;
        }

        part.Initialize(data, _context);
        part.OnAttached();

        RegisterPart(part);

        _powerController.AddRequiredPower(data.RequiredPower);
    }

    private void RegisterPart(IPart part)
    {
        _allParts.Add(part);

        if (part is IWeaponPart weaponPart)
            _weaponParts.Add(weaponPart);

        if (part is ILocomotionPart locomotionPart)
            _locomotionParts.Add(locomotionPart);

        if (part is IBoosterPart boosterPart)
            _boosterParts.Add(boosterPart);
    }

    public void DetachPart(IPart part)
    {
        if (part == null)
            return;

        part.OnDetached();

        _allParts.Remove(part);

        if (part is IWeaponPart weaponPart)
            _weaponParts.Remove(weaponPart);

        if (part is ILocomotionPart locomotionPart)
            _locomotionParts.Remove(locomotionPart);

        if (part is IBoosterPart boosterPart)
            _boosterParts.Remove(boosterPart);

        _powerController.RemoveRequiredPower(part.Data.RequiredPower);

        if (part is MonoBehaviour mono)
            Destroy(mono.gameObject);
    }
}