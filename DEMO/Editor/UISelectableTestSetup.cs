using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds a test canvas with a slider, dropdown and radio buttons wired to the utilities
public static class UISelectableTestSetup {
    const string _settingsFolder = "Assets/UISelectableTest";
    const float _rowHeight = 40f;

    static readonly Color _normalText = new(0.2f, 0.2f, 0.2f);
    static readonly Color _selectedText = new(0f, 0.45f, 0.9f);

    static DefaultControls.Resources _uiResources;
    static TMP_DefaultControls.Resources _tmpResources;

    [MenuItem("Tools/UI Selectable/Create Test Setup")]
    static void CreateTestSetup() {
        LoadResources();
        EnsureEventSystem();

        if (!AssetDatabase.IsValidFolder(_settingsFolder)) {
            AssetDatabase.CreateFolder("Assets", "UISelectableTest");
        }

        // Settings default to fully transparent colors, so every color is set explicitly
        SliderSettings sliderSettings = CreateSettings<SliderSettings>("SliderSettings");
        sliderSettings.FillNormalColor = new Color(0.5f, 0.5f, 0.5f);
        sliderSettings.FillSelectedColor = _selectedText;

        DropdownSettings dropdownSettings = CreateSettings<DropdownSettings>("DropdownSettings");
        dropdownSettings.DropdownNormalColor = Color.white;
        dropdownSettings.DropdownExpandedColor = new Color(0.85f, 0.92f, 1f);

        RadioButtonSettings radioSettings = CreateSettings<RadioButtonSettings>("RadioButtonSettings");
        radioSettings.HighlightedTextColor = new Color(0.3f, 0.6f, 1f);
        radioSettings.ButtonPressedColor = new Color(0.8f, 0.9f, 1f);
        radioSettings.ButtonDefaultColor = Color.white;

        AssetDatabase.SaveAssets();

        // Canvas and vertical panel
        GameObject canvasObject = new("UISelectable Test Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject panelObject = new("Panel", typeof(RectTransform), typeof(VerticalLayoutGroup));
        panelObject.transform.SetParent(canvasObject.transform, false);
        panelObject.GetComponent<RectTransform>().sizeDelta = new Vector2(400f, 300f);

        VerticalLayoutGroup panelLayout = panelObject.GetComponent<VerticalLayoutGroup>();
        panelLayout.spacing = 20f;
        panelLayout.padding = new RectOffset(20, 80, 20, 20);
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        BuildSlider(panelObject.transform, sliderSettings);
        BuildDropdown(panelObject.transform, dropdownSettings);
        BuildRadioRow(panelObject, radioSettings);

        Undo.RegisterCreatedObjectUndo(canvasObject, "Create UISelectable Test Setup");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = canvasObject;
    }

    #region Builders

    static void BuildSlider(Transform parent, SliderSettings settings) {
        GameObject sliderObject = DefaultControls.CreateSlider(_uiResources);
        sliderObject.name = "Test Slider";
        Attach(sliderObject, parent);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.value = 0.5f;

        // Percentage label sits to the right of the slider
        GameObject labelObject = TMP_DefaultControls.CreateText(_tmpResources);
        labelObject.name = "Percentage Text";
        labelObject.transform.SetParent(sliderObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(1f, 0.5f);
        labelRect.anchorMax = new Vector2(1f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(10f, 0f);
        labelRect.sizeDelta = new Vector2(60f, 30f);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.raycastTarget = false;

        SliderUtility utility = sliderObject.AddComponent<SliderUtility>();
        SerializedObject serialized = new(utility);
        SetReference(serialized, "_settings", settings);
        SetReference(serialized, "_percentageText", label);
        SetReference(serialized, "_fill", slider.fillRect.GetComponent<Image>());
        SetReferenceList(serialized, "_texts", label);
        serialized.ApplyModifiedProperties();
    }

    static void BuildDropdown(Transform parent, DropdownSettings settings) {
        GameObject dropdownObject = TMP_DefaultControls.CreateDropdown(_tmpResources);
        dropdownObject.name = "Test Dropdown";
        Attach(dropdownObject, parent);

        TMP_Dropdown dropdown = dropdownObject.GetComponent<TMP_Dropdown>();
        dropdown.options = new List<TMP_Dropdown.OptionData> {
            new("Option A"), new("Option B"), new("Option C")
        };
        dropdown.RefreshShownValue();

        DropDownUtility utility = dropdownObject.AddComponent<DropDownUtility>();
        SerializedObject serialized = new(utility);
        SetReference(serialized, "_settings", settings);
        SetReferenceList(serialized, "_texts", dropdown.captionText as TextMeshProUGUI);
        serialized.ApplyModifiedProperties();
    }

    static void BuildRadioRow(GameObject panel, RadioButtonSettings settings) {
        GameObject rowObject = new("Radio Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        Attach(rowObject, panel.transform);

        HorizontalLayoutGroup rowLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 10f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;

        List<RadioButtonUtility> utilities = new();

        for (int i = 0; i < 3; i++) {
            GameObject buttonObject = TMP_DefaultControls.CreateButton(_tmpResources);
            buttonObject.name = $"Radio Button {i + 1}";
            buttonObject.transform.SetParent(rowObject.transform, false);

            TextMeshProUGUI text = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"Tab {i + 1}";

            // Radio buttons are not selectable, so navigation is disabled
            Button button = buttonObject.GetComponent<Button>();
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;

            RadioButtonUtility utility = buttonObject.AddComponent<RadioButtonUtility>();
            SerializedObject serialized = new(utility);
            SetReference(serialized, "_settings", settings);
            SetReferenceList(serialized, "_texts", text);
            serialized.ApplyModifiedProperties();

            utilities.Add(utility);
        }

        RadioGroupTester tester = panel.AddComponent<RadioGroupTester>();
        SerializedObject testerSerialized = new(tester);
        SetReferenceList(testerSerialized, "_buttons", utilities.ToArray());
        testerSerialized.ApplyModifiedProperties();
    }

    #endregion

    #region Helpers

    // Parents the object and gives it a fixed row height for the layout group
    static void Attach(GameObject target, Transform parent) {
        target.transform.SetParent(parent, false);
        target.AddComponent<LayoutElement>().preferredHeight = _rowHeight;
    }

    static T CreateSettings<T>(string assetName) where T : SelectableSettings {
        T settings = ScriptableObject.CreateInstance<T>();
        string path = AssetDatabase.GenerateUniqueAssetPath($"{_settingsFolder}/{assetName}.asset");
        AssetDatabase.CreateAsset(settings, path);

        settings.TextNormalColor = _normalText;
        settings.TextSelectedColor = _selectedText;
        EditorUtility.SetDirty(settings);
        return settings;
    }

    static void SetReference(SerializedObject serialized, string propertyName, Object value) {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) {
            Debug.LogError($"UISelectableTestSetup: field '{propertyName}' not found on {serialized.targetObject.GetType().Name}");
            return;
        }

        property.objectReferenceValue = value;
    }

    static void SetReferenceList(SerializedObject serialized, string propertyName, params Object[] values) {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) {
            Debug.LogError($"UISelectableTestSetup: field '{propertyName}' not found on {serialized.targetObject.GetType().Name}");
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    static void LoadResources() {
        Sprite standard = LoadSprite("UI/Skin/UISprite.psd");
        Sprite background = LoadSprite("UI/Skin/Background.psd");
        Sprite inputField = LoadSprite("UI/Skin/InputFieldBackground.psd");
        Sprite knob = LoadSprite("UI/Skin/Knob.psd");
        Sprite checkmark = LoadSprite("UI/Skin/Checkmark.psd");
        Sprite dropdown = LoadSprite("UI/Skin/DropdownArrow.psd");
        Sprite mask = LoadSprite("UI/Skin/UIMask.psd");

        _uiResources = new DefaultControls.Resources {
            standard = standard,
            background = background,
            inputField = inputField,
            knob = knob,
            checkmark = checkmark,
            dropdown = dropdown,
            mask = mask
        };
        _tmpResources = new TMP_DefaultControls.Resources {
            standard = standard,
            background = background,
            inputField = inputField,
            knob = knob,
            checkmark = checkmark,
            dropdown = dropdown,
            mask = mask
        };
    }

    static Sprite LoadSprite(string path) {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>(path);
    }

    // The slider reads the module's move action, so it must be the Input System module
    static void EnsureEventSystem() {
        EventSystem existing = Object.FindAnyObjectByType<EventSystem>();

        if (existing == null) {
            GameObject eventSystemObject = new("EventSystem", typeof(EventSystem));
            InputSystemUIInputModule module = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
        }
        else if (existing.GetComponent<InputSystemUIInputModule>() == null) {
            Debug.LogWarning("UISelectableTestSetup: existing EventSystem has no InputSystemUIInputModule, slider input will not work.");
        }
    }

    #endregion
}