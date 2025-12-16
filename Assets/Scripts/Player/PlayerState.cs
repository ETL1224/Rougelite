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
    public float skillHaste = 0f; // 技能急速，百分比形式，例如0.2 = 冷却减少20%

    [Header("嗜血狂怒特殊状态配置")]
    public bool isBloodFrenzyActive = false; // 是否处于嗜血状态
    public float bloodFrenzyAttackSpeedMulti = 1.5f; // 攻击速度倍率
    public float bloodSuckRate = 0.1f; // 吸血比例，造成伤害后恢复X%的血量
    private float originalAttackSpeed; // 记录原始攻速，用于技能结束后还原
    private object bloodFrenzyOwner;

    [Header("光之守护特殊状态配置")]
    public bool isInvincible = false; // 是否处于无敌状态
    private object invincibilityOwner;

    [Header("暴走状态特殊配置")]
    public bool isNormalOperationActive = false; // 是否处于暴走状态
    public float normalOpMoveSpeedMulti = 2.0f; // 移动速度倍率
    private float originalMoveSpeed; // 记录原始移速，技能结束后还原
    private object normalOperationOwner;

    [Header("灵魂互换特殊状态配置")]
    public bool isSoulSwapActive = false; // 是否处于灵魂互换状态
    public float soulSwapAttackMulti = 2.5f; // 攻击力倍率，2.5=+150%基础攻击力
    private float originalAttackDamage; // 记录原始攻击力，技能结束后还原

    [Header("UI引用，回血时更新")]
    public UIManager uiManager; // 角色回血时调用Heal

    void Start()
    {
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
    }

    // 开启嗜血狂怒技能效果
    public void StartBloodFrenzy(float attackSpeedMulti, float suckRate)
    {
        // 兼容旧调用：无owner时直接覆盖
        StartBloodFrenzy(attackSpeedMulti, suckRate, null);
    }

    public void StartBloodFrenzy(float attackSpeedMulti, float suckRate, object owner)
    {
        if (isBloodFrenzyActive && bloodFrenzyOwner != null && owner != null && !ReferenceEquals(bloodFrenzyOwner, owner))
        {
            EndBloodFrenzy();
        }

        isBloodFrenzyActive = true;
        bloodFrenzyAttackSpeedMulti = attackSpeedMulti;
        bloodSuckRate = suckRate;
        bloodFrenzyOwner = owner;

        // 记录原始攻速，应用倍率
        originalAttackSpeed = attackSpeed;
        attackSpeed *= attackSpeedMulti;

        Debug.Log($"嗜血狂怒开启！攻速：{attackSpeedMulti}，吸血比例{suckRate * 100}%");
    }

    // 结束嗜血狂怒技能效果
    public void EndBloodFrenzy()
    {
        // 兼容旧调用：无owner时强制结束
        EndBloodFrenzy(null);
    }

    public void EndBloodFrenzy(object owner)
    {
        if (owner != null && bloodFrenzyOwner != null && !ReferenceEquals(bloodFrenzyOwner, owner))
            return;

        isBloodFrenzyActive = false;
        // 还原原始攻速
        attackSpeed = originalAttackSpeed;
        bloodFrenzyOwner = null;

        Debug.Log("嗜血狂怒结束，攻速已还原");
    }

    // 开启无敌，技能效果
    public void StartInvincibility()
    {
        StartInvincibility(null);
    }

    public void StartInvincibility(object owner)
    {
        isInvincible = true;
        invincibilityOwner = owner;
        Debug.Log("光之守护开启，进入无敌状态");
    }

    // 关闭无敌，技能效果
    public void EndInvincibility()
    {
        EndInvincibility(null);
    }

    public void EndInvincibility(object owner)
    {
        if (owner != null && invincibilityOwner != null && !ReferenceEquals(invincibilityOwner, owner))
            return;

        isInvincible = false;
        invincibilityOwner = null;
        Debug.Log("光之守护结束，无敌状态解除");
    }

    // 开启暴走状态技能效果
    public void StartNormalOperation(float moveSpeedMulti)
    {
        StartNormalOperation(moveSpeedMulti, null);
    }

    public void StartNormalOperation(float moveSpeedMulti, object owner)
    {
        if (isNormalOperationActive && normalOperationOwner != null && owner != null && !ReferenceEquals(normalOperationOwner, owner))
        {
            EndNormalOperation();
        }

        isNormalOperationActive = true;
        normalOpMoveSpeedMulti = moveSpeedMulti;
        normalOperationOwner = owner;

        // 记录原始移速，应用倍率
        originalMoveSpeed = moveSpeed;
        moveSpeed *= moveSpeedMulti;

        Debug.Log($"暴走状态开启！移速：{moveSpeedMulti}，原始{originalMoveSpeed}，当前{moveSpeed}");
    }

    // 结束暴走状态技能效果
    public void EndNormalOperation()
    {
        EndNormalOperation(null);
    }

    public void EndNormalOperation(object owner)
    {
        if (owner != null && normalOperationOwner != null && !ReferenceEquals(normalOperationOwner, owner))
            return;

        isNormalOperationActive = false;
        // 还原原始移速
        moveSpeed = originalMoveSpeed;

        normalOperationOwner = null;

        Debug.Log($"暴走状态结束，移速还原为{originalMoveSpeed}");
    }

    // 开启灵魂互换技能效果
    public void StartSoulSwap()
    {
        isSoulSwapActive = true;
        // 记录原始攻击力，应用倍率
        originalAttackDamage = attack;
        attack *= soulSwapAttackMulti;

        Debug.Log($"灵魂互换开启！攻击{soulSwapAttackMulti}，原始{originalAttackDamage}，当前{attack}");
    }

    // 结束灵魂互换技能效果
    public void EndSoulSwap()
    {
        isSoulSwapActive = false;
        // 还原原始攻击力
        attack = originalAttackDamage;

        Debug.Log($"灵魂互换结束，攻击力还原为{originalAttackDamage}");
    }

    // 消耗生命值百分比，确保角色存活
    public bool SpendMaxHealthPercent(float percent)
    {
        float damageToSpend = maxHealth * percent; // 计算消耗的生命值百分比
        float newHealth = currentHealth - damageToSpend;

        // 保留1点血，避免直接死亡
        newHealth = Mathf.Max(newHealth, 1f);
        currentHealth = newHealth;

        Debug.Log($"灵魂互换消耗{percent * 100}%最大生命值，当前生命值{currentHealth}");
        return true;
    }

    public void SpendOre(int amount)
    {
        if (ore >= amount) ore -= amount;
    }
}