using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace UISelectable {
[RequireComponent(typeof(Button))]
public class RadioButtonUtility : SelectableUtility<RadioButtonSettings, Button>, IPointerExitHandler {
    bool _pressed = false;

    protected override void OnEnable() {
        base.OnEnable();

        SetTextColors(_pressed ? _settings.TextSelectedColor : _settings.TextNormalColor);
        ChangeButtonNormalColor(_pressed ? _settings.ButtonPressedColor : _settings.ButtonDefaultColor);
    }

    #region Pointer Events
    public override void OnPointerEnter(PointerEventData eventData) {
        if (!_selectable.interactable) { return; }

        TweenTextColor(_settings.HighlightedTextColor);
        TransformTween(_settings.TweenRotation, _settings.TweenScale);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (_pressed) {
            TweenTextColor(_settings.TextSelectedColor);
        }
        else {
            TweenTextColor(_settings.TextNormalColor);
            TransformTween(Vector3.zero, Vector3.one);
        }
    }
    #endregion

    public void RadioButtonPressed() {
        _pressed = true;

        ChangeButtonNormalColor(_settings.ButtonPressedColor);

        if (EventSystem.current.currentSelectedGameObject == gameObject) {
            return;
        }
        TweenTextColor(_settings.TextSelectedColor);
        TransformTween(_settings.TweenRotation, _settings.TweenScale);
    }

    public void RadioButtonReleased() {
        _pressed = false;

        ChangeButtonNormalColor(_settings.ButtonDefaultColor);

        if (EventSystem.current.currentSelectedGameObject == gameObject) {
            return;
        }

        TweenTextColor(_settings.TextNormalColor);
        TransformTween(Vector3.zero, Vector3.one);
    }

    void ChangeButtonNormalColor(Color color) {
        ColorBlock block = _selectable.colors;

        block.normalColor = color;

        _selectable.colors = block;
    }
}
}
