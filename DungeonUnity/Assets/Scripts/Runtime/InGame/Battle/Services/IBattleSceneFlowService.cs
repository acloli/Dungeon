using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Save.Model;

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
        /// セーブデータからの初期化
        /// </summary>
        void InitializeFromSave(RunSaveData saveData);

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
        void SelectReward(RuntimeRewardEntry rewardEntry);

        /// <summary>
        /// 休憩適用
        /// </summary>
        void ApplyRest();

        /// <summary>
        /// 強化適用
        /// </summary>
        void ApplyUpgrade();

        /// <summary>
        /// ショップを開く
        /// </summary>
        void OpenShop();

        /// <summary>
        /// ショップアイテム購入
        /// </summary>
        void PurchaseShopItem(int slotIndex);

        /// <summary>
        /// カード削除選択を開く
        /// </summary>
        void OpenCardRemoval();

        /// <summary>
        /// カード削除購入
        /// </summary>
        void PurchaseCardRemoval(RuntimeCard card);

        /// <summary>
        /// ショップから退出
        /// </summary>
        void LeaveShop();

        /// <summary>
        /// 補給画面継続
        /// </summary>
        void ContinueFromRestShop();
    }
}
