using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [HideInInspector] public float radius = 5f;     // 爆炸半径（由Skill传参）
    [HideInInspector] public float damage = 10f;    // 最终伤害（由Skill传参）
    [HideInInspector] public float lifetime = 3f;   // 存活时间（由Skill传参）

    [Header("可视化配置")]
    public Color explosionRangeColor = Color.red;   // 只保留爆炸范围颜色（可选）
    public GameObject explodeEffectPrefab;

    public delegate void ExplosionHandler(Vector3 position, float damage, float radius);
    public event ExplosionHandler OnExplode;

    private Rigidbody rb;
    private bool hasExploded = false;
    private float explodeGizmoDuration = 0.5f;
    private float explodeGizmoTimer = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("火球预制体缺少Rigidbody组件！");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
        if (rb != null)
        {
            rb.useGravity = false;
        }
    }

    private void Update()
    {
        if (hasExploded && explodeGizmoTimer > 0)
        {
            explodeGizmoTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return; // 防止重复触发

        // 命中敌人
        if (other.CompareTag("Enemy"))
        {
            // 调用事件，让技能处理范围伤害
            OnExplode?.Invoke(transform.position, damage, radius);

            // 播放爆炸粒子
            if (explodeEffectPrefab != null)
                Instantiate(explodeEffectPrefab, transform.position, Quaternion.identity);

            hasExploded = true;
            explodeGizmoTimer = explodeGizmoDuration;
            GetComponent<Collider>().enabled = false;

            Destroy(gameObject, 0.3f);
        }
    }

    private void OnDrawGizmos()
    {
        // 1. 保留爆炸范围（红色虚线球）
        Gizmos.color = explosionRangeColor;
        Gizmos.DrawWireSphere(transform.position, radius);

        // 2. 保留爆炸瞬间闪球（蓝色实线球）
        if (hasExploded && explodeGizmoTimer > 0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}