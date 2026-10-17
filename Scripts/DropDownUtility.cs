using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;


namespace UISelectable {
[RequireComponent(typeof(TMP_Dropdown))]
public class DropDownUtility : SelectableUtility<DropdownSettings, TMP_Dropdown> {
        Tween _dropdownColorTween;
        ColorBlock _dropdownDefaultColors;
        Coroutine _watchRoutine;

        protected override void Awake() {
            base.Awake();
            _dropdownDefaultColors = _selectable.colors;
        }

        protected override void OnEnable() {
            base.OnEnable();

            _watchRoutine = StartCoroutine(WatchDropdownState());
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (_watchRoutine != null) {
                StopCoroutine(_watchRoutine);
                _watchRoutine = null;
            }
            KillColorTween();
            SetTextColors(_settings.TextNormalColor);
            _selectable.colors = _dropdownDefaultColors;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            KillColorTween();
        }


        #region Pointer Events

        public override void OnPointerEnter(PointerEventData eventData) {
            if (_selectable.IsExpanded) { return; }
            base.OnPointerEnter(eventData);
        }

        #endregion

        #region Select Events

        public override void OnDeselect(BaseEventData eventData) {
            if (_selectable.IsExpanded) { return; }
            base.OnDeselect(eventData);
        }

        #endregion

        protected IEnumerator WatchDropdownState() {
            yield return null;

            bool wasExpanded = false;

            while (true) {
                bool isExpanded = _selectable.IsExpanded;

                if (isExpanded != wasExpanded) {
                    wasExpanded = isExpanded;
                    if (isExpanded) {
                        TweenDropDownColor(_settings.DropdownExpandedColor);
                    }
                    else {
                        _selectable.Select();
                        TweenDropDownColor(_settings.DropdownNormalColor);
                    }
                }

                yield return null;
            }

        }

        void TweenDropDownColor(Color targetColor) {
            KillColorTween();

            ColorBlock block = _selectable.colors;
            Color startColor = block.normalColor;

            _dropdownColorTween = DOVirtual.Float(0f, 1f, block.fadeDuration, t => {
                Color lerped = Color.Lerp(startColor, targetColor, t);
                block.normalColor = lerped;
                block.highlightedColor = lerped;
                _selectable.colors = block;
            }).SetEase(_settings.TweenEase).SetUpdate(true);
        }

        void KillColorTween() {
            _dropdownColorTween?.Kill();
            _dropdownColorTween = null;
        }
    }
}
