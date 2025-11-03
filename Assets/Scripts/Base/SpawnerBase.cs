using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnerBase : MonoBehaviour
{
    [Header("生成基础设置")]
    [SerializeField] protected GameObject[] spawnPrefabs;  // 生成预制体（敌人、矿石等）
    [SerializeField] protected int initialCount = 10;      // 初始数量
    [SerializeField] protected int countIncrease = 5;      // 每波增加数量
    [SerializeField] protected float mapSize = 120f;       // 生成范围（边缘/中心）
    [SerializeField] protected float minDistance = 3f;     // 生成间距（防止重叠）
    [SerializeField] protected float waveInterval = 20f;   // 波次间隔
    [SerializeField] protected bool isWaveSpawn = true;    // 是否波次生成（false则一次性）

    [Header("目标引用")]
    [SerializeField] protected Transform target; // 生成对象的目标（如玩家）

    protected List<Vector3> usedPositions = new List<Vector3>(); // 已用位置
    protected int currentWave;                                   // 当前波数
    protected float nextWaveTime;                                // 下一波时间

    protected virtual void Start()
    {
        // 自动查找目标（如玩家）
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (isWaveSpawn)
        {
            nextWaveTime = Time.time + waveInterval;
            SpawnWave();
        }
        else
        {
            SpawnWave(); // 一次性生成，直接调用
        }
    }

    protected virtual void Update()
    {
        if (isWaveSpawn && Time.time >= nextWaveTime)
        {
            currentWave++;
            nextWaveTime = Time.time + waveInterval;
            SpawnWave();
        }
    }

    protected virtual void SpawnWave()
    {
        int totalCount = initialCount + currentWave * countIncrease;
        int spawned = 0;
        int attempts = 0;
        int safetyLimit = 200;

        while (spawned < totalCount && attempts < safetyLimit)
        {
            attempts++;
            Vector3 pos = GetRandomPosition(); // 虚方法，子类重写位置逻辑

            if (CheckDistance(pos)) continue; // 检查间距

            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
            GameObject instance = Instantiate(prefab, pos, Quaternion.identity);

            SpawnInitialize(instance); // 抽象方法，子类初始化对象

            usedPositions.Add(pos);
            spawned++;
        }

        Debug.Log($"[{name}] 生成 {spawned} 个对象（尝试 {attempts} 次）");
    }

    // ========== 虚方法：生成位置（子类重写实现边缘/中心生成） ==========
    protected virtual Vector3 GetRandomPosition()
    {
        // 基类默认：边缘生成（敌人逻辑）
        float x = 0f, z = 0f;
        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: x = Random.Range(-mapSize, mapSize); z = mapSize; break;   // 上边
            case 1: x = Random.Range(-mapSize, mapSize); z = -mapSize; break;  // 下边
            case 2: x = -mapSize; z = Random.Range(-mapSize, mapSize); break;  // 左边
            case 3: x = mapSize; z = Random.Range(-mapSize, mapSize); break;   // 右边
        }
        return new Vector3(x, 2f, z);
    }

    // ========== 抽象方法：对象初始化（子类实现，如敌人绑定玩家） ==========
    protected abstract void SpawnInitialize(GameObject instance);

    // ========== 辅助方法：间距检测 ==========
    protected bool CheckDistance(Vector3 pos)
    {
        foreach (var usedPos in usedPositions)
        {
            if (Vector3.Distance(usedPos, pos) < minDistance)
                return true;
        }
        return false;
    }
}