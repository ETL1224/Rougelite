using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string key;          // 用于Spawn时识别
        public GameObject prefab;   // 预制体
        public int initialSize = 10; // 初始数量
        public int maxSize = 30;     // 最大数量
        public string tag = "Enemy"; // 1. 新增：可选标签，默认为 "Enemy"
    }

    public static ObjectPool Instance;
    public List<Pool> pools = new List<Pool>();

    private Dictionary<string, Queue<GameObject>> poolDict;
    private Dictionary<string, Pool> poolConfigDict;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        poolDict = new Dictionary<string, Queue<GameObject>>();
        poolConfigDict = new Dictionary<string, Pool>();

        foreach (var pool in pools)
        {
            Queue<GameObject> queue = new Queue<GameObject>();

            // 初始化队列
            for (int i = 0; i < pool.initialSize; i++)
            {
                // 传入整个 pool 对象
                GameObject obj = CreatePoolObject(pool);
                queue.Enqueue(obj);
            }

            poolDict.Add(pool.key, queue);
            poolConfigDict.Add(pool.key, pool);
        }
    }

//改为接受pool对象用于接受tag内容
    private GameObject CreatePoolObject(Pool pool)
    {
        GameObject obj = Instantiate(pool.prefab); // 使用 pool.prefab
        obj.SetActive(false);


        if (!string.IsNullOrEmpty(pool.tag))
        {
            obj.tag = pool.tag;
        }

        // 添加标记组件
        PoolObjectMarker marker = obj.AddComponent<PoolObjectMarker>();
        marker.prefabName = pool.prefab.name;

        // 统一归为子对象，方便查看
        obj.transform.parent = transform;
        return obj;
    }

    // 先激活，再调用 OnSpawn()
    public GameObject SpawnFromPool(string key, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(key) || !poolConfigDict.ContainsKey(key))
        {
            Debug.LogWarning($"对象池中不存在 Key={key}");
            return null;
        }

        Pool poolConfig = poolConfigDict[key];
        Queue<GameObject> queue = poolDict[key];
        GameObject obj = null;

        // 1. 优先取出队列对象
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else if (queue.Count + poolConfig.initialSize < poolConfig.maxSize)
        {
            // 传入整个 poolConfig 对象
            obj = CreatePoolObject(poolConfig);
            Debug.Log($"Key={key} 队列不足，创建新对象");
        }
        else
        {
            obj = queue.Dequeue();
            Debug.LogWarning($"Key={key} 已达最大池大小：{poolConfig.maxSize}，强制复用旧对象");
        }

        // 2. 设置位置/旋转
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.parent = null;

        // 3. 先激活对象，再调用 OnSpawn()
        obj.SetActive(true);

        // 4. 调用对象自身重置逻辑（重置 Rigidbody、Collider、动画等）
        if (obj.TryGetComponent(out IPoolable poolable))
        {
            poolable.OnSpawn();
        }
        return obj;
    }

    // 回收对象
    public void Despawn(GameObject obj)
    {
        if (obj == null || !obj.TryGetComponent(out PoolObjectMarker marker))
        {
            Debug.LogWarning("回收失败：对象为空或非池化对象");
            return;
        }

        string key = marker.prefabName;

        // 调用对象清理逻辑
        if (obj.TryGetComponent(out IPoolable poolable))
        {
            poolable.OnDespawn();
        }

        // 禁用对象、归位
        obj.SetActive(false);
        obj.transform.parent = transform;

        // 回收时放回队列（Spawn 时不再 enqueue）
        if (poolDict.ContainsKey(key))
        {
            poolDict[key].Enqueue(obj);
        }
        else
        {
            poolDict[key] = new Queue<GameObject>();
            poolDict[key].Enqueue(obj);
        }
    }

    // 内部标记类
    private class PoolObjectMarker : MonoBehaviour
    {
        public string prefabName;
    }
}
