using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 40f;

    private int _damage;
    private float _maxRange;
    private BulletPool _myPool;
    private LayerMask _targetLayer;

    private Vector3 _startPosition;
    private bool _isReturned;

    // 총알 데이터 세팅
    // 공격력, 사거리, 타겟레이어 세팅
    // 총구 위치로 startPosition 지정
    public void Setup(int damage, float range, BulletPool pool, LayerMask targetLayer)
    {
        _damage = damage;
        _maxRange = range;
        _myPool = pool;
        _startPosition = transform.position;
        _targetLayer = targetLayer;
        _isReturned = false;
    }

    private void Update()
    {
        if (_isReturned) return;

        float moveDistance = _speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, moveDistance))
        {
            int hitLayer = hit.collider.gameObject.layer;

            if ((_targetLayer.value & (1 << hitLayer)) != 0)
            {
                DurabilityController targetDurability = hit.collider.GetComponentInParent<DurabilityController>();

                if (targetDurability != null)
                {
                    targetDurability.TakeDamage(_damage);
                }

                ReturnToPool();
                return;
            }

            if (hitLayer != LayerMask.NameToLayer("Player"))
            {
                ReturnToPool();
                return;
            }
        }

        transform.Translate(Vector3.forward * moveDistance);

        if (Vector3.Distance(_startPosition, transform.position) >= _maxRange)
        {
            ReturnToPool();
        }
    }

    // 피격 판정
    // 타겟 레이어를 맞추었는지 확인 후, 맞췄다면 TakeDamage 함수 호출
    // Layer를 지정하지 않은 곳에 부딪히는 경우 총알 제거
    private void OnTriggerEnter(Collider other)
    {
      
    }

    // 총알 회수 함수
    private void ReturnToPool()
    {
        if (_isReturned) return;

        if (_myPool != null)
        {
            _isReturned = true;
            _myPool.ReturnBullet(gameObject);
        }
    }
}