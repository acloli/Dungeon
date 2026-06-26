# Dungeon Wiki

## 概要

このディレクトリは、Vox Dungeon の実装を機能単位で追えるように整理した開発用 Wiki です。  
初めてコードを読むときに「どこから見ればよいか」「何がどの責務を持つか」を短時間でつかめることを目的にしています。

## この機能の責務

- プロジェクト理解の入口を提供する
- 各機能ページへの導線をまとめる
- ディレクトリと責務の対応を素早く確認できるようにする

## 関連クラス / 関連ディレクトリ

- `DungeonUnity/Assets/Scripts/Runtime/InGame/Battle/`
- `DungeonUnity/Assets/Scripts/Runtime/InGame/Save/`
- `DungeonUnity/Assets/Scripts/Runtime/SceneFlow/`
- `DungeonUnity/Assets/Tests/EditMode/`
- `Docs/wiki/`

## 読み進める順番

1. [Project Overview](./Project-Overview.md)
2. [Battle Architecture](./Battle-Architecture.md)
3. [Battle Data Flow](./Battle-Data-Flow.md)
4. [Battle UI and Dialogs](./Battle-UI-and-Dialogs.md)
5. [Save and SceneFlow](./Save-and-SceneFlow.md)
6. [Testing Guide](./Testing-Guide.md)
7. [Readability and Refactor Rules](./Readability-and-Refactor-Rules.md)

## 主要ディレクトリと役割

| パス | 役割 |
|---|---|
| `Runtime/InGame/Battle` | 戦闘進行、表示仲介、UI 表示モデル |
| `Runtime/InGame/Save` | Run の保存と再開 |
| `Runtime/SceneFlow` | シーン間の受け渡しデータ |
| `Generated/MasterData` | 自動生成された MasterData 型 |
| `Tests/EditMode` | Battle / Save / Presenter などの EditMode テスト |

## データやイベントの流れ

- MasterData から runtime DTO を組み立てる
- `BattleSceneFlowService` が進行を更新する
- `BattleSnapshotFactory` が page / dialog ごとに使い分ける section snapshot を含む View 用 snapshot を作る
- Presenter と Coordinator が UI へ反映する

## 変更時の注意点

- `Generated/MasterData` は手動編集しない
- 新しい Battle 機能を追加するときは、関連する Wiki ページも一緒に更新する
- 説明が長くなったら新しいページへ分割し、Home には索引だけを残す
