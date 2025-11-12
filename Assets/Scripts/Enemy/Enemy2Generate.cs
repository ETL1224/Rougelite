using UnityEngine;

public class Enemy2Generate : SpawnerBase
{
    [Header("Enemy2（狼）生成配置")]
    [SerializeField] private GameObject[] enemy2Prefabs; // 狼形敌人预制体（单独配置，不与Enemy1混用）
    [SerializeField] private float startSpawnDelay = 120f; // 延迟120秒生成
    [SerializeField] private int enemy2InitialCount = 5; // 初始生成数量
    [SerializeField] private int enemy2CountIncrease = 3; // 每波递增数量

    // 标记是否已开始生成（避免重复触发）
    private bool hasStartedSpawning = false;

    protected override void Start()
    {
        // 1. 绑定Enemy2预制体
        spawnPrefabs = enemy2Prefabs;
        // 2. 自动查找玩家目标
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        // 3. 波次生成模式（必须开启）
        isWaveSpawn = true;
        // 4. 延迟60秒后启动第一波生成
        Invoke(nameof(StartFirstWave), startSpawnDelay);
    }

    protected override void Update()
    {
        // 未到延迟时间，不执行波次逻辑
        if (!hasStartedSpawning) return;
        // 延迟结束后，执行正常波次生成（继承基类逻辑）
        base.Update();
    }

    // 启动第一波生成（延迟后调用）
    private void StartFirstWave()
    {
        hasStartedSpawning = true;
        currentWave = 0; // 重置波数
        nextWaveTime = Time.time + waveInterval; // 下一波时间
        SpawnWave(); // 生成第一波狼形敌人
        Debug.Log("狼形敌人开始生成！");
    }

    // 重写生成数量逻辑（使用Enemy2专属数量配置）
    protected override void SpawnWave()
    {
        // 狼形敌人数量 = 初始数量 + 波数 × 递增数量（比Enemy1更多）
        int totalCount = enemy2InitialCount + currentWave * enemy2CountIncrease;
        int spawned = 0;
        int attempts = 0;
        int safetyLimit = 200; // 安全限制（比Enemy1高，避免生成失败）

        while (spawned < totalCount && attempts < safetyLimit)
        {
            attempts++;
            Vector3 pos = GetRandomPosition();

            // 检查间距（避免狼群扎堆）
            if (CheckDistance(pos)) continue;

            // 随机选择Enemy2预制体（支持多种狼形变体）
            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
            string key = prefab.name;

            // 从对象池生成（复用性能更好）
            GameObject instance = ObjectPool.Instance.SpawnFromPool(key, pos, Quaternion.identity);
            if (instance == null)
            {
                instance = Instantiate(prefab, pos, Quaternion.identity);
            }

            // 初始化狼形敌人（绑定玩家）
            SpawnInitialize(instance);

            usedPositions.Add(pos);
            spawned++;
        }

        Debug.Log($"[狼形敌人] 第{currentWave + 1}波生成 {spawned} 个（尝试 {attempts} 次）");
    }

    // 重写生成位置（可选：狼形敌人从地图边缘更分散的位置生成）
    protected override Vector3 GetRandomPosition()
    {
        float x = 0f, z = 0f;
        int edge = Random.Range(0, 8); // 8个方向（比Enemy1多4个斜向）
        switch (edge)
        {
            case 0: x = Random.Range(-mapSize, mapSize); z = mapSize * 1.2f; break;   // 上边缘外侧
            case 1: x = Random.Range(-mapSize, mapSize); z = -mapSize * 1.2f; break;  // 下边缘外侧
            case 2: x = -mapSize * 1.2f; z = Random.Range(-mapSize, mapSize); break;  // 左边缘外侧
            case 3: x = mapSize * 1.2f; z = Random.Range(-mapSize, mapSize); break;   // 右边缘外侧
            case 4: x = mapSize * 1.2f; z = mapSize * 1.2f; break; // 右上斜向
            case 5: x = mapSize * 1.2f; z = -mapSize * 1.2f; break; // 右下斜向
            case 6: x = -mapSize * 1.2f; z = mapSize * 1.2f; break; // 左上斜向
            case 7: x = -mapSize * 1.2f; z = -mapSize * 1.2f; break; // 左下斜向
        }
        return new Vector3(x, 2f, z); // y=2f，避免埋地
    }

    // 实现抽象方法：初始化狼形敌人（绑定玩家目标）
    protected override void SpawnInitialize(GameObject instance)
    {
        Enemy2AI wolfEnemy = instance.GetComponent<Enemy2AI>();
        if (wolfEnemy != null)
        {
            wolfEnemy.player = target; // 绑定玩家目标
        }
        else
        {
            Debug.LogError("狼形敌人预制体缺少 Enemy2AI 组件！", instance);
        }
    }
}