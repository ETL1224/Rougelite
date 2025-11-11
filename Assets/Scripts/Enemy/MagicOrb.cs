using System.Collections;
using UnityEngine;

public class MagicOrb : MonoBehaviour
{
    private float _speed;
    private float _damage;
    private Transform _target; // 目标是玩家
    private Rigidbody _rb;
    private float _lifeTime = 5f; // 法球超时销毁（避免无限飞行）

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            Debug.LogError("法球缺少Rigidbody组件！", this);
    }

    // 初始化法球属性（由巫师调用）
    public void Initialize(float speed, float damage, Transform target)
    {
        _speed = speed;
        _damage = damage;
        _target = target;

        // 重置法球状态
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        // 启动超时销毁协程
        StopAllCoroutines();
        StartCoroutine(OrbLifeTimer());
    }

    private void FixedUpdate()
    {
        if (_target == null || _rb == null || !gameObject.activeSelf) return;

        // 法球朝向目标飞行（平滑追踪，不是瞬移）
        Vector3 dirToTarget = (_target.position - transform.position).normalized;
        _rb.velocity = dirToTarget * _speed;

        // 法球朝向飞行方向
        if (dirToTarget.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(dirToTarget);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 命中玩家：造成伤害
        if (other.CompareTag("Player"))
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.TakeDamage(_damage);
                Debug.Log($"魔法法球命中！造成{_damage}点伤害");
            }
            DespawnOrb();
            return;
        }

        // 命中地面：销毁
        if (other.CompareTag("Ground"))
        {
            DespawnOrb();
            return;
        }

        // 命中敌人：不造成伤害（避免误伤队友）
        if (other.CompareTag("Enemy"))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), other); // 忽略敌人碰撞
        }
    }

    // 法球回收（回收到对象池）
    private void DespawnOrb()
    {
        ObjectPool.Instance.Despawn(gameObject);
        // 可选：播放法球爆炸特效
        // Instantiate(orbExplodeEffect, transform.position, Quaternion.identity);
    }

    // 法球超时销毁（防止卡场景）
    private IEnumerator OrbLifeTimer()
    {
        yield return new WaitForSeconds(_lifeTime);
        if (gameObject.activeSelf)
        {
            DespawnOrb();
            Debug.Log("魔法法球超时回收");
        }
    }
}