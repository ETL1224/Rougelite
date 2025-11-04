using UnityEngine;

[AddComponentMenu("Utils/ModelLookAt")]
public class EnemyModel : MonoBehaviour
{
    public float rotationSpeed = 15f; // 调快旋转速度，观察更明显
    public Vector3 eulerOffset = Vector3.zero; // 模型朝向偏移（如需要翻转填(0,180,0)）
    public bool useLocalRotation = false;

    private EnemyBase enemyBase;
    private Transform target; // 不再暴露给Inspector，避免手动绑定错误

    void Start()
    {
        // 1. 先获取父物体的EnemyBase（用于后续死亡状态判断）
        if (transform.parent != null)
        {
            enemyBase = transform.parent.GetComponent<EnemyBase>();
        }

        // 2. 核心：自动查找场景中Tag为「Player」的实际对象（忽略预制体）
        FindScenePlayer();

        // 3. 若没找到，打印警告（帮助排查）
        if (target == null)
        {
            Debug.LogWarning($"{gameObject.name}：场景中未找到Tag为「Player」的对象！");
        }
    }

    void LateUpdate()
    {
        // 死亡后停止旋转
        if (enemyBase != null && enemyBase.isDead)
        {
            return;
        }

        // 每帧检查target是否有效
        if (target == null)
        {
            FindScenePlayer();
            return;
        }

        // 计算方向（过滤Y轴）
        Vector3 dir = target.position - transform.position;
        dir.y = 0f; // 强制水平旋转

        // 距离过近跳过
        if (dir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // 计算目标旋转
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(eulerOffset);

        // 平滑旋转
        if (useLocalRotation)
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, rotationSpeed * Time.deltaTime);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

    }

    // 辅助方法：专门查找场景中激活的Player（忽略Project窗口的预制体）
    private void FindScenePlayer()
    {
        // GameObject.FindGameObjectWithTag 只会查找场景中激活的对象，不会找预制体
        GameObject scenePlayer = GameObject.FindGameObjectWithTag("Player");
        if (scenePlayer != null)
        {
            target = scenePlayer.transform;
            Debug.Log($"{gameObject.name} 自动绑定场景Player：{scenePlayer.name}");
        }
    }
}