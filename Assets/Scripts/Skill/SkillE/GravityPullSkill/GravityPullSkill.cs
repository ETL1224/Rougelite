using UnityEngine;

// 重力牵引技能：Self释放型，吸引范围内矿石向玩家移动，玩家直接受益
public class GravityPullSkill : SkillBase
{
    [Header("重力牵引技能配置")]
    public string targetEffectPointName = "EffectPoint"; // 挂在玩家身上的特效点
    public GameObject gravityEffectPrefab; // 重力特效预制体（如引力场、漩涡）
    public float skillDuration = 3f; // 技能持续时间（秒）
    public float pullRadius = 40f; // 牵引半径（米）
    public float oreMoveSpeed = 80f; // 矿石向玩家移动的速度
    public Sprite skillIcon; // 技能图标（ShopUI展示用）
    private float cooldown;

    [Header("内部状态（可外部赋值）")]
    public Transform effectPoint; // 特效生成点（玩家身上的挂点）
    public PlayerState playerState; // 自动查找的玩家状态
    public UIManager uiManager; // 自动查找的UI管理器（用于收集矿石）
    private GameObject currentGravityEffect; // 当前生效的重力特效
    private float skillEndTime; // 技能结束时间

    // 初始化技能信息（ShopUI展示+冷却初始化）
    private void OnEnable()
    {
        castType = SkillCastType.Self; // Self释放型，按E直接释放
        baseCooldown = 10f; // 技能基础冷却时间
        cooldown = baseCooldown;
    }

    private void Start()
    {
        // 自动查找组件
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        // 初始化：挂在玩家身上的特效点（自动查找）
        AutoFindEffectPoint();
    }

    // 自动查找特效点（挂在玩家身上的挂点，优先按名称查找）
    private void AutoFindEffectPoint()
    {
        if (playerState != null && !string.IsNullOrEmpty(targetEffectPointName))
        {
            Transform playerTransform = playerState.transform;
            // 查找玩家身上的子物体挂点（如空物体的挂点）
            Transform targetPoint = playerTransform.Find(targetEffectPointName);
            if (targetPoint != null)
            {
                effectPoint = targetPoint;
                Debug.Log($"重力牵引技能找到玩家身上的特效点 {targetEffectPointName}");
                return;
            }
            else
            {
                Debug.LogWarning($"重力牵引技能玩家身上未找到 {targetEffectPointName}，使用玩家位置替代");
            }
        }

        // 兜底：使用玩家位置
        effectPoint = playerState != null ? playerState.transform : transform;
    }

    // 尝试释放技能（带冷却判断）
    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (!CanCast(player))
        {
            Debug.Log($"{skillName} 冷却中，剩余：{Mathf.Ceil(Time.time - lastCastTime)}秒");
            return;
        }

        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    // 释放技能核心逻辑：计时+播放重力特效
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (player == null || uiManager == null)
        {
            Debug.LogError("未找到PlayerState或UIManager，无法释放重力牵引技能");
            return;
        }

        // 1. 记录技能结束时间
        skillEndTime = Time.time + skillDuration;
        playerState = player;

        // 2. 播放重力特效（挂在玩家身上）
        PlayGravityEffect();

        // 3. 执行牵引逻辑（每帧拉取矿石）
        InvokeRepeating(nameof(PullOres), 0f, 0.05f); // 每0.05秒执行一次，提升流畅度
    }

    // 播放重力特效
    private void PlayGravityEffect()
    {
        if (currentGravityEffect != null)
            Destroy(currentGravityEffect);

        if (gravityEffectPrefab != null && effectPoint != null)
        {
            currentGravityEffect = Instantiate(
                gravityEffectPrefab,
                effectPoint.position,
                Quaternion.identity,
                effectPoint // 作为特效父物体，跟随玩家移动
            );
            currentGravityEffect.transform.localPosition = Vector3.zero;
        }
    }

    // 核心：牵引矿石逻辑
    private void PullOres()
    {
        // 1. 检测范围内标签为"Ore"或层为"OreDrop"的矿石
        Collider[] ores = Physics.OverlapSphere(effectPoint.position, pullRadius, LayerMask.GetMask("OreDrop"));
        foreach (var oreCollider in ores)
        {
            GameObject ore = oreCollider.gameObject;
            // 2. 让矿石向特效点移动（匀速移动）
            Vector3 dir = (effectPoint.position - ore.transform.position).normalized;
            ore.transform.Translate(dir * oreMoveSpeed * Time.deltaTime);

            // 3. 矿石距离特效点<0.5米时，收集矿石
            if (Vector3.Distance(ore.transform.position, effectPoint.position) < 0.5f)
            {
                CollectOre(ore);
            }
        }

        // 4. 技能结束后停止牵引
        if (Time.time >= skillEndTime)
        {
            CancelInvoke(nameof(PullOres));
            Destroy(currentGravityEffect);
            Debug.Log("重力牵引技能结束，停止牵引矿石");
        }
    }

    // 收集矿石（增加数量+销毁矿石物体）
    private void CollectOre(GameObject ore)
    {
        // 每个矿石默认加1（可根据需求修改）
        uiManager.AddOre(1);
        Destroy(ore);
    }

    // 兜底：组件销毁时停止牵引，防止内存泄漏
    private void OnDestroy()
    {
        CancelInvoke(nameof(PullOres));
        Destroy(currentGravityEffect);
    }

    // 选中时在Scene视图绘制牵引半径（青色线框），方便调试范围
    private void OnDrawGizmosSelected()
    {
        if (effectPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(effectPoint.position, pullRadius);
        }
    }
}