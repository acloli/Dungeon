# Testing Guide

## 概要

このプロジェクトの Battle まわりは、主に EditMode テストで保守しています。  
ロジック、表示仲介、UI coordinator、prefab 整合性を層ごとに分けて確認するのが基本です。

## この機能の責務

- テストの置き場と役割を説明する
- fake / builder の使い分けを整理する
- テスト追加時にどの粒度で書くべきかを示す

## 関連クラス / 関連ディレクトリ

- `DungeonUnity/Assets/Tests/EditMode/`
- `DungeonUnity/Assets/Tests/EditMode/Support/`
- `BattleSceneFlowServiceTests`
- `BattleScenePresenterTests`
- `BattleSceneUiCoordinatorTests`
- `BattleUiPrefabConsistencyTests`

## テストの分担

| テスト | 主な確認内容 |
|---|---|
| `BattleSceneFlowServiceTests` | 戦闘進行、報酬、ショップ、イベント、保存復元 |
| `BattleScenePresenterTests` | snapshot から UI への反映 |
| `BattleSceneUiCoordinatorTests` | page / dialog の開き分け |
| `BattleUiPrefabConsistencyTests` | prefab 命名、継承、参照、UI 挙動 |

## shared support の使い方

- `Support/BattleTestDataBuilders.cs` に共通の test data builder を置く
- Battle 系の DTO 構築は、原則として `BattleTestData` から builder を取り出して始める
- 直接長い constructor を並べず、builder で必要な項目だけ上書きする
- テスト本文では「何を準備したいか」が読める形を優先する

## 追加時の基本方針

- 進行ロジックは `FlowServiceTests`
- 表示整形は `PresenterTests`
- dialog の分岐は `UiCoordinatorTests`
- prefab 参照や button wiring は `PrefabConsistencyTests`

## 変更時の注意点

- 新しい runtime DTO を増やしたら、support builder も更新する
- snapshot の項目を増やしたら、flat field に戻さず section ごとの断言を追加する
- View の serialized field を変えたら prefab consistency テストを更新する
