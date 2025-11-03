using UnityEngine;

public class Enemy1AI : EnemyBase
{
    protected override void Awake()
    {
        base.Awake(); // 继承基类初始化逻辑
        maxHealth = 3f;
        Health = maxHealth;
    }

    void Update()
    {
        if (isDestroyed) return; // 死亡后不再执行任何AI逻辑
        if (player == null) return;

        Vector3 moveDir = Vector3.zero;
        float distance = Vector3.Distance(transform.position, player.position);

        // 1. 追踪玩家逻辑
        if (distance <= detectRange)
        {
            moveDir = (player.position - transform.position).normalized;
            SetRunningAnimation(true);
        }
        else
        {
            SetRunningAnimation(false);
        }

        // 2. 敌人避让
        moveDir += CalculateAvoidance();

        // 3. 移动
        if (distance >= attackRange - 0.1f && moveDir != Vector3.zero)
        {
            transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
        }

        // 4. 攻击逻辑
        Attack();
    }

    // 重写攻击方法（可选，扩展Enemy1特有逻辑）
    protected override void Attack()
    {
        base.Attack(); // 继承基类冷却和范围检查
        // 可添加音效、特效等
    }

    // 实现抽象方法：动画事件触发的伤害逻辑
    public override void DealDamage()
    {
        if (player == null || uiManager == null) return;
        if (isDead) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            uiManager.TakeDamage(damage);
            Debug.Log($"Enemy1攻击，造成{damage}点伤害！");
        }
    }
}