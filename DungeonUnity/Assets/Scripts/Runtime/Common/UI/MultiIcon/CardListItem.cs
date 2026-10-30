using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.Common.UI.MultiIcon
{
    /// <summary>
    /// 共通カードと一覧用の数量表示をまとめる
    /// </summary>
    public sealed class CardListItem : MonoBehaviour
    {
        [SerializeField] private CardIconView _card;
        [SerializeField] private TFTextUGUI _quantityText;
        public CardIconView Card => _card;
        private void OnRectTransformDimensionsChange()
        {
            if (_card != null) _card.transform.localScale = Vector3.one * (((RectTransform)transform).rect.width / 220f);
        }
        public void SetQuantity(int quantity)
        {
            _quantityText.gameObject.SetActive(quantity > 0);
            _quantityText.text = quantity > 0 ? "×" + quantity : string.Empty;
        }
    }
}
