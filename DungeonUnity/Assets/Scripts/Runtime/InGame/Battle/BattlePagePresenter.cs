using System;
using System.Collections.Generic;
using System.Text;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Battle.View;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// 戦闘画面仲介クラス
    /// </summary>
    public sealed class BattlePagePresenter
    {
        private IBattlePageView _view;
        private Action<int> _onHandCardClicked;
        private Action<int> _onOwnedRelicClicked;
        private Action<int> _onEnemyTargetClicked;
        private Action _onEndTurnClicked;

        /// <summary>
        /// View接続初期化
        /// </summary>
        public void Initialize(IBattlePageView view, Action<int> onHandCardClicked, Action<int> onOwnedRelicClicked, Action<int> onEnemyTargetClicked, Action onEndTurnClicked)
        {
            _view = view;
            _onHandCardClicked = onHandCardClicked;
            _onOwnedRelicClicked = onOwnedRelicClicked;
            _onEnemyTargetClicked = onEnemyTargetClicked;
            _onEndTurnClicked = onEndTurnClicked;
            _view.WireButtons(_onEndTurnClicked);
        }

        /// <summary>
        /// 画面描画処理
        /// </summary>
        public void Render(BattleSceneSnapshot snapshot)
        {
            if (_view == null)
            {
                return;
            }

            _view.BuildHandCards(snapshot.HandCards, _onHandCardClicked);
            _view.BuildOwnedRelics(snapshot.OwnedRelics, _onOwnedRelicClicked);
            _view.BuildEnemyButtons(snapshot.Enemies, snapshot.SelectedEnemyIndex, _onEnemyTargetClicked);
            _view.SetBattleStateText(
                BuildPlayerText(snapshot),
                BuildEnemyText(snapshot),
                snapshot.BattleHintMessage);
            _view.SetPileCounters(snapshot.DrawPileCount, snapshot.DiscardPileCount, snapshot.HandCount, snapshot.MaxHandCount);
            _view.SetBattleHud(BuildBattleHud(snapshot));
        }

        /// <summary>
        /// 動的要素消去
        /// </summary>
        public void Clear()
        {
            if (_view == null)
            {
                return;
            }
            _view.ClearDynamicButtons();
        }

        /// <summary>
        /// View切り離し処理
        /// </summary>
        public void Dispose()
        {
            if (_view == null)
            {
                return;
            }
            _view.UnwireButtons();
            _view = null;
        }

        /// <summary>
        /// player表示文言構築
        /// </summary>
        private static string BuildPlayerText(BattleSceneSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(BuildPlayerSummary(snapshot));
            AppendStatusLine(builder, BattleSceneConstants.StatusLabel, snapshot.PlayerStatuses);
            AppendStatusLine(builder, BattleSceneConstants.BuffLabel, snapshot.PlayerBuffs);
            return builder.ToString();
        }

        /// <summary>
        /// enemy表示文言構築
        /// </summary>
        private static string BuildEnemyText(BattleSceneSnapshot snapshot)
        {
            BattleEnemyViewModel selectedEnemy = FindSelectedEnemy(snapshot);
            if (selectedEnemy != null)
            {
                return BuildEnemyViewText(selectedEnemy);
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(
                BattleSceneConstants.EnemyStateFormat,
                snapshot.CurrentEnemy != null ? snapshot.CurrentEnemy.DisplayName : BattleSceneConstants.UnknownEnemyName,
                snapshot.EnemyHp,
                snapshot.EnemyBlock);
            AppendIntentLine(builder, snapshot.EnemyIntent);
            AppendStatusLine(builder, BattleSceneConstants.StatusLabel, snapshot.EnemyStatuses);
            AppendStatusLine(builder, BattleSceneConstants.BuffLabel, snapshot.EnemyBuffs);
            return builder.ToString();
        }

        /// <summary>
        /// 選択中敵表示モデル取得
        /// </summary>
        private static BattleEnemyViewModel FindSelectedEnemy(BattleSceneSnapshot snapshot)
        {
            if (snapshot.Enemies == null || snapshot.Enemies.Count == 0)
            {
                return null;
            }

            if (snapshot.SelectedEnemyIndex >= 0 && snapshot.SelectedEnemyIndex < snapshot.Enemies.Count)
            {
                return snapshot.Enemies[snapshot.SelectedEnemyIndex];
            }

            return snapshot.Enemies[0];
        }

        /// <summary>
        /// 敵詳細表示文言構築
        /// </summary>
        private static string BuildEnemyViewText(BattleEnemyViewModel enemy)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(BuildEnemySummary(enemy));
            AppendIntentLine(builder, enemy.Intent);
            AppendStatusLine(builder, BattleSceneConstants.StatusLabel, enemy.Statuses);
            AppendStatusLine(builder, BattleSceneConstants.BuffLabel, enemy.Buffs);
            return builder.ToString();
        }

        /// <summary>
        /// 戦闘HUD表示モデル構築
        /// </summary>
        private static BattleHudViewModel BuildBattleHud(BattleSceneSnapshot snapshot)
        {
            BattleEnemyViewModel selectedEnemy = FindSelectedEnemy(snapshot);
            if (selectedEnemy != null)
            {
                return new BattleHudViewModel(
                    BuildPlayerSummary(snapshot),
                    BuildEnemySummary(selectedEnemy),
                    BuildIntentSummary(selectedEnemy.Intent),
                    snapshot.PlayerStatuses,
                    snapshot.PlayerBuffs,
                    selectedEnemy.Statuses,
                    selectedEnemy.Buffs);
            }

            return new BattleHudViewModel(
                BuildPlayerSummary(snapshot),
                BuildFallbackEnemySummary(snapshot),
                BuildIntentSummary(snapshot.EnemyIntent),
                snapshot.PlayerStatuses,
                snapshot.PlayerBuffs,
                snapshot.EnemyStatuses,
                snapshot.EnemyBuffs);
        }

        /// <summary>
        /// player概要文言構築
        /// </summary>
        private static string BuildPlayerSummary(BattleSceneSnapshot snapshot)
        {
            return string.Format(
                BattleSceneConstants.PlayerStateFormat,
                snapshot.PlayerHp,
                snapshot.PlayerMaxHp,
                snapshot.PlayerBlock,
                snapshot.PlayerEnergy,
                snapshot.Gold);
        }

        /// <summary>
        /// enemy概要文言構築
        /// </summary>
        private static string BuildEnemySummary(BattleEnemyViewModel enemy)
        {
            return string.Format(
                BattleSceneConstants.EnemyStateFormat,
                enemy.DisplayName,
                enemy.Hp,
                enemy.Block);
        }

        /// <summary>
        /// 代替enemy概要文言構築
        /// </summary>
        private static string BuildFallbackEnemySummary(BattleSceneSnapshot snapshot)
        {
            return string.Format(
                BattleSceneConstants.EnemyStateFormat,
                snapshot.CurrentEnemy != null ? snapshot.CurrentEnemy.DisplayName : BattleSceneConstants.UnknownEnemyName,
                snapshot.EnemyHp,
                snapshot.EnemyBlock);
        }

        /// <summary>
        /// intent概要文言構築
        /// </summary>
        private static string BuildIntentSummary(BattleIntentViewModel intent)
        {
            if (intent == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(BattleSceneConstants.IntentLabelFormat, intent.IntentName);
            if (intent.Damage > 0)
            {
                builder.AppendFormat(BattleSceneConstants.IntentDamageFormat, intent.Damage, Math.Max(1, intent.HitCount));
            }

            if (intent.Block > 0)
            {
                builder.AppendFormat(BattleSceneConstants.IntentBlockFormat, intent.Block);
            }

            if (intent.StatusType != Game.MasterData.Generated.StatusType.None && intent.StatusValue > 0)
            {
                builder.AppendFormat(BattleSceneConstants.IntentStatusFormat, intent.StatusName, intent.StatusValue);
            }

            if (intent.BuffType != Game.MasterData.Generated.BuffType.None && intent.BuffValue > 0)
            {
                builder.AppendFormat(BattleSceneConstants.IntentBuffFormat, intent.BuffName, intent.BuffValue);
            }

            return builder.ToString();
        }

        /// <summary>
        /// intent表示行追加
        /// </summary>
        private static void AppendIntentLine(StringBuilder builder, BattleIntentViewModel intent)
        {
            if (intent == null)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendFormat(BattleSceneConstants.IntentLabelFormat, intent.IntentName);
            if (intent.Damage > 0)
            {
                builder.AppendFormat(BattleSceneConstants.IntentDamageFormat, intent.Damage, Math.Max(1, intent.HitCount));
            }

            if (intent.Block > 0)
            {
                builder.AppendFormat(BattleSceneConstants.IntentBlockFormat, intent.Block);
            }

            if (intent.StatusType != Game.MasterData.Generated.StatusType.None && intent.StatusValue > 0)
            {
                builder.AppendFormat(BattleSceneConstants.IntentStatusFormat, intent.StatusName, intent.StatusValue);
            }

            if (intent.BuffType != Game.MasterData.Generated.BuffType.None && intent.BuffValue > 0)
            {
                builder.AppendFormat(BattleSceneConstants.IntentBuffFormat, intent.BuffName, intent.BuffValue);
            }
        }

        /// <summary>
        /// 状態表示行追加
        /// </summary>
        private static void AppendStatusLine(StringBuilder builder, string label, IReadOnlyList<BattleStatusViewModel> statuses)
        {
            if (statuses == null || statuses.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.Append(label);
            builder.Append(BattleSceneConstants.LabelSeparator);
            for (int i = 0; i < statuses.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(BattleSceneConstants.ValueSeparator);
                }

                BattleStatusViewModel status = statuses[i];
                builder.AppendFormat(BattleSceneConstants.StatusValueFormat, status.Name, status.Value);
            }
        }
    }
}
