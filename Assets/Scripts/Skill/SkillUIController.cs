using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillUIController : MonoBehaviour
{
    [Header("技能控制器引用")]
    public PlayerSkillController skillController;

    [Header("背包三列容器")]
    public Transform qColumn;
    public Transform eColumn;
    public Transform rColumn;
    [Header("技能Item预制体")]
    public GameObject skillItemPrefab;
    [Header("技能管理器")]
    public SkillManager skillManager;


    [Header("Q技能CD UI")]
    public Image qSkillIcon; // Q技能图标（脚本自动赋值）
    public Image qCircleMask; // Q环形进度条遮罩
    public TMP_Text qCDText; // Q技能CD文本
    public TMP_Text qSkillNameText; // Q技能名


    [Header("E技能CD UI")]
    public Image eSkillIcon;
    public Image eCircleMask;
    public TMP_Text eCDText;
    public TMP_Text eSkillNameText; // E技能名


    [Header("R技能CD UI")]
    public Image rSkillIcon;
    public Image rCircleMask;
    public TMP_Text rCDText;
    public TMP_Text rSkillNameText; // R技能名

    void Update()
    {
        UpdateSkillUIVisibility();
        if (skillController == null) return;
        var playerState = skillController.playerState;
        if (playerState == null) return;

        // 更新Q/E/R技能CD和技能名
        UpdateSkillCD(skillController.skillQ, qSkillIcon, qCircleMask, qCDText, qSkillNameText, playerState);
        UpdateSkillCD(skillController.skillE, eSkillIcon, eCircleMask, eCDText, eSkillNameText, playerState);
        UpdateSkillCD(skillController.skillR, rSkillIcon, rCircleMask, rCDText, rSkillNameText, playerState);
    }

    // 背包界面：按Q/E/R分三列显示技能
    public void ShowSkillBag()
    {
        if (qColumn == null || eColumn == null || rColumn == null || skillItemPrefab == null || skillManager == null) return;
        // 清空旧的
        foreach (Transform t in qColumn) Destroy(t.gameObject);
        foreach (Transform t in eColumn) Destroy(t.gameObject);
        foreach (Transform t in rColumn) Destroy(t.gameObject);

        // Q列
        foreach (var skill in skillManager.skillPoolQ)
        {
            var go = Instantiate(skillItemPrefab, qColumn);
            var ui = go.GetComponent<SkillItemUI>();
            if (ui != null) ui.SetSkillInfo(skill);
        }
        // E列
        foreach (var skill in skillManager.skillPoolE)
        {
            var go = Instantiate(skillItemPrefab, eColumn);
            var ui = go.GetComponent<SkillItemUI>();
            if (ui != null) ui.SetSkillInfo(skill);
        }
        // R列
        foreach (var skill in skillManager.skillPoolR)
        {
            var go = Instantiate(skillItemPrefab, rColumn);
            var ui = go.GetComponent<SkillItemUI>();
            if (ui != null) ui.SetSkillInfo(skill);
        }
    }

    // 仿照 PauseManager 的暂停按钮消失逻辑，控制技能UI的显示/隐藏
    private void UpdateSkillUIVisibility()
    {
        var pauseMgr = FindObjectOfType<PauseManager>();
        var shopUI = FindObjectOfType<ShopUIManager>();
        var uiManager = FindObjectOfType<UIManager>();
        bool isPaused = pauseMgr != null && pauseMgr.IsPaused;
        bool shopOpen = shopUI != null && shopUI.IsOpen;
        bool playerDead = uiManager != null && uiManager.IsPlayerDead;
        bool show = !isPaused && !shopOpen && !playerDead;
        SetSkillUIActive(show);
    }

    // 控制所有技能UI的显示/隐藏
    private void SetSkillUIActive(bool active)
    {
        if (qSkillIcon != null) qSkillIcon.gameObject.SetActive(active);
        if (qCircleMask != null) qCircleMask.gameObject.SetActive(active);
        if (qCDText != null) qCDText.gameObject.SetActive(active);
        if (eSkillIcon != null) eSkillIcon.gameObject.SetActive(active);
        if (eCircleMask != null) eCircleMask.gameObject.SetActive(active);
        if (eCDText != null) eCDText.gameObject.SetActive(active);
        if (rSkillIcon != null) rSkillIcon.gameObject.SetActive(active);
        if (rCircleMask != null) rCircleMask.gameObject.SetActive(active);
        if (rCDText != null) rCDText.gameObject.SetActive(active);
    }

    // 通用方法：更新单个技能的CD显示（自动读取技能图标）
    private void UpdateSkillCD(SkillBase skill, Image skillIcon, Image circleMask, TMP_Text cdText, TMP_Text skillNameText, PlayerState playerState)
    {
        if (skill == null)
        {
            // 无技能时：隐藏图标+遮罩，文本显示"-"，技能名置空
            if (skillIcon != null)
            {
                skillIcon.gameObject.SetActive(false);
                skillIcon.sprite = null; // 清空图标
            }
            if (circleMask != null) circleMask.gameObject.SetActive(false);
            if (cdText != null)
            {
                cdText.text = "-";
                cdText.color = Color.gray; // 无技能时文本灰色
            }
            if (skillNameText != null)
                skillNameText.text = "";
            return;
        }

        // 有技能时：显示图标+遮罩，自动赋值技能图标，显示技能名
        if (skillIcon != null)
        {
            skillIcon.gameObject.SetActive(true);
            skillIcon.sprite = skill.icon; // 关键：从技能中读取图标
            skillIcon.preserveAspect = true; // 保持图标比例，避免变形
        }
        if (circleMask != null) circleMask.gameObject.SetActive(true);
        if (skillNameText != null)
            skillNameText.text = string.IsNullOrEmpty(skill.skillName) ? "技能" : skill.skillName;

        // 获取CD数据（总CD + 剩余CD）
        float totalCD = skill.GetTotalCD(playerState); // 依赖你SkillBase中的GetTotalCD方法
        float remainCD = skill.GetRemainCD(playerState);
        float cdRatio = Mathf.Clamp01(remainCD / totalCD); // 限制比例在0~1之间，避免异常

        // 控制环形进度条（填充比例 = 剩余CD/总CD）
        if (circleMask != null)
        {
            circleMask.fillAmount = cdRatio;
            // CD结束后隐藏遮罩，CD中显示半透明黑色
            circleMask.color = remainCD > 0 ? new Color(0, 0, 0, 0.7f) : Color.clear;
        }

        // 控制文本显示
        if (cdText != null)
        {
            if (remainCD > 0)
            {
                cdText.text = remainCD.ToString("F1"); // 冷却中显示剩余时间（1位小数）
                cdText.color = Color.white;
            }
            else
            {
                cdText.text = ""; // 冷却结束隐藏文本（或显示快捷键Q/E/R）
                cdText.color = Color.clear;
            }
        }
    }
}