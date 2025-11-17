using UnityEngine;

// 万向天引技能：Self类型，吸收范围内矿石（不找子物体，仅Player直接子对象）
public class GravityPullSkill : SkillBase
{
    [Header("万象天引核心配置")]
    public string targetEffectPointName = "EffectPoint"; // 必须是Player的直接子对象
    public GameObject gravityEffectPrefab; // 吸附特效（漩涡、引力场粒子）
    public float skillDuration = 3f; // 吸附持续时间（秒）
    public float pullRadius = 40f; // 吸附半径（米）
    public float oreMoveSpeed = 80f; // 矿石飞向玩家的速度
    public Sprite skillIcon; // 技能图标（ShopUI显示）
    private float cooldown;

    [Header("内部状态（不用手动改）")]
    public Transform effectPoint; // 特效生成点（Player直接子对象）
    public PlayerState playerState; // 自动绑定PlayerState
    public UIManager uiManager; // 自动绑定UIManager（收集矿石用）
    private GameObject currentGravityEffect; // 当前吸附特效
    private float skillEndTime; // 技能结束时间

    // 初始化技能信息（ShopUI显示+框架适配）
    private void OnEnable()
    {
        castType = SkillCastType.Self; // Self类型，按E直接释放
        baseCooldown = 10f; // 基础冷却（可调整）
        cooldown = baseCooldown;
    }

    private void Start()
    {
        // 自动绑定引用
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        // 简化查找：仅找Player的直接子对象（不递归）
        AutoFindEffectPoint();
    }

    // 自动找特效点（仅Player直接子对象，符合你的需求）
    private void AutoFindEffectPoint()
    {
        if (playerState != null && !string.IsNullOrEmpty(targetEffectPointName))
        {
            Transform playerTransform = playerState.transform;
            // 仅查找Player的直接子对象（不找模型等嵌套子对象）
            Transform targetPoint = playerTransform.Find(targetEffectPointName);
            if (targetPoint != null)
            {
                effectPoint = targetPoint;
                Debug.Log($"万象天引：找到Player直接子对象特效点 {targetEffectPointName}");
                return;
            }
            else
            {
                Debug.LogWarning($"万象天引：Player直接子对象中未找到 {targetEffectPointName}，用Player位置兜底");
            }
        }

        // 兜底：用Player位置
        effectPoint = playerState != null ? playerState.transform : transform;
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

    // 核心：释放技能（开启吸附+播放特效）
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (player == null || uiManager == null)
        {
            Debug.LogError("未找到PlayerState或UIManager，无法激活万向天引");
            return;
        }

        // 1. 记录技能结束时间
        skillEndTime = Time.time + skillDuration;
        playerState = player;

        // 2. 播放吸附特效（跟随Player直接子对象）
        PlayGravityEffect();

        // 3. 开启吸附逻辑（每帧检测矿石）
        InvokeRepeating(nameof(PullOres), 0f, 0.05f); // 每0.05秒检测一次，吸附流畅
    }

    // 播放吸附特效
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
                effectPoint // 设为特效点子对象，跟随Player移动
            );
            currentGravityEffect.transform.localPosition = Vector3.zero;
        }
    }

    // 核心：吸附矿石逻辑
    private void PullOres()
    {
        // 1. 检测范围内所有标签为"Ore"、层为"Ore"的矿石
        Collider[] ores = Physics.OverlapSphere(effectPoint.position, pullRadius, LayerMask.GetMask("OreDrop"));
        foreach (var oreCollider in ores)
        {
            GameObject ore = oreCollider.gameObject;
            // 2. 让矿石朝向特效点移动（自动飞向玩家）
            Vector3 dir = (effectPoint.position - ore.transform.position).normalized;
            ore.transform.Translate(dir * oreMoveSpeed * Time.deltaTime);

            // 3. 矿石靠近玩家（距离<0.5米）时，收集矿石
            if (Vector3.Distance(ore.transform.position, effectPoint.position) < 0.5f)
            {
                CollectOre(ore);
            }
        }

        // 4. 技能结束，停止吸附
        if (Time.time >= skillEndTime)
        {
            CancelInvoke(nameof(PullOres));
            Destroy(currentGravityEffect);
            Debug.Log("万象天引结束！停止吸附矿石");
        }
    }

    // 收集矿石（增加数量+销毁矿石）
    private void CollectOre(GameObject ore)
    {
        // 每个矿石默认加1个（可根据需求修改）
        uiManager.AddOre(1);
        Destroy(ore);
    }

    // 兜底：技能销毁时停止吸附，避免内存泄漏
    private void OnDestroy()
    {
        CancelInvoke(nameof(PullOres));
        Destroy(currentGravityEffect);
    }

    // 可选：Scene视图绘制吸附半径（青色线框，方便调试范围）
    private void OnDrawGizmosSelected()
    {
        if (effectPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(effectPoint.position, pullRadius);
        }
    }
}