using UnityEngine;

public class ElectricOrbSkill : SkillBase
{
    [Header("电球配置")]
    public GameObject electricOrbPrefab; // 电球投射体预制体
    public float speed = 16f; // 中速
    public float baseDamage = 12f; // 基础伤害（略高于火球）
    public float radius = 3f; // 爆炸范围（比火球小，精准AOE）
    public float lifetime = 3.5f; // 存活时间
    public float knockbackForce = 5f; // 击退力度（专属特性）

    private void OnEnable()
    {
        castType = SkillCastType.Direction; // 方向性技能
        useDirectionIndicator = true; // 启用方向箭头指示器
        indicatorRadius = 2.5f; // 箭头长度（适配中速范围）
    }

    // 重写带方向的施法方法（核心）
    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (!CanCast(player)) return;
        lastCastTime = Time.time;
        Cast(castPos, caster, player, dir);
    }

    // 空实现无方向版本
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player) { }

    // 带方向的施法逻辑（实例化投射体）
    protected virtual void Cast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (electricOrbPrefab == null)
        {
            Debug.LogError("电球技能：未赋值 electricOrbPrefab！");
            return;
        }

        // 实例化电球（位置=施法点，朝向=施法方向）
        GameObject orb = Instantiate(electricOrbPrefab, castPos, Quaternion.LookRotation(dir));
        ElectricOrbProjectile proj = orb.GetComponent<ElectricOrbProjectile>();

        if (proj != null)
        {
            // 传递技能参数
            proj.damage = baseDamage * player.skillPower;
            proj.radius = radius;
            proj.lifetime = lifetime;
            proj.speed = speed;
            proj.knockbackForce = knockbackForce;

            // 订阅爆炸事件（电球命中后触发范围伤害）
            proj.OnExplode += HandleExplosion;
        }

        Debug.Log($"释放电球：方向 {dir}，范围 {radius}，伤害 {baseDamage * player.skillPower}");
    }

    // 处理电球爆炸（范围伤害+击退）
    private void HandleExplosion(Vector3 position, float damage, float radius, float knockbackForce)
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
                    // 计算击退方向（从爆炸中心指向敌人）
                    Vector3 knockbackDir = (enemy.transform.position - position).normalized;
                    enemy.GetComponent<Rigidbody>()?.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                    Debug.Log($"电球命中 {enemy.gameObject.name}，造成 {damage:F1} 点伤害并击退");
                }
            }
        }
    }
}