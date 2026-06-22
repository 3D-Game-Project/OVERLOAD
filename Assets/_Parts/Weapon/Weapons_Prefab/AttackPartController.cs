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
        // 규수: 해당 부분이 AttackPartController의 OwerLayer와 TargetLayer를 프리팹에서 직접 설정하지 않도록(플레이어, 에너미를 따로 할 수 없기에)
        // 자동으로 지정해주는방식으로 수정
        if (_context != null && _context.OwnerRoot != null)
        {
            _aimProvider = _context.OwnerRoot.GetComponentInChildren<IAimProvider>();
            _ownerLayer = _context.OwnerRoot.gameObject.layer;

            if (_ownerLayer == LayerMask.NameToLayer("Player"))
            {
                _targetLayer = 1 << LayerMask.NameToLayer("Enemy");
            }
            else if (_ownerLayer == LayerMask.NameToLayer("Enemy"))
            {
                _targetLayer = 1 << LayerMask.NameToLayer("Player");
            }
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
        //    if (!isReloadPressed)
        //        return;

        //    if (_fireManager == null)
        //        return;

        //    _fireManager.StartReload();

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