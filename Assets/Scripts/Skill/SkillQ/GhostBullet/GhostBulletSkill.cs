using UnityEngine;

public class GhostBulletSkill : SkillBase
{
    [Header("幽灵子弹配置（直线穿透版）")]
    public GameObject ghostBulletPrefab; // 幽灵子弹投射体预制体
    public float speed = 35f; // 高速飞行（比之前略快，方便穿透多体）
    public float baseDamage = 4f; // 低基础伤害（重点是穿透多体）
    public float lifetime = 5f; // 延长存活时间（确保覆盖更长直线）
    public float hitInterval = 0.1f; // 防止同一帧多次命中同一个敌人（避免重复伤害）

    private void OnEnable()
    {
        castType = SkillCastType.Direction; // 方向性技能
        useDirectionIndicator = true; // 启用方向箭头
        indicatorRadius = 2.5f; // 箭头长度适配长距离
    }

    // 重写带方向的施法方法
    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (!CanCast(player)) return;
        lastCastTime = Time.time;
        Cast(castPos, caster, player, dir);
    }

    // 空实现无方向版本
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player) { }

    // 带方向的施法逻辑
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (ghostBulletPrefab == null)
        {
            Debug.LogError("幽灵子弹技能：未赋值 ghostBulletPrefab！");
            return;
        }

        GameObject bullet = Instantiate(ghostBulletPrefab, castPos, Quaternion.LookRotation(dir));
        GhostBulletProjectile proj = bullet.GetComponent<GhostBulletProjectile>();

        if (proj != null)
        {
            proj.damage = baseDamage * player.skillPower; // 低伤害×玩家倍率
            proj.lifetime = lifetime;
            proj.speed = speed;
            proj.hitInterval = hitInterval; // 传递防重复伤害间隔

            // 订阅命中事件（每个敌人命中都触发）
            proj.OnHitEnemy += HandleHitEnemy;
        }

        Debug.Log($"释放直线穿透幽灵子弹：方向 {dir}，单hit伤害 {baseDamage * player.skillPower}");
    }

    // 处理单个敌人命中（穿透时会多次调用，每个敌人触发一次）
    private void HandleHitEnemy(EnemyBase enemy, float damage)
    {
        enemy.TakeDamage(damage);
        Debug.Log($"幽灵子弹命中 {enemy.gameObject.name}，造成 {damage:F1} 点穿透伤害");
    }
}