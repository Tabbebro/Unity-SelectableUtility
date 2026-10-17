using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;


namespace UISelectable {
    public class SelectableUtility : SelectableUtility<SelectableSettings, Selectable> { }

    public abstract class SelectableUtility<TSettings, TSelectable> : MonoBehaviour,
        IPointerEnterHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler
        where TSettings : SelectableSettings
        where TSelectable : Selectable {

        [Header("Settings")]
        [SerializeField] protected TSettings _settings;

        [Header("Texts")]
        [SerializeField] protected List<TextMeshProUGUI> _texts;
        [SerializeField] protected List<Image> _icons;

        [Header("Icon Slide In")]
        [SerializeField] bool _useIconSlideIn;
        [SerializeField] float _slideIconDuration = 0.2f;
        [SerializeField] float _slideIconWidth = 32f;

        // State
        protected bool IsSelected => EventSystem.current != null && EventSystem.current.currentSelectedGameObject == _selectable.gameObject;
        protected bool _isValid;

        protected TSelectable _selectable;
        protected RectTransform _tweenableRect;
        Vector3 _baseRotation;
        Vector3 _baseScale = Vector3.one;
        readonly TweenGroup _textColorTweens = new();
        readonly TweenGroup _iconColorTweens = new ();
        readonly TweenGroup _transformTweens = new();

        List<LayoutElement> _slideIconLayoutElements = new();
        List<CanvasGroup> _slideIconCanvasGroups = new();
        readonly TweenGroup _slideIconTweens = new();


        protected virtual void Awake() {
            _selectable = GetComponent<TSelectable>();
            _isValid = Validate();
            if (!_isValid) { return; }

            EnsureTweenableRect();

            if (_useIconSlideIn && _icons.Count > 0) {
                SetupSlideIcon();
            }
        }

        protected virtual void OnEnable() {
            if (!_isValid) { return; }

            if (IsSelected) {
                SetTextColors(_settings.TextSelectedColor);
                SetIconColors(_settings.TextSelectedColor);
                TransformTween(_settings.TweenRotation, _settings.TweenScale);

                // Restore The Slide Icon When Re-Enabling While Selected
                if (_useIconSlideIn) { SetSlideIconValues(true); }
            }
            else {
                SetTextColors(_settings.TextNormalColor);
                SetIconColors(_settings.TextNormalColor);
                ResetTransformTweenValues();
            }
        }

        protected virtual void OnDisable() {
            if (!_isValid) { return; }

            KillTextColorTweens();
            KillIconColorTweens();
            KillTransformTween();

            ResetTransformTweenValues();
            SetTextColors(_settings.TextNormalColor);
            SetIconColors(_settings.TextNormalColor);

            if (_useIconSlideIn) {
                SetSlideIconValues(false);
            }
        }

        protected virtual void OnDestroy() {
            if (!_isValid) { return; }

            KillTextColorTweens();
            KillIconColorTweens();
            KillTransformTween();

            if (_useIconSlideIn) {
                KillSlideIconTween();
            }
        }

        #region Pointer Events
        public virtual void OnPointerEnter(PointerEventData eventData) {
            if (!_isValid) { return; }

            if (!Cursor.visible || Cursor.lockState == CursorLockMode.Locked) { return; }
            _selectable.Select();
        }

        #endregion

        #region Select Events

        public virtual void OnSelect(BaseEventData eventData) {
            if (!_isValid) { return; }

            if (_selectable.interactable) {
                TweenTextColor(_settings.TextSelectedColor);
                TweenIconColor(_settings.TextSelectedColor);

                if (_useIconSlideIn) {
                    TweenSlideIcon(true);
                }
            }
            TransformTween(_settings.TweenRotation, _settings.TweenScale);
        }

        public virtual void OnDeselect(BaseEventData eventData) {
            if (!_isValid) { return; }

            TweenTextColor(_settings.TextNormalColor);
            TweenIconColor(_settings.TextNormalColor);
            TransformTweenToBase();

            if (_useIconSlideIn) {
                TweenSlideIcon(false);
            }
        }
        #endregion

        #region Submit Event

        public void OnSubmit(BaseEventData eventData) {

        }

        #endregion

        #region OnMoveEvent
        public virtual void OnMove(AxisEventData eventData) {

        }
        #endregion

        #region Validation

        protected virtual bool Validate() {
            if (_settings == null) {
                Debug.LogError($"[{name}] Missing settings asset.", this);
                return false;
            }

            if (_selectable == null) {
                Debug.LogError($"[{name}] Missing {typeof(TSelectable).Name} component.", this);
                return false;
            }

            return true;
        }

        #endregion

        #region Transform Tweening

        public virtual void SetTweenableSelectable() {
            if (_selectable.TryGetComponent<RectTransform>(out RectTransform rect)) {
                _tweenableRect = rect;
            }
            else {
                Debug.LogError($"[<color=#f54c7f>{this.name}</color>]" +
                    $"<color=orange> Couldn't Get RectTransform Of Selectable: </color>" + _selectable.gameObject.name);
            }
        }

        protected virtual void TransformTween(Vector3 targetRotation, Vector3 targetScale) {
            EnsureTweenableRect();
            if (_tweenableRect == null) { return; }

            KillTransformTween();

            // Rotation
            _transformTweens.Add(_tweenableRect.transform.DOLocalRotate(targetRotation, _selectable.colors.fadeDuration).SetEase(_settings.TweenEase).SetUpdate(true));

            // Scale Tween
            _transformTweens.Add(_tweenableRect.transform.DOScale(targetScale, _selectable.colors.fadeDuration).SetEase(_settings.TweenEase).SetUpdate(true));
        }

        protected void TransformTweenToBase() {
            TransformTween(_baseRotation, _baseScale);
        }

        protected virtual void ResetTransformTweenValues() {
            EnsureTweenableRect();
            if (_tweenableRect == null) { return; }

            KillTransformTween();

            _tweenableRect.localRotation = Quaternion.Euler(_baseRotation);
            _tweenableRect.localScale = _baseScale;
        }

        void EnsureTweenableRect() {
            if (_tweenableRect != null) { return; }

            SetTweenableSelectable();
            if (_tweenableRect == null) { return; }

            _baseRotation = _tweenableRect.localEulerAngles;
            _baseScale = _tweenableRect.localScale;
        }

        protected virtual void KillTransformTween() {
            _transformTweens.Kill();
        }

        #endregion

        #region Text Color Tweening

        protected virtual void TweenTextColor(Color targetColor) {
            KillTextColorTweens();

            foreach (var text in _texts) {
                Tween t = text.DOColor(targetColor, _selectable.colors.fadeDuration).SetEase(_settings.TweenEase).SetUpdate(true);
                _textColorTweens.Add(t);
            }
        }

        protected virtual void SetTextColors(Color color) {
            foreach (var text in _texts) {
                text.color = color;
            }
        }

        void KillTextColorTweens() {
            _textColorTweens.Kill();
        }

        #endregion

        #region Icon Color Tweening

        protected virtual void TweenIconColor(Color targetColor) {
            KillIconColorTweens();

            foreach (var icon in _icons) {
                Tween t = icon.DOColor(targetColor, _selectable.colors.fadeDuration).SetEase(_settings.TweenEase).SetUpdate(true);
                _iconColorTweens.Add(t);
            }
        }

        protected virtual void SetIconColors(Color color) {
            foreach (var icon in _icons) {
                icon.color = color;
            }
        }

        void KillIconColorTweens() {
            _iconColorTweens.Kill();
        }

        #endregion

        #region Icon Slide Tweening

        // Adds LayoutElement/CanvasGroup to each icon in _icons if missing, starts them hidden (0 width, invisible)
        void SetupSlideIcon() {
            foreach (var icon in _icons) {
                LayoutElement layoutElement = icon.GetComponent<LayoutElement>();
                if (layoutElement == null) {
                    layoutElement = icon.gameObject.AddComponent<LayoutElement>();
                }

                CanvasGroup canvasGroup = icon.GetComponent<CanvasGroup>();
                if (canvasGroup == null) {
                    canvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
                }

                layoutElement.minWidth = 0f;
                layoutElement.preferredWidth = 0f;
                canvasGroup.alpha = 0f;

                _slideIconLayoutElements.Add(layoutElement);
                _slideIconCanvasGroups.Add(canvasGroup);
            }
        }

        // Growing preferredWidth on each icon pushes siblings (the text) via the HorizontalLayoutGroup
        void TweenSlideIcon(bool show) {
            if (_slideIconLayoutElements.Count == 0) { return; }

            KillSlideIconTween();


            for (int i = 0; i < _slideIconLayoutElements.Count; i++) {
                LayoutElement layoutElement = _slideIconLayoutElements[i];
                CanvasGroup canvasGroup = _slideIconCanvasGroups[i];

                float targetWidth = show ? _slideIconWidth : 0f;
                float targetAlpha = show ? 1f : 0f;

                Tween widthTween = DOTween.To(() => layoutElement.preferredWidth, w => layoutElement.preferredWidth = w, targetWidth, _slideIconDuration)
                    .SetEase(_settings.TweenEase).SetUpdate(true);
                Tween fadeTween = canvasGroup.DOFade(targetAlpha, _slideIconDuration)
                    .SetEase(_settings.TweenEase).SetUpdate(true);

                _slideIconTweens.Add(widthTween);
                _slideIconTweens.Add(fadeTween);
            }
        }

        void SetSlideIconValues(bool show) {
            if (_slideIconLayoutElements.Count == 0) { return; }

            KillSlideIconTween();

            for (int i = 0; i < _slideIconLayoutElements.Count; i++) {
                _slideIconLayoutElements[i].preferredWidth = show ? _slideIconWidth : 0f;
                _slideIconCanvasGroups[i].alpha = show ? 1f : 0f;
            }
        }

        void KillSlideIconTween() {
            _slideIconTweens.Kill();
        }

        #endregion
    }
}
