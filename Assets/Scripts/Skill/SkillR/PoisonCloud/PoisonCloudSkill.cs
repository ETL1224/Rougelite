using UnityEngine;

// 剧毒踪迹：Self类型R大招，持续生成毒雾造成范围伤害
public class PoisonCloudSkill : SkillBase
{
    [Header("剧毒踪迹核心配置（大招级）")]
    public string targetEffectPointName = "EffectPoint"; // Player直接子对象
    public GameObject poisonCloudPrefab; // 毒雾预制体
    public float skillDuration = 5f; // 技能持续时间（生成毒雾的总时长）
    public float cloudSpawnInterval = 0.5f; // 毒雾生成间隔（0.5秒1个，覆盖广）
    public Sprite skillIcon; // 大招图标
    private float cooldown;

    [Header("内部状态（不用手动改）")]
    public Transform effectPoint; // 特效生成点（自动绑定）
    public PlayerState playerState; // 自动绑定PlayerState
    private float skillEndTime; // 技能结束时间
    private float nextSpawnTime; // 下一次生成毒雾的时间

    // 初始化技能信息（ShopUI显示+大招属性）
    private void OnEnable()
    {
        castType = SkillCastType.Self; // Self类型，按R直接释放
        baseCooldown = 30f; // 大招级冷却（30秒，可调整平衡）
        cooldown = baseCooldown;
    }

    private void Start()
    {
        // 自动绑定引用
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();

        // 查找Player直接子对象的特效点（不递归）
        AutoFindEffectPoint();
    }

    // 自动找特效点（仅Player直接子对象）
    private void AutoFindEffectPoint()
    {
        if (playerState != null && !string.IsNullOrEmpty(targetEffectPointName))
        {
            Transform playerTransform = playerState.transform;
            Transform targetPoint = playerTransform.Find(targetEffectPointName);
            if (targetPoint != null)
            {
                effectPoint = targetPoint;
                Debug.Log($"剧毒踪迹：找到Player直接子对象特效点 {targetEffectPointName}");
                return;
            }
            else
            {
                Debug.LogWarning($"剧毒踪迹：未找到特效点，用Player位置兜底");
            }
        }

        // 兜底：用Player位置生成毒雾
        effectPoint = playerState != null ? playerState.transform : transform;
    }

    // 框架约定：尝试释放大招（先判断冷却）
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

    // 核心：释放大招（开始持续生成毒雾）
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (player == null || poisonCloudPrefab == null)
        {
            Debug.LogError("未找到PlayerState或毒雾预制体，无法激活剧毒踪迹");
            return;
        }

        // 初始化时间参数
        skillEndTime = Time.time + skillDuration;
        nextSpawnTime = Time.time;
        playerState = player;

        Debug.Log("剧毒踪迹激活！持续生成毒雾...");
    }

    // 每帧检测：是否需要生成新毒雾
    private void Update()
    {
        if (playerState == null || !IsSkillActive()) return;

        // 到时间生成毒雾
        if (Time.time >= nextSpawnTime)
        {
            SpawnPoisonCloud();
            nextSpawnTime += cloudSpawnInterval;
        }

        // 技能结束，停止生成
        if (Time.time >= skillEndTime)
        {
            Debug.Log("剧毒踪迹结束！停止生成毒雾");
        }
    }

    // 生成毒雾预制体（跟随Player移动轨迹）
    private void SpawnPoisonCloud()
    {
        if (effectPoint == null) return;

        // 生成毒雾（不设为Player子对象，避免跟随移动，保持生成时的位置）
        GameObject cloud = Instantiate(
            poisonCloudPrefab,
            effectPoint.position,
            Quaternion.identity
        );
        cloud.name = "PoisonCloud"; // 重命名，方便调试
    }

    // 判断技能是否处于激活状态（持续生成毒雾中）
    private bool IsSkillActive()
    {
        return Time.time < skillEndTime && lastCastTime != 0 && Time.time - lastCastTime < skillDuration;
    }

    // 兜底：技能销毁时停止生成
    private void OnDestroy()
    {
        skillEndTime = 0;
    }
}