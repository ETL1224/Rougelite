using System.Collections;
using UnityEngine;

public class Enemy4AI : EnemyBase
{
    [Header("巫师专属配置（远程法球）")]
    [Tooltip("巫师移速慢（远程不需要快）")]
    public float wizardMoveSpeed = 7f;
    [Tooltip("远程施法范围（比近战大，默认30f）")]
    public float castRange = 30f;
    [Tooltip("法球预制体（拖入巫师的魔法球预制体）")]
    public GameObject magicOrbPrefab;
    [Tooltip("法球生成位置（巫师手部/法杖尖端，拖入子对象）")]
    public Transform orbSpawnPoint;
    [Tooltip("法球飞行速度")]
    public float orbSpeed = 15f;
    [Tooltip("施法冷却（远程攻击冷却长，默认2.5f）")]
    public float castCooldown = 2.5f;
    [Tooltip("巫师血量（中等，默认15f）")]
    public float wizardMaxHealth = 15f;
    [Tooltip("法球伤害（远程单次伤害，默认4f）")]
    public float orbDamage = 4f;

    protected override void Awake()
    {
        // 1. 先覆盖巫师专属属性（远程特性）
        moveSpeed = wizardMoveSpeed;
        maxHealth = wizardMaxHealth;
        attackRange = castRange; // 施法范围=攻击范围（复用基类攻击逻辑）
        attackCooldown = castCooldown;
        damage = orbDamage; // 法球伤害关联到基类damage（方便调试）

        // 2. 再调用基类初始化（绑定玩家、组件等）
        base.Awake();

        // 3. 校验关键配置（避免漏配报错）
        if (magicOrbPrefab == null)
            Debug.LogError($"[{gameObject.name}] 未配置法球预制体！", this);
        if (orbSpawnPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 未配置法球生成点，默认用自身位置", this);
            orbSpawnPoint = transform;
        }
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

        // 2. 远程AI逻辑：仅在「施法范围外」移动，进入范围后停止并面向玩家
        if (horizontalDistance > castRange)
        {
            moveDir = (playerFlatPos - enemyFlatPos).normalized;
            SetRunningAnimation(true); // 移动时播放奔跑动画
        }
        else
        {
            SetRunningAnimation(false); // 进入施法范围，停止移动
            // 持续面向玩家（准备施法）
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(player.position.x, transform.position.y, player.position.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        }

        // 3. 避让逻辑（巫师体积小，避让半径略小）
        moveDir += CalculateAvoidance() * 0.4f;

        // 4. 移动（仅施法范围外才移动，避免贴脸）
        if (moveDir.magnitude > 0.1f && horizontalDistance > castRange)
        {
            transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
        }

        // 5. 施法逻辑（进入范围且冷却结束）
        if (horizontalDistance <= castRange)
        {
            Attack(); // 重写Attack为「施法」
        }
    }

    // 重写Attack：播放施法动画，动画事件触发生成法球
    protected override void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 统一用「水平距离」判断（忽略Y轴，避免高度差导致误判）
            Vector3 enemyFlatPos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 playerFlatPos = new Vector3(player.position.x, 0, player.position.z);
            float horizontalDistance = Vector3.Distance(enemyFlatPos, playerFlatPos);

            if (horizontalDistance <= castRange)
            {
                Debug.Log($"[{gameObject.name}] 触发施法动画！水平距离：{horizontalDistance:F1}");
                TriggerAnimation("Cast");
                lastAttackTime = Time.time;
            }
        }
    }

    // 动画事件触发：生成法球（绑定到施法动画的「释放帧」）
    public void SpawnMagicOrb()
    {
        if (magicOrbPrefab == null || player == null || isDead || isDestroyed) return;

        // 生成法球（用对象池复用，性能更优）
        GameObject orbInstance = ObjectPool.Instance.SpawnFromPool("MagicOrb", orbSpawnPoint.position, orbSpawnPoint.rotation);
        if (orbInstance == null)
        {
            orbInstance = Instantiate(magicOrbPrefab, orbSpawnPoint.position, orbSpawnPoint.rotation);
            Debug.Log("法球对象池无闲置，新实例化一个");
        }

        // 初始化法球（设置速度、伤害、目标）
        MagicOrb orb = orbInstance.GetComponent<MagicOrb>();
        if (orb != null)
        {
            orb.Initialize(orbSpeed, orbDamage, player);
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] 法球预制体缺少MagicOrb脚本！", orbInstance);
            Destroy(orbInstance);
        }
    }
    public override void TakeDamage(float amount)
    {
        if (isDestroyed || isDead) return;

        // 触发受击动画（动画控制器中已配置"TakeDamage"触发参数）
        TriggerAnimation("TakeDamage");

        base.TakeDamage(amount); // 调用基类扣血逻辑（核心：不删，确保掉血）
    }

    // 抽象方法实现（巫师用不到近战DealDamage，空实现即可）
    public override void DealDamage()
    {

    }
}