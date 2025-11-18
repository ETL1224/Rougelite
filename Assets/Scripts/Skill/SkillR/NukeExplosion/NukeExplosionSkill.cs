using UnityEngine;

public class NukeExplosionSkill : SkillBase
{
    [Header("核弹技能配置")]
    public float radius = 20f; // 爆炸范围（对应指示器半径）
    public float damage = 50f; // 基础伤害（乘player.skillPower）

    [Header("基础特效")]
    public GameObject explosionCoreEffect; // 爆炸核心特效
    public AudioClip explosionSound; // 爆炸音效

    [Header("敌人受击特效")]
    public GameObject hitEffectPrefab; // 敌人受击特效预制体
    public float effectDuration = 0.8f; // 特效持续时间
    public Vector3 effectOffset = new Vector3(0, 1.2f, 0); // 特效偏移

    private void OnEnable()
    {
        castType = SkillCastType.Ground;
        useDirectionIndicator = false;
        indicatorRadius = radius;
        baseCooldown = 60f; // 大招冷却
    }

    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (!CanCast(player))
        {
            Debug.Log($"{skillName} 冷却中！剩余：{Mathf.Ceil(Time.time - lastCastTime)}秒");
            return;
        }
        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        // 1. 播放爆炸核心特效
        if (explosionCoreEffect != null)
        {
            Destroy(Instantiate(explosionCoreEffect, castPos, Quaternion.identity), 2f);
        }

        // 2. 播放爆炸音效
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, castPos, 1.0f);
        }

        // 3. 范围伤害+敌人受击特效（核心功能）
        DealDamage(castPos, player);

        Debug.Log($"科学的地狱火炮触发！落点：{castPos}，伤害：{damage * player.skillPower}");
    }

    private void DealDamage(Vector3 explosionPos, PlayerState player)
    {
        // 范围检测Enemy层+标签
        Collider[] hits = Physics.OverlapSphere(explosionPos, radius, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    float finalDamage = damage * player.skillPower;
                    enemy.TakeDamage(finalDamage);
                    Debug.Log($"核弹击中{hit.name}，造成{finalDamage:F1}伤害！");

                    // 触发敌人受击特效
                    SpawnHitEffect(hit.transform);
                }
            }
        }
    }

    // 生成敌人受击特效
    private void SpawnHitEffect(Transform enemyTransform)
    {
        if (hitEffectPrefab == null) return;

        Vector3 effectPos = enemyTransform.position + effectOffset;
        GameObject effect = Instantiate(hitEffectPrefab, effectPos, Quaternion.identity);
        Destroy(effect, effectDuration);
    }

    // 调试用：Scene视图显示爆炸范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}