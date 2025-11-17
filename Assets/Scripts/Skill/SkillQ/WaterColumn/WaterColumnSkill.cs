using UnityEngine;

public class WaterColumnSkill : SkillBase
{
    [Header("水柱技能配置")]
    public GameObject waterColumnPrefab;   // 水柱预制体
    public float radius = 10f;              // 水柱作用半径
    public float damage = 15f;             // 造成的伤害
    public float showDuration = 1f;        // 水柱存在时长

    private void OnEnable()
    {
        castType = SkillCastType.Ground;   // 使用地面指示器
        useDirectionIndicator = false;     // 不需要方向箭头
        indicatorRadius = radius;          // 和圆圈一致
    }

    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (!CanCast(player)) return;
        lastCastTime = Time.time;

        Cast(castPos, caster, player);
    }

    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (waterColumnPrefab == null)
        {
            Debug.LogError("waterColumnPrefab未赋值！");
            return;
        }

        // 创建水柱实体
        GameObject obj = Instantiate(
            waterColumnPrefab,
            castPos,
            Quaternion.identity
        );

        // 传递技能参数
        WaterColumnSpawner wc = obj.GetComponent<WaterColumnSpawner>();
        wc.radius = radius;
        wc.damage = damage * player.skillPower;
        wc.duration = showDuration;

        Debug.Log($"释放水柱技能：位置 {castPos}");
    }
}
