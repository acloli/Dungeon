# PRD — Vox Dungeon 首章完整策划案与开发计划

- 日期：2026-05-06
- 来源：`$ralplan`
- 依据：`Docs/vox_dungeon_gdd.md` / `Docs/vox_dungeon_m1_technical_plan.md` / `Docs/TFramework/TFramework_Documentation.md` / `Docs/dungeon_coding_rules.md`
- 目标：在现有 GDD 基础上，冻结《Vox Dungeon》首章闭环版本的完整策划、UI 方案、MasterData 方案与开发计划

---

## 0. ADR

### Decision
选择“**首章/首地图完整闭环 MVP**”作为当前正式开发目标：1 角色、1 章节、1 条完整 run、20 张卡、6 个遗物、5 类敌方遭遇、补给/商店/事件/Boss/结算齐全。

### Drivers
1. 当前仓库几乎无业务实现，必须优先验证完整可玩闭环。
2. 现有 GDD 已锁定关键范围：3 能量、5 手牌、5 节点 + Boss、20 卡、6 遗物。
3. TFramework 具备 FSM / R3 / UniTask / VContainer / MasterData，可支撑数据驱动但不适合一开始就做过宽范围。
4. 用户要求接近上线策划案级的细化，并要求数值以 TFramework masterdata 定义。

### Alternatives Considered
- **A. 纯战斗 Demo**：实现快，但无法验证“爬塔 + 局内成长 + 节点选择”的核心乐趣。
- **B. 首章完整闭环 MVP**：实现量可控，能验证产品成立性。**已选**。
- **C. 商业化 Vertical Slice**：加入多角色/局外成长/广告 IAP 真接入，更像上线包，但对当前空仓风险过高。

### Why Chosen
B 是现阶段最小但完整的产品验证单元，既能形成可开发 PRD，也能给后续 `$ralph` / `$team` 清晰边界。

### Consequences
- 首版不做多角色、多地图、长期养成树。
- 真实广告/IAP SDK 接入后置；先完成商品与入口占位。
- 首章所有数值、内容、掉落、商店逻辑必须数据驱动。

### Follow-ups
1. 用本 PRD 产出 ScriptableObject / MasterData 表定义。
2. 用本 PRD 产出测试规格与实施分工。
3. 后续如进入执行，优先冻结战斗奖励卡的 run 内归属逻辑与升级路径。

---

## 1. RALPLAN-DR 摘要

### Principles
1. 先闭环，后扩量。
2. 规则简化，反馈完整。
3. 数据先行，内容驱动。
4. 竖屏单手优先，战斗信息优先于装饰。
5. 付费不影响战斗数值。

### Decision Drivers
1. 工程仍为空壳，必须控制复杂度。
2. 现有 GDD 已固定核心玩法参数。
3. 需要一份能直接支持开发与验收的完整策划案。

### Viable Options
- **A 极简战斗演示**：只做战斗 UI + FSM。
- **B 首章完整闭环 MVP（选定）**：战斗、地图、事件、商店、Boss、结算全部打通。
- **C 中复杂度商业切片**：在 B 上加入多角色、局外养成、SDK。
- **D Playable Gate 优先切面**：先以灰盒 UI + 8 核心卡 + 3 遗物 + 3 遭遇打通系统边界，再扩到 B 的完整内容。

### Selection
选 **B 作为产品范围**，选 **D 作为执行策略**。

---

## 2. 产品定义

### 2.1 定位
《Vox Dungeon》是一款**移动端竖屏、单手操作优先的轻量爬塔卡牌 roguelike**。它以《Slay the Spire》式回合制卡牌决策为核心，但压缩每局时长、降低规则门槛，并用“晶体朋克废墟”做出识别度。

### 2.2 体验目标
- 3 分钟内理解规则
- 15~25 分钟完成首章 run
- 单战斗 1~3 分钟
- 每回合有 1~3 个明确决策
- 玩家始终能一眼看懂：敌人要做什么、我还能做什么、打完会得到什么

### 2.3 目标平台与用户
- 平台：iOS / Android
- 开发验证：Unity Editor 6000.3.10f1
- 屏幕：**竖屏唯一主方案**
- 用户：喜欢 deckbuilder / roguelite，但偏好轻度、碎片化手游体验的玩家

---

## 3. 范围与非目标

### 3.1 当前冻结范围
- 1 个可玩角色
- 1 张地图（首章“晶蚀回廊”）
- 1 条完整 run：起始战斗 → 5 节点 → Boss 前补给 → Boss → 结算
- 20 张基础可获得卡牌
- 6 个遗物
- 3 个随机事件模板
- 5 类敌方遭遇（3 普通 + 1 精英 + 1 Boss）
- 商店、补给、移除、强化、选卡奖励
- UI 风格基于 **Pixel Art UI Kit - Fantasy Game Interface**，但只作为视觉资源基础
- MainScene 的 meta 壳层仅保留最小入口；每日奖励/活动/邮箱/任务/局外商店均为占位，不绑定 MVP 验收

### 3.2 非目标
- 多角色首发
- 多地图/多章节
- 正式多敌同屏战斗
- 长线养成树
- 社交/排行
- 真实广告与 IAP SDK 接入（仅保留商品与入口设计）
- 战斗强度付费

---

## 4. 核心循环与章节结构

### 4.1 核心循环
进入 run → 地图选路 → 战斗/事件/补给 → 选卡/购物/强化/回血 → Boss → 章节结算。

### 4.2 首章结构
- 章节名：**晶蚀回廊**
- 固定起点：普通战斗
- 中段：5 个节点
- Boss 前：固定补给点
- 终点：Boss 战 + 章节结算

### 4.3 节点构成建议
- 普通战斗：2~3
- 随机事件：1~2
- 补给/商店：1~2
- 精英：0~1（首版固定 1 更利于节奏控制）

### 4.4 地图规则
- 每个节点提供 1~3 条可前进路径
- 不允许回退
- 节点类型可见，节点具体内容部分可见（例如敌人池、奖励倾向）
- 使用半随机模板图，避免完全随机破坏节奏

---

## 5. 战斗系统

### 5.1 核心参数
- 玩家最大生命：60
- 每回合基础能量：3
- 手牌上限：5
- 费用层级：0 / 1 / 2 / 3
- 默认战斗形态：单敌
- 预留扩展目标类型：`Self` / `SingleEnemy` / `RandomEnemy` / `AllEnemies`

### 5.2 回合流
1. **TurnStart**：重置能量到 3；补牌至 5；处理回合开始遗物/状态；**生成并展示敌人下一意图**
2. **PlayerAction**：玩家连续出牌
3. **Resolve**：EffectChain 按序结算
4. **EnemyTurn**：敌人**执行先前已展示**的意图
5. **TurnEnd**：处理回合结束状态/遗物

### 5.3 状态关键词（首章冻结）
- 护甲：吸收伤害，回合结束清空
- 易伤：受到攻击伤害增加 50%
- 脆弱：获得护甲减少 50%
- 流血：回合结束失去固定生命
- 充能：本回合/下回合提供额外能量的临时状态

### 5.4 胜败
- 敌方生命 ≤ 0：战斗胜利
- 玩家生命 ≤ 0：战斗失败
- Boss 击败：章节通关

### 5.5 Run 内成长澄清
战斗奖励获得的卡牌**加入本次 run 的临时牌组，并持续到本章 run 结束**；章节结算后清空 run 牌组。此处用于修正原 GDD 中“加入手牌但战斗后消失”的歧义，以保留 deckbuilder 的成长乐趣。

---

## 6. 经济与商业化

### 6.1 局内货币：晶币
#### 获取
- 普通战斗：20~30
- 精英：50~70
- Boss：100
- 事件：10~60
- 宝箱/补给类额外奖励：20~40

#### 消耗
- 普通卡：60
- 稀有卡：90
- 史诗卡：130
- 传说卡：不在普通商店常驻出售，仅事件/稀有奖励产出
- 遗物：140~220
- 移除卡：80 起，之后每次 +20
- 强化卡：补给点内免费次数为 1；商店内不开放直接付费强化

### 6.2 局外货币：回响石
用途仅限：
- UI 主题
- 卡背 / 皮肤
- 图鉴解锁 / 收藏项
- 未来新角色解锁（不影响现有角色强度）

### 6.3 商业化红线
**禁止**售卖战斗内数值强度：生命、能量、初始强卡、强遗物、掉率、额外复活、run 内直接买胜利。

### 6.4 可保留的商业化入口
- 去广告
- 激励视频换局外货币
- 外观包、卡背、UI 主题
- 未来独立角色包（须是不同玩法，不是更强）

---

## 7. UI/UX 方案

### 7.1 视觉原则
- 底色：灰蓝废墟 / 暗铜遗迹 / 破裂石板
- 强调色：青蓝晶体光、少量紫晶裂痕、危险状态用猩红
- 风格关键词：**晶体朋克废墟、祭坛式 HUD、像素发光控件**
- UI Kit 用途：按钮、面板、弹窗、血条/能量条底板、基础 icon 框
- 必须自制：Logo、主视觉背景、敌人立绘、卡牌插画、地图节点图标、意图图标、稀有度边框

### 7.2 交互原则
1. 单手操作优先，高频按钮在屏幕下半区
2. 战斗中“敌人意图 → 当前能量 → 可出牌 → 双方血甲 → End Turn”是固定信息优先级
3. 长文不塞进卡面；短说明 + 长按放大说明
4. 所有可打出/不可打出状态必须有明显视觉反馈

### 7.3 Scene / Page 信息架构
- `TitleScene`
  - `StartPage`
  - `SettingsDialog`
  - `NoticeDialog`
- `MainScene`
  - `HubPage`（MVP 必做）
  - `DeckPage`（仅调试/内容预览，可后置，不属于 Gate A/Gate B 主验收）
  - `ShopPage`（首版仅展示占位，Phase 5+）
  - `CharacterPage`（Phase 5+ 占位）
  - `DailyRewardDialog`（Phase 5+ 占位）
- `BattleScene`
  - `MapExplorePage`
  - `BattlePage`
  - `RewardDialog`
  - `ShopSupplyPage`
  - `PauseDialog`
  - `ResultPage`

### 7.4 关键页面排布
#### 标题页
- 顶部 10%：版本、更新检查、设置
- 中上 25%：Logo + 副标题
- 中部 35%：主视觉（遗迹大门 / 晶体裂隙）
- 中下 15%：`进入地下城` 主按钮，`继续冒险` 次按钮
- 底部 15%：公告 / 用户协议 / 客服

#### 主界面
- 顶部 12%：头像/等级/资源/设置
- 上中 18%：当前章节提示（MVP）；登录奖励条/活动 Banner 仅占位
- 中部 40%：主入口以 `开始探索` 为绝对主 CTA；`卡组预览` 可作为调试或后置入口
- 下中 15%：遗物图鉴/邮件/公告/任务均为后置占位，不进入 MVP 主线
- 底部 15%：当前章节信息 / 推荐行动

#### 地图探索页
- 顶部 10%：章节名 / 当前层数 / 血量简报 / 设置
- 中部 55%：纵向路线图
- 下中 15%：节点说明卡（风险 / 奖励倾向）
- 底部 20%：前往按钮 + 卡组/遗物快捷入口

#### 战斗页
- 顶部 12%：战斗名 / 波次 / 暂停
- 上中 28%：敌人区（立绘、HP、护甲、意图、状态）
- 中部 12%：演出层（飘字、命中特效）
- 下中 14%：玩家区（头像、HP、护甲、状态）
- 底部 22%：手牌区（最多 5 张扇形展开）
- 最底部 12%：左能量、中提示、右结束回合按钮

#### 商店/补给页
- 顶部 10%：标题、晶币、离开按钮
- 中部 60%：纵向一列商品卡（图标、效果、价格、购买）
- 下中 12%：刷新/休息/强化/移除操作入口
- 底部 18%：已选商品简报、购买后余额

#### 章节结算页
- 顶部 15%：胜利/失败/章节完成标题
- 中部 35%：本次收益
- 下中 25%：本局摘要（层数、用时、关键卡牌、受伤量）
- 底部 25%：继续 / 返回主界面 / 再来一局

### 7.5 卡牌视觉规范
- 比例：约 2:3
- 顶部：左费用晶体 / 中卡名 / 右稀有度角标
- 中部：插画
- 下中：关键词图标条
- 下部：2~3 行说明文本
- 底部：类型标签
- 卡名建议 ≤ 8 个中文字符
- 手持态说明建议 24~30 中文字符内
- 状态：
  - 可打出：正常亮度
  - 不可打出：灰暗 + 红费用
  - 选中：上浮 + 青蓝描边
  - 升级：金/青双层边框

### 7.6 与现有 M1 BattleCanvas 的衔接
保留 M1 结构，但重组为：
```text
BattleCanvas
├── TopBattleHUD
├── EnemyFieldPanel
├── EffectOverlayLayer
├── PlayerStatusPanel
├── HandCardPanel
├── BottomActionPanel
└── ModalLayer
```
映射：
- `EnergyDisplay` → `BottomActionPanel/EnergyCluster`
- `PlayerDisplay` → `PlayerStatusPanel`
- `EnemyDisplayArea` → `EnemyFieldPanel`
- `HandCardArea` → `HandCardPanel`
- `DamageNumberLayer` → `EffectOverlayLayer`
- `EndTurnButton` → `BottomActionPanel/EndTurnButton`
- `IntentDisplay` → 并回 `EnemyFieldPanel`

---

## 8. 内容设计

### 8.1 初始卡组
- 裂隙斩 ×4
- 晶壁格挡 ×4
- 预备架势 ×1
- 晶片投射 ×1

### 8.2 卡牌池（20 张）
| 名称 | 稀有度 | 费用 | 类型 | 效果 | 设计意图 |
|---|---|---:|---|---|---|
| 裂隙斩 | 普通 | 1 | 攻击 | 对单体造成 6 伤害 | 基础伤害基准 |
| 晶片投射 | 普通 | 0 | 攻击 | 造成 3 伤害，抽 1 | 0 费润滑 |
| 贯晶重击 | 稀有 | 2 | 攻击 | 12 伤害；若目标有易伤，额外 +4 | 奖励 debuff 联动 |
| 回响连斩 | 稀有 | 1 | 攻击 | 4 伤害 ×2 | 多段触发 |
| 侵蚀爆破 | 史诗 | 2 | 攻击 | 10 伤害并施加 2 层流血 | 持续压血 |
| 断界终式 | 传说 | 3 | 攻击 | 20 伤害；若本回合已出 3 张牌，返 1 能量 | 爆发终结 |
| 晶壁格挡 | 普通 | 1 | 技能 | 获得 6 护甲 | 基础防御基准 |
| 紧急偏折 | 普通 | 0 | 技能 | 获得 3 护甲；下一张牌费用 -1 | 补救/转段 |
| 棱镜护罩 | 稀有 | 1 | 技能 | 获得 8 护甲；若本回合未打攻击牌，抽 1 | 防守收益 |
| 晶髓修补 | 稀有 | 2 | 技能 | 回复 6 生命并获得 4 护甲 | 保底续航 |
| 能量虹吸 | 稀有 | 1 | 技能 | 获得 1 临时能量，抽 1 | 资源引擎 |
| 预备架势 | 普通 | 1 | 技能 | 抽 2 | 牌序修正 |
| 失稳诅咒 | 史诗 | 1 | 技能 | 施加 2 回合易伤 | 爆发前置 |
| 碎甲脉冲 | 稀有 | 1 | 技能 | 施加 2 回合脆弱 | 防御对策 |
| 战术撤离 | 普通 | 0 | 技能 | 弃 1 抽 2 | 手牌过滤 |
| 水晶共鸣 | 史诗 | 2 | 能力 | 每回合第一次打出 0 费牌时抽 1 | 轻连锁体系 |
| 余烬核心 | 史诗 | 2 | 能力 | 每回合第一次失去生命后获得 1 能量 | 逆风换资源 |
| 专注切割 | 稀有 | 1 | 攻击 | 7 伤害；若手牌 ≤3，再抽 1 | 中速压缩 |
| 晶化反制 | 史诗 | 1 | 技能 | 获得 5 护甲；若完全抵消下次伤害，反伤 6 | 防反手感 |
| 王座回响 | 传说 | 3 | 能力 | 回合开始若生命 ≤50%，获得 1 能量和 4 护甲 | 低血翻盘 |

### 8.3 卡牌升级规则
- **最终内容目标**：20 张基础卡牌均预留升级位，但不要求首个可玩门全部实装
- **Gate A 卡池冻结**：实现 **10 张基础卡**，其中 8 张有升级分支、2 张无升级分支
- **Playable Gate 的 8 张核心升级卡**：`裂隙斩`、`晶片投射`、`晶壁格挡`、`预备架势`、`紧急偏折`、`能量虹吸`、`回响连斩`、`贯晶重击`
- **Gate A 中无升级分支但可获得的 2 张基础卡**：`晶髓修补`、`战术撤离`
- **Content Gate**：补齐其余 12 张卡的升级派生定义
- 升级方向只做单一路径
- 升级 ID 命名规则：`<BaseCardId>_UP`
- 升级原则：数值增强或附加 1 个次级效果，不改变卡牌定位
- 因此真实数据规模应按“20 张基础卡 + 8~20 张升级派生卡”估算，而不是仅 20 行

### 8.4 遗物位规则（ADR 覆盖原 GDD）
- 首章 run 的遗物槽位固定为 **3 个**：`1 个初始遗物 + 最多 2 个局内获得遗物`
- 若获得第 4 个遗物，则必须立即丢弃 1 个非初始遗物
- 该规则用于在“成长感”和“堆叠复杂度”之间做控制，显式覆盖原 GDD 的“初始遗物外最多 1 个遗物”限制

### 8.5 遗物池（6 个）
| 名称 | 触发 | 效果 | 设计意图 |
|---|---|---|---|
| 破损晶核 | 战斗开始 | 首回合 +1 能量 | 开局爽感 |
| 猎手护符 | 击败普通敌人后 | 额外 +10 晶币 | 经济路线 |
| 回音容器 | 每回合首次打出 0 费卡 | 获得 2 护甲 | 支持轻连锁 |
| 裂纹圣像 | 生命低于 50% 的回合开始 | 获得 3 护甲 | 逆风保底 |
| 精炼棱镜 | 进入补给点时 | 本次强化免费且回复 5 生命（每局 1 次） | 强化补给点价值 |
| 骨白骰匣 | 精英战胜利后 | 随机将 1 张普通卡升级 | 奖励高风险路线 |

### 8.6 Boss Phase 行为契约
- 首章 Boss：`晶蚀王座残影`
- 固定 **2 个 Phase**
- `Phase 1`：血量 > 50%
  - 意图池：`重击(12)` / `施加易伤(1回合)+6伤害` / `蓄力`
- `Phase 2`：血量 ≤ 50% 时进入
  - **切换时机**：当前结算链结束后立即切换；若本次伤害把 Boss 压到阈值以下，则在同回合结算完毕后触发转阶段，不打断当前卡牌 Resolve
  - **Transition OneShot**：仅在首次进入 Phase 2 时触发 1 次：`获得 8 护甲 + 下一个意图切换为 高压连击(6x2)`
  - 进入 Phase 2 后，下一个玩家回合开始展示 Phase 2 的下一意图
- `Phase 2` 意图池：`高压连击(6x2)` / `晶甲恢复(+10护甲)` / `裂界重击(16)`
- `EnemyIntentState` 在 Boss 身上仍只负责演出提示，不拥有业务裁定权

### 8.6 敌人与遭遇
| 敌人 | 类型 | 行为方案 | 设计意图 |
|---|---|---|---|
| 晶蚀鼠 | 普通 | 6 攻 / 4×2 / 防御 6 | 教学敌人 |
| 废墟守卒 | 普通 | 8 攻 / 8 护甲 / 施加脆弱 | 教防御与 debuff |
| 裂灯祭司 | 普通 | 5 攻+易伤 / 回复 6 / 自我强化 | 逼迫先手压制 |
| 棱镜处刑者 | 精英 | 12 重击 / 8 甲+强化 / 6×2 | 资源管理考核 |
| 晶蚀王座残影 | Boss | Phase1：重击/易伤/蓄力；Phase2：获甲 + 轮换高压 | 首章终局考试 |

### 8.7 随机事件（3 个）
1. **裂隙祭坛**：失去 8 生命换 1 张稀有卡，或直接获得 30 晶币
2. **流浪匠人**：支付 50 晶币随机升级 1 张普通卡，或移除 1 张卡但失去 6 生命
3. **破损补给箱**：回复 12 生命，或获得随机遗物但下场战斗敌人首回合 +2 伤害

---

## 9. MasterData / ScriptableObject 方案

### 9.1 设计原则
- 静态定义全部进 MasterData / ScriptableObject
- 运行时状态与定义数据分离
- 尽量小表拆分，不做万能大表
- 通过 `MasterDataManager` / `IMasterDataService` 加载与查询

### 9.2 建议定义对象
1. `CardDefinition`
2. `CardEffectEntryDefinition`
3. `RelicDefinition`
4. `EnemyDefinition`
5. `EnemyIntentPatternDefinition`
6. `BossPhaseDefinition`
7. `EncounterDefinition`
8. `EventDefinition`
9. `EventOptionDefinition`
10. `ChapterDefinition`
11. `NodeTemplateDefinition`
12. `RewardProfileDefinition`
13. `ShopProfileDefinition`
14. `EconomyBalanceDefinition`
15. `MetaProductDefinition`
16. `LocalizationTextDefinition`（若文案统一走表）

### 9.3 关键字段
#### CardDefinition
- `CardId`
- `DisplayNameKey`
- `Rarity`
- `Cost`
- `CardType`
- `TargetType`
- `ArtAddress`
- `FrameStyle`
- `UpgradeToCardId`
- `FlavorTextKey`

#### CardEffectEntryDefinition
- `CardId`
- `Sequence`
- `EffectType`
- `Value`
- `Times`
- `Duration`
- `ConditionType`
- `ConditionParam`

#### RelicDefinition
- `RelicId`
- `DisplayNameKey`
- `TriggerCondition`
- `EffectType`
- `EffectValue`
- `LimitPerRun`
- `IconAddress`

#### EnemyDefinition
- `EnemyId`
- `DisplayNameKey`
- `Tier`
- `MaxHp`
- `IntentPatternId`
- `RewardGoldMin`
- `RewardGoldMax`
- `PortraitAddress`

#### EnemyIntentPatternDefinition
- `IntentPatternId`
- `OrderIndex`
- `IntentType`
- `Value`
- `RepeatRule`
- `ConditionType`

#### BossPhaseDefinition
- `BossPhaseId`
- `EnemyId`
- `PhaseThresholdHpPercent`
- `IntentPatternId`
- `TransitionFxKey`
- `OneShotBonusEffectType`
- `OneShotBonusValue`

#### EncounterDefinition
- `EncounterId`
- `EncounterType`
- `EnemyId`
- `EventId`
- `ShopProfileId`
- `RewardProfileId`
- `ChapterId`
- `Weight`

#### ChapterDefinition
- `ChapterId`
- `DisplayNameKey`
- `NodeTemplateGroupId`
- `BossEncounterId`
- `DefaultRewardProfileId`
- `BackgroundAddress`

#### ShopProfileDefinition
- `ShopProfileId`
- `CardOfferCount`
- `RelicOfferCount`
- `BaseRemovePrice`
- `RestHealPercent`
- `CanUpgradeCard`

#### EconomyBalanceDefinition
- `BattleGoldMin/Max`
- `EliteGoldMin/Max`
- `BossGold`
- `EventGoldMin/Max`
- `CardPriceCommon/Rare/Epic`
- `RelicPriceMin/Max`

### 9.4 表关系
- `CardDefinition 1 -> N CardEffectEntryDefinition`
- `EnemyDefinition 1 -> N EnemyIntentPatternDefinition`
- `EnemyDefinition 1 -> N BossPhaseDefinition`（Boss 专用）
- `EventDefinition 1 -> N EventOptionDefinition`
- `ChapterDefinition 1 -> N NodeTemplateDefinition`
- `EncounterDefinition -> Enemy/Event/Shop/Reward`

### 9.5 文件组织建议
`DungeonUnity/Assets/ScriptableObjects/MasterData/`
- `Cards/`
- `Relics/`
- `Enemies/`
- `Encounters/`
- `Events/`
- `Economy/`
- `UI/`
- `Meta/`

### 9.6 首阶段优先落地顺序
1. Card / CardEffect
2. Enemy / Intent
3. Reward / Shop / Economy
4. Encounter / Chapter / NodeTemplate
5. Event / MetaProduct

---

## 10. 技术衔接

### 10.1 与 GDD / M1 对齐
- 本 PRD **覆盖并更新** `Docs/vox_dungeon_m1_technical_plan.md` 中较早的 EnemyIntentState 时序表述；若两者冲突，以本 PRD 为唯一权威。
- `EnemyIntentState` 保留为**展示/演出微状态**，不再作为业务上“生成意图”的 authority。
- authority 划分：
  - `TurnStart`：生成并发布“下一敌方意图”
  - `PlayerAction/Resolve`：只消费当前可见意图，不修改“已锁定的下次敌方动作”
  - `EnemyTurn`：执行在上一个 `TurnStart` 已经锁定并展示过的意图
  - `EnemyIntentState`：仅负责 UI 动画/提示条展示，不参与决策
- 保留既定 `TitleScene / MainScene / BattleScene`
- 延续 `BattleFSM`、`EffectChain`、`EnemyAIService`、`BattleCanvas`
- 用 Page 化方式承载地图/战斗/补给/结算，而不是额外拆更多 Scene

### 10.2 建议新增边界对象
- `RunState`：整局地图、牌组、遗物、金币、当前节点
- `BattleSetupFactory`：根据 Encounter + RunState 初始化战斗
- `RewardResolver`：战斗奖励/选卡/晶币结算
- `PageFlowController`：BattleScene 内页面切换
- `MasterDataValidationService`：运行前做表完整性检查

### 10.3 Ownership Matrix（硬边界）
| 对象/服务 | 允许负责 | 明确不负责 |
|---|---|---|
| `BattleContext` | 单场战斗内 HP/护甲/状态/能量/手牌/敌方局部状态 | 地图推进、局内全局货币、章节结算 |
| `RunState` | 整局牌组、遗物、晶币、当前节点、章节进度 | 单帧战斗演出、UI 组件状态 |
| `BattleSetupFactory` | 用 `RunState + EncounterDefinition` 生成 `BattleContext` | 战斗结束后的奖励结算 |
| `RewardResolver` | 将 Battle/Event/Shop/Rest 结果写回 `RunState` | 页面跳转、演出控制 |
| `PageFlowController` | BattleScene 内 Page/Dialog 导航与展示时机 | 直接改领域状态 |
| `MasterDataValidationService` | 启动前校验表完整性、键关联、数值范围 | 运行时平衡调整 |

### 10.4 SceneBridgeData 契约
- `StartRunBridgeData`：从 MainScene 进入 BattleScene 时传递角色、章节、初始 relic/loadout
- `ContinueRunBridgeData`：承载中断继续所需的最小 `RunState` 快照 ID
- `ResultReturnBridgeData`：从 BattleScene 返回 MainScene 时传递通关结果摘要、奖励展示上下文
- 契约原则：BridgeData 只传摘要或引用，不复制完整战斗状态

### 10.5 R3 绑定重点
- `CurrentEnergy`
- `PlayerHP/Armor`
- `EnemyHP/Armor/Intent`
- `HandCards`
- `CanEndTurn`
- `RewardState`
- `SelectedCard`

---

## 11. 风险、假设、开放点

### 11.1 关键风险
1. 项目空壳阶段容易边做边改，造成 PRD 漂移
2. UI Kit 若直接拼装，容易失去晶体朋克识别度
3. MasterData 设计过宽会拖慢首个可玩版本
4. 战斗/地图/商店三线并行时，RunState 边界容易混乱
5. 原 GDD 中“战斗奖励卡战斗后消失”“遗物额外上限 1”已被本 PRD 显式修订，后续实现若混用旧规则会导致规格冲突
6. Boss Phase 需要专门 schema 与状态切换验证，否则内容与数据表不闭合
7. TFramework 当前 MasterData / Scene 仍属可用但需谨慎落地的阶段，必须先验证边界再扩量
8. 自制资源量（Logo、节点图标、卡面、敌人意图图标）可能成为工期主要波动源

### 11.1A 风险缓解矩阵
| Risk | Trigger | Prevention | Detection | Fallback | Owner | Gate |
|---|---|---|---|---|---|---|
| M1 旧 FSM 与新意图时序冲突 | 开发沿用旧 EnemyIntentState 顺序 | 本 PRD 作为唯一权威；先改 FSM 注释与图 | B-01/B-08/Boss intent 用例失败 | 将 EnemyIntentState 降级为纯演出节点 | 客户端负责人 | Gate A |
| 8→20 升级派生卡超时 | Gate A 后升级池实现进度 <50% | 先锁 8 张核心升级卡 | Content Gate 任务燃尽异常 | 其余 12 张仅保留基础卡，无升级分支 | 策划 + 客户端 | Gate B |
| BossPhase 实现复杂度过高 | Phase 切换引发状态机/表设计反复 | 先按 2 phase 固定契约实现 | Boss 用例/日志异常 | **只能触发 re-plan / scope-change；单 phase 降级不能视为 Gate B 通过** | 客户端负责人 | Gate B |
| UI 自制资源爆工时 | 必须自制资源超过 5 类仍未完成 | 先复用 UI Kit 骨架，锁必须自制清单 | UI 任务延误 >2 天 | 卡图/节点图标优先，Banner/活动图后置 | UI/美术 | Gate A/B |
| 文档漂移 | open questions 与 PRD 冲突 | 每次 PRD 冻结同步清问题单 | 审查时发现重复口径 | 以 PRD 为准，清空冲突项 | Leader/Planner | 全阶段 |
| 商业化红线被破坏 | 商品表出现战斗数值项 | MetaProductDefinition 红线校验 | D-02 失败 | 删除相关商品并阻断合并 | 策划 + QA | Gate B |
| 边界串线（RunState/BattleContext/PageFlow） | Page 或 Battle 直接改全局 run 数据 | 严格走 Ownership Matrix；写操作只经 RewardResolver | 边界专项检查失败 / 状态回写异常 | 回退到单一写入口并阻断并行扩张 | 客户端负责人 | Gate A |
| 旧 GDD 规则回归 | 实现重新采用“奖励卡战后消失”或“遗物额外上限 1” | PRD 作为唯一权威；启动前规格走读 | R-01 / R-04 失败或评审发现旧口径 | 立即按 PRD 重置配置与实现 | 策划 + QA | Gate A/B |
| MVP/meta scope creep | 将 Shop/Deck/邮件/任务拉回主线 | Gate 验收矩阵锁定主线页面 | 需求单出现非主线页面阻塞项 | 后置到 Phase 5+，不阻断 MVP | Planner/Leader | 全阶段 |

### 11.2 关键假设
1. 首章只做单角色、单 Boss、单地图
2. run 内成长成立，run 外重置
3. UI 只做竖屏主设计
4. Pixel UI Kit 仅用作视觉资源，不承担逻辑
5. 首版以 ScriptableObject 落地 MasterData

### 11.3 开放点（允许后续调优）
- 稀有度掉落具体权重
- 首章是否引入 1 场双敌战作为可选教学扩展

---

## 12. 完整开发计划

### Phase 0 — 计划冻结与数据骨架（2~3 天）
**目标**：冻结 PRD、UI 线框、MasterData schema、首章内容边界  
**输入**：本 PRD + 现有 GDD / M1 技术方案  
**输出**：
- 冻结版 PRD
- test spec
- MasterData 字段定义
- 低保真 UI 线框
**验收**：不存在 P0 级范围歧义

### Gate A — Playable Gate（内部门，不单独对外）
**目标**：先用灰盒 UI + **10 张基础卡（其中 8 张具升级分支）** + 3 个遗物 + 3 类遭遇打通 `RunState / BattleFSM / PageFlow` 的硬边界。
**通过标准**：单章主链路可跑通，Intent 时序、Reward 回写、SceneBridgeData 均稳定。

### Phase 1 — 核心战斗 Vertical Slice（4~5 天）
**目标**：完成单场战斗闭环  
**输出**：
- BattleScene
- BattleFSM
- BattleContext
- EffectChain
- 3 能量 / 5 手牌 / 敌人意图 / End Turn / 胜败判定
**验收**：通过 `B-01 ~ B-08` 八条核心战斗用例（`B-09` 失败判定可并入 Phase 2 完整流程复核）

### Phase 2 — Run 骨架与地图流（3~4 天）
**目标**：打通从 MainScene 到地图、战斗、奖励、Boss、结算的整章主流程（不含正式 ShopSupplyPage）  
**输出**：
- RunState
- MapExplorePage
- RewardDialog
- ResultPage
- Boss 前补给的灰盒选择
**验收**：能完整跑通一章主流程，无断流、无死页

### Gate B — Content Gate（内部门，不单独对外）
**目标**：在 Playable Gate 稳定后，补齐 20 张基础卡、6 遗物、5 类遭遇、3 事件，以及剩余升级派生卡。
**通过标准**：内容扩量不破坏前序系统边界，MasterData 校验持续通过。

### Phase 3 — 内容接入与首轮平衡（3~4 天）
**目标**：接入 20 卡 / 6 遗物 / 5 遭遇 / 3 事件 / 正式 ShopSupplyPage  
**输出**：
- ScriptableObject 数据资产
- 效果/掉落/价格/奖励表
- ShopSupplyPage
- 首轮平衡参数
**验收**：至少 10 局试玩可完成，且无明显断档套路

### Phase 4 — UI 打磨与最小引导（2~3 天）
**目标**：完成正式 HUD、反馈动画、关键引导  
**输出**：
- 稳定的 Battle HUD
- 地图、商店、结算正式版布局
- 新手提示
**验收**：新玩家 3 分钟内能完成首战并理解节点流

### Phase 5 — 集成验证与候选交付（2~3 天）
**目标**：形成可演示/可继续扩展的 MVP 基线  
**输出**：
- 缺陷清单
- 回归记录
- 平衡修订记录
- 验收报告
**验收**：无 P0/P1 阻断问题

### 建议总工期
- 15~22 个工作日（取决于 UI 自制量、升级派生卡数量、Boss Phase 与测试深度）

---

## 13. 角色分工与执行建议

### 13.1 角色分工
- **策划**：卡牌、遗物、敌人、事件、经济数值、变更控制
- **UI/UX**：线框、信息架构、UI Kit 映射、页面样式
- **客户端**：战斗、RunState、PageFlow、MasterData 读取与验证
- **内容**：ScriptableObject 资产、文案、图标、挂图
- **QA**：规则/数据/UI/流程/经济验证

### 13.2 可用 agent roster
`planner`, `architect`, `critic`, `executor`, `verifier`, `test-engineer`, `designer`, `analyst`, `writer`, `explore`, `researcher`

### 13.3 reasoning 建议
- planner：medium
- architect：high
- critic：high
- executor：medium
- verifier：high
- designer：high
- explore：low

### 13.4 ralph vs team
#### 推荐 `$ralph`
- 当前工程空壳，需要单 owner 串联战斗→地图→数据→UI→验证
- 目标是稳步达成闭环，不是并行吞吐最大化

#### 推荐 `$team`
- 本 PRD 冻结后，可清晰拆分为战斗、流程、UI、数据、QA 多 lane
- 适合已有较强并行协作意愿

### 13.5 team staffing guidance
- Leader：`planner` 或 `team-executor`
- Lane A 战斗：`executor` + `test-engineer`
- Lane B 地图流程：`executor`
- Lane C UI/UX：`designer` + `executor`
- Lane D 数据/内容：`analyst`/`writer` + `executor`
- Lane E 验收：`verifier`

### 13.6 team verification path
1. 每 lane 先交自己的 acceptance evidence
2. 汇总到共享 run-through checklist
3. `verifier` 执行整章 walkthrough
4. 缺陷回流到具体 lane 修复
5. 最后由 leader 对照本 PRD 和 test spec 做 release candidate 判定

