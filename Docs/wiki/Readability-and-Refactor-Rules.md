# Readability and Refactor Rules

## 概要

Battle モジュールでは、機能追加が進むにつれて引数列と constructor 呼び出しが長くなりやすい傾向があります。  
このページでは、可読性を保ちながら機能追加を続けるための整理方針をルール化しています。

## この機能の責務

- 長い引数列を避けるための判断基準を共有する
- Param / Builder / Factory の使い分けを明確にする
- 可読性悪化を防ぐための実践ルールを残す

## 関連クラス / 関連ディレクトリ

- `BattleCardSelectDialogParam`
- `BattleSceneSnapshotBuilder`
- `BattleMultiIconViewModel`
- `BattleIntentViewModel`
- `BattleMasterDataFacade`
- `Assets/Tests/EditMode/Support/BattleTestDataBuilders.cs`

## 使い分けの基準

### Param を使う場面

- 1 回の要求として意味がまとまっているとき
- UI coordinator や dialog へ渡す入力をまとめたいとき
- 呼び出し側と受け取り側の両方で同じ意味単位を共有したいとき

例:

- `BattleCardSelectDialogParam`

### Builder を使う場面

- 項目数が多く、毎回すべてを設定しないとき
- テストや snapshot のように named で読む価値が高いとき
- 順番よりも項目名の理解が重要なとき

例:

- `BattleSceneSnapshotBuilder`
- test data builder 群

### Factory を使う場面

- 呼び出し側に並ぶ位置引数の意味を隠したいとき
- 生成パターンが複数あり、名前で使い分けたいとき
- ViewModel のように用途別生成がはっきりしているとき

例:

- `BattleMultiIconViewModel.CreateCard`
- `BattleMultiIconViewModel.CreateRelic`
- `BattleIntentViewModel.FromAction`

## 避けたい書き方

- UI 境界で 8 個以上の raw 引数を渡す
- 同じまとまりのデータを Presenter と Coordinator の両方で詰め直す
- テストで長い constructor を直接何度も書く
- `Manager` のような広すぎる責務へ逃がす

## 今後の判断ルール

- 引数が増えたときは、まず「意味のある要求単位」にまとめられないかを見る
- constructor が読みにくくなったときは、DTO 自体を分割する前に builder / factory で改善できるか検討する
- 生成 helper を増やすときは、名前から用途が想像できることを優先する
- read model が複数の page / dialog をまたぎ始めたら、まず UI 境界ごとに section へ分けられないかを検討する
- View は ViewModel / Param を受け取る側に寄せ、View の中で domain 意味を再構成しない

## 状態の集中化ルール

`BattleSceneState` のように複数 Service から読み書きされる状態クラスでは、次のルールで書き込みの散在を防ぐ。

- 複数フィールドを常に同時に更新しなければ不変条件が崩れるパターンは、状態クラス自身の public メソッドとして集約する
- 各 Service は生フィールド代入を直接書かず、集約メソッドを呼ぶ
- 例：`ClearPendingRewards()` / `PrepareForNewBattle()` / `ClearOwnedInspections()`

## 重複ロジックの整理ルール

異なるクラスに同一実装の private メソッドが存在する場合、以下の優先順位で一元化を検討する。

1. **既存の共有インターフェースに委譲できるか**  
   例：`GetSelectedEnemy()` → `IBattleEnemyActionSelector` が既に定義されていたため、注入して使う
2. **状態クラスのメソッドとして昇格できるか**  
   例：`CanMoveToNode()` → `BattleSceneState` が状態を保持しているため自然な居場所
3. **static helper に抽出するか**  
   上記 2 つが不可能な場合の最終手段。優先度は低い

## インターフェース分割の判断基準

- Query（読取）と Command（書込）に分離できる場合は小さいインターフェースに分割する
- 消費側（Presenter）が実際に使うメソッドだけを公開する
- テスト Fake のメソッド数が減らなければ、現時点では分割の価値は薄い（consumer が 1 つの場合は特に）
- 継承 (`IFlowService : IQueryService`) で後方互換を保ちながら段階的に拡張できる

## 変更時の注意点

- 可読性改善だけの refactor でも、テスト helper と Wiki を一緒に更新する
- 新しいルールを導入したら、このページへ短く追記する
- 既存ルールと競合する場合は上位のプロジェクト規約を優先する
- 重複ロジックを一元化したら、削除元のテスト呼び出し（コンストラクタ引数など）も必ず更新する
