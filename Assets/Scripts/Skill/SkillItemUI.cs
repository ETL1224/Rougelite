using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillItemUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescText;

    // 设置技能信息
    public void SetSkillInfo(SkillBase skill)
    {
        if (skill == null) return;

        if (skillIcon != null)
        {
            skillIcon.sprite = skill.icon;
            skillIcon.preserveAspect = true;
        }

        if (skillNameText != null)
            skillNameText.text = skill.skillName;

        if (skillDescText != null)
            skillDescText.text = skill.description;
    }
}