using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{
    public string skillName;
    public float cooldown = 3f;
    private float lastCastTime;

    public virtual bool CanCast() => Time.time - lastCastTime >= cooldown;

    public void TryCast(Vector3 castPos, Transform caster)
    {
        if (!CanCast()) return;

        lastCastTime = Time.time;
        Cast(castPos, caster);
    }

    protected abstract void Cast(Vector3 castPos, Transform caster);
}
