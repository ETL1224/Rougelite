using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillUIController : MonoBehaviour
{
    [Header("技能控制器引用")]
    public PlayerSkillController skillController;

    [Header("Q技能CD UI")]
    public Image qSkillIcon; // Q技能图标（脚本自动赋值）
    public Image qCircleMask; // Q环形进度条遮罩
    public TMP_Text qCDText; // Q技能CD文本

    [Header("E技能CD UI")]
    public Image eSkillIcon;
    public Image eCircleMask;
    public TMP_Text eCDText;

    [Header("R技能CD UI")]
    public Image rSkillIcon;
    public Image rCircleMask;
    public TMP_Text rCDText;

    void Update()
    {
        UpdateSkillUIVisibility();
        if (skillController == null) return;
        var playerState = skillController.playerState;
        if (playerState == null) return;

        // 更新Q/E/R技能CD（自动读取图标+进度+文本）
        UpdateSkillCD(skillController.skillQ, qSkillIcon, qCircleMask, qCDText, playerState);
        UpdateSkillCD(skillController.skillE, eSkillIcon, eCircleMask, eCDText, playerState);
        UpdateSkillCD(skillController.skillR, rSkillIcon, rCircleMask, rCDText, playerState);
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
    private void UpdateSkillCD(SkillBase skill, Image skillIcon, Image circleMask, TMP_Text cdText, PlayerState playerState)
    {
        if (skill == null)
        {
            // 无技能时：隐藏图标+遮罩，文本显示"-"
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
            return;
        }

        // 有技能时：显示图标+遮罩，自动赋值技能图标
        if (skillIcon != null)
        {
            skillIcon.gameObject.SetActive(true);
            skillIcon.sprite = skill.icon; // 关键：从技能中读取图标
            skillIcon.preserveAspect = true; // 保持图标比例，避免变形
        }
        if (circleMask != null) circleMask.gameObject.SetActive(true);

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