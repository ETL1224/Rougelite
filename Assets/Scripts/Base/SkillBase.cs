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
    public string skillName = "Unnamed Skill";  // 技能描述
    public Sprite icon;
    public string description;
    public float baseCooldown = 5f;
    public SkillCastType castType = SkillCastType.Self;

    [Header("技能属性（用于显示/指示器）")]
    public float indicatorRadius = 2f;       // 用于指地技能
    public bool useDirectionIndicator = false; // 是否使用方向箭头指示器


    protected float lastCastTime = -999f;

    // 缓存Direction参数，避免频繁计算
    protected Vector3 cachedDirection = Vector3.forward;

    public virtual bool CanCast(PlayerState player)
    {
        float effectiveCD = baseCooldown * (1f - player.skillHaste);
        return Time.time - lastCastTime >= effectiveCD;
    }

    // 获取剩余冷却时间，用于UI显示
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
    // Self和Ground使用这个
    public virtual void TryCast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (!CanCast(player)) return;
        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    // 方向性技能使用这个
    public virtual void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (!CanCast(player)) return;
        cachedDirection = dir.normalized;   // 记录方向
        lastCastTime = Time.time;
        Cast(castPos, caster, player, cachedDirection);  // 使用缓存方向
    }

    // Self和Ground
    protected abstract void Cast(Vector3 castPos, Transform caster, PlayerState player);

    // Direction
    protected virtual void Cast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        Debug.LogWarning($"{skillName}锟斤拷锟斤拷锟剿凤拷锟斤拷施锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷未实锟斤拷Direction Cast");
        Cast(castPos, caster, player);
    }
}
