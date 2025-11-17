using UnityEngine;

// 光之守护技能：Self类型，持续无敌+光效环绕
public class LightGuardSkill : SkillBase
{
    [Header("光之守护核心配置")]
    public string targetEffectPointName = "EffectPoint"; // 特效点名称
    public GameObject guardEffectPrefab; // 光效预制体（比如金色/白色粒子环绕）
    public float skillDuration = 3f; // 无敌持续时间
    public Sprite skillIcon; // 技能图标（ShopUI显示）
    private float cooldown;

    [Header("内部状态（不用手动改）")]
    public Transform effectPoint; // 特效生成点（自动绑定）
    public PlayerState playerState; // 自动绑定PlayerState
    private GameObject currentGuardEffect; // 当前光效
    private float skillEndTime; // 技能结束时间

    // 初始化技能信息（ShopUI显示+框架适配）
    private void OnEnable()
    {
        castType = SkillCastType.Self; // Self类型，按E直接释放
        baseCooldown = 15f; // 基础冷却
        cooldown = baseCooldown;
    }

    private void Start()
    {
        // 自动绑定PlayerState（关键：控制无敌状态）
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();

        // 自动绑定特效点（根据名称查找）
        AutoFindEffectPoint();
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
                Debug.Log($"光之守护：找到特效点 {targetEffectPointName}");
                return;
            }
        }

        // 兜底：用Player位置（光效环绕Player）
        effectPoint = playerState != null ? playerState.transform : transform;
        Debug.LogWarning($"光之守护：未找到指定特效点，用Player位置兜底");
    }

    // 框架约定：尝试释放技能（先判断冷却）
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

    // 核心：释放技能（开启无敌+播放光效）
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (player == null)
        {
            Debug.LogError("未找到PlayerState，无法激活光之守护");
            return;
        }

        // 1. 开启无敌状态
        player.StartInvincibility();
        playerState = player;

        // 2. 记录技能结束时间
        skillEndTime = Time.time + skillDuration;

        // 3. 播放持续光效（环绕Player，跟随移动）
        PlayGuardEffect();
    }

    // 播放光效（持续到技能结束）
    private void PlayGuardEffect()
    {
        // 先销毁之前的光效（避免重复）
        if (currentGuardEffect != null)
            Destroy(currentGuardEffect);

        if (guardEffectPrefab != null && effectPoint != null)
        {
            currentGuardEffect = Instantiate(
                guardEffectPrefab,
                effectPoint.position,
                Quaternion.identity,
                effectPoint // 设为Player子对象，跟随移动
            );
            currentGuardEffect.transform.localPosition = Vector3.zero; // 避免偏移
        }
    }

    // 每帧检查技能是否到期（关闭无敌）
    private void Update()
    {
        if (playerState != null && playerState.isInvincible)
        {
            if (Time.time >= skillEndTime)
            {
                // 结束无敌，销毁光效
                playerState.EndInvincibility();
                if (currentGuardEffect != null)
                    Destroy(currentGuardEffect);
            }
        }
    }

    // 兜底：技能销毁时解除无敌（避免卡死无敌状态）
    private void OnDestroy()
    {
        if (playerState != null && playerState.isInvincible)
        {
            playerState.EndInvincibility();
            Destroy(currentGuardEffect);
        }
    }
}