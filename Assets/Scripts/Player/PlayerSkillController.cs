using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public PlayerState playerState;
    public Transform castPoint;
    public SkillBase skillQ, skillE, skillR;

    private SkillBase currentAimingSkill;
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        playerState ??= GetComponent<PlayerState>();
    }

    private void Update()
    {
        if (currentAimingSkill == null)
        {
            HandleSkillInput();
        }
        else
        {
            HandleAimingMode();
        }
    }

    private void HandleSkillInput()
    {

        // 背包、商店、暂停界面打开时不响应技能释放
        if (BackpackManager.isBackpackOpen) return;
        var shopUI = FindObjectOfType<ShopUIManager>();
        if (shopUI != null && shopUI.IsOpen) return;
        var pauseMgr = FindObjectOfType<PauseManager>();
        if (pauseMgr != null && pauseMgr.IsPaused) return;

        if (Input.GetKeyDown(KeyCode.Q))
            PrepareSkill(skillQ);
        if (Input.GetKeyDown(KeyCode.E))
            PrepareSkill(skillE);
        if (Input.GetKeyDown(KeyCode.R))
            PrepareSkill(skillR);
    }

    void PrepareSkill(SkillBase skill)
    {
        if (skill == null)
        {
            Debug.Log("技能未装备");
            return;
        }

        if (!skill.CanCast(playerState))
        {
            Debug.Log($"{skill.skillName} 冷却中");
            return;
        }

        switch (skill.castType)
        {
            case SkillCastType.Self:
                skill.TryCast(castPoint.position, castPoint, playerState);
                break;

            case SkillCastType.Direction:
            case SkillCastType.Ground:
                StartAiming(skill);
                break;

            default:
                Debug.Log("未实现的释放类型");
                break;
        }
    }

    // 1. 开始瞄准模式，通知SkillIndicatorManager显示指示器
    void StartAiming(SkillBase skill)
    {
        currentAimingSkill = skill;

        float radius = skill.indicatorRadius > 0 ? skill.indicatorRadius : 2f;
        SkillCastType type = skill.castType;

        // 根据技能类型和参数自动调整指示器显示方式
        SkillIndicatorManager.Instance.ShowIndicator(
                castPoint.position,
                radius,
                type,
                castPoint // 关键：传入释放点
            );
    }

    // 2. 瞄准模式中，同步指示器位置
    void HandleAimingMode()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            Vector3 pos = hit.point;
            // 关键：更新指示器位置
            SkillIndicatorManager.Instance.UpdateIndicator(pos, castPoint.forward);

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 dir;

                if (currentAimingSkill.castType == SkillCastType.Direction)
                {
                    // ---- 修改部分 ----
                    Vector3 flatTarget = new Vector3(pos.x, castPoint.position.y, pos.z);
                    dir = (flatTarget - castPoint.position).normalized;

                    currentAimingSkill.TryCast(castPoint.position, castPoint, playerState, dir);
                }
                else
                {
                    currentAimingSkill.TryCast(pos, castPoint, playerState);
                }
                EndAiming();
            }

            if (Input.GetMouseButtonDown(1))
                EndAiming();
        }
    }

    // 3. 结束瞄准模式，隐藏指示器（替换原有的Destroy）
    void EndAiming()
    {
        // 关键：调用单例隐藏指示器，避免重复创建销毁，性能更优
        SkillIndicatorManager.Instance.HideIndicator();
        currentAimingSkill = null;
    }

    public void AssignSkill(string slotKey, SkillBase newSkill)
    {
        switch (slotKey)
        {
            case "Q":
                skillQ = newSkill;
                break;
            case "E":
                skillE = newSkill;
                break;
            case "R":
                skillR = newSkill;
                break;
            default:
                Debug.LogWarning($"未知技能槽: {slotKey}");
                return;
        }

        if (newSkill == null)
        {
            Debug.Log($"技能槽位 {slotKey} 技能装备失败，已清空");
            return;
        }

        Debug.Log($"已装备技能 [{newSkill.skillName}] 到槽位 {slotKey}");
    }

}