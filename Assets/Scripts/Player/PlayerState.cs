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

    [Header("嗜血狂怒专属状态（新增）")]
    public bool isBloodFrenzyActive = false; // 是否处于嗜血状态
    public float bloodFrenzyAttackSpeedMulti = 1.5f; // 攻速加成倍数
    public float bloodSuckRate = 0.1f; // 吸血比例（造成伤害的X%回血）
    private float originalAttackSpeed; // 保存原始攻速（用于技能结束后恢复）

    [Header("光之守护专属状态（新增）")]
    public bool isInvincible = false; // 是否处于无敌状态

    [Header("正常操作专属状态（新增）")]
    public bool isNormalOperationActive = false; // 是否处于提速状态
    public float normalOpMoveSpeedMulti = 2.0f; // 移速加成倍数
    private float originalMoveSpeed; // 保存原始移速（技能结束后恢复）

    [Header("灵魂交换专属状态（新增）")]
    public bool isSoulSwapActive = false; // 是否处于灵魂交换状态
    public float soulSwapAttackMulti = 2.5f; // 攻击加成倍数（2.5=+150%，大幅提升）
    private float originalAttackDamage; // 保存原始攻击力（结束后恢复）

    [Header("引用配置（新增）")]
    public UIManager uiManager; // 用于吸血时调用Heal

    void Start()
    {
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
    }

    // 开启嗜血狂怒（技能调用）
    public void StartBloodFrenzy(float attackSpeedMulti, float suckRate)
    {
        isBloodFrenzyActive = true;
        bloodFrenzyAttackSpeedMulti = attackSpeedMulti;
        bloodSuckRate = suckRate;

        // 保存原始攻速，应用加成
        originalAttackSpeed = attackSpeed;
        attackSpeed *= attackSpeedMulti;

        Debug.Log($"嗜血狂怒激活！攻速×{attackSpeedMulti}，吸血比例{suckRate * 100}%");
    }

    // 新增：结束嗜血狂怒（技能调用）
    public void EndBloodFrenzy()
    {
        isBloodFrenzyActive = false;
        // 恢复原始攻速
        attackSpeed = originalAttackSpeed;

        Debug.Log("嗜血狂怒结束！攻速恢复正常");
    }

    // 开启无敌（技能调用）
    public void StartInvincibility()
    {
        isInvincible = true;
        Debug.Log("光之守护激活！进入无敌状态");
    }

    // 关闭无敌（技能调用）
    public void EndInvincibility()
    {
        isInvincible = false;
        Debug.Log("光之守护结束！无敌状态解除");
    }

    // 开启正常操作（技能调用）
    public void StartNormalOperation(float moveSpeedMulti)
    {
        isNormalOperationActive = true;
        normalOpMoveSpeedMulti = moveSpeedMulti;

        // 保存原始移速，应用加成
        originalMoveSpeed = moveSpeed;
        moveSpeed *= moveSpeedMulti;

        Debug.Log($"正常操作激活！移速×{moveSpeedMulti}（原始：{originalMoveSpeed} → 现在：{moveSpeed}）");
    }

    // 结束正常操作（技能调用）
    public void EndNormalOperation()
    {
        isNormalOperationActive = false;
        // 恢复原始移速
        moveSpeed = originalMoveSpeed;

        Debug.Log($"正常操作结束！移速恢复至：{originalMoveSpeed}");
    }

    // 开启灵魂交换（技能调用）
    public void StartSoulSwap()
    {
        isSoulSwapActive = true;
        // 保存原始攻击力，应用加成
        originalAttackDamage = attack;
        attack *= soulSwapAttackMulti;

        Debug.Log($"灵魂交换激活！攻击力×{soulSwapAttackMulti}（原始：{originalAttackDamage} → 现在：{attack}）");
    }

    // 结束灵魂交换（技能调用）
    public void EndSoulSwap()
    {
        isSoulSwapActive = false;
        // 恢复原始攻击力
        attack = originalAttackDamage;

        Debug.Log($"灵魂交换结束！攻击力恢复至：{originalAttackDamage}");
    }

    // 消耗生命值（技能调用，确保不致死）
    public bool SpendMaxHealthPercent(float percent)
    {
        float damageToSpend = maxHealth * percent; // 消耗最大生命值的percent
        float newHealth = currentHealth - damageToSpend;

        // 最低留1血，避免直接死亡
        newHealth = Mathf.Max(newHealth, 1f);
        currentHealth = newHealth;

        Debug.Log($"灵魂交换消耗{percent * 100}%最大生命值！当前生命值：{currentHealth}");
        return true;
    }

    public void SpendOre(int amount)
    {
        if (ore >= amount) ore -= amount;
    }
}
