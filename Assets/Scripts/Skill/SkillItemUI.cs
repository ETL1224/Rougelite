using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillItemUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescText;

    [Header("切换")]
    [SerializeField] private Button switchButton;

    private System.Action switchAction;

    // 设置技能信息
    public void SetSkillInfo(
        SkillBase skill,
        System.Action onSwitch = null,
        bool showSwitchButton = false)
    {
        if (skill == null) return;
        switchAction = onSwitch;

        if (skillIcon != null)
        {
            skillIcon.sprite = skill.icon;
            skillIcon.preserveAspect = true;
        }

        if (skillNameText != null)
            skillNameText.text = skill.skillName;

        if (skillDescText != null)
            skillDescText.text = skill.description;

        if (switchButton != null)
        {
            switchButton.gameObject.SetActive(showSwitchButton);
            switchButton.onClick.RemoveAllListeners();
            if (showSwitchButton)
                switchButton.onClick.AddListener(() => switchAction?.Invoke());
        }
    }
}