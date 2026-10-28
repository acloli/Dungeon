using TFramework.UI;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main.UI
{
    /// <summary>
    /// カード用語の短い説明表示
    /// </summary>
    public sealed class KeywordTooltip : MonoBehaviour
    {
        [SerializeField] private TFTextUGUI _titleText;
        [SerializeField] private TFTextUGUI _descriptionText;

        public TFTextUGUI TitleText => _titleText;
        public TFTextUGUI DescriptionText => _descriptionText;
    }
}
