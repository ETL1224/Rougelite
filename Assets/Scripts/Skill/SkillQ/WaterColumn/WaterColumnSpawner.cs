using UnityEngine;

public class WaterColumnSpawner : MonoBehaviour
{
    [HideInInspector] public float radius = 10f;   // 都由Skill传参
    [HideInInspector] public float damage = 15f;
    [HideInInspector] public float duration = 1f;

    public GameObject waterEffectPrefab;   // 水柱特效
    [Header("敌人受击特效")]
    public GameObject hitEffectPrefab;     // 敌人受击特效预制体（拖入水花/冲击特效）
    public float effectDuration = 0.8f;    // 受击特效持续时间（自动销毁）
    public Vector3 effectOffset = new Vector3(0, 1.2f, 0); // 特效偏移

    private void Start()
    {
        // 播放水柱特效
        if (waterEffectPrefab != null)
            Instantiate(waterEffectPrefab, transform.position, Quaternion.identity, transform);

        // 造成一次范围伤害
        DealDamage();

        // 自动销毁
        Destroy(gameObject, duration);
    }

    private void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"水柱击中{hit.name}，造成{damage:F1}伤害");

                    SpawnHitEffect(hit.transform);
                }
            }
        }
    }

    // 生成敌人受击特效
    private void SpawnHitEffect(Transform enemyTransform)
    {
        // 空值判断：没拖特效也不会报错
        if (hitEffectPrefab == null) return;

        // 特效生成位置 = 敌人位置 + 偏移（避免贴地/穿模）
        Vector3 effectPos = enemyTransform.position + effectOffset;

        // 实例化特效（不设为敌人子对象，避免跟随敌人移动）
        GameObject effect = Instantiate(
            hitEffectPrefab,
            effectPos,
            Quaternion.identity // 保持特效原有旋转，也可设为水柱旋转：transform.rotation
        );

        // 延迟销毁特效，避免内存泄漏
        Destroy(effect, effectDuration);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
