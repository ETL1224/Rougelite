using System.Collections;
using UnityEngine;

// 实现 IPoolable 接口，适配对象池（关键修改）
public class MagicOrb : MonoBehaviour, IPoolable
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

    // 缓存初始状态（避免重置出错）
    private Collider orbCollider;
    private bool initialRbKinematic;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        orbCollider = GetComponent<Collider>(); // 缓存自身碰撞体

        // 关键：初始化物理状态（法球不需要重力，避免掉地上）
        if (_rb != null)
        {
            initialRbKinematic = _rb.isKinematic;
            _rb.isKinematic = false; // 非运动学，允许物理碰撞
            _rb.useGravity = false; // 法球不受重力，避免下坠
        }

        if (_rb == null)
            Debug.LogError("法球缺少Rigidbody组件！", this);
        if (orbCollider == null)
            Debug.LogError("法球缺少Collider组件！", this);

        // 初始禁用（对象池创建时不激活）
        gameObject.SetActive(false);
    }

    // 【对象池生成时调用】重置所有状态（关键！）
    public void OnSpawn()
    {
        // 1. 重置核心参数（避免复用旧值）
        _speed = 0f;
        _damage = 0f;
        _target = null;
        _initialMoveDir = Vector3.zero;

        // 2. 重置物理状态
        if (_rb != null)
        {
            _rb.isKinematic = initialRbKinematic;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        // 3. 重置碰撞体（确保能触发碰撞）
        if (orbCollider != null)
        {
            orbCollider.enabled = true;
        }

        // 4. 重置Transform
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // 5. 停止所有残留协程（避免重复超时回收）
        StopAllCoroutines();

        Debug.Log("法球从对象池取出，状态重置完成");
    }

    // 【对象池回收时调用】清理残留状态（关键！）
    public void OnDespawn()
    {
        // 1. 停止协程（防止回收后还触发超时）
        StopAllCoroutines();

        // 2. 清理物理残留
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        // 3. 重置碰撞体（避免下次生成时碰撞失效）
        if (orbCollider != null)
        {
            orbCollider.enabled = false;
        }

        Debug.Log("法球回收回对象池，状态清理完成");
    }

    // 初始化法球属性（由巫师调用，在OnSpawn之后执行）
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

        // 强制设置初始速度（第一帧就沿直线飞）
        if (_rb != null)
        {
            _rb.velocity = _initialMoveDir * _speed;
        }

        // 法球朝向飞行方向（视觉统一）
        transform.rotation = Quaternion.LookRotation(_initialMoveDir);

        // 启动超时销毁协程
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
            Physics.IgnoreCollision(orbCollider, other); // 忽略敌人碰撞
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