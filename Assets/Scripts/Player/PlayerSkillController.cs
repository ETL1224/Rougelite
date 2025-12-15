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
            Debug.Log("����δװ��");
            return;
        }

        if (!skill.CanCast(playerState))
        {
            Debug.Log($"{skill.skillName} ��ȴ��");
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
                Debug.Log("δʵ�ֵļ�������");
                break;
        }
    }

    // 1. ��ʼ��׼������SkillIndicatorManager��ʾָʾ��
    void StartAiming(SkillBase skill)
    {
        currentAimingSkill = skill;

        float radius = skill.indicatorRadius > 0 ? skill.indicatorRadius : 2f;
        SkillCastType type = skill.castType;

        // ���ݼ������ͻ������ֶ��Զ�����ָʾ����ʽ
        SkillIndicatorManager.Instance.ShowIndicator(
                castPoint.position,
                radius,
                type,
                castPoint // �ؼ��������ͷŵ�
            );
    }

    // 2. ��׼�и��£�ͬ��ָʾ��λ��
    void HandleAimingMode()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            Vector3 pos = hit.point;
            // �ؼ�������ָʾ��λ��
            SkillIndicatorManager.Instance.UpdateIndicator(pos, castPoint.forward);

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 dir;

                if (currentAimingSkill.castType == SkillCastType.Direction)
                {
                    // ---- �޸����� ----
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

    // 3. ������׼������ָʾ�����滻ԭ����Destroy��
    void EndAiming()
    {
        // �ؼ������õ�������ָʾ�����������٣��������ܸ��ã�
        SkillIndicatorManager.Instance.HideIndicator();
        currentAimingSkill = null;
    }

    public void AssignSkill(string slotKey, SkillBase newSkill)
    {
        if (newSkill == null)
        {
            Debug.LogWarning($"��ͼ�󶨿ռ��ܵ���λ {slotKey}");
            return;
        }

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
                Debug.LogWarning($"δ֪���ܲ�: {slotKey}");
                return;
        }
        Debug.Log($"�Ѱ󶨼��� [{newSkill.skillName}] ������ {slotKey}");
    }

}
