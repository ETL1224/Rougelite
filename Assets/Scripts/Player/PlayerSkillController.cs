using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    [Header("引用")]
    public PlayerState playerState;   // 玩家属性（包含法强、急速等）
    public Transform castPoint;       // 技能释放位置（比如玩家手前方）

    [Header("技能槽（由商店动态赋值）")]
    public SkillBase skillQ;
    public SkillBase skillE;
    public SkillBase skillR;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;

        // 如果没有指定 playerState，就自动寻找
        if (playerState == null)
            playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        HandleSkillInput();
    }

    private void HandleSkillInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            TryCast(skillQ);

        if (Input.GetKeyDown(KeyCode.E))
            TryCast(skillE);

        if (Input.GetKeyDown(KeyCode.R))
            TryCast(skillR);
    }

    private void TryCast(SkillBase skill)
    {
        if (skill == null)
        {
            Debug.Log("该技能槽未装备技能");
            return;
        }

        // 技能的释放方向（可以改为鼠标方向）
        Vector3 castPos = castPoint.position;
        Transform caster = castPoint;

        if (skill.CanCast(playerState))
        {
            skill.TryCast(castPos, caster, playerState);
            Debug.Log($"释放技能：{skill.skillName}");
        }
        else
        {
            Debug.Log($"{skill.skillName} 冷却中...（剩余：{skill.baseCooldown * (1 - playerState.skillHaste) - (Time.time - skill.lastCastTime):F1}秒）");
        }
    }

    /// <summary>
    /// 商店调用的接口：自动绑定技能
    /// </summary>
    public void AssignSkill(string slotKey, SkillBase newSkill)
    {
        if (newSkill == null)
        {
            Debug.LogWarning($"试图绑定空技能到槽位{slotKey}");
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
                Debug.LogWarning($"未知技能槽: {slotKey}");
                return;
        }

        Debug.Log($"已绑定技能 [{newSkill.skillName}] 到按键 {slotKey}");
    }
}
