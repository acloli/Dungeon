# Battle UI and Dialogs

## 概要

Battle 画面では、常駐 UI とモーダル UI を分けて扱っています。  
戦闘本体は host view と battle page が担当し、Reward / Shop / Event / CardSelect などは dialog として開きます。

## この機能の責務

- Battle UI の表示責務を整理する
- どの画面が page で、どの画面が dialog かを説明する
- UI 追加時の基本ルールを共有する

## 関連クラス / 関連ディレクトリ

- `View/BattleSceneView`
- `View/BattlePageView`
- `View/MapPage`
- `View/RewardDialog`
- `View/ShopDialog`
- `View/EventDialog`
- `View/CardSelectDialog`
- `View/PotionReplaceDialog`
- `BattleSceneUiCoordinator`

## 画面の分担

| 画面 | 主な役割 |
|---|---|
| `BattleSceneView` | host と常駐 chrome の表示 |
| `BattlePageView` | 戦闘中の HUD、手札、敵ボタン |
| `MapPage` | マップ進行 |
| `RewardDialog` | 戦闘後報酬 |
| `ShopDialog` | ショップ購入 |
| `EventDialog` | イベント選択 |
| `CardSelectDialog` | Upgrade / CardRemoval 選択 |
| `PotionReplaceDialog` | ポーション入れ替え |

## 表示の流れ

- Presenter が現在の page を見て Coordinator を呼ぶ
- Coordinator が page または dialog を開く
- page / dialog は自分に必要な section snapshot または専用 param object を受け取る
- 各 View は表示に必要な情報だけを使い、進行ロジックを持たない

## 変更時の注意点

- dialog を増やすときは `BattleSceneUiCoordinator` を入口にする
- View に進行判断を入れない
- 表示専用の整形が必要なら、まず `BattleSnapshotFactory` と ViewModel 追加で吸収できないかを考える
