using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public GameObject crosshairUI; // 拖入准心UI对象（Canvas下的Crosshair）
    private RectTransform crosshairRect; // 准心的RectTransform组件
    private Canvas parentCanvas; // 准心所在的Canvas

    private bool isInUI = false;   // 当前是否处于UI模式

    void Start()
    {
        // 获取准心的RectTransform和所在Canvas
        if (crosshairUI != null)
        {
            crosshairRect = crosshairUI.GetComponent<RectTransform>();
            parentCanvas = crosshairUI.GetComponentInParent<Canvas>();
        }
        EnterGameMode(); // 初始进入战斗模式
    }

    void Update()
    {
        // 游戏模式下，更新准心位置为鼠标位置
        if (!isInUI && crosshairRect != null && parentCanvas != null)
        {
            UpdateCrosshairPosition();
        }
    }

    // 更新准心位置到鼠标屏幕坐标
    void UpdateCrosshairPosition()
    {
        // 将鼠标屏幕坐标转换为Canvas内的UI坐标
        Vector2 mousePos;
        // 适配不同Canvas渲染模式（Screen Space - Overlay/ Camera）
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out mousePos
        );

        // 设置准心位置
        crosshairRect.anchoredPosition = mousePos;
    }

    public void EnterUIMode()
    {
        isInUI = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (crosshairUI != null) crosshairUI.SetActive(false);
        Time.timeScale = 0f;
    }

    public void EnterGameMode()
    {
        isInUI = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        if (crosshairUI != null) crosshairUI.SetActive(true);
        Time.timeScale = 1f;
    }

    // 提供外部获取当前模式的接口
    public bool IsInUIMode()
    {
        return isInUI;
    }
}
