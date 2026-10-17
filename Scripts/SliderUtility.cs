using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.UI;
using DG.Tweening;
using TMPro;


namespace UISelectable {
    [RequireComponent(typeof(Slider))]
    public class SliderUtility : SelectableUtility<SliderSettings, Slider> {
        [Header("Refs")]
        [SerializeField] TextMeshProUGUI _percentageText;
        [SerializeField] Image _fill;

        float _previousSliderValue;
        Tween _fillTween;

        MoveDirection _pendingDirection = MoveDirection.None;

        protected override void OnEnable() {
            base.OnEnable();
            if (!_isValid) { return; }

            if (_fill != null) {
                _fill.color = IsSelected ? _settings.FillSelectedColor : _settings.FillNormalColor;
            }

            _previousSliderValue = _selectable.value;

            _selectable.onValueChanged.AddListener(UpdatePercentageText);
            UpdatePercentageText(_selectable.value);
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (!_isValid) { return; }

            KillFillTween();
            _selectable.onValueChanged.RemoveListener(UpdatePercentageText);
            _pendingDirection = MoveDirection.None;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (!_isValid) { return; }

            KillFillTween();
        }

        // Runs After Every OnMove Handler From Slider
        void LateUpdate() {
            if (!_isValid) { return; }

            // No Move This Frame
            if (_pendingDirection == MoveDirection.None) {
                _previousSliderValue = _selectable.value;
                return;
            }

            float step = _settings.GetStep(_selectable);

            // Overwrites The Built-In Step
            _selectable.value = _pendingDirection == MoveDirection.Left ? _previousSliderValue - step : _previousSliderValue + step;

            _previousSliderValue = _selectable.value;
            _pendingDirection = MoveDirection.None;
        }

        #region Selection Events

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            if (!_isValid) { return; }

            TweenFillColor(_settings.FillSelectedColor);
        }

        public override void OnDeselect(BaseEventData eventData) {
            base.OnDeselect(eventData);
            if (!_isValid) { return; }

            _pendingDirection = MoveDirection.None;
            TweenFillColor(_settings.FillNormalColor);
        }

        #endregion

        #region On Move

        public override void OnMove(AxisEventData eventData) {
            base.OnMove(eventData);
            if (!_isValid) { return; }

            if (eventData.moveDir != MoveDirection.Left && eventData.moveDir != MoveDirection.Right) {
                return;
            }

            _pendingDirection = eventData.moveDir;
            eventData.Use();
        }

        #endregion

        #region Text Updating

        public void UpdatePercentageText(float value) {
            if (_percentageText == null) { return; }

            _percentageText.text = Mathf.InverseLerp(_selectable.minValue, _selectable.maxValue, value).ToString("P0");
        }

        #endregion

        #region Tweening

        public override void SetTweenableSelectable() {
            _tweenableRect = _selectable.handleRect;
        }

        void TweenFillColor(Color targetColor) {
            if (_fill == null) { return; }
            KillFillTween();

            _fillTween = _fill.DOColor(targetColor, _selectable.colors.fadeDuration).SetEase(_settings.TweenEase).SetUpdate(true);
        }

        void KillFillTween() {
            if (_fillTween != null && _fillTween.IsActive()) {
                _fillTween.Kill();
                _fillTween = null;
            }
        }

        #endregion

        #region Validtation

        protected override bool Validate() {
            if (!base.Validate()) { return false; }

            if (_selectable.handleRect == null) {
                Debug.LogError($"[{name}] Slider has no handle rect assigned.", this);
                return false;
            }

            return true;
        }

        #endregion
    }
}
