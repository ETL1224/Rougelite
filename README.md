# 开发日志
## 2025-10-21（Slaice）
- 基础移动功能，加入Rigidbody，挂载PlayerMovement脚本
- 基础UI，挂载UIManager脚本
- 相机跟随功能，使用Cinemachine

## 2025-10-23 (Slaice)
- 添加了Ore和OreDrop的prefab，并添加了矿石生成脚本OreGenerate、矿石挖掘脚本Ore以及矿石被拾取脚本OrePickedup
- 解决了Virtual Camera跟随玩家运动过程中的bug，修改了相关参数

## 2025-10-24（Slaice）
- 添加了子弹bullet、敌人enemy预制体以及激光raycast预制体（未实现）
- 添加了玩家移动射击功能，并且由鼠标控制攻击方向，挂载了PlayerShoot脚本以及Bullet属性脚本，实现了基础的受击逻辑提示
- 解决了玩家射线检测时摄像机与鼠标冲突的bug

## 2025-10-25（Slaice）
- 添加了敌人enemy的最基本逻辑（EnemyAI脚本），自动随机生成在地图四周（EnemyGenerate脚本）
- 实现enemy自动侦测玩家位置，自动朝玩家移动，解决了enemy与player在update时的冲突问题：将enemy的位移方式从rb改为transform.position
- 解决了enemy与ore碰撞的问题：直接取消layer的物理碰撞
- 解决了enemy会在玩家周围聚成一坨的问题：添加avoidRadius

## 2025-10-26（Slaice）
- 完善了enemy死亡销毁及掉落矿石机制，修改了EnemyAI
- 完善了ore摧毁后矿石掉落机制，并解决掉落数量以及拾取范围bug
- 解决了生成enemy因为朝向玩家移动导致的悬空bug
- 添加了随时间生成波数enemy的功能

## 2025-10-27（Slaice）
- 添加了enemy近战攻击player的功能，写在EnemyAI中
- 修复了enemy因为is Kinematic会与玩家重合的问题
- 添加了player阵亡时的UI界面，可以选择重新开始游戏

## 2025-10-28（Slaice）
- 去assert store寻找合适资源

## 2025-10-29（Slaice）
- 将找到的模型、贴图import到项目中
- 给ore和oredrop添加模型，调整色彩和大小
- 给enemy1添加模型，并引入动画系统，完成animator controller的连接，并添加Parameters
- 添加了模型始终朝向玩家的功能（EnemyModel脚本）：模型旋转
- 修改脚本（EnemyAI和EnemyGenerate）使其触发Parameters，调整各个动画状态的响应顺序，实现完整动画逻辑
- 修复DealDamage函数在Events无法被找到的问题：子物体没有挂载EnemyAI，添加了中间脚本EventForwarder
- 解决了enemy伤害出伤时间、伤害范围判定、动画切换迟钝等问题

## 2025-10-30（Slaice）
- 给player添加模型，并加入动画系统：实现移动、射击、阵亡效果
- 添加了Avatar Mask（UpperBodyMask），实现了动画的覆盖：实现移动时射击功能
- 修改了firePoint不随模型旋转的问题：将firepoint变成model子物体即可

## 2025-10-31（Slaice）
- 添加天空盒
- 添加子弹模型
- 添加了准心Crosshair及其UI，给UIManager添加了CursorManager脚本

## 2025-11-1（Slaice）
- 添加了Terrain Tools，制作了简略的地图场景，添加了TerrainSampleAssert，改变了参数和材质
- 添加了空气墙AirWalls，防止player越界，并加入了防穿墙的检测
- 添加了一些树

## 2025-11-2（Slaice）
- 修改了代码结构框架，将enemy、player、ore的代码转变为继承多态的格式，增加代码复用性
- 修复了攻击enemy一直掉落oredrop的问题：isDestroyed设置有误
- 修复了enemy不能销毁子弹的bug
- 将代码脚本改为DestructibleBase（可摧毁物体基本逻辑）、EnemyBase（敌人基本逻辑）、PlayerBase（玩家基本逻辑）、SpawnerBase（生成物体基本逻辑）等父类以及修改Enemy1AI（重写攻击方法以及实现伤害逻辑）、EnemyGenerate（敌人生成）、Ore（设置生命值，还可以添加更多属性）、OreGenerate（矿石生成）、PlayerController（实现移动逻辑、鼠标旋转以及射击逻辑）等子类脚本来适配

## 2025-11-3（Slaice）
- 添加了商店UI：ShopPanel，添加了攻击力、攻速、移速、生命值以及三个技能的按钮
- 添加了SkillBase（所有技能的父类）、ShopManager（与PlayerState配合使用，调控属性以及技能购买升级）、ShopUIManager（管理ShopUIManager的脚本）、SkillManager（管理技能的子类）相关物体以及脚本（注意ShopUIManager要挂载到Canvas上，否则按tab无反应）
- 添加了PlayerState脚本，挂载到Player上（存储玩家属性，便于后续的升级）
- 修复了UImanager数据与PlayerState不同步的bug
- 修复了enemy死后有碰撞体的bug：添加bool值isDead
- 解决了按钮无法点击的bug：漏了EventSystem组件绑定到canvas
- 解决了CursorManager错误绑定的问题，新建CursorManager空物体
- 修复了enemy播放死亡动画还朝向player的问题：添加了isDead

## 2025-11-4（Slaice）
- 将ShopUI的ore数量与UIManager进行绑定：给ShopManager引入UIManager，调用其UpdateOreUI方法
- 将PlayerState的数值变化应用到player（数值同步）：在PlayerController里增加一段数值同步逻辑，让它动态读取PlayerState
- PlayerController负责：从PlayerState读取实时属性；驱动移动、射击逻辑；响应输入。
- PlayerState负责：管理数值（生命、攻击、移速、矿石等）；接收ShopManager的升级变化。
- PlayerBase只保留共通逻辑（旋转、射击接口），完全独立于数值层。
- 将attack属性传给子弹bullet的伤害值：修改PlayerController的Shoot方法
- 解决了enemy无法旋转的bug：Inspector里的isDead误触为true了
- Bug未解决：子弹命中敌人时显示扣除n滴血，但是观感只有n - 1滴血
- 实现在商店消耗矿石升级属性的功能，升级后改变商店lv等级文本并且在游戏中生效：修改了ShopUIManager、ShopManager、PlayerState

## 2025-11-5（Slaice）
- 让UI的Slider血条实时反映PlayerState里的数值，使ShopUI里的升级生命值能在游戏UI里生效并修改UIManager，让其只显示数值
- 完善了属性，修改了PlayerState（让其管理所有属性数值）、PlayerBase（删除大部分属性交由PlayerState），添加了法强SkillPower与技能急速SkillHaste属性以及相关UI按钮
- 修改了ShopUIManager以及ShopManager来绑定新增属性,让所有属性能够通过商店购买并生效于游戏内（修改PlayerController来实现具体升级加成效果）以及相应等级文本变化
- 修改SkillBase（基本判断释放逻辑）与SkillManager（让Q/E/R三个技能槽分别对应独立的技能库）

## 2025-11-6（Slaice）
- 修改完善脚本逻辑：SkillBase 所有技能的抽象父类（定义了技能的基础属性和核心流程，子类（如FireballSkill）只需实现具体释放逻辑即可）；FireballSkill 火球的具体实现（继承自SkillBase）FireballProjectile	火球飞行体逻辑（飞行、碰撞、爆炸）；SkillManager 技能库管理（Q/E/R各自独立技能池），支持随机技能玩法（比如玩家升级时随机解锁Q槽技能）；PlayerState	玩家属性（法强、技能急速等）；PlayerSkillController 负责监听玩家键盘输入（Q/E/R），获取技能释放位置/方向，调用技能的释放逻辑，是玩家和技能系统之间的桥梁
- 实现完整商店UI中的技能内容，新增技能图标、名称、描述，实现购买逻辑并绑定到游戏中
- 完整流程：
- 场景 1：打开商店（Tab 键）
- 流程步骤：
- 玩家按 Tab 键 → ShopUIManager.Update() 监听到按键，调用 ToggleShop()；ToggleShop() 打开商店面板，让鼠标进入 UI 模式（cursorManager.EnterUIMode()）；调用 RefreshAllSkillPreviews() → 遍历 Q/E/R 槽位，每个槽位调用 RefreshSkillPreview()；RefreshSkillPreview() 向 ShopManager 要 “当前槽位的可用技能池”（shopManager.GetAvailableSkills(slotKey)）；ShopManager.GetAvailableSkills() 从 SkillManager 拿到对应槽位的总技能池，过滤掉 “已购买的技能”（purchasedSkills 字典存储），返回可用技能；ShopUIManager 从可用技能中随机选 1 个作为预览，更新 UI 文本（显示技能名称），并设置按钮是否可点击（无技能则置灰）；同时 ShopUIManager.Update() 实时刷新矿石数量（调用 shopManager.GetOreCount()，读取 PlayerState.ore）。
- 场景 2：技能购买（点击 Q/E/R 购买按钮）
- 流程步骤（最核心的闭环）：
- 玩家点击 “Q 槽技能购买” 按钮 → ShopUIManager.OnBuySkillClicked("Q") 被调用；拿到当前 Q 槽的预览技能（previewQ），调用 ShopManager.BuySkill("Q", previewQ)；ShopManager.BuySkill() 做多重校验：任意校验失败返回 false，UI 打印 “购买失败” 日志；校验通过 → 生成技能实例（关键步骤）：用 Instantiate(skillPrefab.gameObject) 生成技能预制体的运行时实例（比如 FireballSkill 实例）；从实例中获取 SkillBase 组件（确保是合法技能）；绑定技能到玩家：调用 playerSkillCtrl.AssignSkill("Q", newSkill) → 把技能实例绑定到 PlayerSkillController 的 skillQ 槽位；记录已购技能：把该技能预制体加入 purchasedSkills["Q"] 集合 → 后续 Q 槽商店不会再刷出该技能；
- 场景 3：技能释放（游戏中按 Q/E/R）
- 流程步骤（承接购买后的技能使用）：
- 玩家按 Q 键 → PlayerSkillController.HandleSkillInput() 监听到按键，调用 TryCast(skillQ)；检查 skillQ 是否为空（未购买技能则打印 “该技能槽未装备技能”）；计算技能释放方向：castPos = castPoint.position + castPoint.forward * 10f（沿释放点向前 10 米）；检查技能是否可释放：调用 skillQ.CanCast(playerState) → 按 “基础冷却 × (1 - 技能急速)” 计算有效冷却，判断当前时间是否超过冷却时间；冷却完毕 → 调用 skillQ.TryCast(castPos, castPoint, playerState)：记录当前释放时间（lastCastTime = Time.time）→ 触发冷却；执行 FireballSkill.Cast() 方法（具体技能的释放逻辑）；FireballSkill.Cast() 生成火球投射物：实例化 fireballPrefab（火球预制体），设置位置为释放点前方；给火球的 Rigidbody 赋值速度（沿释放方向飞行）；给火球添加 FireballProjectile 组件，传递伤害（基础伤害 × 玩家法强）、爆炸半径、存活时间；注册爆炸事件（proj.OnHitEnemy += HandleExplosion）；火球飞行：碰撞到物体（敌人 / 地形）或超时（lifetime=3f）→ 触发FireballProjectile.OnCollisionEnter()；调用 OnHitEnemy 事件，传递爆炸位置和伤害；爆炸伤害结算：FireballSkill.HandleExplosion() 被回调 → 用 Physics.OverlapSphere 检测爆炸范围内的敌人；遍历敌人，调用 enemy.TakeDamage(damage) → 敌人掉血；销毁火球投射物 → 技能释放流程结束。
