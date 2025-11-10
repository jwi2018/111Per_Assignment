using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private Dictionary<GameObject, Queue<PooledObjectWrapper>> _poolDictionary = new Dictionary<GameObject, Queue<PooledObjectWrapper>>();
    private Dictionary<GameObject, PoolSetup> _poolSetupMap = new Dictionary<GameObject, PoolSetup>();

    private Transform _poolRoot;

    [System.Serializable]
    public class PoolSetup
    {
        public GameObject prefab;
        [Min(0)] public int initialSize;
        [Min(-1)] public int maxSize;       // -1: 무한대, 0이상: 최대 개수
        [Min(0)] public float shrinkDelaySeconds; // 0: 풀 줄이기 비활성화, 양수: 해당 시간 이후 풀 줄이기 시작

        public static PoolSetup GetDefault(GameObject defaultPrefab)
        {
            return new PoolSetup
            {
                prefab = defaultPrefab,
                initialSize = 3,
                maxSize = 100,
                shrinkDelaySeconds = 5f
            };
        }
    }

    [Header("초기 풀 설정")]
    [SerializeField] private List<PoolSetup> _initialPoolSetups = new List<PoolSetup>();

    [Header("Shrinking Pool 설정")]
    [SerializeField] private float _shrinkCheckInterval = 5f; // 풀 삭제 검사 주기

    // 풀에 들어간 오브젝트와 해당 오브젝트가 풀에 반납된 시간을 기록
    private class PooledObjectWrapper
    {
        public GameObject obj;
        public float despawnTime;
    }

    protected override void Awake()
    {
        base.Awake();

        if (this != Instance)
        {
            return;
        }

        _poolRoot = new GameObject("ObjectPoolRoot").transform;
        _poolRoot.SetParent(this.transform);

        InitializePools();

        // 풀 줄이기 코루틴 시작
        if (_shrinkCheckInterval > 0)
        {
            StartCoroutine(ShrinkPoolsRoutine());
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        StopAllCoroutines();
        ClearAllPools();
    }

    private void InitializePools()
    {
        foreach (PoolSetup setup in _initialPoolSetups)
        {
            if (setup.prefab == null) continue;
            CreatePool(setup);
        }
    }

    /// <summary>
    /// 새로운 오브젝트 풀을 생성
    /// </summary>
    /// <param name="poolSetup"></param>
    public void CreatePool(PoolSetup poolSetup)
    {
        if (poolSetup.prefab == null) return;
        if (_poolDictionary.ContainsKey(poolSetup.prefab)) return;

        Queue<PooledObjectWrapper> newPool = new Queue<PooledObjectWrapper>();
        for (int i = 0; i < poolSetup.initialSize; i++)
        {
            GameObject obj = Instantiate(poolSetup.prefab, _poolRoot);
            obj.SetActive(false);
            newPool.Enqueue(new PooledObjectWrapper { obj = obj, despawnTime = Time.time });
        }
        _poolDictionary.Add(poolSetup.prefab, newPool);
        _poolSetupMap.Add(poolSetup.prefab, poolSetup);
    }

    /// <summary>
    /// 풀에서 오브젝트를 가져옴
    /// </summary>
    public GameObject Spawn(GameObject prefab, Transform parent = null, bool worldPositionStays = false)
    {
        if (prefab == null) return null;

        if (!_poolDictionary.TryGetValue(prefab, out Queue<PooledObjectWrapper> pool))
        {
            PoolSetup defaultSetup = PoolSetup.GetDefault(prefab);

            CreatePool(defaultSetup);
            pool = _poolDictionary[prefab];
        }

        GameObject objToSpawn;
        if (pool.Count > 0)
        {
            objToSpawn = pool.Dequeue().obj;
        }
        else
        {
            objToSpawn = Instantiate(prefab, _poolRoot);
        }

        objToSpawn.transform.SetParent(parent == null ? _poolRoot : parent, worldPositionStays);
        objToSpawn.SetActive(true);
        objToSpawn.GetComponent<IPoolable>()?.OnSpawned();

        return objToSpawn;
    }

    /// <summary>
    /// 오브젝트를 풀에 반환 / 최대 크기에 도달하면 오브젝트를 파괴
    /// </summary>
    public void Despawn(GameObject obj, GameObject originalPrefab)
    {
        if (obj == null) return;
        if (originalPrefab == null)
        {
            Destroy(obj);
            return;
        }

        obj.GetComponent<IPoolable>()?.OnDespawned();
        obj.SetActive(false);
        obj.transform.SetParent(_poolRoot);

        if (_poolDictionary.TryGetValue(originalPrefab, out Queue<PooledObjectWrapper> pool) && _poolSetupMap.TryGetValue(originalPrefab, out PoolSetup setup))
        {
            if (setup.maxSize != -1 && pool.Count >= setup.maxSize)
            {
                Destroy(obj);
            }
            else
            {
                pool.Enqueue(new PooledObjectWrapper { obj = obj, despawnTime = Time.time });
            }
        }
        else
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// 비활성화된 풀의 오브젝트들을 주기적으로 검사하여 줄이는 코루틴
    /// </summary>
    private IEnumerator ShrinkPoolsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_shrinkCheckInterval);

            float currentTime = Time.time;

            foreach (var prefabEntry in _poolDictionary)
            {
                GameObject prefab = prefabEntry.Key;
                Queue<PooledObjectWrapper> pool = prefabEntry.Value;

                if (!_poolSetupMap.TryGetValue(prefab, out PoolSetup setup)) continue;

                if (setup.shrinkDelaySeconds <= 0) continue;

                int currentPoolCount = pool.Count;
                int destroyableCount = currentPoolCount - setup.initialSize;

                while (pool.Count > setup.initialSize && pool.Peek().despawnTime + setup.shrinkDelaySeconds < currentTime)
                {
                    PooledObjectWrapper wrapperToDestroy = pool.Dequeue();
                    Destroy(wrapperToDestroy.obj);
                }
            }
        }
    }

    /// <summary>
    /// 특정 프리팹에 대한 풀을 비우고 모든 오브젝트를 파괴
    /// 필요 시 수동으로 호출
    /// </summary>
    public void ClearPool(GameObject prefab)
    {
        if (_poolDictionary.TryGetValue(prefab, out Queue<PooledObjectWrapper> pool))
        {
            while (pool.Count > 0)
            {
                Destroy(pool.Dequeue().obj);
            }
            _poolDictionary.Remove(prefab);
            _poolSetupMap.Remove(prefab);
        }
    }

    /// <summary>
    /// 모든 풀을 비우고 모든 풀링된 오브젝트를 파괴
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pair in _poolDictionary)
        {
            foreach (PooledObjectWrapper wrapper in pair.Value)
            {
                Destroy(wrapper.obj);
            }
        }
        _poolDictionary.Clear();
        _poolSetupMap.Clear();
    }
}