using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.UI;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(Slider))]
public class SliderUtility : SelectableUtility<SliderSettings, Slider>, IBeginDragHandler, IEndDragHandler, IPointerUpHandler
{
    // State
    bool _isDragging;

    [Header("Refs")]
    [SerializeField] TextMeshProUGUI _percentageText;
    [SerializeField] Image _fill;

    float _previousSliderValue;
    Tween _fillTween;

    [Header("Manual Repeat (for held D-pad/Arrow input)")]
    [SerializeField] float _repeatDelay = 0.4f;
    [SerializeField] float _repeatRate = 0.05f;
    [SerializeField] float _deadzone = 0.5f;

    [SerializeField] MoveDirection _heldDirection = MoveDirection.None;
    float _repeatTimer;
    bool _hasRepeatedOnce;

    protected override void OnEnable() {
        base.OnEnable();
        if (IsSelected) {
            _fill.color = _settings.FillSelectedColor;
        }
        else {
            _fill.color = _settings.FillNormalColor;
        }

        _previousSliderValue = _selectable.value;

        _selectable.onValueChanged.AddListener(UpdatePercentageText);
        UpdatePercentageText(_selectable.value);
    }

    protected override void OnDisable() {
        base.OnDisable();
        KillFillTween();
        _selectable.onValueChanged.RemoveListener(UpdatePercentageText);
        _heldDirection = MoveDirection.None;
        _isDragging = false;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        KillFillTween();
    }

    void Update() {
        if (_heldDirection == MoveDirection.None) { return; }

        float horizontalValue = ReadHorizontalInput();
        bool stillHeld = _heldDirection == MoveDirection.Left ? horizontalValue < -_deadzone : horizontalValue > _deadzone;

        if (!stillHeld) {
            _heldDirection = MoveDirection.None;
            return;
        }

        _repeatTimer -= Time.unscaledDeltaTime;
        if (_repeatTimer <= 0f) {
            StepSlider(_heldDirection);
            _repeatTimer = _hasRepeatedOnce ? _repeatRate : _repeatDelay;
            _hasRepeatedOnce = true;
        }
    }

    #region Selection Events

    public override void OnSelect(BaseEventData eventData) {
        base.OnSelect(eventData);
        _previousSliderValue = _selectable.value;
        TweenFillColor(_settings.FillSelectedColor);
    }

    public override void OnDeselect(BaseEventData eventData) {
        base.OnDeselect(eventData);
        // Stop Held-Input Repeat Once Focus Leaves This Slider
        _heldDirection = MoveDirection.None;
        TweenFillColor(_settings.FillNormalColor);
    }



    #endregion

    #region Pointer & OnMove Events

    public void OnBeginDrag(PointerEventData eventData) {
        _isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData) {
        _isDragging = false;
    }

    public void OnPointerUp(PointerEventData eventData) {
        _previousSliderValue = _selectable.value;
    }

    public override void OnMove(AxisEventData eventData) {
        base.OnMove(eventData);

        if (eventData.moveDir != MoveDirection.Left && eventData.moveDir != MoveDirection.Right) {
            return;
        }

        if (_heldDirection != eventData.moveDir) {
            _heldDirection = eventData.moveDir;
            _repeatTimer = _repeatDelay;
            _hasRepeatedOnce = false;
            StepSlider(eventData.moveDir);
        }

        eventData.Use();
    }

    #endregion

    #region InputReading

    // Read The UI Move Action So Project Bindings & Rebinding Apply
    float ReadHorizontalInput() {
        InputSystemUIInputModule module = EventSystem.current != null ? EventSystem.current.currentInputModule as InputSystemUIInputModule : null;
        
        if (module == null || module.move == null || module.move.action == null) { return 0f; }

        return module.move.action.ReadValue<Vector2>().x;
    }

    #endregion

    void StepSlider(MoveDirection direction) {
        float step = _settings.GetStep(_selectable);

        _selectable.value = direction == MoveDirection.Left
            ? _previousSliderValue - step
            : _previousSliderValue + step;

        _previousSliderValue = _selectable.value;
    }

    public void UpdatePercentageText(float value) {
        _percentageText.text = Mathf.InverseLerp(_selectable.minValue, _selectable.maxValue, value).ToString("P0");
    }

    public override void SetTweenableSelectable() {
        _tweenableRect = _selectable.handleRect;
    }

    void TweenFillColor(Color targetColor) {
        KillFillTween();

        _fillTween = _fill.DOColor(targetColor, _selectable.colors.fadeDuration).SetEase(_settings.TweenEase).SetUpdate(true);
    }

    void KillFillTween() {
        if (_fillTween != null && _fillTween.IsActive()) {
            _fillTween.Kill();
            _fillTween = null;
        }
    }
}
