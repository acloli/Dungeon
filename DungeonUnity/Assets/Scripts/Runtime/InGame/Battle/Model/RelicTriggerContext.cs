using Dungeon.Runtime.InGame.Domain;
using Game.MasterData.Generated;

namespace Dungeon.Runtime.InGame.Battle.Model
{
    /// <summary>
    /// レリック発動条件の評価コンテキストクラス
    /// </summary>
    public sealed class RelicTriggerContext
    {
        public RelicTriggerContext(
            RelicTriggerType triggerType,
            RuntimeCard playedCard = null,
            InGameNodeType? nodeType = null,
            RuntimeRunDefinition runDefinition = null)
        {
            TriggerType = triggerType;
            PlayedCard = playedCard;
            NodeType = nodeType;
            RunDefinition = runDefinition;
        }

        public RelicTriggerType TriggerType { get; }
        public RuntimeCard PlayedCard { get; }
        public InGameNodeType? NodeType { get; }
        public RuntimeRunDefinition RunDefinition { get; }
    }
}
