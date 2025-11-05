using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    public float radius = 2f;     // 爆炸半径
    public float damage = 10f;    // 伤害
    public float lifetime = 3f;   // 存活时间

    public delegate void ExplosionHandler(Vector3 position, float damage);
    public event ExplosionHandler OnHitEnemy;  // 当火球命中敌人时触发

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 触发爆炸事件
        if (OnHitEnemy != null)
        {
            OnHitEnemy(transform.position, damage);
        }

        Destroy(gameObject);  // 销毁火球对象
    }
}
