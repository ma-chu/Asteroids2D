using System;
using UnityEngine;
using UnityEngine.Pool;

public enum PoolTypes
{
    Asteroids,
    Bullets
}
public class PoolOfObjects : MonoBehaviour
{
    public static event Action SpawnAsteroids;

    [SerializeField] private PoolTypes type;
    [SerializeField] private bool collectionChecks;
    [SerializeField] private GameObject prefab;

    //private IObjectPool<GameObject> _pool;
    private ObjectPool<GameObject> _pool;

    private int _countAll;                // реализация этих свойств от unity пока не работает как заявлено
    private int _countInactive;
    
    //public IObjectPool<GameObject> pool
    public ObjectPool<GameObject> pool

    {
        get
        {
            if (_pool == null)
            {
                _pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                    OnDestroyPoolObject, collectionChecks, 10);
            }
            return _pool;
        }
    }

    private GameObject CreatePooledItem()
    {
        var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        obj.GetComponent<SpaceBody>().pool = pool;
        _countAll++;
        _countInactive++;
        return obj;
    }

    // Called when an item is returned to the pool using Release
    private void OnReturnedToPool(GameObject obj)
    {
        obj.SetActive(false);
        _countInactive++;
        if (type == PoolTypes.Asteroids && _countInactive == _countAll && !MenuManager.Instance.Paused)
        {
            SpawnAsteroids?.Invoke();
        }
    }

    // Called when an item is taken from the pool using Get
    private void OnTakeFromPool(GameObject obj)
    {
        obj.SetActive(true);
        _countInactive--;
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    private void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
        _countAll--;
    }

    public void ReleaseAll()
    {
        var first = true;
        foreach (var VARIABLE in transform.GetComponentsInChildren<Transform>())
        {
            if (first)    // себя тоже считает
            {
                first = false;
                continue;
            }
            if (VARIABLE.gameObject.activeSelf) _pool.Release(VARIABLE.gameObject);
        }
    }

}