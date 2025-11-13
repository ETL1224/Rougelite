// 接口：所有需要状态重置的池化对象（比如敌人）都要实现这个接口
public interface IPoolable
{
    void OnSpawn(); // 从对象池取出时调用（生成时重置状态）
    void OnDespawn(); // 回收回对象池时调用（回收时清理状态）
}