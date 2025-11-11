using UnityEngine;

// 挂在模型子对象上，转发动画事件到根对象的Enemy4AI
public class OrbEventForwarder : MonoBehaviour
{
    private Enemy4AI _enemy4AI;

    void Awake()
    {
        // 自动向上查找所有父对象，直到找到Enemy4AI（不用管根对象叫什么）
        _enemy4AI = GetComponentInParent<Enemy4AI>();

        // 找不到就报错，帮你排查问题
        if (_enemy4AI == null)
            Debug.LogError("转发脚本没找到Enemy4AI！请检查：1.脚本挂在Enemy4的子对象上 2.Enemy4根对象挂载了Enemy4AI", this);
    }

    // 动画事件要绑定的方法
    public void OnSpawnMagicOrb()
    {
        // 转发给Enemy4AI的SpawnMagicOrb方法
        _enemy4AI?.SpawnMagicOrb();
        Debug.Log("动画事件触发，已转发给Enemy4AI");
    }
}