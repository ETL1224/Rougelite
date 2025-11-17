using Unity.VisualScripting;
using UnityEngine;

// 救赎技能：继承SkillBase，融入现有技能框架
public class RedemptionSkill : SkillBase
{
    [Header("救赎技能核心配置")]
    public GameObject healEffectPrefab;  // 治疗特效预制体
    public float healAmount = 10f;       // 恢复生命值（可在Inspector调整）
    public Sprite skillIcon;             // 技能图标（用于商店UI显示）
    private float cooldown;

    [Header("特效配置（新增）")]
    public float effectDuration = 4f;    // 特效持续时间（秒），默认2秒（和特效播放时长匹配）
    public string targetEffectPointName = "HealEffectPoint"; // 特效点名称（Player上的）
    public Transform effectPoint;

    [Header("引用配置（代码绑定）")]
    public UIManager uiManager;          // 用于调用回血方法（原有）

    // 初始化技能基础信息（融入框架的关键）
    private void OnEnable()
    {
        castType = SkillCastType.Self;     // 对应框架的「自身释放型」，按E直接释放
        cooldown = baseCooldown;
    }

    private void Start()
    {
        // 自动找UIManager（不用手动拖，兜底）
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        AutoFindEffectPoint();
    }

    private void AutoFindEffectPoint()
    {
        if (string.IsNullOrEmpty(targetEffectPointName))
        {
            Debug.LogWarning("未设置targetEffectPointName，用Player位置兜底");
            effectPoint = GetPlayerTransform();
            return;
        }

        // 找Player→再找指定名称的特效点
        Transform playerTransform = GetPlayerTransform();
        if (playerTransform != null)
        {
            Transform targetPoint = playerTransform.Find(targetEffectPointName);
            if (targetPoint != null)
            {
                effectPoint = targetPoint;
                Debug.Log($"救赎技能：找到特效点 {targetEffectPointName}");
                return;
            }
            else
            {
                Debug.LogWarning($"Player上未找到 {targetEffectPointName}，用Player位置兜底");
            }
        }
        effectPoint = playerTransform ?? transform;
    }

    private Transform GetPlayerTransform()
    {
        PlayerState playerState = FindObjectOfType<PlayerState>();
        return playerState != null ? playerState.transform : null;
    }

    // 框架约定：尝试释放技能（先判断冷却，再释放）
    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        // 调用基类CanCast：判断是否在冷却（自动计算技能急速）
        if (!CanCast(player))
        {
            Debug.Log($"{skillName} 冷却中！剩余：{Mathf.Ceil(Time.time - lastCastTime)}秒");
            return;
        }

        // 记录释放时间（触发冷却）
        lastCastTime = Time.time;
        // 执行真正的释放逻辑
        Cast(castPos, caster, player);
    }

    // 核心：释放技能（回血+播放特效）
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        // 1. 恢复生命值
        if (uiManager != null)
        {
            uiManager.Heal(healAmount);
            Debug.Log($"救赎生效：恢复{healAmount}点生命值！");
        }
        else
        {
            Debug.LogError("未找到UIManager，无法回血！");
        }

        // 2. 播放治疗特效（原有逻辑+新增「延迟销毁」）
        if (healEffectPrefab != null && effectPoint != null)
        {
            // 生成特效时，额外设置父对象为effectPoint
            GameObject effect = Instantiate(
                healEffectPrefab,
                effectPoint.position,  // 生成在effectPoint位置
                Quaternion.identity,   // 不旋转
                effectPoint            // 关键：把特效的父对象设为effectPoint（跟随玩家）
            );
            // 可选：重置特效的局部位置和旋转（避免偏移）
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localRotation = Quaternion.identity;

            // 关键新增：延迟effectDuration秒后，销毁特效对象
            Destroy(effect, effectDuration);
            Debug.Log($"播放治疗特效！将在{effectDuration}秒后自动销毁");
        }
        else
        {
            Debug.LogWarning("治疗特效或生成点未赋值！");
        }
    }
}