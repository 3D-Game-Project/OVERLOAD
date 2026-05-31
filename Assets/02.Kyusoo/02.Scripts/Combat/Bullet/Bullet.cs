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
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);

        if (Vector3.Distance(_startPosition, transform.position) >= _maxRange)
        {
            ReturnToPool();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if ((_targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (other.TryGetComponent(out EnemyCombatController enemy))
            {
                enemy.TakeDamage(_damage);
            }
            else if (other.TryGetComponent(out PlayerCombatController player))
            {
                player.TakeDamage(_damage);
            }

            ReturnToPool();
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            if (other.GetComponentInParent<EnemyCombatController>() != null ||
                other.GetComponentInParent<PlayerCombatController>() != null)
            {
                return; 
            }

            ReturnToPool();
        }
    }

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