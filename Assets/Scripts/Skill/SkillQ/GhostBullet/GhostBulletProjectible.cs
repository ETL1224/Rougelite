using UnityEngine;
using System.Collections.Generic;

public class GhostBulletProjectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public float lifetime;
    [HideInInspector] public float hitInterval;

    [Header("手动绑定特效（均为子弹子物体）")]
    [Tooltip("拖尾特效：直接拖Hierarchy中的子物体到这里")]
    public ParticleSystem trailEffect; // 拖尾特效（一直播放）
    public ParticleSystem hitEnemyEffect; // 受击特效（命中时启用）
    public Vector3 effectScale = new Vector3(0.3f, 0.3f, 0.3f);

    [Header("碰撞体配置")]
    public float colliderRadius = 1f;

    public delegate void HitEnemyHandler(EnemyBase enemy, float damage);
    public event HitEnemyHandler OnHitEnemy;

    private Rigidbody rb;
    private SphereCollider sphereCollider;
    private bool isFlying = true;
    private List<EnemyBase> hitEnemies = new List<EnemyBase>();
    private float lastHitTime = 0f;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            Debug.LogError("幽灵子弹预制体缺少SphereCollider！");
            Destroy(gameObject);
            return;
        }
        sphereCollider.isTrigger = true;
        sphereCollider.radius = colliderRadius;

        // 配置Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("幽灵子弹预制体缺少Rigidbody组件！");
            Destroy(gameObject);
            return;
        }
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // 初始化受击特效（默认禁用，记录初始位置）
        if (hitEnemyEffect != null)
        {
            hitEnemyEffect.gameObject.SetActive(false); // 默认禁用
            hitEnemyEffect.transform.localScale = effectScale * 2f; // 放大2倍，确保看得见

            // 强制设置受击特效为世界空间（移动到敌人位置后不跟随子弹）
            var hitMain = hitEnemyEffect.main;
            hitMain.simulationSpace = ParticleSystemSimulationSpace.World;
            hitMain.startLifetime = 1.2f; // 延长生命周期，避免一闪而过
        }
        else
        {
            Debug.LogError("未手动绑定HitEnemyEffect！请在Hierarchy添加子物体并赋值");
        }
    }

    private void Start()
    {
        // 初始化拖尾特效（一直播放）
        if (trailEffect != null)
        {
            trailEffect.transform.localPosition = Vector3.zero;
            trailEffect.transform.localRotation = Quaternion.identity;
            trailEffect.transform.localScale = effectScale;

            var main = trailEffect.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = 0.4f;
            main.startSpeed = 0f;
            main.startSize = 1f;

            trailEffect.Play();
        }
        else
        {
            Debug.LogError("未手动绑定TrailEffect！请在Hierarchy添加子物体并赋值");
        }

        // 超时销毁（连带子物体特效一起销毁）
        Destroy(gameObject, lifetime);

        // 设置飞行方向
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
    }

    private void FixedUpdate()
    {
        if (isFlying && rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFlying) return;

        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // 打印日志：确认命中逻辑走到了（运行时看Console）
                Debug.Log($"命中敌人：{enemy.gameObject.name}，准备播放受击特效");

                // 只判断是否已命中该敌人（去掉hitInterval，避免拦截特效）
                if (!hitEnemies.Contains(enemy))
                {
                    OnHitEnemy?.Invoke(enemy, damage);

                    // -------------- 核心修复：受击特效播放逻辑 --------------
                    if (hitEnemyEffect != null)
                    {
                        // 1. 移动特效到敌人上方（避免被模型挡住）
                        hitEnemyEffect.transform.position = enemy.transform.position + Vector3.up * 0.6f;

                        // 2. 启用特效（必须先启用游戏对象，再播放粒子）
                        hitEnemyEffect.gameObject.SetActive(true);

                        // 3. 重置粒子状态（避免残留，确保每次都从头播放）
                        hitEnemyEffect.Stop();
                        hitEnemyEffect.Clear(); // 清除残留粒子
                        hitEnemyEffect.Play(); // 手动触发播放

                        // 4. 延迟1.2秒后禁用（和startLifetime一致，确保播放完）
                        Invoke(nameof(DisableHitEffect), 1.2f);
                    }
                    // ---------------------------------------------------

                    hitEnemies.Add(enemy);
                    lastHitTime = Time.time;
                }
            }
            return;
        }

        if (other.CompareTag("Ground"))
        {
            StopFlying();
            return;
        }
    }

    // 单独的禁用方法（去掉重置位置，避免干扰）
    private void DisableHitEffect()
    {
        if (hitEnemyEffect != null && hitEnemyEffect.gameObject.activeSelf)
        {
            hitEnemyEffect.gameObject.SetActive(false);
        }
    }

    private void StopFlying()
    {
        isFlying = false;
        sphereCollider.enabled = false;

        if (trailEffect != null)
        {
            trailEffect.Stop();
        }

        if (hitEnemyEffect != null)
        {
            hitEnemyEffect.gameObject.SetActive(false);
        }

        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 0.8f, 0.3f);
        Gizmos.DrawSphere(transform.position, colliderRadius);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
    }
}