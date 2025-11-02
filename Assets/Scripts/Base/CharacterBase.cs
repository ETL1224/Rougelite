using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    [Header("角色属性")]
    public float health = 10f;
    public float moveSpeed = 5f;

    protected Animator animator;

    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // 扣血
    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        animator?.SetTrigger("TakeDamage");

        if (health <= 0f)
            Die();
    }

    // 死亡（虚方法，让子类扩展）
    protected virtual void Die()
    {
        animator?.SetTrigger("Die");
        Destroy(gameObject, 1f); // 延迟销毁（给动画时间）
    }
}
