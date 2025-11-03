using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    // 在 Inspector 拖入 SkillBase 的 prefab 或 ScriptableObject（取决于你的实现）
    public List<SkillBase> skillPool;

    public SkillBase GetRandomSkill()
    {
        if (skillPool == null || skillPool.Count == 0) return null;
        int i = Random.Range(0, skillPool.Count);
        return skillPool[i];
    }
}
