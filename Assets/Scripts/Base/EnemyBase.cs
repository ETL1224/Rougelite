using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class EnemyBase : DestructibleBase
{
    // ========== 公共属性（子类可调整） ==========
    [Header("移动与检测")]
    public float moveSpeed = 10f;
    public float detectRange = 150f;
    public float avoidRadius = 2.5f;
    public bool isDead = false;

    [Header("战斗属性")]
    public float maxHealth = 10f;
    public float damage = 2f;
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    protected float lastAttackTime = 0f;

    public Transform player;

    // ========== 组件与全局管理 ==========
    protected Rigidbody rb;
    protected UIManager uiManager;
    protected Animator animator;
    protected static List<EnemyBase> allEnemies = new List<EnemyBase>(); // 管理所有敌人

    // ========== 初始化（虚方法，子类可扩展） ==========
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        uiManager = FindObjectOfType<UIManager>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogError("敌人缺少Animator组件！");
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) Debug.LogError("找不到Player对象！");
        Health = maxHealth;
    }

    protected virtual void OnEnable() => allEnemies.Add(this);
    protected virtual void OnDisable() => allEnemies.Remove(this);

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

        // 禁用碰撞体和刚体物理
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        if (rb != null) rb.isKinematic = true;

        // 播放死亡动画
        StartCoroutine(PlayDeathAnimAfterDelay(0.2f));

        // 启动协程等待动画完成再掉落和销毁
        StartCoroutine(DelayedDeathRoutine(2f)); 
    }

    private IEnumerator PlayDeathAnimAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerAnimation("Die"); // 延迟后再播放倒地动画
    }

    private IEnumerator DelayedDeathRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 掉落物品
        Drop();

        // 销毁敌人对象
        Destroy(gameObject);
    }

    // ========== 核心行为（虚方法/抽象方法，子类可重写） ==========
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

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
            }
        }
    }

    // ========== 抽象方法（子类必须实现） ==========
    public abstract void DealDamage(); // 动画事件触发的伤害逻辑
}