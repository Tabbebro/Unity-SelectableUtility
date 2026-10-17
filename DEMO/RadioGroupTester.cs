using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace UISelectable.Testing {
public class RadioGroupTester : MonoBehaviour {
    [SerializeField] List<RadioButtonUtility> _buttons = new();

    int _index;

    void Start() {
        for (int i = 0; i < _buttons.Count; i++) {
            int buttonIndex = i;
            _buttons[i].GetComponent<Button>().onClick.AddListener(() => Press(buttonIndex));
        }

        Press(0);
    }

    void Update() {
        if (Keyboard.current == null) { return; }

        // Q And E Cycle Tabs
        if (Keyboard.current.qKey.wasPressedThisFrame) { Press((_index - 1 + _buttons.Count) % _buttons.Count); }
        if (Keyboard.current.eKey.wasPressedThisFrame) { Press((_index + 1) % _buttons.Count); }
    }

    void Press(int index) {
        _index = index;

        foreach (var button in _buttons) {
            button.RadioButtonReleased();
        }

        _buttons[index].RadioButtonPressed();
    }
}
}
