using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public int ore = 0;
    public float attack = 1f;
    public float attackSpeed = 2f;
    public float moveSpeed = 15f;
    public float maxHealth = 100f;
    public float currentHealth = 50f;

    // ==== 新增：等级追踪 ====
    public int attackLevel = 0;
    public int attackSpeedLevel = 0;
    public int moveSpeedLevel = 0;
    public int healthLevel = 0;

    // 技能槽
    public SkillBase skillQ;
    public SkillBase skillE;
    public SkillBase skillR;

    public void SpendOre(int amount)
    {
        if (ore >= amount) ore -= amount;
    }
}
