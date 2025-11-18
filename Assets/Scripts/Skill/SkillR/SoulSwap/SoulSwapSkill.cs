using System.Collections;
using UnityEngine;

// 灵魂交换：Self类型R大招，消耗20%最大生命值换10秒高额攻击力
public class SoulSwapSkill : SkillBase
{
    [Header("灵魂交换核心配置（大招级）")]
    public string targetEffectPointName = "EffectPoint"; // Player直接子对象（特效点）
    public GameObject activeEffectPrefab; // 技能激活特效
    public float skillDuration = 10f; // 攻击力加成持续时间（10秒）
    public float healthCostPercent = 0.2f; // 消耗最大生命值的比例（20%）
    public float soulSwapAttackMulti = 2.5f;
    public Sprite skillIcon; // 大招图标
    private float cooldown;

    [Header("内部状态（不用手动改）")]
    public Transform effectPoint; // 特效生成点（自动绑定）
    public PlayerState playerState; // 自动绑定PlayerState
    public UIManager uiManager; // 自动绑定UIManager（更新血条UI）
    private GameObject currentActiveEffect; // 当前激活特效
    private Coroutine skillCoroutine; // 技能持续协程

    // 初始化技能信息（ShopUI显示+大招属性）
    private void OnEnable()
    {
        castType = SkillCastType.Self; // Self类型，按R直接释放
        baseCooldown = 40f; // 大招级冷却（40秒，平衡献祭代价）
        cooldown = baseCooldown;
        // 读取PlayerState的攻击加成倍数（保持配置统一）
        if (FindObjectOfType<PlayerState>() != null)
        {
            soulSwapAttackMulti = FindObjectOfType<PlayerState>().soulSwapAttackMulti;
        }
    }

    private void Start()
    {
        // 自动绑定核心引用
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

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
                Debug.Log($"灵魂交换：找到Player直接子对象特效点 {targetEffectPointName}");
                return;
            }
            else
            {
                Debug.LogWarning($"灵魂交换：未找到特效点，用Player位置兜底");
                effectPoint = playerTransform;
            }
        }
    }

    // 尝试释放大招（先判断冷却+生命值是否足够）
    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player, Vector3 dir)
    {
        if (!CanCast(player))
        {
            Debug.Log($"{skillName} 冷却中！剩余：{Mathf.Ceil(Time.time - lastCastTime)}秒");
            return;
        }

        // 额外判断：确保消耗后不会死亡（虽然SpendMaxHealthPercent已兜底，但这里加提示）
        float minHealthAfterCost = player.maxHealth * healthCostPercent;
        if (player.currentHealth - minHealthAfterCost < 1f)
        {
            Debug.LogWarning($"生命值不足，无法释放{skillName}！");
            return;
        }

        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    // 核心：释放大招（消耗生命值+加攻+播放特效）
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (player == null || uiManager == null)
        {
            Debug.LogError("未找到PlayerState或UIManager，无法激活灵魂交换");
            return;
        }

        playerState = player;

        // 1. 消耗20%最大生命值
        player.SpendMaxHealthPercent(healthCostPercent);
        // 2. 调用UpdateHealthUI，即时更新血条
        uiManager.UpdateHealthUI();
        // 3. 开启攻击力加成
        player.StartSoulSwap();
        // 4. 播放激活特效
        PlayActiveEffect();
        // 5. 启动协程
        if (skillCoroutine != null)
            StopCoroutine(skillCoroutine);
        skillCoroutine = StartCoroutine(SkillDurationCoroutine());
    }

    // 播放技能激活特效（持续到技能结束）
    private void PlayActiveEffect()
    {
        // 先销毁之前的特效（避免重复）
        if (currentActiveEffect != null)
            Destroy(currentActiveEffect);

        if (activeEffectPrefab != null && effectPoint != null)
        {
            currentActiveEffect = Instantiate(
                activeEffectPrefab,
                effectPoint.position,
                Quaternion.identity,
                effectPoint // 设为Player子对象，跟随移动+旋转
            );
            currentActiveEffect.transform.localPosition = Vector3.zero; // 避免偏移
        }
    }

    // 技能持续协程（10秒后恢复攻击力）
    private IEnumerator SkillDurationCoroutine()
    {
        yield return new WaitForSeconds(skillDuration);

        // 技能结束：恢复原始攻击力+销毁特效
        if (playerState != null)
            playerState.EndSoulSwap();
        if (currentActiveEffect != null)
            Destroy(currentActiveEffect);

        Debug.Log("灵魂交换持续时间结束！");
        skillCoroutine = null;
    }

    // 兜底：技能销毁时恢复状态+清理特效
    private void OnDestroy()
    {
        if (skillCoroutine != null)
            StopCoroutine(skillCoroutine);
        if (playerState != null && playerState.isSoulSwapActive)
            playerState.EndSoulSwap();
        Destroy(currentActiveEffect);
    }
}