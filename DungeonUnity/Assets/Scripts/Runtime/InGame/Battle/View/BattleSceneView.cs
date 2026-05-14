using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// BattleScene基底Viewクラス
    /// </summary>
    public sealed class BattleSceneView : MonoBehaviour, IBattleSceneHostView
    {
        [Header("Battle Base")]
        [SerializeField] private GameObject _battlePanel;
        [SerializeField] private BattlePageView _battlePageView;

        /// <summary>
        /// 戦闘画面View取得
        /// </summary>
        public IBattlePageView BattlePageView => _battlePageView != null ? _battlePageView : _battlePageView = GetComponent<BattlePageView>();

        /// <summary>
        /// 戦闘基底表示切り替え
        /// </summary>
        public void SetBattleVisible(bool visible)
        {
            if (_battlePanel != null)
            {
                _battlePanel.SetActive(visible);
            }
        }
    }
}
