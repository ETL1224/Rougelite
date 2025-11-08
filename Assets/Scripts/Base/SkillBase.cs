using UnityEngine;

public enum SkillCastType
{
    Self,       // 无目标技能（立即释放）
    Direction,  // 方向性技能（如火球）
    Ground,     // 指地技能（如范围AOE）
}

public abstract class SkillBase : MonoBehaviour
{
    [Header("基础属性")]
    public string skillName = "Unnamed Skill";
    public Sprite icon;
    public string description;
    public float baseCooldown = 5f;
    public SkillCastType castType = SkillCastType.Self; // 新增字段

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
