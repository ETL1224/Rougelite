using Unity.VisualScripting;
using UnityEngine;

// Q技能：Self类型环绕斩击，随机3种特效+敌人受击反馈
public class SlashSkill : SkillBase
{
    [Header("斩击核心配置")]
    public string targetEffectPointName = "EffectPoint";
    public float damage = 6f; // 基础伤害（乘player.skillPower）
    public float slashRadius = 10f; // 斩击范围
    public float skillDuration = 0.5f; // 特效持续时间
    public Sprite skillIcon; // Q技能图标（剑/斩击相关）
    private float cooldown;

    [Header("随机斩击特效")]
    public GameObject[] slashEffectPrefabs; // 数组存储3种斩击特效（拖入3个不同预制体）

    [Header("敌人受击特效")]
    public GameObject hitEffectPrefab; // 敌人受击特效预制体
    public float effectDuration = 0.8f; // 受击特效时长
    public Vector3 effectOffset = new Vector3(0, 1.2f, 0); // 特效偏移（胸口位置）

    [Header("内部引用（自动绑定）")]
    public Transform playerTransform; // 玩家位置（生成特效+检测伤害）
    public Transform effectPoint;
    public PlayerState playerState;

    private void OnEnable()
    {
        castType = SkillCastType.Self;
        baseCooldown = 8f; 
        cooldown = baseCooldown;
    }

    private void Start()
    {
        // 自动绑定核心引用
        playerState = FindObjectOfType<PlayerState>();
        if (playerState != null)
            playerTransform = playerState.transform;

        AutoFindEffectPoint();
    }

    private void AutoFindEffectPoint()
    {
        if (playerTransform != null && !string.IsNullOrEmpty(targetEffectPointName))
        {
            Transform targetPoint = playerTransform.Find(targetEffectPointName);
            if (targetPoint != null)
            {
                effectPoint = targetPoint;
                Debug.Log($"环绕斩击：找到特效点 {targetEffectPointName}");
                return;
            }
            else
            {
                Debug.LogWarning($"未找到斩击特效点，用Player位置兜底");
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

    // 核心：释放斩击（随机特效+范围伤害）
    protected override void Cast(Vector3 castPos, Transform caster, PlayerState player)
    {
        if (playerTransform == null) return;

        // 1. 随机生成一种斩击特效（核心：3选1）
        SpawnRandomSlashEffect();

        // 2. 对周围3米内敌人造成伤害+受击特效
        DealSlashDamage(player);
    }

    // 随机生成3种斩击特效之一
    private void SpawnRandomSlashEffect()
    {
        // 安全判断：确保数组有3种特效，且都赋值了
        if (slashEffectPrefabs == null || slashEffectPrefabs.Length != 3)
        {
            Debug.LogError("斩击特效数组必须添加3种预制体！");
            return;
        }

        // 随机选一个索引（0=第一种，1=第二种，2=第三种）
        int randomIndex = Random.Range(0, 3);
        GameObject selectedEffect = slashEffectPrefabs[randomIndex];

        // 生成特效（在玩家位置，跟随玩家瞬间播放）
        if (selectedEffect != null)
        {
            GameObject effect = Instantiate(
                selectedEffect,
                playerTransform.position,
                effectPoint.rotation * Quaternion.Euler(90f, 0f, 0f),
                effectPoint
            );
            // 特效播放完自动销毁
            Destroy(effect, skillDuration);
        }
    }

    // 斩击范围伤害（
    private void DealSlashDamage(PlayerState player)
    {
        Collider[] hits = Physics.OverlapSphere(
            playerTransform.position,
            slashRadius,
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
                    Debug.Log($"斩击击中{hit.name}，造成{finalDamage:F1}伤害！");

                    // 触发敌人受击特效
                    SpawnHitEffect(hit.transform);
                }
            }
        }
    }

    // 生成敌人受击特效（复用水柱/核弹的成熟逻辑）
    private void SpawnHitEffect(Transform enemyTransform)
    {
        if (hitEffectPrefab == null) return;

        Vector3 effectPos = enemyTransform.position + effectOffset;
        GameObject effect = Instantiate(hitEffectPrefab, effectPos, Quaternion.identity);
        Destroy(effect, effectDuration);
    }

    // Scene视图调试：显示斩击范围（红色线框，方便调半径）
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, slashRadius);
        }
    }
}