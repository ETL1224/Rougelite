using UnityEngine;

public class WaterColumnSpawner : MonoBehaviour
{
    [HideInInspector] public float radius = 10f;   // 都由Skill传参
    [HideInInspector] public float damage = 15f;
    [HideInInspector] public float duration = 1f;

    public GameObject waterEffectPrefab;   // 水柱特效

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
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
