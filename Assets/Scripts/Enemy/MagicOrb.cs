using System.Collections;
using UnityEngine;

public class MagicOrb : MonoBehaviour
{
    private float _speed;
    private float _damage;
    private Transform _target; // 目标是玩家
    private Rigidbody _rb;
    private float _lifeTime = 5f; // 法球超时销毁（避免无限飞行）
    private Vector3 _initialMoveDir; // 存储初始飞行方向（核心）

    [Header("爆炸特效配置")]
    [SerializeField] private GameObject orbExplodeEffect;
    [SerializeField] private float effectDestroyDelay = 2f;

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

        // 强制计算初始方向（忽略Y轴，水平直线）
        if (_target != null)
        {
            _initialMoveDir = _target.position - transform.position;
            _initialMoveDir.y = 0f; // 锁定Y轴，不会上下追
            _initialMoveDir.Normalize(); // 确保方向不影响速度
            Debug.Log($"法球初始方向：{_initialMoveDir}（生成时朝向玩家）");
        }
        else
        {
            _initialMoveDir = transform.forward; // 无目标时沿自身朝向飞
            Debug.Log("法球未找到目标，沿自身朝向飞行");
        }

        // 强制重置状态+设置初始速度（第一帧就沿直线飞）
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.velocity = _initialMoveDir * _speed;
        }

        // 法球朝向飞行方向（视觉统一）
        transform.rotation = Quaternion.LookRotation(_initialMoveDir);

        // 启动超时销毁协程
        StopAllCoroutines();
        StartCoroutine(OrbLifeTimer());
    }

    private void FixedUpdate()
    {
        if (_rb == null || !gameObject.activeSelf) return;

        // 强制固定方向飞行（核心！不再更新目标位置）
        _rb.velocity = _initialMoveDir * _speed;
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
            PlayExplodeEffect();
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
    private void PlayExplodeEffect()
    {
        if (orbExplodeEffect == null) return;
        GameObject effect = Instantiate(orbExplodeEffect, transform.position, Quaternion.identity);
        Destroy(effect, effectDestroyDelay);
    }

    // 法球回收（回收到对象池）
    private void DespawnOrb()
    {
        ObjectPool.Instance.Despawn(gameObject);
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