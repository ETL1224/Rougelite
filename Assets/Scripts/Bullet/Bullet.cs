using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public float damage = 1f;

    void Start() => Destroy(gameObject, lifeTime);

    void OnTriggerEnter(Collider other)
    {
        // 处理所有可破坏物体（继承DestructibleBase）
        DestructibleBase destructible = other.GetComponent<DestructibleBase>();
        if (destructible != null)
        {
            destructible.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Ground")) Destroy(gameObject);
    }
}