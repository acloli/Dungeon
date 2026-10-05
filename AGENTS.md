# AGENTS.md — Dungeon

## プロジェクト

Vox Dungeon は Unity 6000.3.10f1 / C# 9.0 のローグライク・カード構築ゲーム。VContainer、UniTask、TFramework を使用する。Unity プロジェクトは `DungeonUnity/`、ゲームコードは `Assets/Scripts/Runtime/`、テストは `Assets/Tests/` にある。

MasterData の編集元は別リポジトリ Dungeon-data。TFramework の実効バージョンは `DungeonUnity/Packages/packages-lock.json` で確認する。Battle の表示更新は状態 → Snapshot → Presenter / Coordinator → View。

## 必須制約

- 業務ロジックは Model / Service に置き、View は表示と入力に限定する。依存は DI で渡し、`Container.Resolve<T>()` を使わない。インターフェースは境界・差し替え・テストの必要性に応じて導入する。
- 非同期処理は `CancellationToken` を受け取り、下位へ伝播する。`async void` と例外の握りつぶしを禁止する。
- `Generated/MasterData/` を手編集しない。CSV → generator → Facade → runtime DTO の経路を守る。ローカライズ CSV は Dungeon と Dungeon-data の両方へ同期する。
- Scene / Prefab / GameObject の変更には Unity MCP を使う。接続できない場合も `.unity` / `.prefab` の直接編集で迂回せず、`.meta` を手作成しない。
- 業務 UI のクラス名・Prefab 名・Addressables key を一致させ、Page / Dialog は CommonPage / CommonDialog の variant とする。
- 識別子は英語、コードコメント・XML Summary は自然な日本語とし、コメントに `。` を使わない。設計文書・PR 本文はユーザー指定がなければ日本語。

## 検証

- `DungeonUnity/` を Unity Hub で開く。コード変更後は Unity Console にコンパイルエラーがないことと、Test Runner（Window → General → Test Runner）の関連 EditMode テストを確認する。UI / Scene 変更では関連 PlayMode も確認する。独立した CLI テストランナーはない。
- 変更前に関連コードとテストを読む。合格させるためにテストを削除・弱体化したり、期待値を実装に合わせて変更したりしない。
- 実行した検証と未実行項目を区別して報告する。過去の合格記録を現在の結果として扱わない。

## 変更範囲と Git

- 既存のユーザー差分を戻さず、依頼と無関係な変更を混ぜない。仕様は確認済みの要件と現行コードを照合して判断する。
- 複数段階の機能開発は、独立して実装・検証できるタスクへ分割する。PR 本文には実装意図・変更範囲・検証結果を記す。
- コミットは独立して検証・取消可能な単位に分ける。メッセージは英語の `feat:` / `fix:` など、scope なし。ステージ後に `git diff --cached` を確認し、指示なしに push しない。
