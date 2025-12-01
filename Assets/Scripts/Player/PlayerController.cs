using UnityEngine;

// 玩家控制器：从 PlayerState 获取所有数值
public class PlayerController : PlayerBase
{
    private Vector3 lastValidShootDir = Vector3.forward;
    [Header("数值引用")]
    public PlayerState playerState;

    protected override void Awake()
    {
        base.Awake();

        if (playerState == null)
            playerState = GetComponent<PlayerState>();

        if (playerState == null)
            Debug.LogWarning("PlayerController: 未找到 PlayerState 组件！");
    }

    protected override void Update()
    {
        base.Update();
    }

    // ========= 移动逻辑 =========
    protected override void Move()
    {
        if (playerState == null) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 rawInput = new Vector3(x, 0f, z);

        // 输入检测
        Vector3 moveDir = rawInput.magnitude < inputThreshold
            ? Vector3.zero
            : rawInput.normalized;

        // 播放动画
        animator?.SetBool("isRunning", moveDir != Vector3.zero);

        // 执行移动（用 PlayerState.moveSpeed）
        if (moveDir != Vector3.zero)
        {
            float moveDistance = playerState.moveSpeed * Time.fixedDeltaTime; // 修改移速
            if (!Physics.Raycast(rb.position, moveDir, moveDistance + 0.05f))
            {
                rb.MovePosition(rb.position + moveDir * moveDistance);
            }
        }
    }

    // ========= 射击逻辑 =========
    protected override void Shoot()
    {
        if (playerState == null || bulletPrefab == null || firePoint == null) return;

        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + 1f / Mathf.Max(0.1f, playerState.attackSpeed); // 修改攻速

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            Vector3 directionToTarget = hit.point - firePoint.position;
            directionToTarget.y = 0f; // 确保方向在水平面上
            Vector3 shootDir;
            if (directionToTarget.sqrMagnitude < 50f)
            {
                // 3. 如果距离过近，使用玩家的朝向作为射击方向
                shootDir = model.transform.forward;
            }
            else
            {
                shootDir = directionToTarget.normalized;
            }

            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDir));
            Rigidbody bulletRb = bulletObj.GetComponent<Rigidbody>();
            if (bulletRb != null)
                bulletRb.velocity = shootDir * bulletSpeed+rb.velocity; // 考虑玩家移动速度

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
                bullet.damage = playerState.attack; // 修改攻击力
        }
    }

    // ========= 朝向逻辑 =========
    protected override void RotateTowardTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            Vector3 lookPos = hit.point - transform.position;
            lookPos.y = 0f;

            if (lookPos.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookPos);
                model.rotation = Quaternion.Slerp(model.rotation, targetRot, Time.deltaTime * 10f);
            }
        }
    }
}