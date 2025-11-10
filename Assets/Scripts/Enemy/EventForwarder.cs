using UnityEngine;

public class EventForwarder : MonoBehaviour
{
    private EnemyBase parentEnemyBase; // 依赖基类

    void Start()
    {
        parentEnemyBase = GetComponentInParent<EnemyBase>();
        if (parentEnemyBase == null)
            Debug.LogError("找不到父物体的EnemyBase组件！", this);
    }

    public void DealDamage()
    {
        parentEnemyBase?.DealDamage(); // 调用基类的抽象方法（子类实现）
    }
}