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
- 场景1：打开商店（Tab键）
- 流程步骤：
- 玩家按Tab键 → ShopUIManager.Update()监听到按键，调用ToggleShop()；调用 RefreshAllSkillPreviews() → 遍历Q/E/R槽位，每个槽位调用RefreshSkillPreview()向ShopManager要“当前槽位的可用技能池”（shopManager.GetAvailableSkills(slotKey)）；ShopManager.GetAvailableSkills()从SkillManager拿到对应槽位的总技能池，过滤掉已购买的技能（purchasedSkills字典存储），返回可用技能（ShopUIManager从可用技能中随机选1个作为预览），更新 UI 文本（显示技能名称），并设置按钮是否可点击（无技能则置灰）
- 场景2：技能购买（点击Q/E/R购买按钮）
- 玩家点击技能购买按钮 → ShopUIManager.OnBuySkillClicked()被调用；拿到当前槽的预览技能，调用ShopManager.BuySkill()；生成技能实例：用 Instantiate(skillPrefab.gameObject)生成技能预制体的运行时实例（比如 FireballSkill 实例）；从实例中获取SkillBase组件；绑定技能到玩家：调用playerSkillCtrl.AssignSkill("Q", newSkill) → 把技能实例绑定到PlayerSkillController的skillQ槽位；记录已购技能：把该技能预制体加入purchasedSkills["Q"] 集合 → 后续Q槽商店不会再刷出该技能；
- 场景3：技能释放（游戏中按Q/E/R）
- 玩家按Q键 → PlayerSkillController.HandleSkillInput()监听到按键，调用TryCast(skillQ)；计算技能释放方向：castPos = castPoint.position + castPoint.forward * 10f（沿释放点向前 10 米）；检查技能是否可释放：调用skillQ.CanCast(playerState) → 按 “基础冷却 × (1 - 技能急速)” 计算有效冷却，判断当前时间是否超过冷却时间；冷却完毕 → 调用skillQ.TryCast(castPos, castPoint, playerState)：记录当前释放时间（lastCastTime = Time.time）→ 触发冷却；执行FireballSkill.Cast()方法（具体技能的释放逻辑）；FireballSkill.Cast()生成火球投射物：实例化fireballPrefab（火球预制体），设置位置为释放点前方；给火球的Rigidbody赋值速度（沿释放方向飞行）；给火球添加FireballProjectile组件，传递伤害（基础伤害 × 玩家法强）、爆炸半径、存活时间；注册爆炸事件（proj.OnHitEnemy += HandleExplosion）；火球飞行：碰撞到物体（敌人/地形）或超时（lifetime=3f）→ 触发FireballProjectile.OnCollisionEnter()；调用OnHitEnemy事件，传递爆炸位置和伤害；爆炸伤害结算：FireballSkill.HandleExplosion()被回调 → 用Physics.OverlapSphere检测爆炸范围内的敌人；遍历敌人，调用enemy.TakeDamage(damage) → 敌人掉血；销毁火球投射物 → 技能释放流程结束。

## 2025-11-7（Slaice）
- 解决火球不发碰撞检测的问题，改成trigger触发（伤害逻辑是用isTrigger判断，所以用Collider无效）,修改OnCollisionEnter为OnTriggerEnter
- 添加CastPoint,作为技能释放点
- 修改火球radius，添加特效（火球的整个粒子特效系统以及拖尾和爆炸效果：FireExplosion、FireTail），完成第一个Q技能：豪火球之术
- 锁60帧，去除阴影，稍微优化了一下性能（可以从enemy生成和模型再优化一下）

## 2025-11-8（Slaice）
- 完善火球术特效，把“即时释放技能（Q 键立刻放）”改成“按下 Q → 显示技能范围提示 → 鼠标移动选择方向 → 左键确认释放”的完整技能释放流程
- 修改SkillIndicatorManager管理技能预瞄的可视化；PlayerSkillController控制技能输入状态流转（Idle / Aiming / Casting）
- 修改ore掉落矿石数量
- 构思更多可能的技能：
- 一(Q):幽灵子弹、水枪、电球、豪火球之术、盛炎漩涡、生生流转、初烈风斩（都是对周围小范围敌人造成斩击伤害）
- 二(E):救赎/治疗术（回复）、嗜血狂怒（吸血+攻速）、光之守护（无敌）、万向天引（吸收范围内矿石）、正常操作（炸弹猫二技能，大幅提升移速）
- 三(R):科学的地狱火炮(爆弹R)、剧毒踪迹（炼金Q）、灵魂交换（消耗自身最大生命值的20%，接下来10s内攻击力“大幅”增加）、火之神神乐圆舞（对周围敌人造成大规模斩击伤害）
- 解决命中enemy没有伤害的bug：漏了出伤逻辑，添加委托和事件：public delegate void ExplosionHandler(Vector3 position, float damage, float radius);public event ExplosionHandler OnExplode;

## 2025-11-9（Slaice）
- 解决了方向技能释放角度不水平的bug，修改HandleAimingMode()逻辑
- 添加了技能type为direction时的指示器，提升了技能释放的解耦性，解决技能释放无法自动取消以及显示位置异常的bug
- 稍微修改了指示器的材质
- 添加对象池，将enemy1和ore放入池中，添加ObjectPool脚本
- 从Assert Store中引入石头人、外星小狗、外星法术、龙等model（包含动画）

## 2025-11-10（Slaice）
- 添加了更多的enemy类型：狼、石头人，添加了完整的动画系统，添加相关EnemyAI以及EnemyGenerate脚本，修改相应数值，比如：生成时间、数量、伤害、移速、攻击范围等
- 解决enemy2无法一直保持run，一直是idle但是又能切换到attack和die的bug：没有检查动画是否loop
- 解决下一波敌人生成时上一波直接被重置了，而且一动不动的bug：使用usedPositions.Clear()（清空上一波的生成位置记录，下一波可在新位置生成，不会和旧敌人重叠），对象池的 Size（初始容量）小于单波生成数量或多波累计的闲置对象需求，导致新波次生成时，对象池里没有足够的闲置敌人，只能去复用还在活跃的旧敌人—— 这就造成了旧敌人 “突然消失”（被强制重置位置到新生成点），且新敌人可能因状态残留（比如还带着旧的运动状态、血量）而异常
- 解决没有takedamage动画的问题：直接重载了TakeDamage函数
- 解决enemy攻击player没伤害的bug：忘记了给动画的Event加上DealDamage函数
- 解决了改了enemy3的生命值，但是还是跟enemy1生命值一样的bug：要先赋值再调用基类Awake
- 解决了enemy碰撞体没有作用，攻击时经常跟玩家穿模的bug：和Enemy1一样，让石头人只有在 “攻击范围外” 时才移动，一旦靠近玩家（进入攻击范围），就停止位移，就不会不会穿模
- 解决了子弹伤害问题（怀疑受击重复触发），攻击ore伤害是对的的bug：代码里存在双重TakeDamage调用—— 子弹的OnTriggerEnter和敌人的OnTriggerEnter会同时触发扣血，一次攻击实际扣2f

## 2025-11-11（Slaice）
- 添加wizard（远程Enemy），添加完整model以及animator系统，添加Enemy4AI以及EnemyGenerate脚本
- 添加MagicOrb（enemy4子弹）以及对应脚本，解决MagicOrb不生成的bug:Enemy4的Cast动画绑定SpawnMagicOrb时，无法直接绑定根物体，添加了一个帮助绑定的脚本OnSpawnMagicOrb
- wizard动画以及受击问题 1.wizard被子弹命中一次后会一直播放takedamage动画：动画默认设为循环 2.wizard进入攻击范围后不会播放attack动画：参数错了是Cast 3.player在wizard的攻击范围内就会一直是idle，不会播放cast：应该时idle连接到cast而不是run连接到cast，因为进入范围内isRunning会被设为false 4.wizard被攻击后会触发一次takedamage后会一直保持idle状态:idle被设定成loop了
- 优化性能，可以考虑改变生成时间和数量 1.修改动画系统Animator 2.修改渲染设置Skinned Mesh Renderer 3.修改材质和纹理 勾选Enable GPU Instancing

## 2025-11-12（Slaice）
- 修改生成时间以及游戏时间:骷髅0s，石头人60s，狼120s，法师180s
- 解决wizard子弹锁头bug以及添加特效：让magicorb不每帧更新，添加PlayExplodeEffect函数
- 解决生成数量问题，统一加入bool hasStartedSpawning
- 添加两个新的Q方向性技能：幽灵子弹（GhostBullet）以及电球（ElectricOrb），添加相关脚本以及特效

## 2025-11-13（Slaice）
- 解决受击后不显示特效bug：修改GhostBulletProjectible脚本，用代码调用受击特效
- 解决对象池复用的敌人 “带着死亡状态复活”以及提前回收问题：定义统一接口IPoolable，修改ObjectPool以及EnemyBase
- OnSpawn()：生成时调用，相当于“复活重置”：重置isDead和isDestroyed为false，避免“已死状态”残留；恢复生命值Health = maxHealth；启用所有碰撞体（包括子物体），确保能被命中；重置动画到Idle，清除旧触发器；清空物理残留（速度、角速度）
- OnDespawn()：回收时调用，清理残留：停止所有协程（避免死亡动画 / 延迟逻辑残留）；重置动画触发器，避免下次生成时误触发；清空物理残留
- 解决enemy大面积回收问题：Spawn时不再enqueue，让回收时再返回队列，并且进一步扩充对象池size
- 解决生成的个别enemy无法命中问题：怀疑是碰撞体偏移导致
- 解决生成的enemy穿过ground的问题：冻结y轴位置，暂时解决，怀疑是碰撞体被代码设置成！isTrgger
- 解决动画参数个别差异不存在问题：采用HasAnimatorParameter（）方法
- 解决生成逻辑数量问题：EnemyGenerate3的spawnwave（）逻辑有问题

## 2025-11-16（Slaice）
- 添加救赎E技能，添加相应脚本以及特效
- 添加水柱Q技能，添加相应脚本以及特效
- 解决使用redemption后不会特效不会跟随玩家移动bug：修改cast（）方法即可
- 解决使用技能后会再生成一个player，形成分身的效果：ShopManager的E槽技能池（skillPoolE）里，放的不是RedemptionSkill技能实例，而是Player场景对象——购买时框架会实例化技能池里的对象，误把Player当技能克隆了，所以直接使用纯独立技能模板，不要与player纠缠，关键代码在ShopManager的3.1、3.2、3.3，让独立的self技能绑定到player

## 2025-11-17（Slaice）
- 新增嗜血狂怒E技能，添加相关脚本及特效，关键：PlayerState中isBloodFrenzyActive
- 完善了ShopManager中self技能框架，规范统一模板
- 新增光之守护E技能，添加相关脚本及特效，关键：PlayerState中isInvincible
- 新增正常操作E技能，添加相关脚本及特效，关键：PlayerState中isNormalOperationActive
- 新增万象天引E技能，添加相关脚本及特效，关键：PullOres（）

## 2025-11-18（Slaice）
- 新增剧毒踪迹R技能，添加相关脚本及特效，关键：Poison以及PoisonSkill脚本，poisonCloudPrefab预制体
- 添加水柱WaterColumn受击特效
- 新增灵魂交换R技能，添加相关脚本及特效，关键：SoulSwapSkill脚本
- 新增地狱火炮R技能，添加相关脚本及特效，关键：NukeExplosion脚本

## 2025-11-19（Slaice）
- 新增斩击Q技能，添加相关脚本及三种特效，关键：SlashSkill脚本
- 新增圆舞R技能，添加相关脚本及三种特效，关键：CircleDance脚本
## 2025-12-01 (ETL1224)
- 添加了矿石掉落之后30秒后自动消失的功能，并在消失前10秒回闪烁
- 修复了光标离开火点过近时可能会反向开火的问题
- 修复了怪物能穿过矿石的问题