using UnityEngine;

// 正常操作技能：Self类型，大幅提升移速+特效环绕
public class NormalOperationSkill : SkillBase
{
    [Header("正常操作核心配置")]
    public string targetEffectPointName = "HealEffectPoint"; // 特效点名称
    public GameObject speedEffectPrefab; // 提速特效预制体（比如蓝色/紫色流光）
    public float skillDuration = 4f; // 提速持续时间（秒，建议3-5秒）
    public float moveSpeedMultiplier = 2.0f; // 移速加成倍数（2.0=+100%）
    public Sprite skillIcon; // 技能图标（ShopUI显示）
    private float cooldown;

    [Header("内部状态（不用手动改）")]
    public Transform effectPoint; // 特效生成点（自动绑定）
    public PlayerState playerState; // 自动绑定PlayerState
    private GameObject currentSpeedEffect; // 当前提速特效
    private float skillEndTime; // 技能结束时间

    private bool suppressCleanupOnDestroy;

    // 初始化技能信息（ShopUI显示+框架适配）
    private void OnEnable()
    {
        castType = SkillCastType.Self; // Self类型，按E直接释放
        baseCooldown = 12f; // 基础冷却
        cooldown = baseCooldown;
        // 保证每次实例化都初始化特效点
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();
        AutoFindEffectPoint();
    }

    private void Start()
    {
        // Start中无需再初始化依赖，已在OnEnable处理
    }

    // 自动找Player上的特效点（复用之前的逻辑）
    private void AutoFindEffectPoint()
    {
        if (playerState != null && !string.IsNullOrEmpty(targetEffectPointName))
        {
            Transform targetPoint = playerState.transform.Find(targetEffectPointName);
            if (targetPoint != null)
            {
                effectPoint = targetPoint;
                Debug.Log($"正常操作：找到特效点 {targetEffectPointName}");
                return;
            }
        }

        // 兜底：用Player位置（特效环绕Player）
        effectPoint = playerState != null ? playerState.transform : transform;
        Debug.LogWarning($"正常操作：未找到指定特效点，用Player位置兜底");
    }

    // 框架约定：尝试释放技能（先判断冷却）
    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        // 直接释放技能，不再判断cd
        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    // 核心：释放技能（开启提速+播放特效）
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (player == null)
        {
            Debug.LogError("未找到PlayerState，无法激活正常操作");
            return;
        }

        // 1. 开启提速状态（绑定owner，避免旧实例抢夺结束）
        player.StartNormalOperation(moveSpeedMultiplier, this);
        playerState = player;

        // 2. 记录技能结束时间
        skillEndTime = Time.time + skillDuration;

        // 3. 播放持续特效（跟随Player，体现提速感）
        PlaySpeedEffect();
    }

    // 播放提速特效（持续到技能结束）
    private void PlaySpeedEffect()
    {
        // 先销毁之前的特效（避免重复）
        if (currentSpeedEffect != null)
            Destroy(currentSpeedEffect);

        if (speedEffectPrefab != null && effectPoint != null)
        {
            currentSpeedEffect = Instantiate(
                speedEffectPrefab,
                effectPoint.position,
                Quaternion.identity,
                effectPoint // 设为Player子对象，跟随移动
            );
            currentSpeedEffect.transform.localPosition = Vector3.zero; // 避免偏移
        }
    }

    // 每帧检查技能是否到期（恢复移速）
    private void Update()
    {
        if (playerState != null && playerState.isNormalOperationActive)
        {
            if (Time.time >= skillEndTime)
            {
                // 结束提速（带owner；如果不是当前owner则不会结束）
                playerState.EndNormalOperation(this);
                if (playerState.isNormalOperationActive)
                {
                    enabled = false;
                    return;
                }
                if (currentSpeedEffect != null)
                    Destroy(currentSpeedEffect);
            }
        }
    }

    // 兜底：技能销毁时恢复移速（避免卡死提速状态）
    private void OnDestroy()
    {
        if (suppressCleanupOnDestroy) return;
        if (playerState != null && playerState.isNormalOperationActive)
        {
            playerState.EndNormalOperation(this);
            Destroy(currentSpeedEffect);
        }
    }

    public override void OnRemoved()
    {
        suppressCleanupOnDestroy = true;
        enabled = false;

        if (playerState != null && playerState.isNormalOperationActive)
            playerState.EndNormalOperation(this);

        if (currentSpeedEffect != null)
            Destroy(currentSpeedEffect);

        currentSpeedEffect = null;
        playerState = null;
    }
}