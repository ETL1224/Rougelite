using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// 毒雾伤害逻辑：触发器触发+持续伤害+受击特效（高效版）
public class PoisonCloud : MonoBehaviour
{
    public float damagePerTick = 5f; // 每跳伤害（大招级）
    public float tickInterval = 0.5f; // 伤害间隔
    public float cloudDuration = 3f; // 毒雾存在时间
    [Header("敌人受击特效")]
    public GameObject hitEffectPrefab; // 受击特效预制体
    public float effectDuration = 0.5f; // 特效持续时间
    public Vector3 effectOffset = new Vector3(0, 1f, 0); // 特效偏移

    private SphereCollider cloudTrigger; // 毒雾触发器（SphereCollider）
    private List<DestructibleBase> enemiesInCloud = new List<DestructibleBase>(); // 存储在毒雾内的敌人
    private Coroutine damageCoroutine; // 持续伤害协程

    void Start()
    {
        // 获取触发器（必须是SphereCollider，且勾选Is Trigger）
        cloudTrigger = GetComponent<SphereCollider>();
        if (cloudTrigger == null || !cloudTrigger.isTrigger)
        {
            Debug.LogError("毒雾预制体缺少勾选Is Trigger的SphereCollider！");
            Destroy(gameObject);
            return;
        }

        // 3秒后销毁毒雾（避免内存泄漏）
        Destroy(gameObject, cloudDuration);
    }

    // 敌人进入毒雾触发器：添加到列表+启动持续伤害
    private void OnTriggerEnter(Collider other)
    {
        // 只检测Enemy层和Enemy标签的敌人
        if (other.gameObject.layer != LayerMask.NameToLayer("Enemy") || !other.CompareTag("Enemy"))
            return;

        DestructibleBase enemy = other.GetComponent<DestructibleBase>();
        if (enemy != null && !enemiesInCloud.Contains(enemy))
        {
            enemiesInCloud.Add(enemy);
            Debug.Log($"敌人进入毒雾：{enemy.gameObject.name}");

            // 只有当列表有敌人且协程未启动时，才启动协程（避免重复）
            if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(ContinuousDamageCoroutine());
            }
        }
    }

    // 敌人离开毒雾触发器：从列表移除+停止协程（如果没敌人了）
    private void OnTriggerExit(Collider other)
    {
        DestructibleBase enemy = other.GetComponent<DestructibleBase>();
        if (enemy != null && enemiesInCloud.Contains(enemy))
        {
            enemiesInCloud.Remove(enemy);
            Debug.Log($"敌人离开毒雾：{enemy.gameObject.name}");

            // 列表为空时，停止持续伤害协程
            if (enemiesInCloud.Count == 0 && damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    // 持续伤害协程（核心：只在有敌人时运行）
    private IEnumerator ContinuousDamageCoroutine()
    {
        while (enemiesInCloud.Count > 0)
        {
            // 遍历所有在毒雾内的敌人，造成伤害+触发特效
            for (int i = enemiesInCloud.Count - 1; i >= 0; i--)
            {
                DestructibleBase enemy = enemiesInCloud[i];
                // 敌人已销毁（比如死亡），从列表移除
                if (enemy == null)
                {
                    enemiesInCloud.RemoveAt(i);
                    continue;
                }

                // 1. 造成持续伤害
                enemy.TakeDamage(damagePerTick);
                Debug.Log($"毒雾造成 {damagePerTick} 点伤害！目标：{enemy.gameObject.name}");

                // 2. 生成敌人受击特效
                SpawnHitEffect(enemy.transform);
            }

            // 等待tickInterval后，再次造成伤害
            yield return new WaitForSeconds(tickInterval);
        }

        // 协程结束，重置标记
        damageCoroutine = null;
    }

    // 生成敌人受击特效（和之前逻辑一致）
    private void SpawnHitEffect(Transform enemyTransform)
    {
        if (hitEffectPrefab == null) return;

        Vector3 effectPos = enemyTransform.position + effectOffset;
        GameObject effect = Instantiate(
            hitEffectPrefab,
            effectPos,
            Quaternion.identity
        );

        Destroy(effect, effectDuration);
    }

    // 毒雾销毁时，停止协程+清空列表（兜底）
    private void OnDestroy()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        enemiesInCloud.Clear();
    }

    // Scene视图绘制毒雾范围（绿色线框，方便调试）
    private void OnDrawGizmosSelected()
    {
        if (cloudTrigger != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, cloudTrigger.radius);
        }
    }
}