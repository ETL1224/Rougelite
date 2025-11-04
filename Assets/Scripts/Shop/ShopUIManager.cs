using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI 元素")]
    public GameObject shopPanel;
    public TMP_Text oreText;

    [Header("按钮及文字")]
    public Button attackBtn;
    public TMP_Text attackText;

    public Button attackSpeedBtn;
    public TMP_Text attackSpeedText;

    public Button moveSpeedBtn;
    public TMP_Text moveSpeedText;

    public Button healthBtn;
    public TMP_Text healthText;

    public Button skillQBtn;
    public Button skillEBtn;
    public Button skillRBtn;

    [Header("引用")]
    public ShopManager shopManager;
    public CursorManager cursorManager;

    private bool isOpen = false;

    void Start()
    {
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();
        shopManager.shopUI = this; // 建立反向引用

        attackBtn.onClick.AddListener(() => OnUpgradeClicked("attack"));
        attackSpeedBtn.onClick.AddListener(() => OnUpgradeClicked("attackSpeed"));
        moveSpeedBtn.onClick.AddListener(() => OnUpgradeClicked("moveSpeed"));
        healthBtn.onClick.AddListener(() => OnUpgradeClicked("health"));

        skillQBtn.onClick.AddListener(() => OnBuySkillClicked("Q"));
        skillEBtn.onClick.AddListener(() => OnBuySkillClicked("E"));
        skillRBtn.onClick.AddListener(() => OnBuySkillClicked("R"));

        shopPanel.SetActive(false);
        UpdateUpgradeTexts(0, 0, 0, 0); // 初始化文本
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleShop();

        if (oreText != null && shopManager != null)
            oreText.text = $"矿石：{shopManager.GetOreCount()}";
    }

    void ToggleShop()
    {
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
        if (cursorManager != null)
        {
            if (isOpen) cursorManager.EnterUIMode();
            else cursorManager.EnterGameMode();
        }
    }

    void OnUpgradeClicked(string key)
    {
        bool ok = shopManager != null && shopManager.Upgrade(key);
        if (!ok) Debug.Log("升级失败: " + key);
    }

    void OnBuySkillClicked(string slot)
    {
        bool ok = shopManager != null && shopManager.BuyAndAssignRandomSkill(slot);
        if (!ok) Debug.Log("购买技能失败: " + slot);
    }

    // 新增：统一更新所有按钮文字
    public void UpdateUpgradeTexts(int atkLv, int atkSpdLv, int moveLv, int hpLv)
    {
        if (attackText != null)
            attackText.text = $"攻击力: Lv.{atkLv}\n升级：{shopManager.upgradeCost}矿石";
        if (attackSpeedText != null)
            attackSpeedText.text = $"攻速: Lv.{atkSpdLv}\n升级：{shopManager.upgradeCost}矿石";
        if (moveSpeedText != null)
            moveSpeedText.text = $"移速: Lv.{moveLv}\n升级：{shopManager.upgradeCost}矿石";
        if (healthText != null)
            healthText.text = $"生命: Lv.{hpLv}\n升级：{shopManager.upgradeCost}矿石";
    }
}
