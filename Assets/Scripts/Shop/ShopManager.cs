using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("引用（在Inspector里指派）")]
    public PlayerState playerStats;      // 拖Player（含PlayerState）进来
    public UIManager uiManager;
    public SkillManager skillManager;    // 拖SkillManager（空物体）进来
    public ShopUIManager shopUI;
    public PlayerSkillController playerSkillCtrl; // 拖 Player身上的PlayerSkillController
    public PauseManager PauseManager;
    [Header("费用设置")]
    public int upgradeCost = 5;
    public int skillCost = 20;

    // 已购技能存储：key=槽位（Q/E/R），value=该槽位已购买的技能预制体（避免重复购买）
    private Dictionary<string, HashSet<SkillBase>> purchasedSkills = new Dictionary<string, HashSet<SkillBase>>();

    private int attackLv = 0;
    private int attackSpeedLv = 0;
    private int moveSpeedLv = 0;
    private int healthLv = 0;
    private int skillPowerLv = 0;
    private int skillHasteLv = 0;

    void Start()
    {
        // 自动寻找缺失的引用
        if (playerStats == null) playerStats = FindObjectOfType<PlayerState>();
        if (skillManager == null) skillManager = FindObjectOfType<SkillManager>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (shopUI == null) shopUI = FindObjectOfType<ShopUIManager>();
        if (playerSkillCtrl == null) playerSkillCtrl = FindObjectOfType<PlayerSkillController>();
        if (PauseManager == null) PauseManager = FindObjectOfType<PauseManager>();
        // 初始化已购技能集合（给每个槽位创建空集合）
        purchasedSkills.Add("Q", new HashSet<SkillBase>());
        purchasedSkills.Add("E", new HashSet<SkillBase>());
        purchasedSkills.Add("R", new HashSet<SkillBase>());
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
                skillPowerLv++;
                break;
            case "skillHaste":
                playerStats.skillHaste += 0.05f; // 每级减少5%
                playerStats.skillHaste = Mathf.Min(playerStats.skillHaste, 0.5f); // 最多50%
                skillHasteLv++;
                break;
            default:
                Debug.LogWarning("未知升级键：" + statKey);
                return false;
        }

        // 扣除矿石
        playerStats.SpendOre(upgradeCost);

        // 刷新UI
        uiManager?.UpdateOreUI();
        shopUI?.UpdateUpgradeTexts(attackLv, attackSpeedLv, moveSpeedLv, healthLv, skillPowerLv, skillHasteLv);

        // 升级成功，返回true
        return true;
    }

    // 关键修改：购买技能（添加已购去重+实例化+绑定）
    public bool BuySkill(string slotKey, SkillBase skillPrefab)
    {
        // 1. 基础校验
        if (playerStats == null || skillPrefab == null || playerSkillCtrl == null) return false;
        if (playerStats.ore < skillCost) return false;
        if (!purchasedSkills.ContainsKey(slotKey)) return false;

        // 2. 检查是否已购买该技能（同一槽位不重复）
        if (purchasedSkills[slotKey].Contains(skillPrefab))
        {
            Debug.LogWarning($"槽位{slotKey}已购买过技能：{skillPrefab.skillName}");
            return false;
        }

        // 3. 生成技能实例（MonoBehaviour必须实例化）
        GameObject skillObj = Instantiate(skillPrefab.gameObject);
        SkillBase newSkill = skillObj.GetComponent<SkillBase>();
        if (newSkill == null)
        {
            Debug.LogError("技能预制体缺少SkillBase子类组件（如FireballSkill）");
            Destroy(skillObj);
            return false;
        }

        // 新增：自动给Self类型技能绑定Player引用（核心）
        if (newSkill.castType == SkillCastType.Self)
        {
            // 3.1 找到Player（通过playerSkillCtrl，更稳妥）
            Transform playerTransform = playerSkillCtrl.transform;
            Transform playerEffectPoint = null;

            // 3.2 根据技能类型，获取对应的特效点名称（适配不同技能）
            string targetPointName = "";
            if (newSkill is RedemptionSkill redemptionSkill)
            {
                targetPointName = redemptionSkill.targetEffectPointName; // 救赎技能的特效点名称
            }
            else if (newSkill is BloodFrenzySkill bloodFrenzySkill)
            {
                targetPointName = bloodFrenzySkill.targetEffectPointName; // 嗜血狂怒的特效点名称
            }
            else if (newSkill is LightGuardSkill lightGuardSkill)
            {
                targetPointName = lightGuardSkill.targetEffectPointName;
            }
            else if (newSkill is NormalOperationSkill normalOpSkill) // 正常操作特效点名称
            {
                targetPointName = normalOpSkill.targetEffectPointName;
            }
            else if (newSkill is GravityPullSkill gravitySkill)
            {
                targetPointName = gravitySkill.targetEffectPointName;
            }
            else if (newSkill is PoisonCloudSkill poisonSkill) // 剧毒踪迹
            {
                targetPointName = poisonSkill.targetEffectPointName;
            }
            else if (newSkill is SoulSwapSkill soulSkill)
            {
                targetPointName = soulSkill.targetEffectPointName;
            }
            else if (newSkill is SlashSkill slashSkill)
            {
                targetPointName = slashSkill.targetEffectPointName;
            }
            if (newSkill is CircleDanceSkill circleSkill)
            {
                targetPointName = circleSkill.targetEffectPointName;
            }
            // 以后加新Self技能，这里加else if即可

            // 3.3 根据名称找特效点，找不到就用Player位置兜底
            if (!string.IsNullOrEmpty(targetPointName))
            {
                playerEffectPoint = playerTransform.Find(targetPointName);
                if (playerEffectPoint != null)
                {
                    Debug.Log($"ShopManager：绑定特效点 {targetPointName} 到 {newSkill.skillName}");
                }
            }
            if (playerEffectPoint == null)
            {
                playerEffectPoint = playerTransform;
                Debug.LogWarning($"ShopManager：未找到 {targetPointName}，用Player位置兜底");
            }

            // 3.4 给不同技能绑定对应引用
            if (newSkill is RedemptionSkill redSkill)
            {
                redSkill.effectPoint = playerEffectPoint;
                redSkill.uiManager = FindObjectOfType<UIManager>();
            }
            else if (newSkill is BloodFrenzySkill bfSkill)
            {
                bfSkill.effectPoint = playerEffectPoint;
                bfSkill.playerState = FindObjectOfType<PlayerState>();
                bfSkill.uiManager = FindObjectOfType<UIManager>();
            }
            else if (newSkill is LightGuardSkill lgSkill)
            {
                lgSkill.effectPoint = playerEffectPoint;
                lgSkill.playerState = FindObjectOfType<PlayerState>();
            }
            else if (newSkill is NormalOperationSkill noSkill)
            {
                noSkill.effectPoint = playerEffectPoint;
                noSkill.playerState = FindObjectOfType<PlayerState>();
            }
            else if (newSkill is GravityPullSkill gSkill)
            {
                gSkill.effectPoint = playerEffectPoint;
                gSkill.playerState = FindObjectOfType<PlayerState>();
                gSkill.uiManager = FindObjectOfType<UIManager>();
            }
            else if (newSkill is PoisonCloudSkill pSkill)
            {
                pSkill.effectPoint = playerEffectPoint;
                pSkill.playerState = FindObjectOfType<PlayerState>();
            }
            else if (newSkill is SoulSwapSkill sSkill)
            {
                sSkill.effectPoint = playerEffectPoint;
                sSkill.playerState = FindObjectOfType<PlayerState>();
                sSkill.uiManager = FindObjectOfType<UIManager>();
            }
            else if (newSkill is SlashSkill slashSkill)
            {
                slashSkill.playerState = FindObjectOfType<PlayerState>();
                slashSkill.playerTransform = FindObjectOfType<PlayerState>().transform;
            }
            else if (newSkill is CircleDanceSkill cdSkill)
            {
                cdSkill.playerState = FindObjectOfType<PlayerState>();
                cdSkill.playerTransform = FindObjectOfType<PlayerState>().transform;
            }
            // 新技能这里加else if

            // 3.5 技能实例设为Player子对象（不变，避免场景混乱）
            skillObj.transform.SetParent(playerTransform);
            skillObj.transform.localPosition = Vector3.zero;
            skillObj.name = newSkill.skillName;
        }

        // 4. 绑定技能到玩家技能控制器（关键：让Q/E/R能触发）
        playerSkillCtrl.AssignSkill(slotKey, newSkill);

        // 5. 记录已购买技能（避免重复出现）
        purchasedSkills[slotKey].Add(skillPrefab);

        // 6. 扣矿石+刷新UI
        playerStats.SpendOre(skillCost);
        uiManager?.UpdateOreUI();

        return true;
    }

    // 新增：获取某槽位的「可用技能池」（总池 - 已购池）
    public List<SkillBase> GetAvailableSkills(string slotKey)
    {
        if (skillManager == null) return new List<SkillBase>();

        // 1. 获取该槽位的总技能池
        List<SkillBase> totalPool = slotKey switch
        {
            "Q" => skillManager.skillPoolQ,
            "E" => skillManager.skillPoolE,
            "R" => skillManager.skillPoolR,
            _ => new List<SkillBase>()
        };

        // 2. 过滤掉已购买的技能，得到可用池
        List<SkillBase> available = new List<SkillBase>();
        foreach (var skill in totalPool)
        {
            if (skill != null && !purchasedSkills[slotKey].Contains(skill))
            {
                available.Add(skill);
            }
        }

        return available;
    }

    public int GetOreCount() => playerStats != null ? playerStats.ore : 0;

    // 返回指定槽位已购买的技能列表（返回副本，避免外部修改内部集合）
    public List<SkillBase> GetPurchasedSkills(string slotKey)
    {
        List<SkillBase> result = new List<SkillBase>();
        if (purchasedSkills == null || !purchasedSkills.ContainsKey(slotKey)) return result;
        foreach (var s in purchasedSkills[slotKey])
            result.Add(s);
        return result;
    }

    // 返回所有槽位已购买的技能（用于背包显示）
    public List<SkillBase> GetAllPurchasedSkills()
    {
        List<SkillBase> all = new List<SkillBase>();
        if (purchasedSkills == null) return all;
        foreach (var kv in purchasedSkills)
        {
            foreach (var s in kv.Value)
            {
                if (s != null && !all.Contains(s))
                    all.Add(s);
            }
        }
        return all;
    }
}