using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Model;
using TFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// パイル確認ダイアログクラス
    /// </summary>
    public sealed class PileInspectDialog : UIDialogBase
    {
        [SerializeField] private TFTextUGUI _titleText;
        [SerializeField] private Transform _cardRoot;
        [SerializeField] private BattleCardIconView _cardTemplate;
        [SerializeField] private Button _closeButton;
        [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("_backgroundButton")] private Button _backgroundAreaButton;

        private readonly List<BattleCardIconView> _cardViews = new List<BattleCardIconView>();
        private int _selectedCardIndex = -1;
        private BattlePileInspectDialogParam _param;

        /// <summary>
        /// パラメータ受取
        /// </summary>
        protected override UniTask OnPreOpenAsync(object param, CancellationToken ct)
        {
            _param = param as BattlePileInspectDialogParam;
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 表示反映
        /// </summary>
        protected override void OnOpened()
        {
            if (_param?.Snapshot == null)
            {
                return;
            }

            BattlePileInspectSnapshot snapshot = _param.Snapshot;
            _selectedCardIndex = -1;

            if (_titleText != null)
            {
                _titleText.text = snapshot.Title ?? string.Empty;
            }

            BuildCards(snapshot.Cards);
            WireButtons();
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        protected override void OnClosed()
        {
            ClearCards();
            UnwireButtons();
            _selectedCardIndex = -1;
        }

        /// <summary>
        /// カード一覧構築
        /// </summary>
        private void BuildCards(IReadOnlyList<BattleMultiIconViewModel> cards)
        {
            ClearCards();
            if (_cardRoot == null || _cardTemplate == null || cards == null)
            {
                return;
            }

            _cardTemplate.gameObject.SetActive(false);

            for (int i = 0; i < cards.Count; i++)
            {
                BattleMultiIconViewModel model = cards[i];
                if (model == null)
                {
                    continue;
                }

                int cardIndex = i;
                BattleCardIconView cardView = Instantiate(_cardTemplate, _cardRoot);
                cardView.gameObject.SetActive(true);
                cardView.Bind(CreateSelectableModel(model, cardIndex), () => OnCardClicked(cardIndex));
                _cardViews.Add(cardView);
            }
        }

        /// <summary>
        /// 選択状態付き表示モデルを生成する
        /// </summary>
        private BattleMultiIconViewModel CreateSelectableModel(BattleMultiIconViewModel source, int cardIndex)
        {
            return new BattleMultiIconViewModel(
                source.IconKind,
                source.DisplayName,
                source.Description,
                source.ImageId,
                source.Rarity,
                source.Cost,
                source.ShowCost,
                isInteractable: true,
                isSelected: cardIndex == _selectedCardIndex,
                source.IsAffordable,
                source.Quantity,
                source.ShowQuantity,
                source.FooterLabel,
                source.ShowFooterLabel);
        }

        /// <summary>
        /// 動的カード消去
        /// </summary>
        private void ClearCards()
        {
            for (int i = 0; i < _cardViews.Count; i++)
            {
                BattleCardIconView cardView = _cardViews[i];
                if (cardView == null)
                {
                    continue;
                }

                cardView.Clear();
                if (Application.isPlaying)
                {
                    Destroy(cardView.gameObject);
                }
                else
                {
                    DestroyImmediate(cardView.gameObject);
                }
            }

            _cardViews.Clear();
        }

        /// <summary>
        /// カードクリック通知
        /// </summary>
        private void OnCardClicked(int index)
        {
            _selectedCardIndex = _selectedCardIndex == index
                ? BattleSceneConstants.UnselectedCardIndex
                : index;

            BuildCards(_param?.Snapshot?.Cards);
        }

        /// <summary>
        /// ボタン登録
        /// </summary>
        private void WireButtons()
        {
            UnwireButtons();

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (_backgroundAreaButton != null)
            {
                _backgroundAreaButton.onClick.AddListener(OnBackgroundAreaClicked);
            }
        }

        /// <summary>
        /// ボタン解除
        /// </summary>
        private void UnwireButtons()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnCloseClicked);
            }

            if (_backgroundAreaButton != null)
            {
                _backgroundAreaButton.onClick.RemoveListener(OnBackgroundAreaClicked);
            }
        }

        /// <summary>
        /// 閉じるボタン通知
        /// </summary>
        private void OnCloseClicked()
        {
            Close();
        }

        /// <summary>
        /// 背景クリック通知（カード選択時は選択解除）
        /// </summary>
        private void OnBackgroundAreaClicked()
        {
            if (_selectedCardIndex < 0)
            {
                return;
            }

            _selectedCardIndex = BattleSceneConstants.UnselectedCardIndex;
            BuildCards(_param?.Snapshot?.Cards);
        }
    }
}
