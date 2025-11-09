using UnityEngine;

public class EnemyGenerate : SpawnerBase
{
    [Header("敌人生成扩展")]
    [SerializeField] private GameObject[] enemyPrefabs; // 暴露给Inspector的敌人预制体

    // 初始化（覆盖父类逻辑，传递预制体）
    protected override void Start()
    {
        spawnPrefabs = enemyPrefabs; // 将子类的敌人预制体传给父类
        base.Start();                // 执行父类初始化
    }

    // 实现抽象方法：初始化敌人（绑定玩家）
    protected override void SpawnInitialize(GameObject instance)
    {
        EnemyBase enemy = instance.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.player = target; // 绑定目标（如玩家）
        }
        else
        {
            Debug.LogError("敌人预制体缺少 EnemyBase 组件！", instance);
        }
    }
}