using UnityEngine;

public class FireballSkill : SkillBase
{
    [Header("火球配置")]
    public GameObject fireballPrefab;
    public float speed = 15f;
    public float radius = 5f;
    public float baseDamage = 10f;
    public float lifetime = 3f;

    private void OnEnable()
    {
        castType = SkillCastType.Direction;
        useDirectionIndicator = true;      // 用方向指示器
        indicatorRadius = 3f;              // 箭头长度
    }

    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (!CanCast(player)) return;
        lastCastTime = Time.time;
        Cast(castPos, caster, player, dir);
    }

    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        // 默认版本留空，用方向版本
    }

    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (fireballPrefab == null)
        {
            Debug.LogError("未赋值 fireballPrefab！");
            return;
        }

        GameObject fireball = Instantiate(fireballPrefab, castPos, Quaternion.LookRotation(dir));
        FireballProjectile proj = fireball.GetComponent<FireballProjectile>();

        if (proj != null)
        {
            proj.damage = baseDamage * player.skillPower;
            proj.radius = radius;
            proj.lifetime = lifetime;

            // 注册事件，处理范围伤害
            proj.OnExplode += HandleExplosion;
        }

        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = dir * speed;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        Debug.Log($"释放火球：方向 {dir}");
    }

    // 处理火球爆炸事件（范围伤害）
    private void HandleExplosion(Vector3 position, float damage, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"敌人 {hit.gameObject.name} 受到 {damage:F1} 点伤害");
                }
            }
        }
    }
}
