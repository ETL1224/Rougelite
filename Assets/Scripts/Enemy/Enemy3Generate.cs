using UnityEngine;

public class Enemy3Generate : SpawnerBase
{
    [Header("Enemy3（石头人）生成配置")]
    [SerializeField] private GameObject[] enemy3Prefabs; // 石头人预制体（单独配置）
    [SerializeField] private int enemy3InitialCount = 2; // 初始生成数量
    [SerializeField] private int enemy3CountIncrease = 3; // 每波递增数量
    [SerializeField] private float stoneMinDistance = 10f; // 生成间距（石头人体积大，避免重叠）

    protected override void Start()
    {
        // 1. 绑定石头人预制体
        spawnPrefabs = enemy3Prefabs;
        // 2. 自动查找玩家目标
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        // 3. 波次生成模式（和Enemy1同步）
        isWaveSpawn = true;
        // 4. 第一波立即生成（无延迟）
        nextWaveTime = Time.time + waveInterval;
        SpawnWave();
        Debug.Log("石头人生成器启动！每20秒生成一波肉盾敌人");
    }

    // 重写生成数量逻辑（使用石头人专属数量）
    protected override void SpawnWave()
    {
        usedPositions.Clear(); // 每波清空已用位置，避免重叠
        int totalCount = enemy3InitialCount + currentWave * enemy3CountIncrease;
        int spawned = 0;
        int attempts = 0;
        int safetyLimit = 200; // 安全尝试次数

        while (spawned < totalCount && attempts < safetyLimit)
        {
            attempts++;
            Vector3 pos = GetRandomPosition();

            // 检查间距（石头人体积大，间距要更大）
            if (CheckDistance(pos)) continue;

            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
            string key = prefab.name;

            // 从对象池生成（复用性能）
            GameObject instance = ObjectPool.Instance.SpawnFromPool(key, pos, Quaternion.identity);
            if (instance == null)
            {
                instance = Instantiate(prefab, pos, Quaternion.identity);
                Debug.Log($"石头人对象池无闲置，新实例化一个");
            }

            SpawnInitialize(instance); // 初始化石头人（绑定玩家）
            usedPositions.Add(pos);
            spawned++;
        }

        Debug.Log($"[石头人] 第{currentWave + 1}波生成 {spawned} 个（尝试{attempts}次）");
    }

    // 重写生成位置：地图中边缘（不用太远，石头人移速慢）
    protected override Vector3 GetRandomPosition()
    {
        float x = 0f, z = 0f;
        int edge = Random.Range(0, 4);
        // 生成范围=mapSize×0.7（中边缘，比Enemy1近）
        float spawnRadius = mapSize * 0.7f;

        switch (edge)
        {
            case 0: x = Random.Range(-spawnRadius, spawnRadius); z = spawnRadius; break;   // 上中边缘
            case 1: x = Random.Range(-spawnRadius, spawnRadius); z = -spawnRadius; break;  // 下中边缘
            case 2: x = -spawnRadius; z = Random.Range(-spawnRadius, spawnRadius); break;  // 左中边缘
            case 3: x = spawnRadius; z = Random.Range(-spawnRadius, spawnRadius); break;   // 右中边缘
        }
        return new Vector3(x, 2f, z); // y=2f，避免埋地
    }

    // 重写间距检测（使用石头人专属大间距）
    protected override bool CheckDistance(Vector3 pos)
    {
        foreach (var usedPos in usedPositions)
        {
            if (Vector3.Distance(usedPos, pos) < stoneMinDistance)
                return true;
        }
        // 额外检查场景中已激活的石头人（避免和旧石头人重叠）
        foreach (var enemy in EnemyBase.allEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf && enemy is Enemy3AI)
            {
                float dist = Vector3.Distance(enemy.transform.position, pos);
                if (dist < stoneMinDistance)
                    return true;
            }
        }
        return false;
    }

    // 实现抽象方法：初始化石头人（绑定玩家）
    protected override void SpawnInitialize(GameObject instance)
    {
        Enemy3AI stoneEnemy = instance.GetComponent<Enemy3AI>();
        if (stoneEnemy != null)
        {
            stoneEnemy.player = target;
        }
        else
        {
            Debug.LogError("石头人预制体缺少Enemy3AI组件！", instance);
        }
    }
}