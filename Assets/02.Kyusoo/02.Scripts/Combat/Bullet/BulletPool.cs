using UnityEngine;
using UnityEngine.Pool; 

public class BulletPool : MonoBehaviour
{
    private GameObject _bulletPrefab;
    private Transform _poolParent;

    private ObjectPool<GameObject> _unityObjectPool;

    public void Initialize(GameObject prefab, int defaultCount)
    {
        _bulletPrefab = prefab;
        _poolParent = new GameObject($"[Pool] {prefab.name}").transform;

        _unityObjectPool = new ObjectPool<GameObject>(
            createFunc: CreateNewBullet,          
            actionOnGet: OnTakeBulletFromPool,    
            actionOnRelease: OnReturnBulletToPool, 
            actionOnDestroy: OnDestroyPoolObject, 
            collectionCheck: true,               
            defaultCapacity: defaultCount,           
            maxSize: defaultCount * 2            
        );

        GameObject[] tempArray = new GameObject[defaultCount];
        for (int i = 0; i < defaultCount; i++)
        {
            tempArray[i] = _unityObjectPool.Get(); 
        }
        for (int i = 0; i < defaultCount; i++)
        {
            _unityObjectPool.Release(tempArray[i]);
        }
    }

    private GameObject CreateNewBullet()
    {
        return Instantiate(_bulletPrefab, _poolParent);
    }

    private void OnTakeBulletFromPool(GameObject bullet)
    {
        bullet.SetActive(true);
    }

    private void OnReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
    }

    private void OnDestroyPoolObject(GameObject bullet)
    {
        Destroy(bullet);
    }

    public GameObject GetBullet()
    {
        return _unityObjectPool.Get();
    }

    public void ReturnBullet(GameObject bullet)
    {
        _unityObjectPool.Release(bullet);
    }
}