using UnityEngine;
using UnityEngine.UI;


namespace UISelectable {
[CreateAssetMenu(fileName = "UI Slider Settings", menuName = "UI/Interactable/Slider Settings")]
public class SliderSettings : SelectableSettings {
    [Header("Move Step")]
    public float StepAmount = 5f;

    [Header("Fill Colors")]
    public Color FillNormalColor;
    public Color FillSelectedColor;

    public float GetStep(Slider slider) {
        return (slider.maxValue - slider.minValue) * (StepAmount / 100);
    }
}
}
