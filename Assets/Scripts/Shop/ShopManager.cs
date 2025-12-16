using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("引用，在Inspector中指定")]
    public PlayerState playerStats;      // 玩家身上的PlayerState组件
    public UIManager uiManager;
    public SkillManager skillManager;    // 技能管理器，用于获取技能池
    public ShopUIManager shopUI;
    public PlayerSkillController playerSkillCtrl; // 玩家身上的PlayerSkillController
    public PauseManager PauseManager;
    [Header("价格配置")]
    public int upgradeCost = 5;
    public int skillCost = 20;

    // 已购买技能存储，key=槽位（Q/E/R），value=该槽位已购买的技能预制体（避免重复购买）
    private Dictionary<string, HashSet<SkillBase>> purchasedSkills = new Dictionary<string, HashSet<SkillBase>>();

    private int attackLv = 0;
    private int attackSpeedLv = 0;
    private int moveSpeedLv = 0;
    private int healthLv = 0;
    private int skillPowerLv = 0;
    private int skillHasteLv = 0;

    void Start()
    {
        // 自动查找缺失的组件
        if (playerStats == null) playerStats = FindObjectOfType<PlayerState>();
        if (skillManager == null) skillManager = FindObjectOfType<SkillManager>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (shopUI == null) shopUI = FindObjectOfType<ShopUIManager>();
        if (playerSkillCtrl == null) playerSkillCtrl = FindObjectOfType<PlayerSkillController>();
        if (PauseManager == null) PauseManager = FindObjectOfType<PauseManager>();
        // 初始化已购买技能集合，每个槽位对应一个集合
        purchasedSkills.Add("Q", new HashSet<SkillBase>());
        purchasedSkills.Add("E", new HashSet<SkillBase>());
        purchasedSkills.Add("R", new HashSet<SkillBase>());
    }

    // 属性升级
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
                playerStats.skillPower += 0.5f;  // 提升技能威力系数
                skillPowerLv++;
                break;
            case "skillHaste":
                playerStats.skillHaste += 0.05f; // 每级提升5%
                playerStats.skillHaste = Mathf.Min(playerStats.skillHaste, 0.5f); // 上限50%
                skillHasteLv++;
                break;
            default:
                Debug.LogWarning("未知的属性键" + statKey);
                return false;
        }

        // 消耗矿石
        playerStats.SpendOre(upgradeCost);

        // 更新UI
        uiManager?.UpdateOreUI();
        shopUI?.UpdateUpgradeTexts(attackLv, attackSpeedLv, moveSpeedLv, healthLv, skillPowerLv, skillHasteLv);

        // 升级成功返回true
        return true;
    }

    // 核心修改：购买技能，包含实例化+挂载+装备
    public bool BuySkill(string slotKey, SkillBase skillPrefab)
    {
        // 1. 前置校验
        if (playerStats == null || skillPrefab == null || playerSkillCtrl == null) return false;
        if (playerStats.ore < skillCost) return false;
        if (!purchasedSkills.ContainsKey(slotKey)) return false;

        // 2. 检查是否已购买该技能，同一槽位避免重复
        if (purchasedSkills[slotKey].Contains(skillPrefab))
        {
            Debug.LogWarning($"槽位{slotKey}已购买该技能：{skillPrefab.skillName}");
            return false;
        }

        // 3. 执行实例化MonoBehaviour技能实例
        GameObject skillObj = Instantiate(skillPrefab.gameObject);
        SkillBase newSkill = skillObj.GetComponent<SkillBase>();
        if (newSkill == null)
        {
            Debug.LogError("技能预制体缺少SkillBase组件，例如FireballSkill等");
            Destroy(skillObj);
            return false;
        }

        // 如果是自动释放Self类型技能，挂载到玩家身上
        if (newSkill.castType == SkillCastType.Self)
        {
            // 3.1 找到Player的Transform，通过playerSkillCtrl获取
            Transform playerTransform = playerSkillCtrl.transform;
            Transform playerEffectPoint = null;

            // 3.2 根据技能类型，获取对应特效挂点（不同技能挂点可能不同）
            string targetPointName = "";
            if (newSkill is RedemptionSkill redemptionSkill)
            {
                targetPointName = redemptionSkill.targetEffectPointName; // 救赎技能的特效挂点
            }
            else if (newSkill is BloodFrenzySkill bloodFrenzySkill)
            {
                targetPointName = bloodFrenzySkill.targetEffectPointName; // 嗜血狂怒的特效挂点
            }
            else if (newSkill is LightGuardSkill lightGuardSkill)
            {
                targetPointName = lightGuardSkill.targetEffectPointName;
            }
            else if (newSkill is NormalOperationSkill normalOpSkill) // 暴走的特效挂点
            {
                targetPointName = normalOpSkill.targetEffectPointName;
            }
            else if (newSkill is GravityPullSkill gravitySkill)
            {
                targetPointName = gravitySkill.targetEffectPointName;
            }
            else if (newSkill is PoisonCloudSkill poisonSkill) // 毒云技能
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
            else if (newSkill is CircleDanceSkill circleSkill)
            {
                targetPointName = circleSkill.targetEffectPointName;
            }
            // 后续新增Self技能，添加else if

            // 3.3 查找对应特效点，找不到则用Player位置
            if (!string.IsNullOrEmpty(targetPointName))
            {
                playerEffectPoint = playerTransform.Find(targetPointName);
                if (playerEffectPoint != null)
                {
                    Debug.Log($"ShopManager找到特效点 {targetPointName} 用于 {newSkill.skillName}");
                }
            }
            if (playerEffectPoint == null)
            {
                playerEffectPoint = playerTransform;
                Debug.LogWarning($"ShopManager未找到 {targetPointName}，使用Player位置替代");
            }

            // 3.4 给不同技能赋值对应引用
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
            // 后续新增技能，添加else if

            // 3.5 将实例设为Player的子物体，防止场景混乱
            skillObj.transform.SetParent(playerTransform);
            skillObj.transform.localPosition = Vector3.zero;
            skillObj.name = newSkill.skillName;
        }

        // 4. 将技能添加到玩家技能控制器的Q/E/R技能槽中
        // 4.1 先清理该槽位旧技能实例，避免旧实例残留影响新技能持续效果
        SkillBase oldSkill = slotKey switch
        {
            "Q" => playerSkillCtrl.skillQ,
            "E" => playerSkillCtrl.skillE,
            "R" => playerSkillCtrl.skillR,
            _ => null
        };
        if (oldSkill != null)
        {
            var oldGo = oldSkill.gameObject;
            oldSkill.OnRemoved();
            oldGo.SetActive(false);
            Destroy(oldGo);
        }

        playerSkillCtrl.AssignSkill(slotKey, newSkill);

        // 5. 记录已购买技能，防止重复购买
        purchasedSkills[slotKey].Add(skillPrefab);

        // 6. 扣矿石+更新UI
        playerStats.SpendOre(skillCost);
        uiManager?.UpdateOreUI();

        return true;
    }

    // 获取指定槽位的可购买技能列表，总技能池 - 已购买的
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

        // 2. 过滤已购买的技能，得到可购买列表
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

    // 获取指定槽位已购买的技能列表，供外部修改或显示（可删除）
    public List<SkillBase> GetPurchasedSkills(string slotKey)
    {
        List<SkillBase> result = new List<SkillBase>();
        if (purchasedSkills == null || !purchasedSkills.ContainsKey(slotKey)) return result;
        foreach (var s in purchasedSkills[slotKey])
            result.Add(s);
        return result;
    }

    // 获取所有槽位已购买的技能，供UI显示
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

    public bool TrySwitchSkill(string slotKey, SkillBase skillPrefab)
    {
        var backpack = FindObjectOfType<BackpackManager>();
        if (backpack == null) return false;
        if (backpack.shopManager == null) backpack.shopManager = this;
        return backpack.TrySwitchSkill(slotKey, skillPrefab);
    }
}