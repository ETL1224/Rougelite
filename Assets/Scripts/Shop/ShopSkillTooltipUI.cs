using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSkillTooltipUI : MonoBehaviour
{
    [Header("UI (可选：不填则运行时自动创建)")]
    public RectTransform panel;
    public TextMeshProUGUI text;

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Layout")]
    public Vector2 panelSize = new Vector2(320f, 180f);
    public bool followMouse = true;
    public Vector2 followOffset = new Vector2(16f, -16f);

    private Canvas rootCanvas;
    private RectTransform parentRect;

    public void EnsureCreated(Transform parent)
    {
        if (panel != null && text != null) return;
        if (parent == null) parent = transform;

        rootCanvas = parent.GetComponentInParent<Canvas>();

        // Panel
        var panelGo = new GameObject(
            "SkillTooltipPanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        panelGo.transform.SetParent(parent, false);

        panel = panelGo.GetComponent<RectTransform>();
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.zero;
        panel.pivot = new Vector2(0f, 1f);
        panel.sizeDelta = panelSize;
        panel.anchoredPosition = Vector2.zero;

        parentRect = panel.parent as RectTransform;

        var img = panelGo.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.75f);
        img.raycastTarget = false;

        // Text
        var textGo = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );
        textGo.transform.SetParent(panelGo.transform, false);

        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(12f, 12f);
        textRt.offsetMax = new Vector2(-12f, -12f);

        text = textGo.GetComponent<TextMeshProUGUI>();
        text.fontSize = 30;
        text.enableWordWrapping = true;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;

        if (fontAsset != null)
        {
            text.font = fontAsset;
        }

        panelGo.SetActive(false);
        panelGo.transform.SetAsLastSibling();
    }

    public void Show(string content)
    {
        if (panel == null || text == null)
        {
            EnsureCreated(transform);
        }

        if (panel == null || text == null) return;

        text.SetText(content);
        panel.gameObject.SetActive(true);

        if (followMouse)
        {
            UpdateFollowPosition(Input.mousePosition);
        }
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!followMouse) return;
        if (panel == null) return;
        if (!panel.gameObject.activeSelf) return;

        UpdateFollowPosition(Input.mousePosition);
    }

    private void UpdateFollowPosition(Vector2 screenPos)
    {
        if (panel == null) return;
        if (parentRect == null) parentRect = panel.parent as RectTransform;
        if (parentRect == null) return;

        Camera uiCamera = null;
        if (rootCanvas == null) rootCanvas = parentRect.GetComponentInParent<Canvas>();
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCamera, out var localPoint))
        {
            return;
        }

        // localPoint 原点在 parentRect.pivot；anchoredPosition 在锚点(0,0)时以左下为原点
        Vector2 parentSize = parentRect.rect.size;
        Vector2 pivotOffset = new Vector2(parentSize.x * parentRect.pivot.x, parentSize.y * parentRect.pivot.y);
        panel.anchoredPosition = localPoint + pivotOffset + followOffset;
    }
}
