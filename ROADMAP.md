# Vox Dungeon - Development Roadmap

本ドキュメントは、Vox Dungeon の中長期的な開発の方向性を示すロードマップです。
開発の進行状況や技術的な検証結果に基づいて、計画は柔軟に変更される可能性があります。

> **最終更新: 2026-07-10** — Phase 3 の主要 gameplay core は完了。Shop/Event/報酬多様化・RunSave・多敵戦闘・Battle 表示層 UI・Relic/Potion core・Relic trigger 拡張・Potion capacity runtime/MasterData 接続・Upgrade core・Exhaust core・Map auto-generation・Targeted Potion Use まで実装済み。次の重点は Potion/Relic content pool 拡張、Map polish。

---

## 🟢 Phase 1: 基礎サイクルの構築 (M1) - [完了]

ゲームとしての最低限のループを成立させ、アーキテクチャの骨組みを検証するフェーズ。

- [x] **シーン遷移基盤:** Title -> Main -> Battle -> Result の遷移フロー確立
- [x] **バトルシステムのコア:** FSM (Finite State Machine) を用いたターン制バトルの進行管理
- [x] **アーキテクチャの導入:** VContainer(DI) と R3(Rx) による疎結合なイベント制御
- [x] **データ駆動のプロトタイプ:** ScriptableObject を用いた仮データ（カード、敵、マップ）によるゲーム駆動

---

## 🟢 Phase 2: アーキテクチャの洗練とUIの高度化 - [完了]

プロトタイプのコードを整理し、よりスケーラブルな構造へとリファクタリングを行うフェーズ。

- [x] **UIの責務分割:** `BattleSceneController` の整理、View / Presenter パターンへのリファクタリング（TFramework の Page/Dialog フロー移行）
- [x] **正式なデータパイプライン:** CSV/Excel を用いたマスターデータ自動生成ワークフローを導入し、Battle runtime の参照先を MasterData ベースへ移行
- [x] **リソース管理の最適化:** Addressables を本格運用し、シーン・プレハブの非同期ロードを実装（Title/Main/Battle scene を Addressables 登録済み）
- [x] **UIUXの改善:** Battle 表示層専用 UI への移行（Intent / Status / Buff の専用表示領域）、ターゲット先行操作による誤操作防止 UX の導入
- [x] **多敵戦闘対応:** 単体敵から複数敵 formation へ拡張、敵ごとの HP/Block/Intent/Status/Buff 管理、`TargetSide.AllEnemies` 対応
- [x] **RunProfile 入口整理:** MainScene から BattleScene への RunProfile 基準導線の確立、MapTemplate 分岐マップ対応
- [x] **旧 ScriptableObject 削除:** カード・敵・マップ・Run 定義の旧 SO asset を全削除し、MasterData 一本化

### 完了時点の成果

- Unity compile error ゼロ
- EditMode test: **46/46 Passed**
- 全シーン遷移が Addressables + TFramework `ISceneService` 経由に統一

---

## 🟢 Phase 3: ゲームプレイの拡張とシステム深化 - [完了]

ローグライク・デッキ構築ゲームとして必要な複雑な仕様を実装し、設計の堅牢性を検証するフェーズ。

### 完了済み

- [x] **RunSave によるゲーム内状態保存:** 進行状況の保存と再開機能（TFramework `ISaveDataService` 活用、Map/RestShop でのオートセーブ、Save&Quit 導線、MainScene Continue ボタン）
- [x] **報酬の多様化:** Card 以外の Gold / Potion / Relic 報酬に対応、`RewardDialog` の動的行生成、Slay the Spire 方式の報酬モデル再設計
- [x] **Shop 機能:** `ShopDialog` / `CardSelectDialog` 新設、カード購入・削除・売り切れ表示、価格計算（MasterData 駆動）、RunSave 対応
- [x] **Event 機能:** `EventDialog` 新設、分岐選択肢（HP減少/最大HP増加/Gold獲得）、ランダムイベント抽選、localization 対応
- [x] **サービス責務分離:** `BattleRewardService` / `BattleShopService` / `BattleEventService` / `BattleSnapshotFactory` / `BattleDisplayTextService` を新設し `BattleSceneFlowService` の責務を分散
- [x] **Facade 分割:** `EventMasterDataFacade` / `ShopMasterDataFacade` を新設し God Class 化を防止
- [x] **UI 表示モデル:** `BattleDisplayViewModels`（Intent / Status / Buff / ShopItem）を導入し表示情報を型安全に集約
- [x] **高度なカードエフェクト:** Status（Weak / Vulnerable / Slimed）と Buff（Strength / Ritual / Enrage）の MasterData 駆動処理
- [x] **エネミーAI の拡張:** 複数行動パターン（OpeningOnly / RepeatAfterOpening / AfterOpeningRandom / Random / Cycle）、多敵 formation 対応
- [x] **Battle 表示基盤の再構築:** `MultiIcon` / `CardIcon` / `RelicIcon` / `PotionIcon` を導入し、手札・Shop・Reward・選択UIの表示を共通化
- [x] **Draw / Discard / Hand 表示:** 山札/弃牌/手牌枚数カウンタを Battle HUD に追加
- [x] **Relic コア実装:** 所持、Save/Load、Shop/Reward 取得、パッシブ効果、ingame relic strip を実装
- [x] **Potion コア実装:** 所持、Save/Load、Shop/Reward 取得、入れ替え、ingame potion strip、直接使用フローを実装
- [x] **Ingame host chrome:** battle page 依存ではない relic / potion 常駐表示を BattleScene host に移行
- [x] **Combat event hook:** `OnCombatStart` / `OnPlayerTurnStart` / `OnPlayerTurnEnd` / `OnCardPlayed` / `OnPlayerDamaged` を追加
- [x] **Relic trigger 拡張:** `OnShuffle` / `OnCardExhausted` / `OnLoseHp` を追加し、戦闘中の主要イベントから Relic effect を発火できるようにした
- [x] **Potion capacity ランタイム化:** `MaxPotionCount` を runtime state / RunSave / MasterData mapping に接続し、`RelicEffectMaster.PotionCapacityDelta` から容量変更 relic を表現できるようにした
- [x] **Battle modal/input 整理:** framework dialog の重なり順修正、host chrome input freeze、inspect state cleanup を実装
- [x] **Upgrade コア実装:** RestShop からカード強化選択へ遷移し、`UpgradeCardId` の置換、Gold 消費、Save/Load 永続化を実装
- [x] **Exhaust コア実装:** `CardMaster.ExhaustsOnPlay` から `RuntimeCard.ExhaustsOnPlay` へ接続し、使用カードを `DiscardPile` ではなく `ExhaustPile` へ移動
- [x] **Pile inspect UI:** Draw / Discard / Exhaust / Hand count 表示と、`PileInspectDialog` による pile 内容確認を実装
- [x] **マップ自動生成:** `BattleMapGenerator` による seed 駆動の決定論的マップ生成、DI 登録、RunSave の `MapSeed` / `MapLayoutVersion` 復元を実装
- [x] **Targeted Potion Use:** 使用後に対象待ち状態へ入り、敵クリックで単体対象ポーションを発動。`AllEnemies` は即時発動。`BattlePotionUseTarget` / `PendingPotionUseIndex` による状態管理、効果解決 (DealDamage/ApplyStatus)、無効対象時の非消費、全敵撃破時の Reward flow 遷移を実装

### Phase 3 完了時点の残る拡張候補

- [ ] **Potion / Relic content pool 拡張:** 容量変更系 relic、追加 potion、追加 passive / combat relic、バランス調整用の MasterData を拡充する
- [ ] **Map polish:** act-scale の大きいマップ、宝箱などの特殊ノード、経路線表示を追加する

---

## ⚪ Phase 4: ポリッシュとパフォーマンス最適化 - [計画中]

プロジェクトの最終的な完成度を高めるフェーズ。

- [ ] **VFX / Audio:** パーティクルシステムやサウンド管理機能の統合
- [ ] **ローカライズ:** TFramework のローカライゼーション機能を用いた多言語対応の完了（現在は日本語中心、一部 key 対応済み）
- [ ] **プロファイリング:** Unity Profiler, Memory Profiler を用いたボトルネックの特定とパフォーマンス最適化
- [ ] **自動テスト:** 重要なビジネスロジック（ダメージ計算やカード解決）に対するユニットテスト（NUnit）の拡充
- [ ] **カード強化（Upgrade）ポリッシュ:** 強化前後比較や演出、より分かりやすい UI
- [ ] **Potion / Relic 拡張:** 追加 content pool、効果バリエーション、報酬/Shop 出現率とバランス調整
- [ ] **Map 表示 polish:** 生成済みノードの経路線、フロア配置、特殊ノード表示を強化
