using UnityEngine;

public abstract class PlayerBase : MonoBehaviour
{
    [Header("射击设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 50f;

    [Header("通用组件")]
    protected Rigidbody rb;
    protected Animator animator;
    protected Transform model;

    [Header("移动设置")]
    public float inputThreshold = 0.1f;
    public LayerMask groundMask;

    protected float nextFireTime;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        if (transform.childCount > 0) model = transform.GetChild(0);
    }

    protected virtual void Update()
    {
        RotateTowardTarget();
        Shoot();
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    protected abstract void Move();
    protected abstract void Shoot();
    protected abstract void RotateTowardTarget();
}
