using UnityEngine;

public enum SkillCastType
{
    Self,       // 自身目标技能（直接释放）
    Direction,  // 方向指向技能（朝向释放）
    Ground,     // 地面指向技能（指定范围AOE）
    None,       // 无释放类型
}

public abstract class SkillBase : MonoBehaviour
{
    [Header("技能基础配置")]
    public string skillName = "Unnamed Skill";  // 技能名称
    public Sprite icon;                         // 技能图标
    public string description;                  // 技能描述
    public float baseCooldown = 5f;             // 技能基础冷却时间
    public SkillCastType castType = SkillCastType.Self; // 技能释放类型

    [Header("技能指示器配置（用于显示/指示范围）")]
    public float indicatorRadius = 2f;          // 技能指示器半径
    public bool useDirectionIndicator = false;  // 是否使用方向指示器

    protected float lastCastTime = -999f;

    protected Vector3 cachedDirection = Vector3.forward;

    public virtual bool CanCast(PlayerState player)
    {
        // 计算实际冷却时间（受玩家技能急速影响）
        float effectiveCD = baseCooldown * (1f - player.skillHaste);
        return Time.time - lastCastTime >= effectiveCD;
    }

    public float GetRemainCD(PlayerState player)
    {
        float effectiveCD = baseCooldown * (1f - player.skillHaste);
        float remain = effectiveCD - (Time.time - lastCastTime);
        return Mathf.Max(0f, remain);
    }

    public virtual float GetTotalCD(PlayerState playerState)
    {
        return baseCooldown * (1 - playerState.skillHaste);
    }

    public virtual void TryCast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (!CanCast(player)) return;
        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    public virtual void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (!CanCast(player)) return;
        cachedDirection = dir.normalized;   // 记录释放方向
        lastCastTime = Time.time;
        Cast(castPos, caster, player, cachedDirection);  // 调用带方向的释放方法
    }

    protected abstract void Cast(Vector3 castPos, Transform caster, PlayerState player);

    protected virtual void Cast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        Debug.LogWarning($"{skillName}技能的方向释放逻辑未实现，默认调用无方向释放方法");
        Cast(castPos, caster, player);
    }

    public virtual void OnRemoved() { }
}