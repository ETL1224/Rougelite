using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BackpackManager : MonoBehaviour
{
    // 静态标志，供技能控制器判断背包是否打开
    public static bool isBackpackOpen = false;
    [Header("UI 引用")]
    public GameObject backpackPanel; // 背包面板
    // public Transform skillContainer; // 技能图标容器（已弃用）
    public GameObject skillItemPrefab; // 技能图标预制体
    public CursorManager cursorManager; // 光标管理器（复用现有）

    [Header("三列技能容器")]
    public Transform qColumn;
    public Transform eColumn;
    public Transform rColumn;

    [Header("外部引用")]
    public ShopManager shopManager; // 商店管理器

    private bool isOpen = false;
    private List<GameObject> instantiatedSkillItems = new List<GameObject>(); // 缓存实例化的技能项

    void Start()
    {
        // 自动查找引用
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();

        // 初始化背包状态
        backpackPanel.SetActive(false);
    }

    void Update()
    {
        // 按B键切换背包状态（商店和暂停时不能打开）
        if (Input.GetKeyDown(KeyCode.B))
        {
            // 商店或暂停时禁止打开背包
            bool shopOpen = false;
            var shopUI = FindObjectOfType<ShopUIManager>();
            if (shopUI != null && shopUI.IsOpen) shopOpen = true;
            if (shopOpen) return;
            if (shopManager != null && shopManager.PauseManager != null && shopManager.PauseManager.IsPaused) return;
            ToggleBackpack();
        }

        // ESC键关闭背包
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseBackpack();
        }

        // 背包打开时，按Q/E/R切换显示对应技能列
        if (isOpen)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ShowSkillColumn("Q");
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                ShowSkillColumn("E");
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ShowSkillColumn("R");
            }
        }
    }
    // 显示指定技能列，其余隐藏
    public void ShowSkillColumn(string type)
    {
        if (qColumn != null) qColumn.gameObject.SetActive(type == "Q");
        if (eColumn != null) eColumn.gameObject.SetActive(type == "E");
        if (rColumn != null) rColumn.gameObject.SetActive(type == "R");
    }

    // 打开背包时默认显示Q列
    public void OnBackpackOpened()
    {
        ShowSkillColumn("Q");
    }

    // 切换背包显示状态
    public void ToggleBackpack()
    {
        if (shopManager != null && shopManager.PauseManager != null && shopManager.PauseManager.IsPaused)
            return; // 暂停时不打开

        isOpen = !isOpen;
        backpackPanel.SetActive(isOpen);

        isBackpackOpen = isOpen;

        if (cursorManager != null)
        {
            if (isOpen)
            {
                cursorManager.EnterUIMode();
                RefreshSkillDisplay(); // 刷新技能显示
                OnBackpackOpened(); // 默认显示Q列
            }
            else
            {
                cursorManager.EnterGameMode();
            }
        }
    }

    // 关闭背包
    public void CloseBackpack()
    {
        isOpen = false;
        backpackPanel.SetActive(false);
        cursorManager?.EnterGameMode();
        isBackpackOpen = false;
    }

    // 刷新技能显示（三列）
    public void RefreshSkillDisplay()
    {
        // 清空三列
        if (qColumn != null) foreach (Transform t in qColumn) Destroy(t.gameObject);
        if (eColumn != null) foreach (Transform t in eColumn) Destroy(t.gameObject);
        if (rColumn != null) foreach (Transform t in rColumn) Destroy(t.gameObject);

        instantiatedSkillItems.Clear();
        if (shopManager == null) return;

        // Q列
        foreach (var skill in shopManager.GetPurchasedSkills("Q"))
        {
            if (skill == null) continue;
            var go = Instantiate(skillItemPrefab, qColumn);
            var ui = go.GetComponent<SkillItemUI>();
            if (ui != null)
                ui.SetSkillInfo(
                    skill,
                    onSwitch: () => SwitchSkill("Q", skill),
                    showSwitchButton: true);
            instantiatedSkillItems.Add(go);
        }
        // E列
        foreach (var skill in shopManager.GetPurchasedSkills("E"))
        {
            if (skill == null) continue;
            var go = Instantiate(skillItemPrefab, eColumn);
            var ui = go.GetComponent<SkillItemUI>();
            if (ui != null)
                ui.SetSkillInfo(
                    skill,
                    onSwitch: () => SwitchSkill("E", skill),
                    showSwitchButton: true);
            instantiatedSkillItems.Add(go);
        }
        // R列
        foreach (var skill in shopManager.GetPurchasedSkills("R"))
        {
            if (skill == null) continue;
            var go = Instantiate(skillItemPrefab, rColumn);
            var ui = go.GetComponent<SkillItemUI>();
            if (ui != null)
                ui.SetSkillInfo(
                    skill,
                    onSwitch: () => SwitchSkill("R", skill),
                    showSwitchButton: true);
            instantiatedSkillItems.Add(go);
        }
    }

    private void SwitchSkill(string slotKey, SkillBase skill)
    {
        if (skill == null) return;

        bool ok = TrySwitchSkill(slotKey, skill);
        if (!ok) Debug.LogWarning($"SwitchSkill failed: slot={slotKey}, skill={skill.skillName}");
    }

    public bool TrySwitchSkill(string slotKey, SkillBase skillPrefab)
    {
        if (string.IsNullOrEmpty(slotKey) || skillPrefab == null) return false;

        shopManager ??= FindObjectOfType<ShopManager>();
        if (shopManager == null) return false;

        var playerSkillCtrl = shopManager.playerSkillCtrl;
        if (playerSkillCtrl == null)
        {
            playerSkillCtrl = FindObjectOfType<PlayerSkillController>();
            shopManager.playerSkillCtrl = playerSkillCtrl;
        }
        if (playerSkillCtrl == null) return false;

        // 该技能来自背包展示（已购列表），这里仍做一次最小校验，避免外部误调用
        var purchased = shopManager.GetPurchasedSkills(slotKey);
        if (purchased == null || !purchased.Contains(skillPrefab)) return false;

        SkillBase equipped = slotKey switch
        {
            "Q" => playerSkillCtrl.skillQ,
            "E" => playerSkillCtrl.skillE,
            "R" => playerSkillCtrl.skillR,
            _ => null
        };

        // 冷却检测：该槽位当前技能不在CD时才能切换
        if (equipped != null)
        {
            var ps = playerSkillCtrl.playerState != null ? playerSkillCtrl.playerState : shopManager.playerStats;
            if (ps == null) ps = FindObjectOfType<PlayerState>();
            if (ps != null)
            {
                float remain = equipped.GetRemainCD(ps);
                if (remain > 0.001f)
                {
                    Debug.LogWarning($"{slotKey} 槽位技能冷却中(剩余{remain:F1}s)，禁止切换");
                    return false;
                }
            }
        }

        // 如果当前已经装备的就是它，则不重复切换
        if (equipped != null && IsSameSkill(equipped, skillPrefab)) return true;

        // 实例化新技能
        GameObject skillObj = Instantiate(skillPrefab.gameObject);
        SkillBase newSkill = skillObj.GetComponent<SkillBase>();
        if (newSkill == null)
        {
            Destroy(skillObj);
            return false;
        }

        // Self技能：绑定挂点、引用，并挂到Player下
        if (newSkill.castType == SkillCastType.Self)
        {
            Transform playerTransform = playerSkillCtrl.transform;
            Transform playerEffectPoint = null;

            string targetPointName = "";
            if (newSkill is RedemptionSkill redemptionSkill)
            {
                targetPointName = redemptionSkill.targetEffectPointName;
            }
            else if (newSkill is BloodFrenzySkill bloodFrenzySkill)
            {
                targetPointName = bloodFrenzySkill.targetEffectPointName;
            }
            else if (newSkill is LightGuardSkill lightGuardSkill)
            {
                targetPointName = lightGuardSkill.targetEffectPointName;
            }
            else if (newSkill is NormalOperationSkill normalOpSkill)
            {
                targetPointName = normalOpSkill.targetEffectPointName;
            }
            else if (newSkill is GravityPullSkill gravitySkill)
            {
                targetPointName = gravitySkill.targetEffectPointName;
            }
            else if (newSkill is PoisonCloudSkill poisonSkill)
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

            if (!string.IsNullOrEmpty(targetPointName))
            {
                playerEffectPoint = playerTransform.Find(targetPointName);
            }
            if (playerEffectPoint == null)
            {
                playerEffectPoint = playerTransform;
            }

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
            else if (newSkill is SlashSkill slashSkill2)
            {
                slashSkill2.playerState = FindObjectOfType<PlayerState>();
                slashSkill2.playerTransform = FindObjectOfType<PlayerState>().transform;
            }
            else if (newSkill is CircleDanceSkill cdSkill)
            {
                cdSkill.playerState = FindObjectOfType<PlayerState>();
                cdSkill.playerTransform = FindObjectOfType<PlayerState>().transform;
            }

            skillObj.transform.SetParent(playerTransform);
            skillObj.transform.localPosition = Vector3.zero;
            skillObj.name = newSkill.skillName;
        }

        // 清理该槽位旧技能实例，避免残留脚本影响持续效果
        if (equipped != null)
        {
            var oldGo = equipped.gameObject;
            equipped.OnRemoved();
            oldGo.SetActive(false);
            Destroy(oldGo);
        }

        playerSkillCtrl.AssignSkill(slotKey, newSkill);
        return true;
    }

    private bool IsSameSkill(SkillBase instanceSkill, SkillBase prefabSkill)
    {
        if (instanceSkill == null || prefabSkill == null) return false;
        // 用“类型 + skillName”做弱匹配，覆盖 prefab/instance 引用不同的问题
        return instanceSkill.GetType() == prefabSkill.GetType() && instanceSkill.skillName == prefabSkill.skillName;
    }

}