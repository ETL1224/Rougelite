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
            CheckBloodFrenzyHeal();
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Ground")) Destroy(gameObject);
    }

    // 新增：检测嗜血狂怒，自动吸血
    private void CheckBloodFrenzyHeal()
    {
        // 自动查找PlayerState（判断是否处于嗜血状态）
        PlayerState playerState = FindObjectOfType<PlayerState>();
        // 自动查找UIManager（调用Heal方法回血）
        UIManager uiManager = FindObjectOfType<UIManager>();

        // 条件：找到PlayerState+UIManager+处于嗜血狂怒状态
        if (playerState != null && uiManager != null && playerState.isBloodFrenzyActive)
        {
            // 吸血量 = 子弹伤害 × 吸血比例（PlayerState里的bloodSuckRate）
            float healAmount = damage * playerState.bloodSuckRate;
            // 调用UIManager回血，和救赎、嗜血狂怒的回血逻辑统一
            uiManager.Heal(healAmount);
            // 可选：打印日志，方便调试
            Debug.Log($"子弹吸血：{healAmount:F1}点HP（伤害{damage} × 吸血比例{playerState.bloodSuckRate * 100}%）");
        }
    }
}