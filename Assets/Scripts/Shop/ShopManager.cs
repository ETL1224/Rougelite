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
                playerStats.attack += 2f;
                attackLv++;
                break;
            case "attackSpeed":
                playerStats.attackSpeed += 10f;
                attackSpeedLv++;
                break;
            case "moveSpeed":
                playerStats.moveSpeed += 0.2f;
                moveSpeedLv++;
                break;
            case "health":
                playerStats.maxHealth += 10f;
                playerStats.currentHealth += 10f;
                healthLv++;
                break;
            default:
                Debug.LogWarning("未知升级键：" + statKey);
                return false;
        }

        playerStats.SpendOre(upgradeCost);

        // 刷新UI
        uiManager?.UpdateOreUI();
        shopUI?.UpdateUpgradeTexts(attackLv, attackSpeedLv, moveSpeedLv, healthLv); // 更新UI
        return true;
    }

    // 给 UI 调用：为槽位分配一个随机技能（Q/E/R）
    public bool BuyAndAssignRandomSkill(string slotKey)
    {
        if (playerStats == null || skillManager == null) return false;
        if (playerStats.ore < skillCost) return false;

        SkillBase skill = skillManager.GetRandomSkill(); // 从 SkillManager 获取
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

        if (uiManager != null)
            uiManager.UpdateOreUI();

        return true;
    }

    public int GetOreCount() => playerStats != null ? playerStats.ore : 0;
}
