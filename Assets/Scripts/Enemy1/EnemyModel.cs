using UnityEngine;

[AddComponentMenu("Utils/ModelLookAt")]
public class EnemyModel : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 8f;
    public Vector3 eulerOffset = Vector3.zero;
    public bool useLocalRotation = false;

    private EnemyBase enemyBase;

    void Start()
    {
        if (target == null && transform.parent != null)
        {
            enemyBase = transform.parent.GetComponent<EnemyBase>(); // “¿¿µª˘¿‡
            if (enemyBase != null) target = enemyBase.player;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion want = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(eulerOffset);
        if (useLocalRotation)
            transform.localRotation = Quaternion.Slerp(transform.localRotation, want, rotationSpeed * Time.deltaTime);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, want, rotationSpeed * Time.deltaTime);
    }
}