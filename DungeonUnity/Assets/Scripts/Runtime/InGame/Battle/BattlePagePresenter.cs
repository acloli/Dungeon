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
        private Action _onEnemyTargetClicked;
        private Action _onEndTurnClicked;

        /// <summary>
        /// View 接続初期化
        /// </summary>
        public void Initialize(IBattlePageView view, Action<int> onHandCardClicked, Action onEnemyTargetClicked, Action onEndTurnClicked)
        {
            _view = view;
            _onHandCardClicked = onHandCardClicked;
            _onEnemyTargetClicked = onEnemyTargetClicked;
            _onEndTurnClicked = onEndTurnClicked;
            _view.WireButtons(_onEnemyTargetClicked, _onEndTurnClicked);
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

            _view.BuildHandButtons(snapshot.Hand, _onHandCardClicked);
            _view.SetBattleStateText(
                BuildPlayerText(snapshot),
                BuildEnemyText(snapshot),
                snapshot.BattleHintMessage);
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
            builder.AppendFormat(
                BattleSceneConstants.PlayerStateFormat,
                snapshot.PlayerHp,
                snapshot.PlayerMaxHp,
                snapshot.PlayerBlock,
                snapshot.PlayerEnergy,
                snapshot.Gold);
            AppendStatusLine(builder, BattleSceneConstants.StatusLabel, snapshot.PlayerStatuses);
            AppendStatusLine(builder, BattleSceneConstants.BuffLabel, snapshot.PlayerBuffs);
            return builder.ToString();
        }

        /// <summary>
        /// enemy表示文言構築
        /// </summary>
        private static string BuildEnemyText(BattleSceneSnapshot snapshot)
        {
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
        /// intent表示行追加
        /// </summary>
        private static void AppendIntentLine(StringBuilder builder, BattleIntentViewModel intent)
        {
            if (intent == null)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendFormat(BattleSceneConstants.IntentLabelFormat, intent.IntentType);
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
                builder.AppendFormat(BattleSceneConstants.IntentStatusFormat, intent.StatusType, intent.StatusValue);
            }

            if (intent.BuffType != Game.MasterData.Generated.BuffType.None && intent.BuffValue > 0)
            {
                builder.AppendFormat(BattleSceneConstants.IntentBuffFormat, intent.BuffType, intent.BuffValue);
            }
        }

        /// <summary>
        /// 状態表示行追加
        /// </summary>
        private static void AppendStatusLine(
            StringBuilder builder,
            string label,
            IReadOnlyList<BattleStatusViewModel> statuses)
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
