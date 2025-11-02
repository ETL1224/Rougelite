public class Ore : DestructibleBase
{
    // 覆盖基类默认值：矿石需要3次攻击（假设子弹伤害1，health=3）
    public override float Health{ get; set; } = 5f;

    // （可选）重写掉落逻辑，实现矿石特有掉落（如不同物品）
    // protected override void Drop() { ... }
}