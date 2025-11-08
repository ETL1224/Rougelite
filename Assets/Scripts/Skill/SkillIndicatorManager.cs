using UnityEngine;

public class SkillIndicatorManager : MonoBehaviour
{
    public static SkillIndicatorManager Instance;

    public GameObject indicatorPrefab;   // 圆圈或方向指示模型
    private GameObject currentIndicator;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowIndicator(Vector3 position, float radius)
    {
        if (indicatorPrefab == null) return;

        if (currentIndicator == null)
            currentIndicator = Instantiate(indicatorPrefab);

        currentIndicator.transform.position = position;
        currentIndicator.transform.localScale = new Vector3(radius * 2, 0.1f, radius * 2);
        currentIndicator.SetActive(true);
    }

    public void UpdateIndicator(Vector3 position, Vector3 forward)
    {
        if (currentIndicator == null) return;
        currentIndicator.transform.position = position;
        currentIndicator.transform.forward = forward;
    }

    public void HideIndicator()
    {
        if (currentIndicator != null)
            currentIndicator.SetActive(false);
    }
}
