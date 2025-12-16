using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopSkillHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Debug")]
    [SerializeField] private string slotKey;

    private SkillBase skillPrefab;
    private ShopManager shopManager;
    private PlayerSkillController playerSkillController;
    private ShopSkillTooltipUI tooltipUI;

    public void Bind(string slot, SkillBase skill, ShopManager manager, ShopSkillTooltipUI tooltip)
    {
        slotKey = slot;
        skillPrefab = skill;
        shopManager = manager;
        tooltipUI = tooltip;

        if (playerSkillController == null)
        {
            playerSkillController = FindObjectOfType<PlayerSkillController>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillPrefab == null) return;

        var info = BuildPreviewInfo(skillPrefab);
        // 方案一：Self 类型不显示范围指示器（SkillIndicatorManager 未配置 Self prefab 会报警）
        if (info.castType == SkillCastType.Ground || info.castType == SkillCastType.Direction)
        {
            ShowRangeIndicator(info.castType, info.radius);
        }
        ShowTooltip(info);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideRangeIndicator();
        tooltipUI?.Hide();
    }

    private void ShowTooltip(PreviewInfo info)
    {
        if (tooltipUI == null) return;

        // 确保tooltip面板存在（优先挂在ShopPanel下）
        if (tooltipUI.panel == null || tooltipUI.text == null)
        {
            Transform parent = null;
            if (shopManager != null && shopManager.shopUI != null && shopManager.shopUI.shopPanel != null)
            {
                parent = shopManager.shopUI.shopPanel.transform;
            }
            tooltipUI.EnsureCreated(parent != null ? parent : tooltipUI.transform);
        }

        tooltipUI.Show(info.content);
    }

    private void ShowRangeIndicator(SkillCastType castType, float radius)
    {
        if (SkillIndicatorManager.Instance == null) return;

        Transform castPoint = null;
        if (playerSkillController != null)
        {
            castPoint = playerSkillController.castPoint != null ? playerSkillController.castPoint : playerSkillController.transform;
        }

        if (castPoint == null)
        {
            var ps = FindObjectOfType<PlayerState>();
            castPoint = ps != null ? ps.transform : null;
        }

        if (castPoint == null) return;

        float r = radius > 0 ? radius : 2f;
        SkillIndicatorManager.Instance.ShowIndicator(castPoint.position, r, castType, castPoint);
    }

    private void HideRangeIndicator()
    {
        if (SkillIndicatorManager.Instance == null) return;
        SkillIndicatorManager.Instance.HideIndicator();
    }

    private static bool TryGetDamageValue(SkillBase skill, out float value, out string label)
    {
        value = 0f;
        label = "伤害";
        if (skill == null) return false;

        var type = skill.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        (string fieldName, string pretty)[] priority =
        {
            ("damage", "伤害"),
            ("baseDamage", "基础伤害"),
            ("damagePerTick", "每跳伤害"),
            ("damagePerHit", "每段伤害"),
        };

        foreach (var (fieldName, pretty) in priority)
        {
            if (TryReadNumericMember(type, flags, skill, fieldName, out float v))
            {
                value = v;
                label = pretty;
                return true;
            }
        }

        foreach (var f in type.GetFields(flags))
        {
            if (!IsNumeric(f.FieldType)) continue;
            if (f.Name.IndexOf("damage", StringComparison.OrdinalIgnoreCase) < 0) continue;

            try
            {
                value = Convert.ToSingle(f.GetValue(skill));
                label = "伤害";
                return true;
            }
            catch { }
        }

        foreach (var p in type.GetProperties(flags))
        {
            if (!p.CanRead) continue;
            if (!IsNumeric(p.PropertyType)) continue;
            if (p.Name.IndexOf("damage", StringComparison.OrdinalIgnoreCase) < 0) continue;

            try
            {
                value = Convert.ToSingle(p.GetValue(skill));
                label = "伤害";
                return true;
            }
            catch { }
        }

        return false;
    }

    private static bool TryReadNumericMember(Type type, BindingFlags flags, object instance, string name, out float v)
    {
        v = 0f;

        var f = type.GetField(name, flags);
        if (f != null && IsNumeric(f.FieldType))
        {
            try
            {
                v = Convert.ToSingle(f.GetValue(instance));
                return true;
            }
            catch { return false; }
        }

        var p = type.GetProperty(name, flags);
        if (p != null && p.CanRead && IsNumeric(p.PropertyType))
        {
            try
            {
                v = Convert.ToSingle(p.GetValue(instance));
                return true;
            }
            catch { return false; }
        }

        return false;
    }

    private static bool IsNumeric(Type t)
    {
        return t == typeof(float) || t == typeof(int) || t == typeof(double);
    }

    private struct PreviewInfo
    {
        public SkillCastType castType;
        public float radius;
        public string content;
    }

    private PreviewInfo BuildPreviewInfo(SkillBase prefab)
    {

        SkillBase effective = prefab;
        GameObject temp = null;

        try
        {
            if (!prefab.gameObject.scene.IsValid())
            {
                temp = Instantiate(prefab.gameObject);
                effective = temp.GetComponent<SkillBase>() ?? prefab;
            }

            var ps = shopManager != null ? shopManager.playerStats : null;
            float cd = effective.baseCooldown;
            if (ps != null)
            {
                cd = effective.GetTotalCD(ps);
            }

            float radius = effective.indicatorRadius;
            string damageLine = "伤害：-";
            if (TryGetDamageValue(effective, out float dmg, out string dmgLabel))
            {
                damageLine = $"{dmgLabel}：{dmg:F1}";
            }

            string content =
                $"{effective.skillName}\n" +
                $"类型：{effective.castType}\n" +
                $"范围：{radius:F1}\n" +
                $"冷却：{cd:F1}s\n" +
                $"{damageLine}";

            return new PreviewInfo
            {
                castType = effective.castType,
                radius = radius,
                content = content
            };
        }
        finally
        {
            if (temp != null)
            {
                Destroy(temp);
            }
        }
    }
}
