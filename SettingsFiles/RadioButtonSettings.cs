using UnityEngine;

[CreateAssetMenu(fileName = "UI Radio Button Settings", menuName = "UI/Interactable/Radio Button Settings")]
public class RadioButtonSettings : SelectableSettings
{
    public Color HighlightedTextColor;

    [Header("Button Color")]
    public Color ButtonPressedColor = Color.white;
    public Color ButtonDefaultColor;
}
