# Dungeon

![Unity](https://img.shields.io/badge/Unity-6+-black?style=flat-square&logo=unity)
![C#](https://img.shields.io/badge/C%23-9.0-blue?style=flat-square&logo=c-sharp)
![Architecture](https://img.shields.io/badge/Architecture-VContainer%20%7C%20R3%20%7C%20UniTask-brightgreen?style=flat-square)

Project Vox Dungeon(仮) は、Unityを用いたローグライク・デッキ構築型ゲームの基盤システムの実装と、モダンなゲームアーキテクチャの研究を目的とした個人開発プロジェクトです。


## 概要 (Overview)

本プロジェクトは、プレイヤーがマップを進行し、カードを用いて敵と戦闘を行うターン制のデッキ構築ゲームのコアサイクルを実装しています。
ゲームとしての面白さだけでなく、「保守性が高く、拡張容易なコードベース」を構築することを目的としています。

## 技術的な特徴 (Technical Features)

本プロジェクトでは、近年のUnity開発におけるモダンなアプローチを積極的に採用しています。

* **依存性注入 (Dependency Injection):**
  [VContainer](https://vcontainer.hadashikick.jp/) を採用し、システム間の結合度を下げることで、テスト容易性とモジュール性を向上させています。
* **リアクティブプログラミング (Reactive Programming):**
  [R3](https://github.com/Cysharp/R3) を用いたイベント駆動設計でUIの更新や非同期処理（エフェクトチェーンの解決など）を宣言的かつクリーンに記述しています。
* **非同期処理 (Async/Await):**
  [UniTask](https://github.com/Cysharp/UniTask) を採用し、async/awaitで処理を記述しています。
* **ステートマシン (FSM) による進行管理:**
  複雑なターン制バトルのフェーズ（ターンの開始、プレイヤー行動、カード解決、敵の意図表示・行動など）を厳格なFinite State Machineで管理しています。
* **データ駆動設計 (Data-Driven Design):**
  カードの効果、敵のステータス、ランの初期状態などは `ScriptableObject` として定義されており、エンジニア以外でも調整が容易な基盤を作成しています。
* **独自フレームワーク `TFramework` の統合:**
  自作の基盤フレームワークを活用し、シーン遷移やUI管理を抽象化しています。

## ディレクトリ構造 (Directory Structure)

```text
.
├── DungeonUnity/                  # Unityプロジェクトルート
│   ├── Assets/
│   │   ├── Scripts/               # コアロジック (Runtime/Editor)
│   │   ├── Prefabs/               # UIRootなどのコアプレハブ
│   │   ├── ScriptableObjects/     # ゲームデータ (Mock)
│   │   └── ...
├── ROADMAP.md                     # 今後の開発マイルストーン
└── README.md
```

## 開発ロードマップ (Roadmap)

今後の開発方針や予定されている技術的な課題については、以下のドキュメントを参照してください。

**[ROADMAP.md](./ROADMAP.md)** を見る

## 免責事項 (Disclaimer)

本プロジェクトはコアとなる技術基盤とゲームループの公開を目的としています。そのため、商用利用を前提としたアセットや、ゲームデザインに関するドキュメント（GDD等）、プロダクション向けのバランスデータなどはリポジトリに含まれておりません。予めご了承ください。
