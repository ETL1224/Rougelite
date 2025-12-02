using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // 引用暂停面板（后面会赋值）
    public GameObject pausePanel;

    public GameObject pauseIconBtn;
    // 标记是否处于暂停状态（避免重复触发）
    private bool isPaused = false;

    // 记录当前场景名称（用于重新开始时重载）
    private string currentScene;
    public bool IsPaused => isPaused;

    void Start()
    {
        // 初始化：获取当前场景名称（自动适配任意场景）
        currentScene = SceneManager.GetActiveScene().name;
        // 确保开始时暂停图标能显示出来
        UpdatePauseIconVisibility();
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {

        UpdatePauseIconVisibility();

        // 只有在未打开商店时才响应ESC进行暂停/恢复
        var shopUI = FindObjectOfType<ShopUIManager>();
        if (shopUI == null || !shopUI.IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    ResumeGame(); // 已暂停 → 恢复
                else
                    PauseGame(); // 未暂停 → 暂停
            }
        }
    }

    // 暂停游戏（核心逻辑）
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // 游戏时间停止（物理、动画都暂停）
        pausePanel.SetActive(true); // 显示暂停面板
        UpdatePauseIconVisibility(); // 暂停时隐藏图标按钮
        var cursorMgr = FindObjectOfType<CursorManager>();
        if (cursorMgr != null)
            cursorMgr.EnterUIMode();
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // 继续游戏（绑定到「继续游戏」按钮）
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // 恢复游戏时间
        pausePanel.SetActive(false); // 隐藏暂停面板
        // 显示准星并切换为游戏模式
        UpdatePauseIconVisibility(); // 暂停时隐藏图标按钮
        var cursorMgr = FindObjectOfType<CursorManager>();
        if (cursorMgr != null)
            cursorMgr.EnterGameMode();
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 重新开始游戏（绑定到「重新开始」按钮）
    public void RestartGame()
    {
        Time.timeScale = 1f; // 先恢复时间（避免重载后游戏卡住）
        SceneManager.LoadScene(currentScene); // 重载当前场景（回到初始状态）
        Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标
        Cursor.visible = false; // 隐藏鼠标
        UpdatePauseIconVisibility(); // 重新开始时更新图标显示
    }
    public void OnPauseIconClicked()
    {
        PauseGame();
    }
    private void UpdatePauseIconVisibility()
    {
        // 空值保护：如果图标按钮未赋值，不执行操作（避免空引用错误）
        if (pauseIconBtn == null)
        {
            Debug.LogWarning("PauseManager：pauseIconBtn 未赋值！请在Inspector中关联图标按钮");
            return;
        }
        var shopUI = FindObjectOfType<ShopUIManager>();
        UIManager uiManager = FindObjectOfType<UIManager>();
        bool shopOpen = shopUI != null && shopUI.IsOpen;
        pauseIconBtn.SetActive(!isPaused && !shopOpen&& uiManager.IsPlayerDead == false); // 仅在未暂停且商店未打开时显示图标
    }
}