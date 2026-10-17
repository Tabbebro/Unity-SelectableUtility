using UnityEngine;

namespace UISelectable {
[CreateAssetMenu(fileName = "UI Dropdown Settings", menuName = "UI/Interactable/Dropdown Settings")]
public class DropdownSettings : SelectableSettings {
    [Header("Dropdown Colors")]
    public Color DropdownNormalColor;
    public Color DropdownExpandedColor = Color.white;
}
}
