using UnityEngine;

/// <summary>
/// 负责商店的业务逻辑：升级属性、购买/分配技能等（不直接处理 UI 显示）。
/// 依赖 PlayerStats 和 SkillManager（你项目里已有/稍后添加）。
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("引用（在 Inspector 里指派）")]
    public PlayerState playerStats;      // 拖 Player（含 PlayerStats）进来
    public UIManager uiManager;
    public SkillManager skillManager;    // 拖 SkillManager（空物体）进来
    public ShopUIManager shopUI;

    [Header("费用设置")]
    public int upgradeCost = 5;
    public int skillCost = 20;

    private int attackLv = 0;
    private int attackSpeedLv = 0;
    private int moveSpeedLv = 0;
    private int healthLv = 0;
    private int skillPowerLv = 0;
    private int skillHasteLv = 0;

    void Start()
    {
        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerState>();

        if (skillManager == null)
            skillManager = FindObjectOfType<SkillManager>();

        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        if (shopUI == null)
            shopUI = FindObjectOfType<ShopUIManager>();
    }

    // 升级属性
    public bool Upgrade(string statKey)
    {
        if (playerStats == null) return false;
        if (playerStats.ore < upgradeCost) return false;

        switch (statKey)
        {
            case "attack":
                playerStats.attack += 0.5f;
                attackLv++;
                break;
            case "attackSpeed":
                playerStats.attackSpeed += 0.5f;
                attackSpeedLv++;
                break;
            case "moveSpeed":
                playerStats.moveSpeed += 1f;
                moveSpeedLv++;
                break;
            case "health":
                playerStats.maxHealth += 5f;
                playerStats.currentHealth += 5f;
                healthLv++;
                break;
            case "skillPower":
                playerStats.skillPower += 0.5f;  // 法术伤害乘以法术强度
                playerStats.skillPowerLevel++;
                break;
            case "skillHaste":
                playerStats.skillHaste += 0.05f; // 每级减少5%
                playerStats.skillHaste = Mathf.Min(playerStats.skillHaste, 0.5f); // 最多50%
                playerStats.skillHasteLevel++;
                break;
            default:
                Debug.LogWarning("未知升级键：" + statKey);
                return false;
        }

        playerStats.SpendOre(upgradeCost);

        // 刷新UI
        uiManager?.UpdateOreUI();
        shopUI?.UpdateUpgradeTexts(attackLv, attackSpeedLv, moveSpeedLv, healthLv, skillPowerLv, skillHasteLv); // 更新UI
        return true;
    }

    public bool BuyAndAssignRandomSkill(string slotKey)
    {
        if (playerStats == null || skillManager == null) return false;
        if (playerStats.ore < skillCost) return false;

        SkillBase skill = skillManager.GetRandomSkill(slotKey); // 根据槽位获取技能
        if (skill == null) return false;

        // 根据槽位赋值
        switch (slotKey)
        {
            case "Q": playerStats.skillQ = skill; break;
            case "E": playerStats.skillE = skill; break;
            case "R": playerStats.skillR = skill; break;
            default:
                Debug.LogWarning("未知技能槽：" + slotKey);
                return false;
        }

        playerStats.SpendOre(skillCost);

        uiManager?.UpdateOreUI();
        return true;
    }

    public int GetOreCount() => playerStats != null ? playerStats.ore : 0;
}
