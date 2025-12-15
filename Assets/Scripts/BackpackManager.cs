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
            if (ui != null) ui.SetSkillInfo(skill);
            instantiatedSkillItems.Add(go);
        }
        // E列
        foreach (var skill in shopManager.GetPurchasedSkills("E"))
        {
            if (skill == null) continue;
            var go = Instantiate(skillItemPrefab, eColumn);
            var ui = go.GetComponent<SkillItemUI>();
            if (ui != null) ui.SetSkillInfo(skill);
            instantiatedSkillItems.Add(go);
        }
        // R列
        foreach (var skill in shopManager.GetPurchasedSkills("R"))
        {
            if (skill == null) continue;
            var go = Instantiate(skillItemPrefab, rColumn);
            var ui = go.GetComponent<SkillItemUI>();
            if (ui != null) ui.SetSkillInfo(skill);
            instantiatedSkillItems.Add(go);
        }
    }

}