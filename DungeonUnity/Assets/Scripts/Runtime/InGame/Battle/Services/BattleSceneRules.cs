using System.Collections.Generic;
using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleScene の基本ルールクラス
    /// </summary>
    public sealed class BattleSceneRules : IBattleSceneRules
    {
        /// <summary>
        /// Run 状態初期化
        /// </summary>
        public void InitializeRun(BattleSceneState state, RunStartConfig runStartConfig)
        {
            if (state == null)
            {
                return;
            }

            state.PlayerMaxHp = runStartConfig != null ? runStartConfig.PlayerMaxHp : BattleSceneConstants.DefaultPlayerMaxHp;
            state.PlayerHp = state.PlayerMaxHp;
            state.PlayerEnergy = BattleSceneConstants.DefaultPlayerEnergy;
            state.Gold = runStartConfig != null ? runStartConfig.StartingGold : BattleSceneConstants.DefaultStartingGold;
            state.CurrentNodeIndex = BattleSceneConstants.DefaultNodeIndex;
            state.CurrentEnemy = null;
            state.EnemyHp = 0;
            state.BattleFinished = false;
            state.RewardChoices.Clear();
            state.Deck.Clear();
            state.Hand.Clear();
            state.Nodes.Clear();

            if (runStartConfig != null)
            {
                IReadOnlyList<CardDefinition> starterDeck = runStartConfig.StarterDeck;
                if (starterDeck != null)
                {
                    for (int i = 0; i < starterDeck.Count; i++)
                    {
                        if (starterDeck[i] != null)
                        {
                            state.Deck.Add(starterDeck[i]);
                        }
                    }
                }

                MapTemplate mapTemplate = runStartConfig.MapTemplate;
                if (mapTemplate != null)
                {
                    IReadOnlyList<MapTemplate.Node> nodes = mapTemplate.Nodes;
                    if (nodes != null)
                    {
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            state.Nodes.Add(nodes[i]);
                        }
                    }
                }
            }

            if (state.Nodes.Count == 0)
            {
                AddDefaultNodes(state.Nodes);
            }
        }

        /// <summary>
        /// 手札補充
        /// </summary>
        public void DrawHand(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null)
            {
                return;
            }

            state.Hand.Clear();

            if (state.Deck.Count == 0)
            {
                return;
            }

            int drawCount = Mathf.Min(BattleSceneConstants.DefaultHandSize, state.Deck.Count);
            for (int i = 0; i < drawCount; i++)
            {
                int index = randomProvider.Range(0, state.Deck.Count);
                CardDefinition card = state.Deck[index];
                if (card != null)
                {
                    state.Hand.Add(card);
                }
            }
        }

        /// <summary>
        /// 敵選出
        /// </summary>
        public EnemyDefinition SelectEnemy(RunStartConfig runStartConfig, InGameNodeType nodeType)
        {
            if (runStartConfig == null)
            {
                return null;
            }

            if (nodeType == InGameNodeType.EliteBattle)
            {
                if (runStartConfig.EliteEnemy != null)
                {
                    return runStartConfig.EliteEnemy;
                }

                if (runStartConfig.NormalEnemy != null)
                {
                    return runStartConfig.NormalEnemy;
                }
            }

            if (nodeType == InGameNodeType.Boss)
            {
                if (runStartConfig.BossEnemy != null)
                {
                    return runStartConfig.BossEnemy;
                }

                if (runStartConfig.EliteEnemy != null)
                {
                    return runStartConfig.EliteEnemy;
                }
            }

            if (runStartConfig.NormalEnemy != null)
            {
                return runStartConfig.NormalEnemy;
            }

            return null;
        }

        /// <summary>
        /// 報酬候補選出
        /// </summary>
        public IReadOnlyList<CardDefinition> SelectRewardChoices(BattleSceneState state, RunStartConfig runStartConfig, IBattleRandomProvider randomProvider)
        {
            List<CardDefinition> candidates = BuildRewardCandidates(state, runStartConfig);
            List<CardDefinition> rewards = new List<CardDefinition>();

            if (candidates.Count == 0)
            {
                return rewards;
            }

            int maxCount = Mathf.Min(BattleSceneConstants.DefaultRewardChoiceCount, candidates.Count);
            for (int i = 0; i < maxCount; i++)
            {
                int index = randomProvider.Range(0, candidates.Count);
                rewards.Add(candidates[index]);
                candidates.RemoveAt(index);
            }

            return rewards;
        }

        /// <summary>
        /// 使用可否判定
        /// </summary>
        public bool CanPlayCard(BattleSceneState state, CardDefinition card)
        {
            if (state == null || card == null)
            {
                return false;
            }

            return state.PlayerEnergy >= card.Cost;
        }

        /// <summary>
        /// カード適用
        /// </summary>
        public void PlayCard(BattleSceneState state, CardDefinition card)
        {
            if (state == null || card == null)
            {
                return;
            }

            state.PlayerEnergy -= card.Cost;
            state.EnemyHp -= card.Damage;
        }

        /// <summary>
        /// 敵ターン解決
        /// </summary>
        public int ResolveEnemyTurn(BattleSceneState state)
        {
            if (state == null)
            {
                return 0;
            }

            int intentDamage = BattleSceneConstants.DefaultEnemyIntentDamage;
            if (state.CurrentEnemy != null)
            {
                intentDamage = state.CurrentEnemy.IntentDamage;
            }

            state.PlayerHp -= intentDamage;
            return intentDamage;
        }

        /// <summary>
        /// 戦闘報酬金額取得
        /// </summary>
        public int GetBattleGoldReward(InGameNodeType nodeType)
        {
            if (nodeType == InGameNodeType.EliteBattle || nodeType == InGameNodeType.Boss)
            {
                return BattleSceneConstants.EliteBattleGoldReward;
            }

            return BattleSceneConstants.NormalBattleGoldReward;
        }

        /// <summary>
        /// 休憩適用
        /// </summary>
        public void ApplyRest(BattleSceneState state)
        {
            if (state == null)
            {
                return;
            }

            state.PlayerHp = Mathf.Min(state.PlayerMaxHp, state.PlayerHp + BattleSceneConstants.RestHealAmount);
        }

        /// <summary>
        /// 購入適用
        /// </summary>
        public bool ApplyShopPurchase(BattleSceneState state, IBattleRandomProvider randomProvider)
        {
            if (state == null || state.Deck.Count == 0)
            {
                return false;
            }

            if (state.Gold < BattleSceneConstants.ShopPurchaseCost)
            {
                return false;
            }

            state.Gold -= BattleSceneConstants.ShopPurchaseCost;
            int index = randomProvider.Range(0, state.Deck.Count);
            state.Deck.Add(state.Deck[index]);
            return true;
        }

        /// <summary>
        /// 既定ノード補完
        /// </summary>
        private static void AddDefaultNodes(List<MapTemplate.Node> nodes)
        {
            nodes.Add(new MapTemplate.Node
            {
                NodeType = InGameNodeType.Battle,
                Label = BattleSceneConstants.DefaultBattleNodeLabel
            });
            nodes.Add(new MapTemplate.Node
            {
                NodeType = InGameNodeType.RestShop,
                Label = BattleSceneConstants.DefaultRestNodeLabel
            });
            nodes.Add(new MapTemplate.Node
            {
                NodeType = InGameNodeType.Battle,
                Label = BattleSceneConstants.DefaultBattleNodeTwoLabel
            });
            nodes.Add(new MapTemplate.Node
            {
                NodeType = InGameNodeType.EliteBattle,
                Label = BattleSceneConstants.DefaultEliteNodeLabel
            });
            nodes.Add(new MapTemplate.Node
            {
                NodeType = InGameNodeType.RestShop,
                Label = BattleSceneConstants.DefaultShopNodeLabel
            });
            nodes.Add(new MapTemplate.Node
            {
                NodeType = InGameNodeType.Boss,
                Label = BattleSceneConstants.DefaultBossNodeLabel
            });
        }

        /// <summary>
        /// 報酬候補一覧構築
        /// </summary>
        private static List<CardDefinition> BuildRewardCandidates(BattleSceneState state, RunStartConfig runStartConfig)
        {
            List<CardDefinition> candidates = new List<CardDefinition>();
            if (runStartConfig != null && runStartConfig.RewardPool != null)
            {
                for (int i = 0; i < runStartConfig.RewardPool.Count; i++)
                {
                    CardDefinition card = runStartConfig.RewardPool[i];
                    if (card != null)
                    {
                        candidates.Add(card);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                candidates.AddRange(state.Deck);
            }

            return candidates;
        }
    }
}
