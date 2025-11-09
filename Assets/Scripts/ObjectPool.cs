using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string key;         // 关键字，用于Spawn时识别
        public GameObject prefab;  // 预制体
        public int size = 10;      // 初始池大小
    }

    public static ObjectPool Instance;
    public List<Pool> pools = new List<Pool>();

    private Dictionary<string, Queue<GameObject>> poolDict;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        poolDict = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            poolDict.Add(pool.key, queue);
        }
    }

    public GameObject SpawnFromPool(string key, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(key))
        {
            Debug.LogWarning($"对象池中不存在 Key={key}");
            return null;
        }

        Queue<GameObject> queue = poolDict[key];
        GameObject obj = queue.Dequeue();

        obj.SetActive(true);
        obj.transform.SetPositionAndRotation(position, rotation);

        queue.Enqueue(obj); // 放回队尾以循环复用

        return obj;
    }

    public void Despawn(GameObject obj)
    {
        obj.SetActive(false);
    }
}
