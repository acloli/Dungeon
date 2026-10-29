using UnityEngine;
using UnityEngine.EventSystems;

namespace Dungeon.Runtime.Common.Character
{
    /// <summary>
    /// キャラクターの演出
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class CharacterMotion : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField, Tooltip("アニメーション対象")] private RectTransform _visualRoot;

        [SerializeField, Min(0f), Tooltip("待機時の上下移動幅")]
        private float _idleHeight = 4f;

        [SerializeField, Min(0.1f), Tooltip("待機アニメーションの周期 秒")]
        private float _idlePeriod = 3.5f;

        [SerializeField, Range(0f, 0.05f), Tooltip("呼吸を表す縦方向の拡縮率")]
        private float _breathingAmount = 0.008f;

        [SerializeField, Min(0f), Tooltip("クリック時のジャンプの高さ")]
        private float _hopHeight = 18f;

        [SerializeField, Min(0.1f), Tooltip("ジャンプ開始から着地までの時間")]
        private float _hopDuration = 0.45f;

        [SerializeField, Tooltip("TimeScaleに依存しない")]
        private bool _useUnscaledTime = true;

        private RectTransform _animatedRoot;
        private Vector2 _restPosition;
        private Vector3 _restScale;
        private float _idleElapsed;
        private float _hopElapsed;
        private bool _isHopping;
        private bool _isClickable;

        private void OnEnable()
        {
            _animatedRoot = _visualRoot != null ? _visualRoot : (RectTransform)transform;
            _restPosition = _animatedRoot.anchoredPosition;
            _restScale = _animatedRoot.localScale;
            _idleElapsed = 0f;
            _hopElapsed = 0f;
            _isHopping = false;
            _isClickable = false;
        }

        private void Update()
        {
            Advance(_useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        private void OnDisable()
        {
            _isHopping = false;
            if (_animatedRoot == null)
            {
                return;
            }

            _animatedRoot.anchoredPosition = _restPosition;
            _animatedRoot.localScale = _restScale;
            _animatedRoot = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isClickable || eventData is not { button: PointerEventData.InputButton.Left })
            {
                return;
            }

            PlayHop();
        }

        /// <summary>
        /// ジャンプの演出
        /// </summary>
        public void PlayHop()
        {
            if (!isActiveAndEnabled || _animatedRoot == null || _isHopping)
            {
                return;
            }

            _hopElapsed = 0f;
            _isHopping = true;
        }

        /// <summary>
        /// クリック可能/不可を設定する
        /// </summary>
        public void SetClickable(bool isClickable)
        {
            _isClickable = isClickable;
        }

        /// <summary>
        /// 演出を合成する
        /// </summary>
        private void Advance(float deltaTime)
        {
            if (_animatedRoot == null)
            {
                return;
            }

            float idlePeriod = Mathf.Max(0.1f, _idlePeriod);
            _idleElapsed = Mathf.Repeat(_idleElapsed + Mathf.Max(0f, deltaTime), idlePeriod);
            float wave = Mathf.Sin(_idleElapsed / idlePeriod * Mathf.PI * 2f);
            float hopOffset = 0f;

            if (_isHopping)
            {
                _hopElapsed += Mathf.Max(0f, deltaTime);
                float progress = Mathf.Clamp01(_hopElapsed / Mathf.Max(0.1f, _hopDuration));
                if (progress >= 1f)
                {
                    _isHopping = false;
                }
                else
                {
                    hopOffset = Mathf.Sin(progress * Mathf.PI) * Mathf.Max(0f, _hopHeight);
                }
            }

            _animatedRoot.anchoredPosition = _restPosition +
                                             Vector2.up * (wave * Mathf.Max(0f, _idleHeight) + hopOffset);
            Vector3 scale = _restScale;
            scale.y *= 1f + wave * Mathf.Clamp(_breathingAmount, 0f, 0.05f);
            _animatedRoot.localScale = scale;
        }
    }
}
