using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.ParticleSystem;

public class GhostBulletProjectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public float lifetime;
    [HideInInspector] public float hitInterval;

    [Header("手动绑定/赋值特效")]
    public ParticleSystem trailEffect; // 拖尾特效（仍手动绑定为子弹子物体）
    [Tooltip("受击特效预制体：直接拖入Project窗口的特效预制体，不用绑定子物体")]
    public GameObject hitEnemyEffectPrefab; // 受击特效预制体（代码实例化）
    public Vector3 effectScale = new Vector3(1f, 1f, 1f);

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

        // 检查受击特效预制体是否赋值（代码实例化核心）
        if (hitEnemyEffectPrefab == null)
        {
            Debug.LogError("未赋值HitEnemyEffectPrefab！请拖入受击特效预制体");
        }
    }

    private void Start()
    {
        // 初始化拖尾特效（仍手动绑定子物体，保持跟随）
        if (trailEffect != null)
        {
            trailEffect.transform.localPosition = Vector3.zero;
            trailEffect.transform.localRotation = Quaternion.identity;
            trailEffect.transform.localScale = effectScale;

            var main = trailEffect.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = new MinMaxCurve(0.4f);
            main.startSpeed = new MinMaxCurve(0f);
            main.startSize = new MinMaxCurve(1f);

            trailEffect.Play();
        }
        else
        {
            Debug.LogError("未手动绑定TrailEffect！请在Hierarchy添加子物体并赋值");
        }

        // 超时销毁
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
                Debug.Log($"命中敌人：{enemy.gameObject.name}，准备播放受击特效");

                if (!hitEnemies.Contains(enemy))
                {
                    OnHitEnemy?.Invoke(enemy, damage);

                    // -------------- 核心：代码触发受击特效（实例化+强制播放） --------------
                    if (hitEnemyEffectPrefab != null)
                    {
                        // 1. 生成位置：敌人上方0.6f，避免被模型挡住
                        Vector3 spawnPos = enemy.transform.position + Vector3.up * 0.6f;
                        // 2. 实例化特效（世界空间，不跟随任何物体）
                        GameObject hitEffect = Instantiate(hitEnemyEffectPrefab, spawnPos, Quaternion.Euler(90f, 0f, 0f));
                        // 3. 缩放特效（确保可见）
                        hitEffect.transform.localScale = effectScale * 10f;

                        // 4. 强制配置粒子系统（关键！确保100%播放）
                        ParticleSystem ps = hitEffect.GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            var main = ps.main;
                            main.simulationSpace = ParticleSystemSimulationSpace.World; // 世界空间，固定在生成位置
                            main.startLifetime = 1.2f; // 特效持续时间
                            main.startSpeed = 1.5f; // 粒子扩散速度
                            main.startSize = 1.5f; // 粒子大小

                            // 强制播放（不管预制体是否勾选Play On Awake）
                            ps.Stop();
                            ps.Clear();
                            ps.Play();

                            // 5. 延迟销毁（和startLifetime一致，确保播放完）
                            Destroy(hitEffect, main.startLifetime.constant);
                        }
                        else
                        {
                            // 若特效不是ParticleSystem（比如动画），直接延迟销毁
                            Destroy(hitEffect, 1.2f);
                        }
                    }
                    else
                    {
                        Debug.LogError("hitEnemyEffectPrefab未赋值！无法播放受击特效");
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

    private void StopFlying()
    {
        isFlying = false;
        sphereCollider.enabled = false;

        if (trailEffect != null)
        {
            trailEffect.Stop();
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