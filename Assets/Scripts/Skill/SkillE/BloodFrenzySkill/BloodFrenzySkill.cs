using UnityEngine;

// 嗜血狂怒技能：Self类型，吸血+攻速加成（调用UIManager.Heal）
public class BloodFrenzySkill : SkillBase
{
    [Header("嗜血狂怒核心配置")]
    public string targetEffectPointName = "EffectPoint"; // 特效点名称
    public GameObject frenzyEffectPrefab; // 嗜血特效预制体（红色粒子）
    public float skillDuration = 4f; // 技能持续时间（秒）
    public float attackSpeedMultiplier = 1.5f; // 攻速加成倍数（1.5=+50%）
    public float bloodSuckRate = 0.2f; // 吸血比例（0.2=造成伤害的20%回血）
    public Sprite skillIcon; // 技能图标（ShopUI显示）
    private float cooldown;

    [Header("内部状态（不用手动改）")]
    public Transform effectPoint; // 特效生成点（自动绑定）
    public PlayerState playerState; // 自动绑定Player状态
    public UIManager uiManager; // 自动绑定UIManager（吸血用）
    private GameObject currentFrenzyEffect; // 当前特效
    private float skillEndTime; // 技能结束时间

    private bool suppressCleanupOnDestroy;

    // 初始化技能信息（ShopUI显示）
    private void OnEnable()
    {
        castType = SkillCastType.Self;
        baseCooldown = 12f; // 基础冷却（可调整）
        cooldown = baseCooldown;
    }

    private void Start()
    {
        // 自动绑定PlayerState
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();

        // 自动绑定UIManager（吸血用）
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        // 自动绑定特效点
        AutoFindEffectPoint();
    }

    // 自动找Player上的特效点
    private void AutoFindEffectPoint()
    {
        if (playerState != null && !string.IsNullOrEmpty(targetEffectPointName))
        {
            Transform targetPoint = playerState.transform.Find(targetEffectPointName);
            if (targetPoint != null)
            {
                effectPoint = targetPoint;
                Debug.Log($"嗜血狂怒：找到特效点{targetEffectPointName}");
                return;
            }
        }

        // 兜底：用Player位置
        effectPoint = playerState != null ? playerState.transform : transform;
        Debug.LogWarning($"嗜血狂怒：未找到指定特效点，用Player位置兜底");
    }

    // 框架约定：尝试释放技能
    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (!CanCast(player))
        {
            Debug.Log($"{skillName} 冷却中！剩余：{Mathf.Ceil(Time.time - lastCastTime)}秒");
            return;
        }

        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    // 核心：释放技能
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (player == null || uiManager == null)
        {
            Debug.LogError("未找到PlayerState或UIManager，无法激活嗜血狂怒");
            return;
        }

        // 1. 开启嗜血状态（绑定owner，避免旧实例抢夺结束）
        player.StartBloodFrenzy(attackSpeedMultiplier, bloodSuckRate, this);
        playerState = player;

        // 2. 记录结束时间
        skillEndTime = Time.time + skillDuration;

        // 3. 播放持续特效
        PlayFrenzyEffect();
    }

    // 播放特效（跟随Player）
    private void PlayFrenzyEffect()
    {
        if (currentFrenzyEffect != null)
            Destroy(currentFrenzyEffect);

        if (frenzyEffectPrefab != null && effectPoint != null)
        {
            currentFrenzyEffect = Instantiate(
                frenzyEffectPrefab,
                effectPoint.position,
                Quaternion.identity,
                effectPoint
            );
            currentFrenzyEffect.transform.localPosition = Vector3.zero;
        }
    }

    // 每帧检查技能是否到期
    private void Update()
    {
        if (playerState != null && playerState.isBloodFrenzyActive)
        {
            if (Time.time >= skillEndTime)
            {
                // 结束技能（带owner；如果不是当前owner则不会结束）
                playerState.EndBloodFrenzy(this);
                if (playerState.isBloodFrenzyActive)
                {
                    enabled = false;
                    return;
                }
                Destroy(currentFrenzyEffect);
            }
        }
    }

    // 兜底：技能销毁时恢复状态
    private void OnDestroy()
    {
        if (suppressCleanupOnDestroy) return;
        if (playerState != null && playerState.isBloodFrenzyActive)
        {
            playerState.EndBloodFrenzy(this);
            Destroy(currentFrenzyEffect);
        }
    }

    public override void OnRemoved()
    {
        suppressCleanupOnDestroy = true;
        enabled = false;

        if (playerState != null && playerState.isBloodFrenzyActive)
            playerState.EndBloodFrenzy(this);

        if (currentFrenzyEffect != null)
            Destroy(currentFrenzyEffect);

        currentFrenzyEffect = null;
        playerState = null;
    }
}