using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [HideInInspector] public float radius = 5f;     // 爆炸半径（由Skill传参）
    [HideInInspector] public float damage = 10f;    // 最终伤害（由Skill传参）
    [HideInInspector] public float lifetime = 3f;   // 存活时间（由Skill传参）

    [Header("可视化配置")]
    public Color explosionRangeColor = Color.red;
    public Color trajectoryColor = Color.blue;
    public float trajectoryWidth = 0.1f;
    public int maxTrajectoryPoints = 50;
    public GameObject explodeEffectPrefab;

    public delegate void ExplosionHandler(Vector3 position, float damage, float radius);
    public event ExplosionHandler OnExplode;

    private Rigidbody rb;
    private LineRenderer lineRenderer;  // 轨迹组件
    private Vector3[] trajectoryPoints;
    private int currentTrajectoryIndex = 0;
    private bool hasExploded = false;
    private float explodeGizmoDuration = 0.5f;
    private float explodeGizmoTimer = 0;

    private void Awake()
    {
        // 1. 确保Rigidbody存在
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("火球预制体缺少Rigidbody组件！");
            Destroy(gameObject);
            return;
        }

        // 核心修复：强制添加LineRenderer（如果没有），并确保不为null
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            Debug.LogWarning("火球预制体没有LineRenderer，已自动添加！");
        }

        // 2. 配置LineRenderer（避免材质缺失报错）
        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        }
        lineRenderer.startColor = trajectoryColor;
        lineRenderer.endColor = trajectoryColor;
        lineRenderer.startWidth = trajectoryWidth;
        lineRenderer.endWidth = trajectoryWidth;
        lineRenderer.positionCount = 0;

        // 3. 初始化轨迹点数组
        trajectoryPoints = new Vector3[maxTrajectoryPoints];
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
        // 绘制轨迹（先判断lineRenderer不为null）
        if (lineRenderer != null && !hasExploded && currentTrajectoryIndex < maxTrajectoryPoints)
        {
            trajectoryPoints[currentTrajectoryIndex] = transform.position;
            currentTrajectoryIndex++;
            lineRenderer.positionCount = currentTrajectoryIndex;
            lineRenderer.SetPositions(trajectoryPoints);
        }

        // 爆炸闪球计时器
        if (hasExploded && explodeGizmoTimer > 0)
        {
            explodeGizmoTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy"))
        {
            return;
        }

        Debug.Log($"火球命中敌人：{other.gameObject.name}（标签：{other.tag}）");
        OnExplode?.Invoke(transform.position, damage, radius);

        // 播放爆炸粒子（可选）
        if (explodeEffectPrefab != null)
        {
            Instantiate(
                explodeEffectPrefab,
                transform.position,   // 粒子位置=火球爆炸位置
                Quaternion.identity   // 粒子旋转=默认（可根据需求调整）
            );
        }

        hasExploded = true;
        explodeGizmoTimer = explodeGizmoDuration;
        Destroy(gameObject, 0.3f);

        // 核心修复：访问lineRenderer前先判断是否为null
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        GetComponent<Collider>().enabled = false;
    }

    private void OnDrawGizmos()
    {
        // 绘制爆炸范围
        Gizmos.color = explosionRangeColor;
        Gizmos.DrawWireSphere(transform.position, radius);

        // 绘制爆炸闪球
        if (hasExploded && explodeGizmoTimer > 0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, radius);
        }

        // 绘制简化轨迹（备用，避免LineRenderer失效）
        if (currentTrajectoryIndex > 1)
        {
            Gizmos.color = trajectoryColor;
            for (int i = 0; i < currentTrajectoryIndex - 1; i++)
            {
                Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
            }
        }
    }
}