using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public int ore = 0;

    [Header("基础属性")]
    public float attack = 1f;
    public float attackSpeed = 2f;
    public float moveSpeed = 15f;
    public float maxHealth = 50f;
    public float currentHealth = 50f;
    public float skillPower = 1f; 
    public float skillHaste = 0f; // 技能急速（百分比形式，例如0.2 = 冷却缩短20%）

    [Header("等级追踪")]
    public int attackLevel = 0;
    public int attackSpeedLevel = 0;
    public int moveSpeedLevel = 0;
    public int healthLevel = 0;
    public int skillPowerLevel = 0;
    public int skillHasteLevel = 0;

    [Header("技能槽")]
    public SkillBase skillQ;
    public SkillBase skillE;
    public SkillBase skillR;

    public void SpendOre(int amount)
    {
        if (ore >= amount) ore -= amount;
    }
}
