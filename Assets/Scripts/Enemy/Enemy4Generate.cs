using UnityEngine;

public class Enemy4Generate : SpawnerBase
{
    [Header("巫师生成配置")]
    [SerializeField] private GameObject[] enemy4Prefabs; // 巫师预制体
    [SerializeField] private int wizardInitialCount = 3; // 初始生成3个（远程敌人数量少）
    [SerializeField] private int wizardCountIncrease = 2; // 每波递增1个（避免远程压制）
    [SerializeField] private float wizardMinDistance = 15f; // 生成间距（远程敌人分散）
    [SerializeField] private float firstWaveDelay = 120f; // 第一波延迟30秒生成（先出近战，再出远程）

    protected override void Start()
    {
        spawnPrefabs = enemy4Prefabs;
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        isWaveSpawn = true;
        nextWaveTime = Time.time + firstWaveDelay; // 延迟生成
        Debug.Log($"巫师生成器启动！{firstWaveDelay}秒后生成第一波远程敌人");
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