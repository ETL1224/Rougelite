using UnityEngine;

public class Enemy2AI : EnemyBase
{
    [Header("Enemy2专属配置")]
    [Tooltip("比Enemy1更高的移速，默认20f")]
    public float wolfMoveSpeed = 15f;
    [Tooltip("探测范围更远，默认200f")]
    public float wolfDetectRange = 200f;
    [Tooltip("攻击范围稍近")]
    public float wolfAttackRange = 8f;

    protected override void Awake()
    {
        // 覆盖基类属性，设置狼的高移速特性
        moveSpeed = wolfMoveSpeed;       // 核心：更高移速
        detectRange = wolfDetectRange;   // 扩展探测范围
        attackRange = wolfAttackRange;   // 调整攻击范围
        maxHealth = 3f;                  // 狼的血量
        damage = 3f;                     // 单次伤害比Enemy1高（突袭特性）
        attackCooldown = 1f;             // 攻击冷却

        base.Awake(); // 继承基类初始化（绑定玩家、组件等）
    }

    void Update()
    {
        if (isDestroyed || isDead) return; // 死亡后停止AI
        if (player == null) return;

        Vector3 moveDir = Vector3.zero;
        float distance = Vector3.Distance(transform.position, player.position);

        // 1. 追踪逻辑：探测范围更远，一旦发现玩家立即高速追击
        if (distance <= detectRange)
        {
            moveDir = (player.position - transform.position).normalized;
            SetRunningAnimation(true); // 播放奔跑动画
        }
        else
        {
            SetRunningAnimation(false);
        }

        // 2. 敌人避让：避免狼群扎堆（继承基类逻辑）
        moveDir += CalculateAvoidance() * 1.2f; // 增强避让权重，避免狼群拥挤

        // 3. 高速移动（核心差异：moveSpeed更高）
        if (distance >= attackRange - 0.1f && moveDir != Vector3.zero)
        {
            // 平滑移动，保持朝向玩家
            transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }

        // 4. 攻击逻辑（冷却更短，伤害更高）
        Attack();
    }

    // 重写攻击方法：适配狼的快速攻击特性
    protected override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackRange)
            {
                TriggerAnimation("Attack"); // 播放狼的攻击动画
                lastAttackTime = Time.time;
            }
        }
    }
    public override void TakeDamage(float amount)
    {
        if (isDestroyed || isDead) return;

        // 核心：不触发TakeDamage动画（Enemy2无该动画）
        float healthBefore = Health;
        Health -= amount;
        Health = Mathf.Max(Health, 0f);

        // 血量为0时触发死亡
        if (Health <= 0f && healthBefore > 0f)
        {
            Die();
        }
    }

    // 实现抽象方法：动画事件触发伤害（狼的突袭伤害）
    public override void DealDamage()
    {
        if (player == null || uiManager == null) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            uiManager.TakeDamage(damage);
            Debug.Log($"狼形敌人攻击，造成{damage}点突袭伤害！");
        }
    }
}