using UnityEngine;

// 光之守护技能：Self释放型，提供无敌状态+特效展示
public class LightGuardSkill : SkillBase
{
    [Header("光之守护技能配置")]
    public string targetEffectPointName = "EffectPoint"; // 特效挂载点名称
    public GameObject guardEffectPrefab; // 防护特效预制体（护盾/光效等视觉表现）
    public float skillDuration = 3f; // 技能持续时间
    public Sprite skillIcon; // 技能图标（ShopUI展示用）
    private float cooldown;

    [Header("依赖状态与组件（可手动赋值）")]
    public Transform effectPoint; // 特效挂载点（默认自动查找）
    public PlayerState playerState; // 引用的玩家状态组件
    private GameObject currentGuardEffect; // 当前生效的防护特效
    private float skillEndTime; // 技能结束时间

    private bool suppressCleanupOnDestroy;

    // 初始化技能基础信息（ShopUI展示+冷却初始化）
    private void OnEnable()
    {
        castType = SkillCastType.Self; // Self释放型，按E直接释放
        baseCooldown = 15f; // 技能基础冷却时间
        cooldown = baseCooldown;
    }

    private void Start()
    {
        // 自动查找PlayerState组件（用于控制无敌状态）
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();

        // 自动查找特效挂载点（优先按名称查找）
        AutoFindEffectPoint();
    }

    // 自动查找玩家身上的特效挂载点（释放技能前初始化）
    private void AutoFindEffectPoint()
    {
        if (playerState != null && !string.IsNullOrEmpty(targetEffectPointName))
        {
            Transform targetPoint = playerState.transform.Find(targetEffectPointName);
            if (targetPoint != null)
            {
                effectPoint = targetPoint;
                Debug.Log($"光之守护技能找到特效挂载点 {targetEffectPointName}");
                return;
            }
        }

        // 未找到时，使用Player位置作为特效挂载点（默认跟随Player）
        effectPoint = playerState != null ? playerState.transform : transform;
        Debug.LogWarning($"光之守护技能未找到指定特效挂载点，使用Player位置作为替代");
    }

    // 尝试释放技能（重写父类方法，此处直接跳过冷却判断）
    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        // 直接释放技能，不再判断冷却（根据业务需求可恢复冷却判断）
        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    // 释放技能核心逻辑：开启无敌状态+播放防护特效
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (player == null)
        {
            Debug.LogError("未找到PlayerState，无法释放光之守护技能");
            return;
        }

        // 1. 释放前先结束旧的无敌状态，避免切换技能后无敌状态残留
        if (player.isInvincible)
            player.EndInvincibility();

        // 2. 开启无敌状态（绑定owner，避免其他实例误关闭）
        player.StartInvincibility(this);
        playerState = player;

        // 3. 记录技能结束时间
        skillEndTime = Time.time + skillDuration;

        // 4. 播放防护特效
        PlayGuardEffect();
    }

    // 播放防护特效（包含旧特效清理+新特效生成）
    private void PlayGuardEffect()
    {
        // 清理之前的防护特效（避免重复显示）
        if (currentGuardEffect != null)
            Destroy(currentGuardEffect);

        if (guardEffectPrefab != null && effectPoint != null)
        {
            currentGuardEffect = Instantiate(
                guardEffectPrefab,
                effectPoint.position,
                Quaternion.identity,
                effectPoint // 挂载到Player的特效节点，跟随玩家移动
            );
            currentGuardEffect.transform.localPosition = Vector3.zero; // 重置本地偏移
        }
    }

    // 每帧检测：技能持续时间结束后关闭无敌状态并清理特效
    private void Update()
    {
        if (playerState != null && playerState.isInvincible)
        {
            if (Time.time >= skillEndTime)
            {
                // 结束无敌（带owner：如果不是当前owner则不会执行）
                playerState.EndInvincibility(this);
                if (playerState.isInvincible)
                {
                    // 不是当前owner（其他实例），停止后续检测
                    enabled = false;
                    return;
                }
                // 清理防护特效
                if (currentGuardEffect != null)
                    Destroy(currentGuardEffect);
            }
        }
    }

    // 组件销毁时：清理无敌状态和特效（防止内存泄漏）
    private void OnDestroy()
    {
        if (suppressCleanupOnDestroy) return;
        if (playerState != null && playerState.isInvincible)
        {
            playerState.EndInvincibility(this);
            Destroy(currentGuardEffect);
        }
    }

    // 技能被移除时的清理逻辑（外部调用）
    public override void OnRemoved()
    {
        suppressCleanupOnDestroy = true;
        enabled = false;

        if (playerState != null && playerState.isInvincible)
            playerState.EndInvincibility(this);

        if (currentGuardEffect != null)
            Destroy(currentGuardEffect);

        currentGuardEffect = null;
        playerState = null;
    }
}