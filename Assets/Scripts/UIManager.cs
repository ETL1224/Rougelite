using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider HP;                       // 血条 Slider
    public TextMeshProUGUI hpText;          // 血量数值显示
    public TextMeshProUGUI oreText;         // 矿石数量
    public TextMeshProUGUI timerText;       // 倒计时文本
    public GameObject deathUI;              // 阵亡UI界面

    [Header("Game Timer")]
    public float gameTime = 240f;           // 总时长
    private float timeLeft;
    private bool isGameActive = true;       // 控制计时是否运行

    [Header("Player Object")]
    public GameObject player;               // 玩家对象引用（用于销毁或动画）
    public Animator playerAnimator;         // 可在 Inspector 手动指定（优先使用）
    public float deathUiDelay = 1.25f;       // 默认等待动画时间（秒），可按动画长度调整

    [Header("Cursor Manager")]
    public CursorManager cursorManager; // 拖入场景中的CursorManager对象

    [Header("Player State")]
    public PlayerState playerState;

    void Start()
    {
        Application.targetFrameRate = 60; // 强制限制帧率为60
        QualitySettings.vSyncCount = 1;   // 开启垂直同步，避免帧率波动导致的额外负载

        // 自动查找 PlayerState
        if (playerState == null && player != null)
            playerState = player.GetComponent<PlayerState>();

        if (playerState == null)
        {
            Debug.LogError("UIManager：找不到 PlayerState，无法显示血量！");
            return;
        }

        // 初始化血条
        HP.minValue = 0;
        HP.maxValue = playerState.maxHealth;
        HP.value = playerState.currentHealth;

        UpdateHealthUI();
        UpdateOreUI();
        timeLeft = gameTime;
        UpdateTimerUI();

        if (deathUI != null)
            deathUI.SetActive(false);

        // 缓存Animator
        if (playerAnimator == null && player != null)
        {
            playerAnimator = player.GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (!isGameActive || playerState == null) return;

        // 实时同步UI
        UpdateHealthUI();
        UpdateOreUI();

        // 计时
        UpdateTimer();
    }

    // 玩家收到伤害
    public void TakeDamage(float amount)
    {
        if (playerState == null) return;

        playerState.currentHealth = Mathf.Clamp(playerState.currentHealth - amount, 0, playerState.maxHealth);
        UpdateHealthUI();

        if (playerState.currentHealth <= 0 && isGameActive)
        {
            PlayerDie();
        }
    }

    // 玩家恢复血量
    public void Heal(float amount)
    {
        if (playerState == null) return;

        playerState.currentHealth = Mathf.Clamp(playerState.currentHealth + amount, 0, playerState.maxHealth);
        UpdateHealthUI();
    }

    public void AddOre(int amount)
    {
        if (playerState == null) return;

        playerState.ore += amount;
        UpdateOreUI();
    }

    // UI更新函数
    private void UpdateHealthUI()
    {
        if (playerState == null) return;

        HP.maxValue = playerState.maxHealth;
        HP.value = playerState.currentHealth;
    }

    public void UpdateOreUI()
    {
        if (playerState != null && oreText != null)
            oreText.text = $"矿石：{playerState.ore}";
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"时间：{Mathf.CeilToInt(timeLeft)}s";
    }

    private void UpdateTimer()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0) timeLeft = 0;
            UpdateTimerUI();
        }
        else
        {
            isGameActive = false;
            Debug.Log("时间到，可以触发BOSS战或商店逻辑");
        }
    }

    // 玩家阵亡逻辑
    void PlayerDie()
    {
        isGameActive = false;
        Debug.Log("玩家死亡！");

        if (player != null)
        {
            // 播放死亡动画
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("isDead", true);
            }

            // 获取并禁用 PlayerController
            var playerCtrl = player.GetComponent<PlayerController>();
            if (playerCtrl != null)
                playerCtrl.enabled = false;

            // 启动协程等待固定时间
            StartCoroutine(ShowDeathUIAfterDelay(deathUiDelay));
        }
        else
        {
            // 没找到player，直接显示UI
            if (deathUI != null) deathUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private IEnumerator ShowDeathUIAfterDelay(float delay)
    {
        // 等待固定时间（不受 timescale 影响）
        yield return new WaitForSecondsRealtime(delay);

        // 弹出死亡UI
        if (deathUI != null)
            deathUI.SetActive(true);

        // 切换鼠标状态
        if (cursorManager != null)
            cursorManager.EnterUIMode();

        // 最后暂停游戏
        Time.timeScale = 0f;
    }



    // 重新开始游戏
    public void RestartGame()
    {
        Time.timeScale = 1f; // 恢复时间
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
