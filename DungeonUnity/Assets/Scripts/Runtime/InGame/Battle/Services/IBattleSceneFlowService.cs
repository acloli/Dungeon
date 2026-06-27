using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.InGame.Save.Model;
using System.Collections.Generic;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// BattleScene進行仲介インターフェース
    /// </summary>
    public interface IBattleSceneFlowService : IBattleSceneQueryService
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
        /// 現在デッキ取得
        /// </summary>
        IReadOnlyList<RuntimeCard> GetDeckCards();

        /// <summary>
        /// 現在のカード選択候補取得
        /// </summary>
        IReadOnlyList<RuntimeCard> GetCardSelectCards();

        /// <summary>
        /// 現在のカード選択価格取得
        /// </summary>
        IReadOnlyDictionary<int, int> GetCardSelectPrices();

        /// <summary>
        /// 現在のカード選択強化後カード取得
        /// </summary>
        IReadOnlyDictionary<int, RuntimeCard> GetCardSelectUpgradedCards();

        /// <summary>
        /// 現在のカード選択メッセージ取得
        /// </summary>
        string GetCardSelectMessage();

        /// <summary>
        /// 現在のカード選択用途取得
        /// </summary>
        CardSelectMode GetCardSelectMode();

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
        void ClaimGold();
        void ClaimPotion();
        void ClaimRelic();
        void InspectOwnedRelic(int index);
        void InspectOwnedPotion(int index);
        void UsePotion(int index);
        void ReplaceOwnedPotion(int index);
        void CancelPendingPotionReplace();
        void ClearOwnedInspections();
        /// <summary>
        /// 報酬画面継続
        /// </summary>
        void ContinueFromReward();

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
        /// カード選択キャンセル
        /// </summary>
        void CancelCardSelect();

        /// <summary>
        /// カード選択確定
        /// </summary>
        void ConfirmCardSelect(RuntimeCard card);

        /// <summary>
        /// ショップから退出
        /// </summary>
        void LeaveShop();

        /// <summary>
        /// 補給画面継続
        /// </summary>
        void ContinueFromRestShop();

        /// <summary>
        /// イベント選択肢決定
        /// </summary>
        void SelectEventChoice(int choiceId);
    }
}
