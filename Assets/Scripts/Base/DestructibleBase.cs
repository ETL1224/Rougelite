using UnityEngine;

public abstract class DestructibleBase : MonoBehaviour
{
    [Header("可破坏物体基础属性")]
    private float _health = 10f; // 私有字段承载数据
    public virtual float Health  // 虚拟属性封装字段
    {
        get => _health;
        set => _health = value;
    }

    public GameObject dropPrefab;       // 死亡掉落物预制体
    protected bool isDestroyed = false; // 销毁标志（防止重复处理）

    // ========== 受击逻辑（虚方法，子类可扩展特效） ==========
    public virtual void TakeDamage(float damage)
    {
        if (isDestroyed) return; // 已销毁则跳过
        float healthBefore = Health;
        Health -= damage;
        Health = Mathf.Max(Health, 0f);
        if (Health <= 0f && healthBefore > 0f)
        {
            Die();
        }
    }

    // ========== 掉落逻辑（虚方法，子类可重写掉落规则） ==========
    protected virtual void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        Drop();
        Destroy(gameObject);
    }

    protected virtual void Drop()
    {
        if (dropPrefab == null) return;
        int dropCount = Random.Range(1, 4); // 随机掉落1~3个
        for (int i = 0; i < dropCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1.5f, 1.5f),
                -1.5f,
                Random.Range(-1.5f, 1.5f)
            );
            Instantiate(dropPrefab, transform.position + offset, Quaternion.identity);
        }
    }
}