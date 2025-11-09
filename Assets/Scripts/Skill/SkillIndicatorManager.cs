using UnityEngine;

public class SkillIndicatorManager : MonoBehaviour
{
    public static SkillIndicatorManager Instance { get; private set; }

    [Header("指示器Prefab")]
    public GameObject groundIndicatorPrefab;     // 圆圈范围
    public GameObject directionIndicatorPrefab;  // 箭头方向

    private Transform castPoint;
    private GameObject currentIndicator;
    private SkillCastType currentType = SkillCastType.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 显示指示器
    public void ShowIndicator(Vector3 position, float radius, SkillCastType type,Transform castPoint)
    {
        HideIndicator(); // 保证不重复

        currentType = type;
        GameObject prefab = null;
        this.castPoint = castPoint;

        switch (type)
        {
            case SkillCastType.Ground:
                prefab = groundIndicatorPrefab;
                break;
            case SkillCastType.Direction:
                prefab = directionIndicatorPrefab;
                break;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"未设置 {type} 类型的指示器Prefab！");
            return;
        }

        currentIndicator = Instantiate(prefab, position, Quaternion.identity);
        if (currentType == SkillCastType.Ground)
        {
            currentIndicator.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);
        }
    }


    // 更新位置与方向
    public void UpdateIndicator(Vector3 targetPos, Vector3 forward)
    {
        if (currentIndicator == null || castPoint == null) return;

        if (currentType == SkillCastType.Direction)
        {
            // 计算从释放点到目标的方向
            Vector3 direction = (targetPos - castPoint.position).normalized;
            // 箭头位置：离释放点6单位（避免贴脸）
            currentIndicator.transform.position = castPoint.position + direction * 6f;
            // 箭头朝向：指向目标
            currentIndicator.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            currentIndicator.transform.position = targetPos + Vector3.up * 0.05f;
        }
    }

    // 隐藏并销毁当前指示器
    public void HideIndicator()
    {
        if (currentIndicator != null)
            Destroy(currentIndicator);
        currentIndicator = null;
        currentType = SkillCastType.None;
    }
}
