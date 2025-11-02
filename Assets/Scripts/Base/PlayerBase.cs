using UnityEngine;

public abstract class PlayerBase : MonoBehaviour
{
    // ========== 公共属性（子类可调整） ==========
    [Header("移动设置")]
    public float moveSpeed = 20f;
    public float inputThreshold = 0.1f; // 输入阈值（过滤微小操作）

    [Header("射击设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 50f;
    public float fireRate = 0.2f;
    public LayerMask groundMask;

    // ========== 组件引用（保护级别，子类可访问） ==========
    protected Rigidbody rb;
    protected Animator animator;
    protected Transform model; // 角色模型（用于旋转）
    protected float nextFireTime; // 射击冷却计时

    // ========== 初始化（虚方法，子类可扩展） ==========
    protected virtual void Awake()
    {
        // 自动获取核心组件（子类可重写补充）
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (model == null) model = transform.GetChild(0); // 默认取第一个子物体作为模型

        // 检查必要组件
        if (rb == null) Debug.LogError("PlayerBase: 缺少Rigidbody组件！");
        if (animator == null) Debug.LogWarning("PlayerBase: 未指定Animator组件！");
    }

    // ========== 核心行为接口（抽象方法，子类必须实现） ==========
    protected abstract void Move(); // 移动逻辑
    protected abstract void Shoot(); // 射击逻辑
    protected abstract void RotateTowardTarget(); // 朝向目标旋转（如鼠标位置）

    // ========== 帧更新（统一调用子类实现） ==========
    protected virtual void Update()
    {
        RotateTowardTarget(); // 每帧更新朝向
        if (Time.time >= nextFireTime)
        {
            Shoot(); // 冷却结束后射击
            nextFireTime = Time.time + fireRate;
        }
    }

    protected virtual void FixedUpdate()
    {
        Move(); // 物理帧更新移动
    }
}