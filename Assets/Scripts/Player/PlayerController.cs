using UnityEngine;

// 具体玩家控制器：实现基类的抽象方法（整合移动、射击、旋转）
public class PlayerController : PlayerBase
{
    [Header("数值引用")]
    public PlayerState playerState;

    // 重写初始化（可选：补充子类特有逻辑）
    protected override void Awake()
    {
        base.Awake(); // 先执行基类的初始化（获取组件）
                      // 这里可以添加子类特有的初始化（如额外组件）
        if (playerState == null)
            playerState = GetComponent<PlayerState>();

        if (playerState == null)
            Debug.LogWarning("PlayerController: 没找到 PlayerState！");
    }

    protected override void Update()
    {
        // 在更新前同步玩家属性
        SyncStatsFromState();

        base.Update(); // 保持基类的旋转、射击更新逻辑
    }

    private void SyncStatsFromState()
    {
        if (playerState == null) return;

        // 从 PlayerState 动态同步属性
        moveSpeed = playerState.moveSpeed;
        fireRate = 1f / Mathf.Max(0.1f, playerState.attackSpeed);// attackSpeed越大，攻速越快
    }

    // 实现移动逻辑（迁移自PlayerMovement）
    protected override void Move()
    {
        // 获取输入
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 rawInput = new Vector3(x, 0f, z);

        // 过滤微小输入（避免轻微摇杆/按键触发移动）
        Vector3 moveDir = rawInput.magnitude < inputThreshold
            ? Vector3.zero
            : rawInput.normalized;

        // 更新跑步动画
        animator?.SetBool("isRunning", moveDir != Vector3.zero);

        // 物理移动（避免穿墙）
        if (moveDir != Vector3.zero)
        {
            float moveDistance = moveSpeed * Time.fixedDeltaTime;
            // 射线检测：如果前方没有障碍物，才移动
            if (!Physics.Raycast(rb.position, moveDir, moveDistance + 0.05f))
            {
                rb.MovePosition(rb.position + moveDir * moveDistance);
            }
        }
    }

    // 实现射击逻辑（迁移自PlayerShoot）
    protected override void Shoot()
    {
        // 射线检测鼠标点击的地面位置（确定射击方向）
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            // 计算水平射击方向（忽略Y轴，避免子弹上天/入地）
            Vector3 shootDir = (hit.point - firePoint.position).normalized;
            shootDir.y = 0f;// 忽略y轴

            // 生成并发射子弹
            if (bulletPrefab != null && firePoint != null)
            {
                GameObject bulletObj = Instantiate(
                    bulletPrefab,
                    firePoint.position,
                    Quaternion.LookRotation(shootDir)
                );

                // 让子弹朝向射击方向
                bulletObj.transform.forward = shootDir;

                // 给子弹速度
                Rigidbody bulletRb = bulletObj.GetComponent<Rigidbody>();
                if (bulletRb != null)
                    bulletRb.velocity = shootDir * bulletSpeed;

                // 将玩家攻击力传入子弹
                Bullet bullet = bulletObj.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.damage = playerState.attack; // 攻击力传递
                }
            }
        }
    }

    // 实现朝向鼠标旋转（迁移自PlayerShoot）
    protected override void RotateTowardTarget()
    {
        // 射线检测鼠标在地面的位置
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            // 计算水平朝向（忽略Y轴，避免模型抬头/低头）
            Vector3 lookPos = hit.point - transform.position;
            lookPos.y = 0f;

            // 只有当目标位置有足够距离时，才旋转（避免抖动）
            if (lookPos.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookPos);
                // 平滑旋转到目标方向
                model.rotation = Quaternion.Slerp(model.rotation, targetRot, Time.deltaTime * 10f);
            }
        }
    }
}