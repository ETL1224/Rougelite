using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [Header("Q/E/R¶ÀÁ¢¼¼ÄÜ¿â")]
    public List<SkillBase> skillPoolQ;
    public List<SkillBase> skillPoolE;
    public List<SkillBase> skillPoolR;

    public SkillBase GetRandomSkill(string slotKey)
    {
        List<SkillBase> pool = null;

        switch (slotKey)
        {
            case "Q": pool = skillPoolQ; break;
            case "E": pool = skillPoolE; break;
            case "R": pool = skillPoolR; break;
        }

        if (pool == null || pool.Count == 0) return null;

        int i = Random.Range(0, pool.Count);
        return pool[i];
    }
}
