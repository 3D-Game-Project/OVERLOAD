using UnityEngine;

public class AttackPartController : PartBehaviour, IAttackPart
{
    [Header("Fire")]
    [SerializeField] private FireManager _fireManager;

    [Header("Owner / Target")]
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private int _ownerLayer;

    private AttackPartsData _attackData;
    private IAimProvider _aimProvider;

    public override void Initialize(PartsData data, CorePartContext context)
    {
        base.Initialize(data, context);

        if (!TryGetData(out _attackData))
        {
            Debug.LogError($"{gameObject.name}에는 AttackPartsData가 필요합니다.");
            return;
        }

        if (_fireManager == null)
            _fireManager = GetComponent<FireManager>();

        if (_fireManager == null)
        {
            Debug.LogError($"{gameObject.name}에 FireManager가 없습니다.");
            return;
        }

        if (_context != null && _context.OwnerRoot != null)
        {
            _aimProvider = _context.OwnerRoot.GetComponentInChildren<IAimProvider>();
        }

        _fireManager.InitializeWeapon(
            _attackData,
            _targetLayer,
            _ownerLayer
        );

        Debug.Log(
            $"공격 파츠 초기화 완료\n" +
            $"Parts Name: {_attackData.PartsName}\n" +
            $"Damage: {_attackData.Damage}\n" +
            $"Fire Type: {_attackData.FireType}"
        );
    }

    public void HandleAttack(bool isAttackPressed)
    {
        if (!isAttackPressed)
            return;

        if (_fireManager == null)
            return;

        if (_attackData == null)
            return;

        Vector3 targetPoint = GetTargetPoint();

        _fireManager.TryFire(targetPoint);
    }

    public void HandleReload(bool isReloadPressed)
    {
        if (!isReloadPressed)
            return;

        if (_fireManager == null)
            return;

        _fireManager.StartReload();
    }

    private Vector3 GetTargetPoint()
    {
        if (_aimProvider != null)
            return _aimProvider.GetAimPoint();

        if (Camera.main != null)
        {
            return Camera.main.transform.position +
                   Camera.main.transform.forward * _attackData.Range;
        }

        return transform.position + transform.forward * _attackData.Range;
    }
}