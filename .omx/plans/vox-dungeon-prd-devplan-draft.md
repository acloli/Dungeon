# Vox Dungeon 完整策划案 + 开发计划初稿（供 Architect / Critic 审查）

> 规划角色：Planner（ralplan）
> 依据文档：`Docs/vox_dungeon_gdd.md`、`Docs/vox_dungeon_m1_technical_plan.md`、`Docs/TFramework/TFramework_Documentation.md`
> 工程状态：`DungeonUnity` 当前为 Unity 壳工程，业务实现近乎空白；本稿以“首章/首地图 MVP 落地”作为第一交付

---

## 一、RALPLAN-DR 摘要

### Principles（4 条）
1. **先做一局可完整结束的好玩体验，再扩内容量。** 首章/首地图 MVP 必须能从标题页进入、打完 5 节点 + Boss、结算返回，形成闭环。
2. **规则简化但反馈完整。** 保持 3 能量 / 5 手牌 / 单敌主战斗的低复杂度，同时把意图、伤害、防御、抽牌、卡牌去留等关键信息反馈做清楚。
3. **数据先行、内容可配置。** 卡牌、遗物、敌人、遭遇、掉落、商店、事件均优先用 TFramework MasterData/ScriptableObject 配置，不把数值写死在逻辑里。
4. **商业化不触碰战斗强度。** 所有付费内容仅限外观、去广告、解锁便利，不提供战斗内数值优势。

### Decision Drivers（Top 3）
1. **当前工程几乎无实现代码**：必须控制第一交付范围，让战斗、地图、结算闭环优先于复杂 meta。
2. **现有 GDD/M1 已锁定关键参数**：1 地图、5 节点 + Boss、20 卡、6 遗物、3 能量、5 手牌，需要方案与既有文档可衔接而非推倒重来。
3. **TFramework 已提供 MasterData/FSM/DI/Reactive 基础**：策划与技术定义应围绕可配置、可测试、可扩展的数据边界组织。

### Viable Options

#### 方案 A：极简战斗演示型 MVP
- 范围：仅单场连续战斗，不做地图分支、不做商店、不做事件，只验证 BattleScene 战斗骨架。
- 优点：实现最快，技术风险最低。
- 缺点：无法验证 roguelike 卡组成长、节点决策、经济与回合外选择；对产品定位支撑不足。

#### 方案 B：首章/首地图完整闭环 MVP（选定）
- 范围：1 张地图、5 个节点 + Boss、20 张卡、6 个遗物、基础商店/休息/事件、单角色、单章节结算。
- 优点：能直接验证产品核心乐趣＝战斗 + 路线选择 + 局内成长；与现有 GDD 完整一致；后续容易扩章与扩池。
- 缺点：比方案 A 多出地图、经济、事件、UI 流程与数据配置工作。

#### 方案 C：中复杂度系统型 Vertical Slice
- 范围：除方案 B 外，再加入 2 角色、局外养成、广告/IAP 假链路、多 Boss、多地图掉率演进。
- 优点：更接近商业化成品。
- 缺点：超出当前工程成熟度；会把验证重点从“核心乐趣”稀释为“系统拼装”。

### 选型结论
**选择方案 B。**
原因：它是在现有 GDD 与技术方案约束下，既能验证产品是否成立，又能把工程风险控制在可落地范围内的最优平衡点。方案 A 太轻，不足以支持“完整策划案”；方案 C 太重，不适合当前空壳工程。

### ADR（供后续审查）
- **Decision**：以“首章/首地图完整闭环 MVP”作为第一交付，围绕单角色、单地图、单 run 完整体验组织内容与技术。
- **Drivers**：工程近空白、现有 GDD 已锁定 MVP 参数、需优先验证核心乐趣而非扩系统。
- **Alternatives considered**：极简战斗演示型 MVP；中复杂度 Vertical Slice。
- **Why chosen**：方案 B 能同时验证战斗、地图、经济、内容配置与 UI 流程，是最小可玩的产品切片。
- **Consequences**：需要先定义稳定的 masterdata 结构和 UI 信息架构；暂缓局外深养成、多角色、多地图掉率演进。
- **Follow-ups**：交给 Architect 校验系统边界与状态流；交给 Critic 检查内容规模、数据建模和验证闭环是否过重或遗漏。

---

## 二、完整 PRD 初稿

## 2.1 产品定位与体验目标

### 产品定位
《Vox Dungeon》是一款**移动端竖屏、单手可玩的轻量爬塔卡牌 roguelike**。核心体验参考 Slay the Spire 的“战斗—选牌—路线选择”结构，但在操作复杂度、会话长度、信息密度与内容规模上进一步收敛，适配手游用户碎片化体验。

### 体验目标
1. **3 分钟内理解规则，15~25 分钟完成一局首章。**
2. **每回合做 1~3 个明确决策**：打哪张、是否保留资源、是否赌高风险节点。
3. **每个节点都带来有感成长**：卡牌、遗物、金币、回复、强化至少命中其一。
4. **像素 UI 清晰而不拥挤**：玩家一眼看懂“我能做什么 / 敌人下回合会做什么 / 我打完会发生什么”。
5. **付费不改变战斗胜率**：商业化只服务留存与便利，不破坏 run 公平性。

## 2.2 目标平台 / 会话长度 / 用户画像

### 目标平台
- 第一平台：**iOS / Android**
- 演示与开发环境：Unity Editor / PC
- 画面方向：**竖屏优先**（如后续考虑横屏，需另开 UI 适配议题）

### 会话长度
- 单战斗：1~3 分钟
- 单节点决策：10~20 秒
- 首章完整 run：15~25 分钟
- 失败后重开：< 15 秒

### 用户画像
1. **轻度卡牌策略用户**：玩过炉石单人、杀戮尖塔、月圆之夜，但希望更轻更快。
2. **单手手游用户**：通勤、排队、睡前愿意完成一局。
3. **像素风/废土奇幻偏好者**：接受低成本美术，但要求 UI 有风格统一性。

## 2.3 核心循环与章节结构

### 核心循环
`进入 run → 地图选路 → 战斗/事件/补给 → 选择卡牌/购物/强化/回血 → Boss → 结算`

### 首章结构（MVP）
- 固定 1 张地图：**晶蚀回廊**
- 节点序列：起点战斗 → 节点 1 → 节点 2 → 节点 3 → 节点 4 → 节点 5 → Boss 前补给 → Boss → 结算
- 节点池：普通战斗 / 精英战斗 / 随机事件 / 补给商店
- 地图目标：让玩家在有限分支中体验“保守续航”与“高风险高回报”两种路线

### 推荐节点分布规则（首章）
- 起点固定普通战斗
- 第 1~5 节点中：
  - 普通战斗：2~3 次
  - 事件：1~2 次
  - 补给商店：1~2 次
  - 精英：0~1 次（MVP 先固定 1 次更利于验证）
- Boss 前固定补给商店

## 2.4 战斗规则

### 战斗对象
- MVP 先采用**单敌主战斗**，减少目标切换与 UI 复杂度
- 系统层面保留多敌扩展字段，但 UI/玩法不在首交付启用

### 玩家资源
- 初始最大生命：**60**
- 每回合能量：**3**
- 手牌上限：**5**
- 抽牌规则：回合开始补到 5
- 弃牌规则：**无传统弃牌层常驻管理**；本回合未打出的牌回合结束进入弃牌堆，再按牌库规则循环（为后续抽牌逻辑留一致性）
- 牌库规则：run 内牌组构成动态变化；抽牌堆空时，将弃牌堆洗回抽牌堆

### 目标选择
- 单敌卡自动指向当前敌人
- 非指定目标技能（例如加甲、抽牌、能量）不需要目标选择
- 预留 `TargetType`：Self / SingleEnemy / RandomEnemy / AllEnemies 以便扩展

### 回合结构
1. **TurnStart**：能量重置为 3 → 抽到 5 → 回合开始型遗物触发 → 刷新敌方意图
2. **PlayerAction**：玩家连续出牌，直到主动结束或无可行动作
3. **Resolve**：卡牌效果按 EffectChain 逐段执行，期间触发被动
4. **EnemyTurn**：敌人按预告意图执行
5. **TurnEnd**：回合结束型遗物/状态结算；若未分胜负则进入下一回合

### 胜败规则
- 敌方 HP ≤ 0：战斗胜利
- 玩家 HP ≤ 0：run 失败
- 首章 Boss 击败：章节通关，进入结算页

### 状态与关键词（MVP）
- **护甲**：吸收伤害；默认敌我回合结束保留 0（即回合结束清空）
- **易伤**：受到攻击伤害 +50%，持续 N 回合
- **脆弱**：获得护甲量 -50%，持续 N 回合
- **充能**：本回合额外获得能量
- **流血**：回合结束受到固定伤害，持续 N 回合

> 关键假设：为匹配“简化能量系统，无弃牌层”的原始 GDD，本稿保留简化牌库认知，但仍建议内部使用抽牌堆/弃牌堆实现，以避免后续扩展成本过高。

## 2.5 地图与节点规则

### 节点类型
1. **普通战斗**：主要金币来源，胜利后 3 选 1 卡牌奖励
2. **精英战斗**：更高难度，胜利后必得遗物并给更多金币
3. **随机事件**：在“血量 / 金币 / 卡牌 / 遗物 / 风险”中做二选一或三选一
4. **补给商店**：休息 / 强化 1 张卡 / 商店购买 / 移除 1 张卡
5. **Boss**：章节终局

### 路径规则
- 每节点显示 1~3 条可选路径
- 玩家只能前进，不能回退
- 节点类型对玩家可见，具体敌人或事件内容可部分未知
- 为降低首版复杂度，**地图生成采用半随机模板**：先定义 3 套可控模板，再在 run 开始时抽取一套并填充节点内容

### 补给节点规则
- **休息**：回复 20% 最大生命（向上取整）
- **强化卡牌**：选择 1 张可强化卡，将其升级为“+”版本
- **商店**：购买卡牌/遗物/移除卡牌
- 玩家进入补给点后只能执行一项主要操作，再离开节点

## 2.6 经济系统

### 局内经济（Run Currency）
货币名建议：**晶币**

#### 获取来源
- 普通战斗胜利：20~30
- 精英战斗胜利：50~70
- Boss 胜利：100
- 事件奖励：10~60 波动
- 宝箱事件：40 固定或随机遗物二选一

#### 消耗项
- 普通卡：60
- 稀有卡：90
- 史诗卡：130
- 遗物：140~220
- 移除卡牌：80（每局第 2 次起 +40）

### 局外经济（Meta Currency）
货币名建议：**回响石**

#### 用途边界
- 解锁新角色（后续版本）
- 解锁外观 / 头像 / 卡背 / 主题边框
- 解锁图鉴展示位或收藏陈列
- **不允许**购买战斗数值、初始强卡、局内能量、额外遗物槽

#### 首章 MVP 建议
- 仅保留“占位入口 + 假数据展示”，不做真实局外商店闭环
- 原因：当前应先验证核心 run，而非 meta 消费

## 2.7 商业化边界

### 可接受内容
- 去广告一次性购买
- 外观包：卡背、UI 主题皮肤、角色立绘皮肤
- 章节通关纪念展示物
- 激励视频：结算后额外获取局外货币、每日免费外观碎片

### 禁止内容
- 购买战斗属性
- 购买更高初始能量/生命/手牌
- 购买更高稀有度卡牌掉率
- 购买 Boss 弱化或额外复活

### 原则表述
**付费不影响战斗数值，不改变 run 内胜率，只改变表现、收藏或非战斗便利。**

## 2.8 UI/UX 信息架构与主要页面布局

### 视觉资源约束
- 以 **Pixel Art UI Kit - Fantasy Game Interface** 作为底层 UI 视觉资源
- 仅把它视为：边框、按钮、面板、图标、装饰件、分页容器、状态底框资源
- **不假设其具备脚本、布局逻辑、状态机或业务组件能力**
- 所有交互逻辑、状态同步、可点击区域、分页控制均由项目自行实现

### 全局信息优先级
1. 当前能否行动（能量 / 可出牌）
2. 当前生死状态（玩家 HP/护甲 / 敌人 HP/意图）
3. 当前收益与风险（节点、奖励、商店价格、事件代价）
4. 次级信息（图鉴、设置、收藏、装饰）

### 页面 IA（首章 MVP）
1. TitleScene
2. MainScene
   - 开始 run
   - 图鉴
   - 商店（可先占位）
   - 设置
3. BattleScene
   - 地图页
   - 战斗页
   - 奖励页
   - 事件页
   - 补给/商店页
   - 结算页

### BattleScene 主战斗布局（竖屏）

#### 顶部 20%
- 左上：返回/暂停按钮
- 中上：当前章节名、当前节点进度（如 3/6）
- 右上：局内金币、遗物槽、设置入口

#### 中部 40%
- 顶中偏右：敌人立绘
- 敌人头顶：意图图标 + 数值
- 敌人下方：HP 条 / 状态图标
- 背景：地图主题场景 + 轻微视差/粒子

#### 下中 18%
- 左侧：玩家立绘/头像
- 中间：玩家 HP / 护甲 / 状态
- 右侧：End Turn 按钮（高对比、拇指易点）

#### 底部 22%
- 中下：5 张手牌扇形展开
- 手牌上方中央：3 点能量显示
- 手牌拖拽/点击反馈：高亮、可打/不可打态、目标指示

### 地图页布局
- 顶部：章节标题、当前 HP、金币、遗物
- 中央：纵向地图路径，节点用不同图标区分
- 底部：本节点说明 / 预期奖励提示 / 进入按钮
- 交互：点击节点高亮路径，确认后进入下一页

### 奖励页布局
- 顶部：战斗结果摘要（胜利、掉血、金币）
- 中部：3 张卡并排展示，点击查看详情
- 底部：`选择 1 张` / `跳过` 按钮
- 若为精英：先弹遗物，再展示卡牌奖励

### 商店页布局
- 顶部：金币与退出按钮
- 中央上：售卖卡牌 3 张
- 中央下：售卖遗物 2 个
- 底部：休息 / 强化 / 移除卡牌分页按钮
- 卡牌/遗物点开时，右侧浮层显示说明与购买按钮

### 交互流原则
- 单手触达优先：主要 CTA 放屏幕下半部
- 文本描述短句化：卡牌描述尽量 2 行内读完
- 任何“不可执行动作”都必须给出明确反馈：灰态、红边、抖动、tips 三选二

## 2.9 卡牌内容方案（20 张）

> 稀有度分层：普通 / 稀有 / 史诗 / 传说
> 类型分层：攻击 / 技能 / 能力（持续性被动，打出后本战斗生效）
> 说明：MVP 先做 20 张，其中建议初始卡组从这 20 张里选 10 张

| # | 名称 | 稀有度 | 费用 | 类型 | 效果 | 设计意图 |
|---|---|---|---:|---|---|---|
| 1 | 裂隙斩 | 普通 | 1 | 攻击 | 对单体造成 6 伤害 | 最基础攻击基准卡 |
| 2 | 晶片投射 | 普通 | 0 | 攻击 | 对单体造成 3 伤害，抽 1 张牌 | 0 费润滑，保持手感 |
| 3 | 贯晶重击 | 稀有 | 2 | 攻击 | 对单体造成 12 伤害；若目标有易伤，额外 +4 | 奖励先上 debuff |
| 4 | 回响连斩 | 稀有 | 1 | 攻击 | 对单体造成 4 伤害 2 次 | 用于触发多段收益 |
| 5 | 侵蚀爆破 | 史诗 | 2 | 攻击 | 对单体造成 10 伤害并施加 2 层流血 | 建立持续压血思路 |
| 6 | 断界终式 | 传说 | 3 | 攻击 | 对单体造成 20 伤害；若本回合已打出 3 张以上牌，返还 1 能量 | 大招卡，鼓励连段 |
| 7 | 晶壁格挡 | 普通 | 1 | 技能 | 获得 6 护甲 | 最基础防御基准卡 |
| 8 | 紧急偏折 | 普通 | 0 | 技能 | 获得 3 护甲，本回合抽到的下一张牌费用 -1 | 危机补救 |
| 9 | 棱镜护罩 | 稀有 | 1 | 技能 | 获得 8 护甲；若本回合未打攻击牌，再抽 1 张 | 防守回合补偿 |
|10 | 晶髓修补 | 稀有 | 2 | 技能 | 回复 6 生命，获得 4 护甲 | 续航工具 |
|11 | 能量虹吸 | 稀有 | 1 | 技能 | 获得 1 点临时能量，抽 1 张 | 连段引擎 |
|12 | 预备架势 | 普通 | 1 | 技能 | 抽 2 张牌 | 牌序修正 |
|13 | 失稳诅咒 | 史诗 | 1 | 技能 | 施加敌方 2 回合易伤 | 爆发前置 |
|14 | 碎甲脉冲 | 稀有 | 1 | 技能 | 使目标脆弱 2 回合 | 让防御策略有对策 |
|15 | 战术撤离 | 普通 | 0 | 技能 | 弃掉 1 张手牌，抽 2 张 | 手牌优化与找 key card |
|16 | 水晶共鸣 | 史诗 | 2 | 能力 | 本战斗中，你每回合第一次打出 0 费卡时抽 1 张 | 支持轻量连锁流 |
|17 | 余烬核心 | 史诗 | 2 | 能力 | 本战斗中，你每回合第一次失去生命后获得 1 能量 | 鼓励风险换资源 |
|18 | 专注切割 | 稀有 | 1 | 攻击 | 对单体造成 7 伤害；若你当前手牌数≤3，再抽 1 张 | 中速压缩收益 |
|19 | 晶化反制 | 史诗 | 1 | 技能 | 获得 5 护甲；若完全抵消下次伤害，对攻击者造成 6 伤害 | 建立“防反”手感 |
|20 | 王座回响 | 传说 | 3 | 能力 | 本战斗中，你每回合开始时若生命≤50%，获得 1 能量与 4 护甲 | 逆风翻盘核心 |

### 初始卡组建议（10 张）
- 裂隙斩 ×4
- 晶壁格挡 ×4
- 预备架势 ×1
- 晶片投射 ×1

### 强化方向原则
- 攻击卡：+2~+4 伤害或增加附带效果
- 防御卡：+2~+3 护甲或附加抽牌/减费
- 能力卡：降低费用或提高触发收益

## 2.10 遗物方案（6 个）

| 名称 | 触发 | 效果 | 设计意图 |
|---|---|---|---|
| 破损晶核 | 战斗开始 | 本战斗首回合获得 +1 能量 | 最直观的开局强感 |
| 猎手护符 | 击败普通敌人后 | 额外获得 10 晶币 | 强化经济路线 |
| 回音容器 | 每回合首次打出 0 费卡时 | 获得 2 护甲 | 支持轻量连段牌组 |
| 裂纹圣像 | 生命低于 50% 时回合开始 | 获得 3 护甲 | 提供逆风保底 |
| 精炼棱镜 | 进入补给点时 | 本次强化卡牌免费，且额外回复 5 生命（每局 1 次） | 让补给节点更有吸引力 |
| 骨白骰匣 | 精英战胜利后 | 随机将 1 张普通卡升级为 + 版 | 奖励挑战房路线 |

> 关键约束继承：GDD 提到 MVP 最多同时携带 1 个额外遗物。本稿建议改为“初始遗物 1 + 可持有最多 2 个战利遗物”，否则遗物系统存在感过弱。此项为**可调整项 A**，需 Architect/Critic 评审是否坚持原案“最多 1 个额外遗物”。

## 2.11 敌人与 Encounter 方案

### 普通敌人（3）
1. **晶蚀鼠**
   - HP：28
   - 意图：6 攻击 / 4 攻击 ×2 / 防御 6
   - 意图设计：新手教学敌，突出“看意图出牌”

2. **废墟守卒**
   - HP：36
   - 意图：8 攻击 / 获得 8 护甲 / 施加 1 回合脆弱
   - 意图设计：教玩家理解 debuff 与对手防御

3. **裂灯祭司**
   - HP：32
   - 意图：5 攻击并施加易伤 / 回复 6 HP / 获得 1 层强化（后续攻击 +2）
   - 意图设计：强调先手压制与集火价值

### 精英敌人（1）
4. **棱镜处刑者**
   - HP：68
   - 意图循环：12 重击 / 8 护甲 + 下回合伤害提升 / 6×2 连击
   - 奖励：必掉遗物 + 高金币 + 卡牌奖励
   - 设计意图：检验防御、爆发和资源保留是否成立

### Boss（1）
5. **晶蚀王座残影**
   - HP：120
   - Phase 1：单体重击、上易伤、蓄力
   - Phase 2（HP ≤ 60）：每回合获得 4 护甲；轮换使用 16 重击 / 6×2 连击 / 自我强化
   - 设计意图：要求玩家在前期保留成长资源、并在中后期做爆发窗口管理

### Encounter 池建议
- 普通战斗池：晶蚀鼠 / 废墟守卒 / 裂灯祭司
- 精英池：棱镜处刑者
- Boss 池：晶蚀王座残影
- 首章 MVP 可采用“固定顺序 + 少量随机”混合：
  - 起点：晶蚀鼠
  - 中段随机：废墟守卒 / 裂灯祭司
  - 精英固定：棱镜处刑者
  - Boss 固定

## 2.12 随机事件方案（建议 3 个）

1. **裂隙祭坛**
- 选项 A：失去 8 生命，获得 1 张稀有卡
- 选项 B：获得 30 晶币
- 设计意图：早期风险换成长

2. **流浪匠人**
- 选项 A：支付 50 晶币，随机升级 1 张普通卡
- 选项 B：移除 1 张卡，但失去 6 生命
- 设计意图：提供牌组修剪路线

3. **破损补给箱**
- 选项 A：回复 12 生命
- 选项 B：随机获得 1 个遗物，但下场战斗敌人首回合 +2 伤害
- 设计意图：高风险高回报选择

## 2.13 MasterData 设计（TFramework / ScriptableObject）

### 设计原则
- 每类 masterdata 对应一种 `ScriptableObject` 资产集合或可导入表
- 运行时统一通过 `MasterDataManager / IMasterDataService` 查询
- 逻辑层只消费 ID 与字段，不直接依赖具体表现资源
- 配置粒度优先按“单实体单条记录”组织，避免把整个系统硬编码进一张大表

### 建议数据对象 / 表

#### 1. CardDefinition
- 主键：`CardId`
- 关键字段：
  - `CardId`
  - `NameKey`
  - `DescriptionKey`
  - `Rarity`
  - `CardType`
  - `Cost`
  - `TargetType`
  - `ArtAddress`
  - `FrameStyleId`
  - `UpgradeToCardId`
  - `IsStarterCard`
  - `UnlockChapter`
- 关系：1 对多关联 `CardEffectEntry`

#### 2. CardEffectEntry
- 主键：`CardEffectId`
- 字段：
  - `CardEffectId`
  - `CardId`
  - `Sequence`
  - `EffectType`
  - `TargetSide`
  - `Value`
  - `ValueFormulaId`（预留）
  - `Turns`
  - `ConditionType`
  - `ConditionParam`
  - `VfxId`
- 作用：把一张卡拆成可顺序执行的效果链

#### 3. RelicDefinition
- 主键：`RelicId`
- 字段：
  - `RelicId`
  - `NameKey`
  - `DescriptionKey`
  - `Rarity`
  - `TriggerType`
  - `TriggerLimitPerBattle`
  - `EffectPayloadType`
  - `EffectValue`
  - `IconAddress`
  - `StackRule`

#### 4. EnemyDefinition
- 主键：`EnemyId`
- 字段：
  - `EnemyId`
  - `NameKey`
  - `MaxHp`
  - `EnemyTier`（Normal/Elite/Boss）
  - `PortraitAddress`
  - `IntentSetId`
  - `RewardGoldMin`
  - `RewardGoldMax`
  - `RewardRelicGroupId`

#### 5. EnemyIntentDefinition
- 主键：`EnemyIntentId`
- 字段：
  - `EnemyIntentId`
  - `IntentSetId`
  - `OrderIndex`
  - `IntentType`
  - `Value`
  - `SecondaryEffectType`
  - `SecondaryValue`
  - `Weight`
  - `ConditionType`
- 作用：驱动 EnemyAIService 的配置化意图序列

#### 6. EncounterDefinition
- 主键：`EncounterId`
- 字段：
  - `EncounterId`
  - `EncounterType`
  - `EnemyId` / `EventId` / `ShopProfileId`
  - `ChapterId`
  - `NodeTag`
  - `Weight`
  - `MinFloorIndex`
  - `MaxFloorIndex`

#### 7. EventDefinition
- 主键：`EventId`
- 字段：
  - `EventId`
  - `TitleKey`
  - `BodyKey`
  - `IllustrationAddress`
  - `OptionGroupId`

#### 8. EventOptionDefinition
- 主键：`EventOptionId`
- 字段：
  - `EventOptionId`
  - `EventId`
  - `TextKey`
  - `CostType`
  - `CostValue`
  - `RewardType`
  - `RewardValue`
  - `RewardRefId`
  - `IsRepeatable`

#### 9. ChapterDefinition
- 主键：`ChapterId`
- 字段：
  - `ChapterId`
  - `NameKey`
  - `MapThemeId`
  - `NodeTemplateGroupId`
  - `BossEncounterId`
  - `CardRewardProfileId`
  - `RelicRewardProfileId`

#### 10. NodeTemplateDefinition
- 主键：`NodeTemplateId`
- 字段：
  - `NodeTemplateId`
  - `TemplateGroupId`
  - `NodeIndex`
  - `NodeType`
  - `BranchIndex`
  - `NextNodeIndices`
  - `ForcedEncounterId`
- 作用：支持半随机模板地图

#### 11. RewardProfileDefinition
- 主键：`RewardProfileId`
- 字段：
  - `RewardProfileId`
  - `CardChoiceCount`
  - `CardRarityWeightCommon`
  - `CardRarityWeightRare`
  - `CardRarityWeightEpic`
  - `GoldMin`
  - `GoldMax`
  - `RelicGroupId`

#### 12. ShopProfileDefinition
- 主键：`ShopProfileId`
- 字段：
  - `ShopProfileId`
  - `CardOfferCount`
  - `RelicOfferCount`
  - `RemoveCardBasePrice`
  - `RemoveCardPriceStep`
  - `RestHealPercent`
  - `UpgradeServiceEnabled`

#### 13. MetaProductDefinition（占位）
- 主键：`ProductId`
- 字段：
  - `ProductId`
  - `ProductType`
  - `PriceJpy`
  - `RewardType`
  - `RewardValue`
  - `CosmeticId`
- 备注：MVP 可只做占位，不接真实支付

### 表关系概览
- `ChapterDefinition` → `NodeTemplateDefinition` / `RewardProfileDefinition`
- `EncounterDefinition` → `EnemyDefinition` 或 `EventDefinition` 或 `ShopProfileDefinition`
- `EnemyDefinition` → `EnemyIntentDefinition`
- `CardDefinition` → `CardEffectEntry`
- `EventDefinition` → `EventOptionDefinition`
- `RewardProfileDefinition` 决定战斗/章节奖励抽取规则

### 配置粒度建议
- 卡牌、遗物、敌人、事件：**一条记录一个实体**
- 卡牌效果、敌人意图、事件选项：**一条记录一个子行为**
- 地图模板：**一条记录一个节点**
- 奖励与商店：**一条记录一个配置档**

### TFramework 对接建议
- `CardDefinition`、`RelicDefinition` 等均实现 `IMasterDataObject`
- 新增游戏域服务接口，例如：
  - `ICardCatalogService`
  - `IRelicCatalogService`
  - `IEncounterCatalogService`
- 这些服务仅作为对 `IMasterDataService` 的领域包装，避免上层业务散落查询条件

## 2.14 技术配合点：与现有 GDD / TFramework / M1 technical plan 的衔接

### 与现有 GDD 的衔接
- 继承已确认参数：1 地图、5 节点 + Boss、20 卡、6 遗物、3 能量、5 手牌
- 将 GDD 中“masterdata 待设计”部分具体化为可执行数据表
- 将“补给商店房”明确拆成休息/强化/商店/移除卡四种交互结果

### 与 M1 技术方案的衔接
- `BattleCanvas / CardView / FSM / EffectChain / EnemyAIService` 继续保留为核心实现骨架
- 本 PRD 补足这些骨架所依赖的：
  - 卡牌 effect schema
  - 敌人 intent schema
  - 奖励与节点 schema
  - 页面流转与 BattleScene 分页结构
- M1 原验收用例可直接成为战斗层第一批 smoke case

### 与 TFramework 的衔接
- FSM：使用现有 BattleFSM 状态定义落地回合结构
- R3：用于 HP/Armor/Energy/Intent/CardPlayed/UI 状态同步
- UniTask：用于效果链和页面转场异步编排
- VContainer：注册 catalog/service/controller
- MasterData：作为卡牌、遗物、敌人、事件、商店的唯一静态数据来源

### 建议新增技术边界
- `RunState`：持有本局地图进度、当前牌组、金币、遗物、当前节点
- `BattleSetupFactory`：从 `RunState + EncounterDefinition` 生成 `BattleContext`
- `RewardResolver`：统一结算奖励并回写 RunState
- `PageFlowController`：控制 BattleScene 内地图页/战斗页/奖励页/商店页/结算页切换

## 2.15 风险与非目标

### 主要风险
1. **内容与系统同时定义，易超首版实现能力。** 需保持首章单角色、单 Boss、不上多敌。
2. **GDD 中“战斗奖励卡是否进永久牌组”表述不够清晰。** 若当局不成长，roguelike 感会弱；建议明确为 run 内成长、run 外重置。
3. **Pixel UI 套件若直接拼装，易风格统一但信息层级混乱。** 需先出线框与层级规范，再套美术资源。
4. **MasterData 若一次性设计过宽，会拖慢实现。** 应先落 MVP 必需字段，扩展字段可预留但不强依赖。

### 非目标（首章 MVP 不做）
- 多角色切换
- 多地图章节
- 多敌同屏战斗正式启用
- 真实 IAP/广告 SDK 接入
- 长线局外养成树
- 公会、排行、社交

---

## 三、完整开发计划

## 3.1 分阶段里程碑

### 阶段 0：规划冻结与数据骨架
- **目标**：冻结首章范围、卡池、遗物、敌人、UI 线框、masterdata 结构
- **输入**：本初稿、现有 GDD、M1 技术方案
- **输出**：
  - 冻结版 PRD
  - MasterData schema 清单
  - 页面 IA 与线框草图
  - 首批数值表
- **依赖**：Planner / Architect / Critic 审查完成
- **验收**：无 P0 范围争议；战斗、地图、奖励、商店字段已足够支撑实现

### 阶段 1：核心战斗 Vertical Slice
- **目标**：完成单场战斗闭环
- **输入**：Card / Enemy / Effect masterdata，M1 FSM 方案
- **输出**：
  - BattleScene 战斗页
  - BattleFSM + BattleContext + EffectChain
  - CardView / EnemyView / IntentView / EnergyView
  - 单场胜负结算
- **依赖**：阶段 0 数据结构冻结
- **验收**：能从样例配置进入战斗，完成 8 条 M1 战斗验收用例

### 阶段 2：Run 骨架与地图节点流
- **目标**：让玩家可以跑完整个 1 地图流程
- **输入**：Chapter / NodeTemplate / Encounter / Reward / Shop masterdata
- **输出**：
  - RunState
  - 地图页与节点选择
  - 战斗后奖励页
  - 事件页 / 补给页骨架
  - Boss 前补给与章节结算
- **依赖**：阶段 1 战斗稳定
- **验收**：从 MainScene 开始 1 局，能经历 5 节点 + Boss + 结算，流程无阻断

### 阶段 3：内容填充与数值首轮平衡
- **目标**：接入完整 20 卡 / 6 遗物 / 5 敌人 / 3 事件
- **输入**：所有配置表与页面骨架
- **输出**：
  - 完整卡牌与遗物内容接入
  - 敌人意图与奖励完整配置
  - 商店价格 / 掉落率 / 血量伤害首轮平衡
- **依赖**：阶段 2 流程可玩
- **验收**：至少完成 10 局自动/手动 playtest，出现可复现的胜率分布与问题清单

### 阶段 4：UI 打磨与新手引导最小集
- **目标**：把“能看懂、能操作、能复盘”做到可交付
- **输入**：稳定内容与流程
- **输出**：
  - Pixel UI 套件整合后的正式版 HUD
  - 新手提示（能量、意图、节点、奖励）
  - 关键动效与反馈（不可出牌、伤害、获得卡牌、购买成功）
- **依赖**：前 3 阶段核心逻辑稳定
- **验收**：新用户 3 分钟内能理解并完成至少 1 场战斗

### 阶段 5：验证、修正与候选交付
- **目标**：形成可供下一阶段扩章/扩角色的稳定基线
- **输入**：完整首章 MVP
- **输出**：
  - 缺陷清单与修复回合
  - 平衡性调整表
  - 首章 MVP 验收报告
- **依赖**：前述阶段完成
- **验收**：Acceptance criteria 全部通过；无 P0/P1 阻断缺陷

## 3.2 每阶段角色分工建议

### 策划
- 冻结 card/relic/enemy/event/shop 数值
- 维护 masterdata 表与变更日志
- 跟进体验问题单

### UI/UX
- 出信息架构线框
- 建立 Pixel UI 资源映射表（哪个组件用套件哪类 frame/button/panel）
- 跟进竖屏可点击热区和字体可读性

### 客户端
- 战斗系统、RunState、页面流转、masterdata 读取、DI 注册
- 负责 BattleScene 和 MainScene 的主流程

### 内容实现
- 配置 ScriptableObject / 表资产
- 卡牌描述、图标、事件文案、敌人意图资源填充

### QA
- 基于流程、规则、数据、UI 的验收清单执行
- 关注 deadlock、错误状态、奖励错配、流程断档

## 3.3 推荐执行路径：ralph vs team staffing guidance

### 适合 `ralph` 的情况
- 单人连续推进、需要一位 owner 从“数据结构 → 战斗 → 地图 → UI → 验证”顺序串起来
- 当前仓库实现近空白，很多边界还需边做边收敛
- 团队人力少，或者想先做一条最稳的 MVP 主线

### `ralph` 建议编制
- 1 × architect（前置审结构）
- 1 × executor（主实现 owner）
- 1 × verifier（阶段验收）
- 需要时插入 test-engineer / designer 进行阶段性复核

### 适合 `team` 的情况
- 已冻结本稿，准备并行推进 UI、战斗、地图、数据接入
- 有明确分工，且可以接受多 lane 并行带来的整合成本
- 目标是压缩日历时间而不是最小沟通成本

### `team` 建议编制
- **Leader**：planner / team-executor
- **Lane A（战斗）**：executor + test-engineer
- **Lane B（地图/流程）**：executor
- **Lane C（UI/UX）**：designer + executor
- **Lane D（数据/内容）**：analyst 或 writer 配合 executor
- **Lane E（验收）**：verifier

### 可用 agent types roster（建议）
- planner
- architect
- critic
- executor
- verifier
- test-engineer
- designer
- analyst
- writer
- explore
- researcher（本任务后续若需补官方 Unity/TFramework 文档核对时启用）

### reasoning levels by lane（建议）
- planner：medium
- architect：high
- critic：high
- executor：medium
- verifier：high
- test-engineer：medium
- designer：high
- explore：low

### launch hints
- 顺序稳妥路径：`$ralph`，先做 battle vertical slice，再扩 run
- 并行路径：`$team`，按“战斗 / 流程 / UI / 数据 / QA”五 lane 拆分
- 若先走 team，建议先单独冻结 `.omx/plans` 下的 PRD 与 test-spec，再启动 team-exec

### team verification path
1. 战斗 lane 提交 battle acceptance evidence
2. 地图 lane 提交流程闭环 evidence
3. UI lane 提交 BattleScene / Map / Reward / Shop 截图与交互说明
4. 数据 lane 提交 masterdata 清单与抽样校验
5. verifier 汇总执行完整首章 walkthrough + 回归清单

## 3.4 关键假设与可后续调整项

### 关键假设
1. 首章 MVP 采用**单角色、单地图、单 Boss**。
2. run 内卡牌成长成立，run 结束后重置；局外不保留战斗牌组。
3. UI 以竖屏单手体验为第一目标。
4. Pixel UI Kit 仅提供视觉资源，不承担任何业务逻辑。
5. TFramework MasterData 可通过 ScriptableObject 资产先行落地，不等待额外后端或表工具链。

### 可调整项
- A：遗物上限到底是“初始 1 + 额外 1”还是“初始 1 + 额外 2”
- B：普通战斗金币区间与商店价格曲线
- C：卡牌稀有度掉率
- D：是否在首章就加入多敌 encounter
- E：局外货币在 MVP 是否只展示不生效

---

## 四、Test / Validation Shape 草案

## 4.1 关键 Acceptance Criteria
1. 玩家可以从主界面进入 run，并完整经历：地图 → 战斗/事件/补给 → Boss → 结算。
2. 战斗内 3 能量、5 手牌、出牌扣能量、意图显示、胜败判定全部正确工作。
3. 至少 20 张卡、6 个遗物、5 种敌人/遭遇均可通过 masterdata 驱动接入，无硬编码专案逻辑阻塞。
4. 商店、奖励、事件、补给的输入输出与经济变化可被验证并可回写 RunState。
5. UI 主要页面在移动竖屏下信息优先级清晰，可单手完成关键交互。
6. 所有商业化入口均不影响战斗数值，只提供占位或非数值奖励。

## 4.2 需要的验证类型

### 规则验证
- 能量扣除/不足拦截
- 抽牌补牌
- 护甲吸收
- debuff 持续回合
- 回合切换
- 胜败判定

### 数据验证
- CardDefinition 与 CardEffectEntry 关联完整
- EnemyDefinition 与 IntentSet 映射正确
- RewardProfile 掉落权重符合预期
- ShopProfile 价格计算正确
- 事件选项成本/收益不越界

### UI 验证
- Battle HUD 层级清晰
- 5 张手牌在不同分辨率下不遮挡关键按钮
- 不可出牌反馈明显
- 地图节点与当前进度可读
- 商店价格、购买按钮、余额提示明确

### 流程验证
- 单局 run 无死路、无软锁
- 战斗结束后奖励页一定出现
- 补给点操作后能返回地图
- Boss 前必定出现补给点
- 结算后可返回主界面并重新开始

### 经济验证
- 局内晶币收益与消耗闭环成立
- 高风险节点的长期收益高于保守节点
- 商店不会出现“买不起且无意义”的负体验密集情况
- 局外货币仅影响外观/解锁占位，不影响战斗数值

## 4.3 建议测试层次
- **单元测试**：效果结算、价格计算、奖励抽取、状态持续回合
- **集成测试**：BattleFSM、RunState → BattleSetupFactory → RewardResolver 流
- **内容校验**：masterdata 主键唯一、外键完整、升级链闭合、文本 key 完整
- **手动体验测试**：新手首局、老手速通、低血苟活、全防御流、全攻击流
- **回归冒烟**：M1 8 条战斗用例 + 首章整局 walkthrough

