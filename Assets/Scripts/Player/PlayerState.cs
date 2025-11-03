using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public int ore = 10;  // 玩家矿石数量
    public float attack = 10f;
    public float attackSpeed = 1f;
    public float moveSpeed = 3f;
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    // 技能槽（最多3个）
    public SkillBase skillQ;
    public SkillBase skillE;
    public SkillBase skillR;

    public void SpendOre(int amount)
    {
        if (ore >= amount) ore -= amount;
    }
}
