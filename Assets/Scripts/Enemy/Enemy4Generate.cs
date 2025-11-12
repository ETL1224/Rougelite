using UnityEngine;

public class Enemy4Generate : SpawnerBase
{
    [Header("巫师生成配置")]
    [SerializeField] private GameObject[] enemy4Prefabs; // 巫师预制体
    [SerializeField] private int wizardInitialCount = 3; // 初始生成3个（远程敌人数量少）
    [SerializeField] private int wizardCountIncrease = 2; // 每波递增1个（避免远程压制）
    [SerializeField] private float wizardMinDistance = 15f; // 生成间距（远程敌人分散）
    [SerializeField] private float firstWaveDelay = 180f; // 第一波延迟180秒生成

    // 延迟启动开关（核心！避免基类提前触发）
    private bool hasStartedSpawning = false;

    protected override void Start()
    {
        // 1. 绑定预制体
        spawnPrefabs = enemy4Prefabs;
        // 2. 自动查找玩家目标
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        // 3. 启用波次生成模式
        isWaveSpawn = true;

        // 核心：延迟180秒后，调用「启动第一波」方法（而非直接赋值nextWaveTime）
        Invoke(nameof(StartFirstWave), firstWaveDelay);

        Debug.Log($"[巫师生成器] 已初始化，将在{firstWaveDelay}秒后生成第一波！");
    }

    protected override void Update()
    {
        // 未启动生成前，不执行任何基类波次逻辑（避免一开始就生成）
        if (!hasStartedSpawning) return;
        // 启动后，执行基类的波次循环（判断下一波时间）
        base.Update();
    }

    // 启动第一波生成（仅执行一次，延迟后触发）
    private void StartFirstWave()
    {
        hasStartedSpawning = true; // 打开开关，允许后续波次循环
        currentWave = 0; // 重置波数（从第1波开始）
        nextWaveTime = Time.time + waveInterval; // 设定下一波时间
        SpawnWave(); // 生成第一波巫师
        Debug.Log($"[巫师生成器] 第一波开始生成！后续每{waveInterval}秒生成一波");
    }


    protected override void SpawnWave()
    {
        usedPositions.Clear();
        int totalCount = wizardInitialCount + currentWave * wizardCountIncrease;
        int spawned = 0;
        int attempts = 0;
        int safetyLimit = 200;

        while (spawned < totalCount && attempts < safetyLimit)
        {
            attempts++;
            Vector3 pos = GetRandomPosition();
            if (CheckDistance(pos)) continue;

            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
            string key = prefab.name;

            GameObject instance = ObjectPool.Instance.SpawnFromPool(key, pos, Quaternion.identity);
            if (instance == null)
            {
                instance = Instantiate(prefab, pos, Quaternion.identity);
            }

            SpawnInitialize(instance);
            usedPositions.Add(pos);
            spawned++;
        }

        Debug.Log($"[巫师] 第{currentWave + 1}波生成 {spawned} 个");
    }

    // 巫师生成位置：地图边缘（远程敌人不用靠近）
    protected override Vector3 GetRandomPosition()
    {
        float x = 0f, z = 0f;
        int edge = Random.Range(0, 4);
        float spawnRadius = mapSize * 0.9f; // 生成范围=地图90%（边缘位置）

        switch (edge)
        {
            case 0: x = Random.Range(-spawnRadius, spawnRadius); z = spawnRadius; break;
            case 1: x = Random.Range(-spawnRadius, spawnRadius); z = -spawnRadius; break;
            case 2: x = -spawnRadius; z = Random.Range(-spawnRadius, spawnRadius); break;
            case 3: x = spawnRadius; z = Random.Range(-spawnRadius, spawnRadius); break;
        }
        return new Vector3(x, 2f, z); // y=2f避免埋地
    }

    // 间距检测（巫师分散生成）
    protected override bool CheckDistance(Vector3 pos)
    {
        foreach (var usedPos in usedPositions)
        {
            if (Vector3.Distance(usedPos, pos) < wizardMinDistance)
                return true;
        }
        foreach (var enemy in EnemyBase.allEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf && enemy is Enemy4AI)
            {
                float dist = Vector3.Distance(enemy.transform.position, pos);
                if (dist < wizardMinDistance)
                    return true;
            }
        }
        return false;
    }

    // 初始化巫师（绑定玩家）
    protected override void SpawnInitialize(GameObject instance)
    {
        Enemy4AI wizard = instance.GetComponent<Enemy4AI>();
        if (wizard != null)
        {
            wizard.player = target;
        }
        else
        {
            Debug.LogError("巫师预制体缺少Enemy4AI组件！", instance);
        }
    }
}