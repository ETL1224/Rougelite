using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{
    public string skillName = "Unnamed Skill";
    public string description = "Skill description";
    public Sprite icon;
    public float baseCooldown = 5f;
    public float lastCastTime = -999f; // 确保技能初始状态冷却完毕

    // 判断是否可释放
    public virtual bool CanCast(PlayerState player)
    {
        float effectiveCD = baseCooldown * (1f - player.skillHaste);
        return Time.time - lastCastTime >= effectiveCD;
    }

    // 尝试释放技能
    public void TryCast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (!CanCast(player)) return;
        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    // 技能具体逻辑
    protected abstract void Cast(Vector3 castPos, Transform caster, PlayerState player);
}
