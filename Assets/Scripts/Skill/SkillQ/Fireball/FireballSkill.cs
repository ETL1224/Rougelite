using UnityEngine;

public class FireballSkill : SkillBase
{
    [Header("火球设置")]
    public GameObject fireballPrefab;  // 火球预制体
    public float speed = 15f;          // 飞行速度
    public float radius = 2f;          // 爆炸半径
    public float baseDamage = 10f;     // 基础伤害
    public float lifetime = 3f;        // 火球存活时间

    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (fireballPrefab == null) return;

        // 创建火球实例
        GameObject fireball = Instantiate(fireballPrefab, caster.position + caster.forward, Quaternion.identity);
        Rigidbody rb = fireball.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = caster.forward * speed;  // 使火球沿着玩家面朝的方向飞行
        }

        // 设置火球的伤害和爆炸半径
        FireballProjectile proj = fireball.AddComponent<FireballProjectile>();
        if (proj != null)
        {
            proj.damage = baseDamage * player.skillPower;  // 使用玩家的法强来修改伤害
            proj.radius = radius;
            proj.lifetime = lifetime;
            proj.OnHitEnemy += HandleExplosion; // 注册爆炸事件处理
        }
    }

    private void HandleExplosion(Vector3 position, float damage)
    {
        // 获取范围内所有敌人并造成伤害
        Collider[] hitEnemies = Physics.OverlapSphere(position, radius);
        foreach (var hit in hitEnemies)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);  // 对敌人造成伤害
            }
        }
    }

    // 检查是否可以释放技能
    public override bool CanCast(PlayerState player)
    {
        // 使用技能急速调整冷却时间
        float effectiveCooldown = baseCooldown * (1f - player.skillHaste);
        return Time.time - lastCastTime >= effectiveCooldown;
    }
}
