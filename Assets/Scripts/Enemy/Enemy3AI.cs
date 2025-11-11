using System.Collections;
using UnityEngine;

public class Enemy3AI : EnemyBase
{
    [Header("Enemy3（石头人）专属配置")]
    [Tooltip("石头人移速极慢")]
    public float stoneMoveSpeed = 5f;
    [Tooltip("石头人探测范围中等")]
    public float stoneDetectRange = 150f;
    [Tooltip("石头人攻击范围大")]
    public float stoneAttackRange = 8f;
    [Tooltip("石头人血量高")]
    public float stoneMaxHealth = 20f;
    [Tooltip("石头人单次伤害高")]
    public float stoneDamage = 8f;
    [Tooltip("石头人攻速慢")]
    public float stoneAttackCooldown = 3f;

    protected override void Awake()
    {
        // 覆盖基类属性，配置石头人特性
        moveSpeed = stoneMoveSpeed;         // 核心：极慢移速
        detectRange = stoneDetectRange;     // 中等探测范围（不用太远，笨重不适合追击）
        attackRange = stoneAttackRange;     // 大攻击范围（近战重击）
        maxHealth = stoneMaxHealth;         // 高血量（肉盾）
        damage = stoneDamage;               // 高单次伤害
        attackCooldown = stoneAttackCooldown; // 慢攻速

        base.Awake(); // 继承基类初始化（绑定玩家、组件、对象池等）
    }

    void Update()
    {
        if (isDestroyed || isDead) return;
        if (player == null) return;

        // 1. 水平距离计算（忽略Y轴，避免高度差误判）
        Vector3 enemyFlatPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerFlatPos = new Vector3(player.position.x, 0, player.position.z);
        float horizontalDistance = Vector3.Distance(enemyFlatPos, playerFlatPos);

        Vector3 moveDir = Vector3.zero;

        // 2. 追踪逻辑：仅在探测范围内缓慢逼近
        if (horizontalDistance <= detectRange)
        {
            moveDir = (playerFlatPos - enemyFlatPos).normalized;
            SetRunningAnimation(true); 
        }
        else
        {
            SetRunningAnimation(false);
        }

        // 3. 避让逻辑：弱化（石头人体积大，不用太刻意避让）
        moveDir += CalculateAvoidance() * 0.3f;

        // 4. 修复：距离 <= 攻击范围
        if (horizontalDistance >= attackRange - 0.1f && moveDir.magnitude > 0.1f)
        {
            transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }

        // 5. 重击逻辑：距离足够时触发（攻速慢，单次伤害高）
        if (horizontalDistance <= attackRange)
        {
            Attack();
        }
    }

    // 重写攻击方法：适配石头人的重击动画（播放时间长）
    protected override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackRange)
            {
                TriggerAnimation("Attack"); // 石头人的Attack动画要长（比如1.5秒），匹配重击感
                lastAttackTime = Time.time;
            }
        }
    }

    // 实现抽象方法：重击伤害触发
    public override void DealDamage()
    {
        if (player == null || uiManager == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            uiManager.TakeDamage(damage);
            Debug.Log($"[{gameObject.name}] 石头人重击！造成{damage}点高额伤害！");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 重击未命中（距离{dist:F1} > 攻击范围{attackRange}）");
        }
    }

    // 重写受击逻辑：石头人血厚，受击反馈更明显
    public override void TakeDamage(float amount)
    {
        if (isDestroyed || isDead) return;

        // 强化受击反馈（石头人皮糙肉厚，受击时轻微震动+颜色变深）
        StartCoroutine(FlashAndShake());

        // 基类伤害计算（扣血+死亡判断）
        float healthBefore = Health;
        Health -= amount;
        Health = Mathf.Max(Health, 0f);

        if (Health <= 0f && healthBefore > 0f)
        {
            Die();
        }
    }

    // 石头人受击反馈：材质变深+轻微震动
    private IEnumerator FlashAndShake()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null) yield break;

        Color originalColor = rend.material.color;
        Vector3 originalPos = transform.position;

        // 颜色变深（暗灰色）
        rend.material.color = new Color(0.3f, 0.3f, 0.3f);
        // 轻微震动（3次小位移）
        for (int i = 0; i < 3; i++)
        {
            transform.position = originalPos + Random.insideUnitSphere * 0.1f;
            yield return new WaitForSeconds(0.05f);
        }

        // 恢复原状
        transform.position = originalPos;
        yield return new WaitForSeconds(0.1f);
        rend.material.color = originalColor;
    }
}