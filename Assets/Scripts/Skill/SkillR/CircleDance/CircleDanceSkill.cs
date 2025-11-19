using Unity.VisualScripting;
using UnityEngine;

// R大招：圆舞（Self类型，升级版环绕斩击）
public class CircleDanceSkill : SkillBase
{
    [Header("圆舞大招核心配置")]
    public float damage = 20f; // 基础伤害
    public float circleRadius = 20f; // 圆舞范围
    public float skillDuration = 1.2f; // 特效持续时间
    public string targetEffectPointName = "EffectPoint"; 
    public Sprite skillIcon; // 大招图标
    private float cooldown;

    [Header("随机圆舞特效")]
    public GameObject[] circleDanceEffectPrefabs; 

    [Header("敌人受击特效（复用现有）")]
    public GameObject hitEffectPrefab; // 敌人受击特效（可复用水柱/Q技能的）
    public float effectDuration = 0.8f; // 受击特效时长
    public Vector3 effectOffset = new Vector3(0, 1.2f, 0); // 特效偏移（避免贴地）

    [Header("内部引用（自动绑定）")]
    public Transform playerTransform; // 玩家位置（伤害检测中心）
    public Transform effectPoint; // 圆舞特效生成点
    public PlayerState playerState;

    private void OnEnable()
    {
        castType = SkillCastType.Self;
        baseCooldown = 2f;
        cooldown = baseCooldown;
    }

    private void Start()
    {
        // 自动绑定核心引用
        playerState = FindObjectOfType<PlayerState>();
        if (playerState != null)
            playerTransform = playerState.transform;

        // 自动查找特效点
        AutoFindEffectPoint();
    }

    // 自动查找特效点
    private void AutoFindEffectPoint()
    {
        if (playerTransform != null && !string.IsNullOrEmpty(targetEffectPointName))
        {
            // 只找Player直接子对象，避免混淆
            Transform targetPoint = playerTransform.Find(targetEffectPointName);
            if (targetPoint != null)
            {
                effectPoint = targetPoint;
                Debug.Log($"圆舞大招：找到特效点 {targetEffectPointName}");
                return;
            }
            else
            {
                Debug.LogWarning($"未找到圆舞特效点，用Player位置兜底");
                effectPoint = playerTransform;
            }
        }
        else
        {
            effectPoint = playerTransform;
        }
    }

    // 框架约定：尝试释放技能（判断冷却）
    public override void TryCast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (!CanCast(player))
        {
            Debug.Log($"{skillName} 冷却中！剩余：{Mathf.Ceil(Time.time - lastCastTime)}秒");
            return;
        }
        lastCastTime = Time.time;
        Cast(castPos, caster, player);
    }

    // 核心：释放圆舞大招（随机特效+大范围伤害）
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (effectPoint == null) return;

        // 1. 从特效点随机生成3种圆舞特效之一
        SpawnRandomCircleDanceEffect();

        // 2. 对8米内所有敌人造成巨额伤害+受击特效
        DealCircleDanceDamage(player);

        Debug.Log($"圆舞大招触发！覆盖{circleRadius}米，基础伤害：{damage}");
    }

    // 随机生成3种圆舞特效（支持X轴90度旋转，可按需关闭）
    private void SpawnRandomCircleDanceEffect()
    {
        if (circleDanceEffectPrefabs == null || circleDanceEffectPrefabs.Length != 3)
        {
            Debug.LogError("圆舞特效数组必须添加3种预制体！");
            return;
        }

        // 随机选一种特效（0=第一种，1=第二种，2=第三种）
        int randomIndex = Random.Range(0, 3);
        GameObject selectedEffect = circleDanceEffectPrefabs[randomIndex];

        if (selectedEffect != null)
        {
            GameObject effect = Instantiate(
                selectedEffect,
                effectPoint.position, 
                effectPoint.rotation * Quaternion.Euler(90f, 0f, 0f),
                effectPoint 
            );
            // 特效播放完自动销毁（
            Destroy(effect, skillDuration);
        }
    }

    // 大范围圆舞伤害
    private void DealCircleDanceDamage(PlayerState player)
    {
        Collider[] hits = Physics.OverlapSphere(
            playerTransform.position,
            circleRadius,
            LayerMask.GetMask("Enemy")
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    float finalDamage = damage * player.skillPower;
                    enemy.TakeDamage(finalDamage);
                    Debug.Log($"圆舞击中{hit.name}，造成{finalDamage:F1}巨额伤害！");

                    // 触发敌人受击特效（和所有技能统一）
                    SpawnHitEffect(hit.transform);
                }
            }
        }
    }

    // 生成敌人受击特效
    private void SpawnHitEffect(Transform enemyTransform)
    {
        if (hitEffectPrefab == null) return;

        Vector3 effectPos = enemyTransform.position + effectOffset;
        GameObject effect = Instantiate(hitEffectPrefab, effectPos, Quaternion.identity);
        Destroy(effect, effectDuration);
    }

    // Scene视图调试：区分Q技能和大招（蓝色范围+黄色特效点）
    private void OnDrawGizmosSelected()
    {
        // 圆舞范围（蓝色线框，和Q技能红色区分）
        if (playerTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, circleRadius);
        }
        // 特效点位置（黄色小球，方便调试）
        if (effectPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(effectPoint.position, 0.3f); // 比Q技能的球大一点，更容易识别
        }
    }
}