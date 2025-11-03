using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI 元素")]
    public GameObject shopPanel;         // 指向 ShopPanel（整个面板）
    public TMP_Text oreText;             // 指向 OreText (TextMeshPro)
    public Button attackBtn;
    public Button attackSpeedBtn;
    public Button moveSpeedBtn;
    public Button healthBtn;
    public Button skillQBtn;
    public Button skillEBtn;
    public Button skillRBtn;

    [Header("引用")]
    public ShopManager shopManager;      // 指向场景里的 ShopManager
    public CursorManager cursorManager;  // 拖入场景中的 CursorManager 对象

    private bool isOpen = false;

    void Start()
    {
        if (shopPanel == null)
            Debug.LogError("ShopPanel 未绑定到 ShopUIManager！");
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();

        // 绑定按钮事件（也可以在 Inspector 用 OnClick 绑定）
        attackBtn.onClick.AddListener(() => OnUpgradeClicked("attack"));
        attackSpeedBtn.onClick.AddListener(() => OnUpgradeClicked("attackSpeed"));
        moveSpeedBtn.onClick.AddListener(() => OnUpgradeClicked("moveSpeed"));
        healthBtn.onClick.AddListener(() => OnUpgradeClicked("health"));

        skillQBtn.onClick.AddListener(() => OnBuySkillClicked("Q"));
        skillEBtn.onClick.AddListener(() => OnBuySkillClicked("E"));
        skillRBtn.onClick.AddListener(() => OnBuySkillClicked("R"));

        // 初始状态：商店UI隐藏
        shopPanel.SetActive(false);
        isOpen = false;
    }

    void Update()
    {
        // 只在 ShopUIManager 中监听 Tab 键（唯一触发源）
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleShop();
        }

        // 更新矿石显示
        if (oreText != null && shopManager != null)
            oreText.text = $"矿石：{shopManager.GetOreCount()}";
    }

    void ToggleShop()
    {
        isOpen = !isOpen;

        // 1. 控制商店UI显示/隐藏
        if (shopPanel != null)
        {
            shopPanel.SetActive(isOpen);
        }

        // 2. 同步 CursorManager 状态（关键！避免冲突）
        if (cursorManager != null)
        {
            if (isOpen)
                cursorManager.EnterUIMode(); // 打开商店 → 进入UI模式（显示鼠标、隐藏准心）
            else
                cursorManager.EnterGameMode(); // 关闭商店 → 进入游戏模式（隐藏鼠标、显示准心）
        }
    }

    void OnUpgradeClicked(string key)
    {
        bool ok = shopManager != null && shopManager.Upgrade(key);
        if (!ok) Debug.Log("升级失败（矿石不足或未配置）: " + key);
    }

    void OnBuySkillClicked(string slot)
    {
        bool ok = shopManager != null && shopManager.BuyAndAssignRandomSkill(slot);
        if (!ok) Debug.Log("购买技能失败（矿石不足或未配置）: " + slot);
    }
}
