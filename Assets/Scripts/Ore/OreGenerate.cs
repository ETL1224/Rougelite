using UnityEngine;

public class OreGenerate : SpawnerBase
{
    [Header("矿石生成扩展")]
    [SerializeField] private GameObject[] orePrefabs; // 矿石预制体
    [SerializeField] private int initialOreCount = 30; // 一次性生成数量
    [SerializeField] private float generateRange = 150f; // 中心范围
    [SerializeField] private float minOreDistance = 5f;   // 矿石间距

    protected override void Start()
    {
        // 覆盖基类参数
        initialCount = initialOreCount;
        mapSize = generateRange;
        minDistance = minOreDistance;
        isWaveSpawn = false; // 关闭波次，一次性生成
        spawnPrefabs = orePrefabs;

        base.Start();
    }

    // 重写：中心范围生成（替代基类的边缘生成）
    protected override Vector3 GetRandomPosition()
    {
        float x = Random.Range(-mapSize, mapSize);
        float z = Random.Range(-mapSize, mapSize);
        return new Vector3(x, 2f, z);
    }

    // 重写：矿石初始化（无特殊逻辑时空实现，或设置属性）
    protected override void SpawnInitialize(GameObject instance)
    {
        Ore ore = instance.GetComponent<Ore>();
        if (ore != null)
        {
            // 可设置矿石属性（如health），但建议Inspector配置
        }
    }
}