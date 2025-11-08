using UnityEngine;

public class Ore : DestructibleBase
{
    // 覆盖基类默认值：矿石需要3次攻击（假设子弹伤害1，health=3）
    public override float Health { get; set; } = 10f;

    // 重写掉落逻辑：自定义矿石掉落数
    protected override void Drop()
    {
        if (dropPrefab == null) return; // 空预制体或已销毁则跳过

        // 自定义掉落数：
        int dropCount = Random.Range(5, 11); // Random.Range(最小值, 最大值)：最大值不包含

        // 按新数量生成掉落物（位置偏移逻辑保留基类的随机范围，避免扎堆）
        for (int i = 0; i < dropCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1.5f, 1.5f),
                -1.5f,
                Random.Range(-1.5f, 1.5f)
            );
            Instantiate(dropPrefab, transform.position + offset, Quaternion.identity);
        }
    }
}