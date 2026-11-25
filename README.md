# Neko Relic

![Unity](https://img.shields.io/badge/Unity-6000.3.10f1-black?style=flat-square&logo=unity)
![C#](https://img.shields.io/badge/C%23-9.0-blue?style=flat-square)

Neko Relic は、Unity を用いたローグライク・デッキ構築ゲームの個人開発プロジェクトです。2026 年 5 月に開発を開始し、最小のゲームループから、データ駆動の戦闘・構築・探索システムへ段階的に拡張してきました。

当初から、ゲームとしての面白さと、保守・拡張しやすいコードベースの両立を目指しています。自作の TFramework をゲームへ統合しながら、機能追加に伴う責務分割、データ管理、テストの整備を進めています。現在は、構築した単一 Chapter の基盤を、アプリストアへ公開できる品質へ仕上げる段階です。

## ゲームの概要

プレイヤーは分岐マップで進路を選び、ターン制のカードバトルを通じてデッキを構築します。報酬、ショップ、カード強化、Relic / Potion を組み合わせ、Chapter の Boss 撃破を目指します。

## 主な実装

- **タイトルとロビー** — タイトルからロビーへ遷移し、キャラクター選択、初期デッキの確認、図鑑、設定、冒険結果へアクセスできます。
- **冒険の開始と再開** — 選択したキャラクターの RunProfile から新しい冒険を開始し、保存済みの冒険はマップや休憩所・ショップのチェックポイントから再開できます。
- **図鑑と冒険結果** — MasterData をもとにカード・レリック・キャラクターを表示します。カードとレリックの発見状況を保存し、直近の冒険結果では到達階と戦闘種別ごとのクリア数を確認できます。
- **戦闘と探索** — 複数の敵との戦闘、状態効果を反映した敵の行動予告（Intent）、シードに基づく 8 フロアのマップ、イベントや宝箱を実装しています。

コンテンツのバランス、保存の信頼性、モバイル実機での体験は引き続き検証・改善しています。

## 開発の歩み

| 段階                        | 時期               | 積み上げた内容                                                                                                   |
| --------------------------- | ------------------ | ---------------------------------------------------------------------------------------------------------------- |
| Phase 1：基礎サイクル       | 2026 年 5 月       | Unity とフレームワークの導入、シーン遷移、最小バトル、ScriptableObject（SO）による仮データ、初期 EditMode テスト |
| Phase 2：構造とデータの整備 | 5 月下旬〜6 月上旬 | MasterData への移行、View / Presenter と Page / Dialog の整理、Addressables、多敵戦闘、RunProfile 入口           |
| Phase 3：ゲームプレイの拡張 | 6 月〜7 月         | 保存・再開、報酬・ショップ・イベント、Relic / Potion、Upgrade / Exhaust、マップ生成、PlayMode テスト             |
| Phase 4：OutGame の統合と戦闘の改善 | 10 月〜 | タイトルとロビーの刷新、図鑑と冒険結果の接続、新規開始と再開の導線、敵の行動予告と実行の整合性 |

各段階の実装内容、対応する Git 履歴、今後の優先順は [ROADMAP.md](ROADMAP.md) にまとめています。

## 技術的な取り組み

| 課題                       | 実装方針                                                                                | コードの入口                                                                                                   |
| -------------------------- | --------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| 機能追加と責務の整理       | VContainer による DI、Model / Service / Presenter / View の分離、用途別サービスへの抽出 | [BattleSceneFlowService](DungeonUnity/Assets/Scripts/Runtime/InGame/Battle/Services/BattleSceneFlowService.cs) |
| 定義データと実行状態の分離 | 初期 SO データから CSV / MasterData へ移行し、Facade で runtime DTO に変換              | [BattleMasterDataFacade](DungeonUnity/Assets/Scripts/Runtime/InGame/Battle/Services/BattleMasterDataFacade.cs) |
| 複雑化する UI の更新       | State から Snapshot を構築し、Presenter / Coordinator を通じて Page / Dialog に反映     | [BattleSnapshotFactory](DungeonUnity/Assets/Scripts/Runtime/InGame/Battle/Services/BattleSnapshotFactory.cs)   |
| ロビーの画面遷移と入力管理 | TFramework と Addressables で Page / Dialog を開き、画面のライフサイクルに合わせて入力を登録・解除 | [MainSceneUiCoordinator](DungeonUnity/Assets/Scripts/Runtime/OutGame/Main/Controller/MainSceneUiCoordinator.cs) |
| カード図鑑の表示コスト     | スクロールの可視範囲に応じてセルを再利用し、表示データと入力を差し替え                   | [RecyclingGridView](DungeonUnity/Assets/Scripts/Runtime/Common/UI/List/RecyclingGridView.cs) |
| 図鑑と冒険結果の保存       | 再開用のチェックポイントとは別に、カード・レリックの発見状況と直近の冒険結果を保存          | [CollectionProgressService](DungeonUnity/Assets/Scripts/Runtime/InGame/Save/Services/CollectionProgressService.cs) |
| 探索と復元の再現性         | seed によるマップ生成、接続に基づく移動判定、チェックポイント保存                       | [BattleMapGenerator](DungeonUnity/Assets/Scripts/Runtime/InGame/Battle/Services/BattleMapGenerator.cs)         |
| 変更時の回帰確認           | EditMode で戦闘進行やデータ変換を、PlayMode でロビーの画面遷移や入力の復帰を確認          | [EditMode の例](DungeonUnity/Assets/Tests/EditMode/BattleSceneFlowServiceTests.cs) / [PlayMode の例](DungeonUnity/Assets/Tests/PlayMode/OutGame/MainMenuNavigationPlayModeTests.cs) |

非同期処理には UniTask、シーン・UI・保存・ローカライズ・データ基盤には TFramework を使用します。R3 も依存に含まれます。現在の Battle の表示更新は Snapshot を中心に構成しています。

MasterData の編集元は別リポジトリ Dungeon-data の CSV です。生成済み容器は ScriptableObject ですが、初期の手編集するゲーム定義 SO からは移行済みです。

## 主要ディレクトリ

| パス                                                | 用途                             |
| --------------------------------------------------- | -------------------------------- |
| `DungeonUnity/`                                     | Unity プロジェクト               |
| `DungeonUnity/Assets/Scripts/Runtime/`              | Battle、Save、OutGame、SceneFlow |
| `DungeonUnity/Assets/Scripts/Generated/MasterData/` | 自動生成されたデータ型           |
| `DungeonUnity/Assets/Data/MasterData/`              | インポート済みデータ容器         |
| `DungeonUnity/Assets/Tests/`                        | EditMode / PlayMode テスト       |

## 開発環境と検証

Unity 6000.3.10f1 / C# 9.0 を使用します。Unity Hub で `DungeonUnity/` を開き、コンパイルとビルドを Unity Editor で行います。テストは Window → General → Test Runner から実行します。

機能の実装と、公開品質の検証は段階を分けて進めています。検証結果には対象バージョンと条件を記録し、過去の結果を現在の合格保証として扱いません。
