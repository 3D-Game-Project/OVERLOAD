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

    private Transform _targetPart;

    // 총알 데이터 세팅
    // 공격력, 사거리, 타겟레이어 세팅
    // 총구 위치로 startPosition 지정
    public void Setup(int damage, float range, BulletPool pool, LayerMask targetLayer, Transform targetPart = null)
    {
        _damage = damage;
        _maxRange = range;
        _myPool = pool;
        _startPosition = transform.position;
        _targetLayer = targetLayer;
        _targetPart = targetPart;
        _isReturned = false;
    }

    private void Update()
    {
        if (_isReturned) return;

        float moveDistance = _speed * Time.deltaTime;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, moveDistance);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool shouldKeepFlying = true;

        foreach (RaycastHit hit in hits)
        {
            int hitLayer = hit.collider.gameObject.layer;

            if ((_targetLayer.value & (1 << hitLayer)) != 0)
            {
                DurabilityController targetDurability = hit.collider.GetComponentInParent<DurabilityController>();

                if (targetDurability != null)
                {
                    if (_targetPart != null && targetDurability.transform != _targetPart)
                    {
                        continue; 
                    }

                    targetDurability.TakeDamage(_damage);

                    ReturnToPool();
                    shouldKeepFlying = false;
                    break; 
                }
            }
            else if (hitLayer != LayerMask.NameToLayer("Player"))
            {
                ReturnToPool();
                shouldKeepFlying = false;
                break;
            }
        }

        if (shouldKeepFlying)
        {
            transform.Translate(Vector3.forward * moveDistance);

            if (Vector3.Distance(_startPosition, transform.position) >= _maxRange)
            {
                ReturnToPool();
            }
        }
    }

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