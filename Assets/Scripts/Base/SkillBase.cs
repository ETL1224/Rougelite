using UnityEngine;

public enum SkillCastType
{
    Self,       // 无目标技能（立即释放）
    Direction,  // 方向性技能（如火球）
    Ground,     // 指地技能（如范围AOE）
    None,
}

public abstract class SkillBase : MonoBehaviour
{
    [Header("基础属性")]
    public string skillName = "Unnamed Skill";
    public Sprite icon;
    public string description;
    public float baseCooldown = 5f;
    public SkillCastType castType = SkillCastType.Self; // 新增字段

    [Header("技能属性（用于显示/指示器）")]
    public float indicatorRadius = 2f;       // 用于指地技能
    public bool useDirectionIndicator = false; // 是否使用方向箭头指示器

    protected float lastCastTime = -999f;

    public virtual bool CanCast(PlayerState player)
    {
        float effectiveCD = baseCooldown * (1f - player.skillHaste);
        return Time.time - lastCastTime >= effectiveCD;
    }

    public void TryCast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (!CanCast(player)) return;
        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    // 新增带方向的重载
    public virtual void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        TryCast(castPos, caster, player);
    }

    protected abstract void Cast(Vector3 castPos, Transform caster, PlayerState player);
}
