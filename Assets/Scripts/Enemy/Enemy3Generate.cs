using UnityEngine;

public class Enemy3Generate : SpawnerBase
{
    [Header("Enemy3（石头人）生成配置")]
    [SerializeField] private GameObject[] enemy3Prefabs; // 石头人预制体（单独配置）
    [SerializeField] private float startSpawnDelay = 60f; // 延迟60秒生成第一波（核心参数）
    [SerializeField] private int enemy3InitialCount = 2; // 初始生成数量
    [SerializeField] private int enemy3CountIncrease = 2; // 每波递增数量
    [SerializeField] private float stoneMinDistance = 10f; // 石头人体积大，生成间距更大

    // 标记是否已开始生成（避免重复触发，核心控制）
    private bool hasStartedSpawning = false;

    protected override void Start()
    {
        // 1. 绑定石头人预制体（不与其他敌人混用）
        spawnPrefabs = enemy3Prefabs;
        // 2. 自动查找玩家目标（无需手动绑定）
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        // 3. 启用波次生成模式（必须开启，与基类逻辑联动）
        isWaveSpawn = true;
        // 4. 延迟60秒后，调用「启动第一波」方法（核心延迟逻辑）
        Invoke(nameof(StartFirstWave), startSpawnDelay);

        Debug.Log($"[石头人生成器] 已初始化，将在{startSpawnDelay}秒后生成第一波！");
    }

    protected override void Update()
    {
        // 未到延迟时间/未启动生成，不执行任何波次逻辑（避免一开始就生成）
        if (!hasStartedSpawning) return;
        // 启动后，执行基类的波次循环逻辑（自动判断下一波时间）
        base.Update();
    }

    // 启动第一波生成（延迟后触发，仅执行一次）
    private void StartFirstWave()
    {
        hasStartedSpawning = true; // 标记为已启动，允许后续波次循环
        currentWave = 0; // 重置波数（从第1波开始）
        nextWaveTime = Time.time + waveInterval; // 设定下一波生成时间（当前波生成后，间隔waveInterval秒）
        SpawnWave(); // 生成第一波石头人
        Debug.Log($"[石头人生成器] 第一波开始生成！后续每{waveInterval}秒生成一波");
    }

    // 重写生成数量逻辑（石头人专属：初始少、递增慢，符合体积大的设定）
    protected override void SpawnWave()
    {
        usedPositions.Clear(); // 每波清空已用位置，避免同波重叠
        // 石头人数量 = 初始数量 + 波数 × 递增数量（波数从0开始，第一波就是初始数量）
        int totalCount = enemy3InitialCount + currentWave * enemy3CountIncrease;
        int spawned = 0;
        int attempts = 0;
        int safetyLimit = 200; // 安全尝试次数（避免无限循环）

        while (spawned < totalCount && attempts < safetyLimit)
        {
            attempts++;
            Vector3 pos = GetRandomPosition(); // 用石头人专属生成位置

            // 检查间距：避免和同波/已存在的石头人重叠
            if (CheckDistance(pos)) continue;

            // 随机选择石头人预制体（支持多种变体）
            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
            string key = prefab.name;

            // 从对象池生成（复用性能，避免频繁Instantiate）
            GameObject instance = ObjectPool.Instance.SpawnFromPool(key, pos, Quaternion.identity);
            if (instance == null)
            {
                instance = Instantiate(prefab, pos, Quaternion.identity);
                Debug.Log($"[石头人] 对象池无闲置实例，新实例化一个");
            }

            // 初始化石头人（绑定玩家目标）
            SpawnInitialize(instance);

            usedPositions.Add(pos);
            spawned++;
        }

        // 打印波次日志（方便调试）
        Debug.Log($"[石头人] 第{currentWave + 1}波生成完成！生成数量：{spawned}/{totalCount}（尝试{attempts}次）");
    }

    // 重写生成位置：石头人移速慢，从中边缘生成（比Enemy2近，比Enemy1稍远）
    protected override Vector3 GetRandomPosition()
    {
        float x = 0f, z = 0f;
        int edge = Random.Range(0, 4); // 4个正方向（石头人不需要斜向，避免太分散）
        float spawnRadius = mapSize * 0.7f; // 生成范围=地图大小×0.7（中边缘）

        switch (edge)
        {
            case 0: x = Random.Range(-spawnRadius, spawnRadius); z = spawnRadius; break;   // 上中边缘
            case 1: x = Random.Range(-spawnRadius, spawnRadius); z = -spawnRadius; break;  // 下中边缘
            case 2: x = -spawnRadius; z = Random.Range(-spawnRadius, spawnRadius); break;  // 左中边缘
            case 3: x = spawnRadius; z = Random.Range(-spawnRadius, spawnRadius); break;   // 右中边缘
        }
        return new Vector3(x, 2f, z); // y=2f，避免生成在地下
    }

    // 重写间距检测：石头人体积大，用专属大间距（避免重叠卡顿）
    protected override bool CheckDistance(Vector3 pos)
    {
        // 检查同波已生成的位置
        foreach (var usedPos in usedPositions)
        {
            if (Vector3.Distance(usedPos, pos) < stoneMinDistance)
                return true; // 间距不够，跳过该位置
        }

        // 检查场景中已存在的石头人（避免和旧波次重叠）
        foreach (var enemy in EnemyBase.allEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf && enemy is Enemy3AI)
            {
                float dist = Vector3.Distance(enemy.transform.position, pos);
                if (dist < stoneMinDistance)
                    return true; // 间距不够，跳过该位置
            }
        }

        return false; // 间距合格，可生成
    }

    // 实现抽象方法：初始化石头人（绑定玩家目标，必须实现）
    protected override void SpawnInitialize(GameObject instance)
    {
        Enemy3AI stoneEnemy = instance.GetComponent<Enemy3AI>();
        if (stoneEnemy != null)
        {
            stoneEnemy.player = target; // 给石头人绑定玩家目标，使其能追踪攻击
        }
        else
        {
            Debug.LogError("[石头人] 预制体缺少 Enemy3AI 组件！生成失败！", instance);
            Destroy(instance); // 缺少核心组件，直接销毁避免异常
        }
    }
}