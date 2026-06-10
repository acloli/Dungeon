using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    public sealed class CardSelectDialog : UIDialogBase<CardSelectDialogResult>
    {
        [SerializeField] private Button _cancelButton;

        // デッキカード表示用のUIなどをここに追加

        private void Awake()
        {
            _cancelButton.onClick.AddListener(() =>
            {
                CloseWithResult(new CardSelectDialogResult { IsCanceled = true });
            });
        }

        protected override Cysharp.Threading.Tasks.UniTask OnPreOpenAsync(object param, System.Threading.CancellationToken ct)
        {
            var p = (BattleCardSelectDialogParam)param;
            // TODO: p.Snapshot.DeckCards を用いてカード一覧を表示する処理を実装
            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        // TODO: カードがクリックされたときのハンドラ
        // private void OnCardClicked(Game.MasterData.Generated.RuntimeCard card)
        // {
        //     CloseWithResult(new CardSelectDialogResult { IsCanceled = false, SelectedCard = card });
        // }
    }
}
