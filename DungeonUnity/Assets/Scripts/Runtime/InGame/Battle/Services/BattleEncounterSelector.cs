using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;
using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleSceneの遭遇選択を扱うクラス
    /// </summary>
    public sealed class BattleEncounterSelector : IBattleEncounterSelector
    {
        /// <summary>
        /// ノード種別に応じた遭遇編成を選択する
        /// </summary>
        public RuntimeEncounterFormation SelectEncounterFormation(RuntimeRunDefinition runDefinition, InGameNodeType nodeType, IBattleRandomProvider randomProvider)
        {
            if (runDefinition == null ||
                !runDefinition.EncountersByNodeType.TryGetValue(nodeType, out System.Collections.Generic.IReadOnlyList<RuntimeEncounterEntry> encounters) ||
                encounters == null ||
                encounters.Count == 0)
            {
                return CreateFallbackFormation(nodeType);
            }

            RuntimeEncounterEntry selected = SelectWeightedEntry(encounters, randomProvider);
            return selected != null ? selected.Formation : CreateFallbackFormation(nodeType);
        }

        /// <summary>
        /// 敵の初期HPを決定する
        /// </summary>
        public int RollEnemyHp(RuntimeEnemy enemy, IBattleRandomProvider randomProvider)
        {
            if (enemy == null)
            {
                return BattleSceneConstants.DefaultEnemyHp;
            }

            int minHp = Mathf.Max(1, enemy.HpMin);
            int maxHp = Mathf.Max(minHp, enemy.HpMax);
            if (minHp == maxHp)
            {
                return maxHp;
            }

            return randomProvider.Range(minHp, maxHp + 1);
        }

        /// <summary>
        /// データ不足時のフォールバック敵生成
        /// </summary>
        private static RuntimeEncounterFormation CreateFallbackFormation(InGameNodeType nodeType)
        {
            int baseHp = nodeType == InGameNodeType.Boss ? 60 : BattleSceneConstants.DefaultEnemyHp;
            int damage = nodeType == InGameNodeType.EliteBattle ? 8 : 4;
            RuntimeEnemyAction action = new RuntimeEnemyAction(
                1,
                IntentType.Attack,
                damage,
                1,
                0,
                StatusType.None,
                0,
                BuffType.None,
                0,
                RepeatRule.RepeatAfterOpening);

            RuntimeEnemy enemy = new RuntimeEnemy(
                0,
                "fallback_enemy",
                BattleSceneConstants.UnknownEnemyName,
                string.Empty,
                nodeType == InGameNodeType.Boss ? EnemyTier.Boss : nodeType == InGameNodeType.EliteBattle ? EnemyTier.Elite : EnemyTier.Normal,
                baseHp,
                baseHp,
                20,
                new[] { action });
            return new RuntimeEncounterFormation(
                0,
                "fallback_formation",
                BattleSceneConstants.UnknownEnemyName,
                new[] { new RuntimeEncounterEnemyEntry(enemy, 0) });
        }

        /// <summary>
        /// 重み付き遭遇候補を選択する
        /// </summary>
        private static RuntimeEncounterEntry SelectWeightedEntry(
            System.Collections.Generic.IReadOnlyList<RuntimeEncounterEntry> entries,
            IBattleRandomProvider randomProvider)
        {
            if (entries == null || entries.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                totalWeight += Mathf.Max(0, entries[i].Weight);
            }

            if (totalWeight <= 0)
            {
                return entries[0];
            }

            int roll = randomProvider.Range(0, totalWeight);
            int currentWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                currentWeight += Mathf.Max(0, entries[i].Weight);
                if (roll < currentWeight)
                {
                    return entries[i];
                }
            }

            return entries[entries.Count - 1];
        }
    }
}
