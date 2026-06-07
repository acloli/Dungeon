using Dungeon.Runtime.InGame.Battle.Model;
namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleScene進行仲介インターフェース
    /// </summary>
    public interface IBattleSceneFlowService
    {
        /// <summary>
        /// Run初期化
        /// </summary>
        void Initialize(int runProfileId);

        /// <summary>
        /// 現在状態取得
        /// </summary>
        BattleSceneSnapshot CreateSnapshot();

        /// <summary>
        /// マップノード選択
        /// </summary>
        void SelectMapNode(int index);

        /// <summary>
        /// 手札選択
        /// </summary>
        void SelectHandCard(int index);

        /// <summary>
        /// 敵対象選択
        /// </summary>
        void SelectEnemyTarget(int index);

        /// <summary>
        /// 選択カードが敵個別対象を必要とするか
        /// </summary>
        bool DoesSelectedCardRequireEnemyTarget();

        /// <summary>
        /// 選択カード使用
        /// </summary>
        void TryPlaySelectedCard();

        /// <summary>
        /// ターン終了
        /// </summary>
        void EndTurn();

        /// <summary>
        /// 報酬選択
        /// </summary>
        void SelectReward(RuntimeCard card);

        /// <summary>
        /// 休憩適用
        /// </summary>
        void ApplyRest();

        /// <summary>
        /// 強化適用
        /// </summary>
        void ApplyUpgrade();

        /// <summary>
        /// 購入適用
        /// </summary>
        void ApplyShopPurchase();

        /// <summary>
        /// 補給画面継続
        /// </summary>
        void ContinueFromRestShop();
    }
}
