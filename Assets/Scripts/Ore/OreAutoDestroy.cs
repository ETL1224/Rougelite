using UnityEngine;

public class OreAutoDestroy : MonoBehaviour
{
    [Header("自动消失设置")]
    public float lifetime = 120f; // 存在时间（秒）
    public float warningTime = 10f; // 最后几秒开始闪烁警告

    private float timer;
    private Renderer[] renderers; // 用于控制闪烁
    private bool isWarning = false;

    void Start()
    {
        timer = lifetime;
        renderers = GetComponentsInChildren<Renderer>(); // 获取所有渲染器
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // 开始警告闪烁
        if (timer <= warningTime && !isWarning)
        {
            isWarning = true;
            InvokeRepeating("ToggleVisibility", 0, 0.2f); // 每0.2秒切换一次可见性
        }
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    // 切换渲染器的可见性（实现闪烁）
    void ToggleVisibility()
    {
        if (renderers == null) return;
        foreach (var renderer in renderers)
        {
            renderer.enabled = !renderer.enabled;
        }
    }

    // 被拾取时调用，取消闪烁并立即销毁
    public void OnPickedUp()
    {
        CancelInvoke("ToggleVisibility"); // 停止闪烁
        if (renderers != null)
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = true; // 确保最后是可见的
            }
        }
        Destroy(gameObject);
    }
}