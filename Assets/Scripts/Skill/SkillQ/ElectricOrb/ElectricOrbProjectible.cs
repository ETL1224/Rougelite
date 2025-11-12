using UnityEngine;

public class ElectricOrbProjectile : MonoBehaviour
{
    [HideInInspector] public float damage; // 伤害（由Skill传参）
    [HideInInspector] public float speed; // 速度（由Skill传参）
    [HideInInspector] public float radius; // 爆炸范围（由Skill传参）
    [HideInInspector] public float lifetime; // 存活时间（由Skill传参）
    [HideInInspector] public float knockbackForce; // 击退力度（由Skill传参）

    [Header("特效配置")]
    public GameObject trailEffect; // 电光轨迹特效
    public GameObject explodeEffectPrefab; // 爆炸特效
    public Color explosionRangeColor = Color.blue; // 爆炸范围Gizmos颜色

    // 爆炸回调（通知Skill类处理范围伤害+击退）
    public delegate void ExplosionHandler(Vector3 position, float damage, float radius, float knockbackForce);
    public event ExplosionHandler OnExplode;

    private Rigidbody rb;
    private bool hasExploded = false;
    private float explodeGizmoDuration = 0.3f;
    private float explodeGizmoTimer = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("电球预制体缺少Rigidbody组件！");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 启动时播放轨迹特效
        if (trailEffect != null)
            Instantiate(trailEffect, transform.position, Quaternion.identity, transform);

        // 超时自动销毁
        Destroy(gameObject, lifetime);

        // 配置Rigidbody（无重力、中速飞行）
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = transform.forward * speed;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void Update()
    {
        // 爆炸范围Gizmos计时器
        if (hasExploded && explodeGizmoTimer > 0)
        {
            explodeGizmoTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        // 命中敌人/地面/障碍物：触发爆炸
        if (other.CompareTag("Enemy") || other.CompareTag("Ground"))
        {
            // 通知Skill类处理范围伤害+击退
            OnExplode?.Invoke(transform.position, damage, radius, knockbackForce);
            // 播放爆炸特效
            if (explodeEffectPrefab != null)
                Instantiate(explodeEffectPrefab, transform.position, Quaternion.identity);

            // 标记爆炸状态
            hasExploded = true;
            explodeGizmoTimer = explodeGizmoDuration;
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;

            Destroy(gameObject, 0.3f);
        }
    }

    // Gizmos：绘制爆炸范围（调试用）
    private void OnDrawGizmos()
    {
        // 常驻：爆炸范围蓝色虚线球
        Gizmos.color = explosionRangeColor;
        Gizmos.DrawWireSphere(transform.position, radius);

        // 爆炸瞬间：蓝色实线球（可视化范围）
        if (hasExploded && explodeGizmoTimer > 0)
        {
            Gizmos.color = new Color(0, 0.8f, 1f, 0.5f); // 半透明蓝色
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}