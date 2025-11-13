using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

// 实现 IPoolable 接口，适配对象池
public abstract class EnemyBase : DestructibleBase, IPoolable
{
    // ========== 公共属性（子类可调整） ==========
    [Header("移动与检测")]
    public float moveSpeed = 10f;
    public float detectRange = 175f;
    public float avoidRadius = 2.5f;
    public bool isDead = false;

    [Header("战斗属性")]
    public float maxHealth = 4f;
    public float damage = 2f;
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    protected float lastAttackTime = 0f;

    public Transform player;

    // ========== 组件与全局管理 ==========
    protected Rigidbody rb;
    protected UIManager uiManager;
    protected Animator animator;
    public static List<EnemyBase> allEnemies = new List<EnemyBase>(); // 管理所有敌人

    // ========== 初始状态缓存（关键！避免重置出错） ==========
    private Collider[] allColliders; // 缓存所有碰撞体（自身+子物体）
    private Vector3 initialLocalScale; // 初始缩放
    public float groundY = 0f; // 手动设置Ground的Y坐标（根据你的场景修改，比如Ground在Y=0就填0）

    private bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType type)
    {
        if (animator == null) return false;

        foreach (var param in animator.parameters)
        {
            if (param.type == type && param.name == paramName)
                return true;
        }
        return false;
    }


    // ========== 初始化（虚方法，子类可扩展） ==========
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        uiManager = FindObjectOfType<UIManager>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogError($"{gameObject.name} 缺少 Animator 组件！");
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) Debug.LogError("找不到 Player 对象（标签为 Player）！");

        // 缓存初始状态（只执行一次，避免后续修改影响）
        allColliders = GetComponentsInChildren<Collider>(); // 包括子物体碰撞体
        initialLocalScale = transform.localScale;

        // 关键：检查是否有Rigidbody，没有则自动添加（兜底）
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.LogWarning($"{gameObject.name} 缺少 Rigidbody，已自动添加！");
        }

        // 初始禁用对象（对象池创建时调用，避免一开始就激活）
        gameObject.SetActive(false);
    }

    // 启用时添加到敌人列表（生成时触发）
    protected virtual void OnEnable()
    {
        if (!allEnemies.Contains(this))
            allEnemies.Add(this);
    }

    // 禁用时从敌人列表移除（回收时触发）
    protected virtual void OnDisable()
    {
        if (allEnemies.Contains(this))
            allEnemies.Remove(this);
    }

    // ========== IPoolable 接口实现（对象池核心方法） ==========
    // ====================== 从对象池取出时 ======================
    public void OnSpawn()
    {
        isDead = false;
        isDestroyed = false;
        Health = maxHealth;

        // Transform 重置
        Vector3 pos = transform.position;
        if (pos.y < 0.5f) pos.y = 0.5f; // 你的地面高度根据情况调
        transform.position = pos;
        transform.rotation = Quaternion.identity;

        // Rigidbody 重置
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // Collider 重置
        if (allColliders != null)
        {
            foreach (var col in allColliders)
            {
                col.enabled = true;
            }
        }

        // Animator 安全播放 Idle
        if (animator != null && animator.layerCount > 0)
        {
            int idleHash = Animator.StringToHash("Idle");
            if (animator.HasState(0, idleHash))
                animator.Play(idleHash, 0, 0f);
        }

        lastAttackTime = 0f;
    }

    // ====================== 回收回对象池时 ======================
    public void OnDespawn()
    {
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }

        StopAllCoroutines();

        if (animator != null)
        {
            if (HasAnimatorParameter("TakeDamage", AnimatorControllerParameterType.Trigger))
                animator.ResetTrigger("TakeDamage");

            if (HasAnimatorParameter("Attack", AnimatorControllerParameterType.Trigger))
                animator.ResetTrigger("Attack");

            if (HasAnimatorParameter("Die", AnimatorControllerParameterType.Trigger))
                animator.ResetTrigger("Die");
        }

        if (allColliders != null)
        {
            foreach (var col in allColliders)
            {
                col.enabled = false;
            }
        }

        // Reset transform（防止 Collider 偏移）
        transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    // ========== 原有战斗逻辑（仅调整 Despawn 相关） ==========
    public override void TakeDamage(float amount)
    {
        if (isDestroyed) return;

        TriggerAnimation("TakeDamage");
        base.TakeDamage(amount); // 调用基类逻辑，统一生命值与掉落机制
    }

    protected override void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        isDead = true;

        // 禁用所有碰撞体（避免死亡后还被命中）
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        // 禁用刚体物理（避免死亡后还在移动）
        if (rb != null)
        {
            // 第一步：先清空速度（此时刚体还是非运动学，允许设置velocity）
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero; // 顺便清空角速度，避免旋转残留
            // 第二步：再把刚体设为运动学（顺序不能反！）
            rb.isKinematic = true;
        }

        // 播放死亡动画
        StartCoroutine(PlayDeathAnimAfterDelay(0.2f));

        // 启动协程等待动画完成再掉落和回收
        StartCoroutine(DelayedDeathRoutine(2f));
    }
    private IEnumerator PlayDeathAnimAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerAnimation("Die"); // 延迟后播放倒地动画
    }

    private IEnumerator DelayedDeathRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 掉落物品（继承自 DestructibleBase 的 Drop 方法）
        Drop();

        // 回收回对象池（关键：替换原有的 Destroy，调用对象池的 Despawn）
        ObjectPool.Instance.Despawn(gameObject);
    }

    // ========== 原有辅助方法（无修改） ==========
    protected virtual void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackRange)
            {
                TriggerAnimation("Attack");
                lastAttackTime = Time.time;
            }
        }
    }

    protected Vector3 CalculateAvoidance()
    {
        Vector3 avoidDir = Vector3.zero;
        foreach (var other in allEnemies)
        {
            if (other == this) continue;
            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < avoidRadius)
            {
                Vector3 pushDir = (transform.position - other.transform.position).normalized;
                avoidDir += pushDir * (avoidRadius - dist);
            }
        }
        return avoidDir;
    }

    protected virtual void TriggerAnimation(string triggerName)
    {
        animator?.SetTrigger(triggerName);
    }

    protected virtual void SetRunningAnimation(bool isRunning)
    {
        animator?.SetBool("IsRunning", isRunning);
    }

    // ========== 抽象方法（子类必须实现） ==========
    public abstract void DealDamage(); // 动画事件触发的伤害逻辑
}