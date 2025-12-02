using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI 元素")]
    public GameObject shopPanel;
    public TMP_Text oreText;

    [Header("属性升级按钮及文字")]
    public Button attackBtn;
    public TMP_Text attackText;
    public Button attackSpeedBtn;
    public TMP_Text attackSpeedText;
    public Button moveSpeedBtn;
    public TMP_Text moveSpeedText;
    public Button healthBtn;
    public TMP_Text healthText;
    public Button skillPowerBtn;
    public TMP_Text skillPowerText;
    public Button skillHasteBtn;
    public TMP_Text skillHasteText;

    [Header("技能购买按钮")]
    public Button skillQBtn;
    public Button skillEBtn;
    public Button skillRBtn;

    [Header("技能名称文本（按钮下方/内部）")]
    public TMP_Text skillQText;
    public TMP_Text skillEText;
    public TMP_Text skillRText;

    [Header("技能图标与描述（核心新增）")]
    // Q槽
    public Image skillQIcon;   // 技能图标UI
    public TMP_Text skillQDesc;// 技能描述文本
    // E槽
    public Image skillEIcon;
    public TMP_Text skillEDesc;
    // R槽
    public Image skillRIcon;
    public TMP_Text skillRDesc;
    // 无技能时的默认图标（必须赋值）
    public Sprite defaultSkillIcon;

    // 缓存当前预览的技能
    private SkillBase previewQ;
    private SkillBase previewE;
    private SkillBase previewR;

    [Header("核心引用")]
    public ShopManager shopManager;
    public CursorManager cursorManager;

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    void Start()
    {
        // 自动寻找缺失引用
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();
        shopManager.shopUI = this; // 反向绑定，让ShopManager能调用UI刷新

        // 绑定属性升级按钮事件
        attackBtn.onClick.AddListener(() => OnUpgradeClicked("attack"));
        attackSpeedBtn.onClick.AddListener(() => OnUpgradeClicked("attackSpeed"));
        moveSpeedBtn.onClick.AddListener(() => OnUpgradeClicked("moveSpeed"));
        healthBtn.onClick.AddListener(() => OnUpgradeClicked("health"));
        skillPowerBtn.onClick.AddListener(() => OnUpgradeClicked("skillPower"));
        skillHasteBtn.onClick.AddListener(() => OnUpgradeClicked("skillHaste"));

        // 绑定技能购买按钮事件
        skillQBtn.onClick.AddListener(() => OnBuySkillClicked("Q"));
        skillEBtn.onClick.AddListener(() => OnBuySkillClicked("E"));
        skillRBtn.onClick.AddListener(() => OnBuySkillClicked("R"));

        // 初始化UI状态
        shopPanel.SetActive(false);
        UpdateUpgradeTexts(0, 0, 0, 0, 0, 0);

        // 初始化技能图标（避免一开始显示异常）
        InitSkillIconDefaultState();
    }

    void Update()
    {
        // 实时更新矿石数量
        if (oreText != null && shopManager != null)
            oreText.text = $"矿石：{shopManager.GetOreCount()}";

        // Tab键打开/关闭商店
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleShop();

        // ESC键：如果商店已打开，按ESC关闭商店并恢复游戏模式
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = false;
            shopPanel.SetActive(false);
            if (cursorManager != null)
            {
                cursorManager.EnterGameMode();
            }
        }
    }

    // 打开/关闭商店
    void ToggleShop()
    {
        if (shopManager != null && shopManager.PauseManager != null && shopManager.PauseManager.IsPaused)
        {
            // 如果游戏已暂停，禁止打开商店
            return;
        }

        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);

        if (cursorManager != null)
        {
            if (isOpen)
            {
                cursorManager.EnterUIMode();
                RefreshAllSkillPreviews(); // 打开时刷新所有技能预览
        
            }
            else
            {
                cursorManager.EnterGameMode();
            }
        }
    }

    // 初始化技能图标默认状态（无技能时）
    void InitSkillIconDefaultState()
    {
        // Q槽
        SetSkillIconState(skillQIcon, defaultSkillIcon);
        skillQDesc?.SetText("未解锁技能");
        skillQDesc?.gameObject.SetActive(true);
        // E槽
        SetSkillIconState(skillEIcon, defaultSkillIcon);
        skillEDesc?.SetText("未解锁技能");
        skillEDesc?.gameObject.SetActive(true);
        // R槽
        SetSkillIconState(skillRIcon, defaultSkillIcon);
        skillRDesc?.SetText("未解锁技能");
        skillRDesc?.gameObject.SetActive(true);
    }

    // 刷新所有槽位的技能预览（名称+图标+描述）
    void RefreshAllSkillPreviews()
    {
        // 传递Q槽的所有UI引用：名称文本、按钮、图标、描述
        RefreshSkillPreview(
            "Q", ref previewQ,
            skillQText, skillQBtn,
            skillQIcon, skillQDesc
        );
        // E槽
        RefreshSkillPreview(
            "E", ref previewE,
            skillEText, skillEBtn,
            skillEIcon, skillEDesc
        );
        // R槽
        RefreshSkillPreview(
            "R", ref previewR,
            skillRText, skillRBtn,
            skillRIcon, skillRDesc
        );
    }

    // 核心方法：刷新单个槽位的所有UI（名称+图标+描述+按钮状态）
    /// <param name="slotKey">槽位（Q/E/R）</param>
    /// <param name="previewSkill">当前预览的技能</param>
    /// <param name="skillNameText">技能名称文本</param>
    /// <param name="skillBtn">购买按钮</param>
    /// <param name="skillIcon">技能图标</param>
    /// <param name="skillDesc">技能描述文本</param>
    void RefreshSkillPreview(
        string slotKey, ref SkillBase previewSkill,
        TMP_Text skillNameText, Button skillBtn,
        Image skillIcon, TMP_Text skillDesc
    )
    {
        if (shopManager == null) return;

        // 1. 获取该槽位的可用技能池（总池 - 已购池）
        List<SkillBase> availableSkills = shopManager.GetAvailableSkills(slotKey);

        // 2. 随机选一个预览技能（无可用则为null）
        previewSkill = availableSkills.Count > 0
            ? availableSkills[Random.Range(0, availableSkills.Count)]
            : null;

        // 3. 统一更新UI（分「有技能」和「无技能」两种情况）
        if (previewSkill != null)
        {
            // 3.1 有可用技能：显示技能信息
            // 名称
            string skillName = string.IsNullOrEmpty(previewSkill.skillName) ? "未知技能" : previewSkill.skillName;
            skillNameText?.SetText(skillName);
            // 图标（优先用技能自带图标，没有则用默认）
            Sprite targetIcon = previewSkill.icon != null ? previewSkill.icon : defaultSkillIcon;
            SetSkillIconState(skillIcon, targetIcon);
            // 描述（显示技能说明）
            skillDesc?.SetText(previewSkill.description);
            skillDesc?.gameObject.SetActive(true);
            // 按钮可点击
            skillBtn.interactable = true;
        }
        else
        {
            // 3.2 无可用技能：显示默认信息
            skillNameText?.SetText("无新技能可用");
            // 图标（用默认图标）
            SetSkillIconState(skillIcon, defaultSkillIcon);
            // 描述（提示已解锁所有）
            skillDesc?.SetText("该槽位已解锁全部技能");
            skillDesc?.gameObject.SetActive(true);
            // 按钮置灰
            skillBtn.interactable = false;
        }
    }

    // 辅助方法：设置技能图标状态（避免拉伸，统一显示）
    void SetSkillIconState(Image iconImage, Sprite targetSprite)
    {
        if (iconImage == null) return;
        iconImage.sprite = targetSprite;
        iconImage.enabled = true; // 显示图标
        iconImage.SetNativeSize(); // 自适应图片原始大小（避免拉伸）
        iconImage.preserveAspect = true; // 保持宽高比（可选，根据UI设计）
    }

    // 技能购买按钮点击事件
    void OnBuySkillClicked(string slotKey)
    {
        // 1. 获取当前预览的技能
        SkillBase skillToBuy = slotKey switch
        {
            "Q" => previewQ,
            "E" => previewE,
            "R" => previewR,
            _ => null
        };

        // 2. 调用ShopManager执行购买逻辑
        bool buySuccess = shopManager != null && shopManager.BuySkill(slotKey, skillToBuy);
        if (buySuccess)
        {
            Debug.Log($"槽位{slotKey}购买成功：{skillToBuy.skillName}");
            // 3. 购买成功后，仅刷新当前槽位的预览（传递所有UI引用）
            switch (slotKey)
            {
                case "Q":
                    RefreshSkillPreview("Q", ref previewQ, skillQText, skillQBtn, skillQIcon, skillQDesc);
                    break;
                case "E":
                    RefreshSkillPreview("E", ref previewE, skillEText, skillEBtn, skillEIcon, skillEDesc);
                    break;
                case "R":
                    RefreshSkillPreview("R", ref previewR, skillRText, skillRBtn, skillRIcon, skillRDesc);
                    break;
            }
        }
        else
        {
            Debug.Log($"槽位{slotKey}购买失败：矿石不足或无可用技能");
        }
    }

    // 属性升级按钮点击事件
    void OnUpgradeClicked(string key)
    {
        bool ok = shopManager != null && shopManager.Upgrade(key);
        if (!ok) Debug.Log("升级失败: " + key);
    }

    // 更新属性升级文本（原有逻辑不变）
    public void UpdateUpgradeTexts(int atkLv, int atkSpdLv, int moveLv, int hpLv, int skiPow, int skiHas)
    {
        attackText?.SetText($"攻击力: Lv.{atkLv}\n升级：{shopManager.upgradeCost}矿石");
        attackSpeedText?.SetText($"攻击速度: Lv.{atkSpdLv}\n升级：{shopManager.upgradeCost}矿石");
        moveSpeedText?.SetText($"移速: Lv.{moveLv}\n升级：{shopManager.upgradeCost}矿石");
        healthText?.SetText($"生命: Lv.{hpLv}\n升级：{shopManager.upgradeCost}矿石");
        skillPowerText?.SetText($"法强: Lv.{skiPow}\n升级：{shopManager.upgradeCost}矿石");
        skillHasteText?.SetText($"技能急速: Lv.{skiHas}\n升级：{shopManager.upgradeCost}矿石");
    }
}