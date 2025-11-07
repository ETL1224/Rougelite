using UnityEngine;

public class FireballSkill : SkillBase
{
    [Header("火球技能配置")]
    public GameObject fireballPrefab;  // 火球预制体（必须挂载FireballProjectile）
    public float speed = 15f;          // 飞行速度
    public float radius = 5f;          // 爆炸半径
    public float baseDamage = 10f;     // 基础伤害
    public float lifetime = 3f;        // 火球存活时间

    // 实现释放逻辑
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        // 1. 基础校验（避免空引用）
        if (fireballPrefab == null)
        {
            Debug.LogError("火球技能：未赋值 fireballPrefab！");
            return;
        }
        if (caster == null || player == null)
            return;

        Debug.Log($"释放火球术！法强：{player.skillPower}，最终伤害：{baseDamage * player.skillPower}");

        // 2. 生成火球（修正位置：直接用castPos（CastPoint位置），避免偏移错误）
        GameObject fireball = Instantiate(fireballPrefab, castPos, caster.rotation);

        // 3. 给火球赋值数据（预制体已挂载FireballProjectile，不用重复添加！）
        FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
        if (projectile != null)
        {
            projectile.damage = baseDamage * player.skillPower;
            projectile.radius = radius;
            projectile.lifetime = lifetime;
            projectile.OnExplode += HandleExplosion;
            // 火球销毁时取消事件注册（避免内存泄漏）
            Destroy(fireball, lifetime);
        }
        else
        {
            Debug.LogError("火球预制体缺少 FireballProjectile 组件！");
            Destroy(fireball);
            return;
        }

        // 4. 给火球加飞行力（沿caster面朝方向，即玩家/CastPoint朝向）
        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = caster.forward * speed;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // 防穿模
        }
    }

    // 爆炸伤害处理（事件回调）
    private void HandleExplosion(Vector3 position, float damage, float radius)
    {
        Debug.Log($"火球爆炸：位置{position}，伤害{damage}，范围{radius}");

        Collider[] hitColliders = Physics.OverlapSphere(position, radius);
        foreach (var hitCol in hitColliders)
        {
            if (hitCol.CompareTag("Enemy"))
            {
                EnemyBase enemy = hitCol.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"敌人 {hitCol.gameObject.name} 受到 {damage:F1} 点伤害");
                }
                else
                {
                    Debug.LogWarning($"物体 {hitCol.gameObject.name} 标签是Enemy，但缺少 EnemyBase 组件！");
                }
            }
        }
    }
}