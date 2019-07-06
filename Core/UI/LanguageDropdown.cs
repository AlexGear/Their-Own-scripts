using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Dropdown))]
public class LanguageDropdown : MonoBehaviour {
    private Dropdown dropdown;
    private UnityAction<int> onValueChangedListener;

    void Awake() {
        dropdown = GetComponent<Dropdown>();
        onValueChangedListener = new UnityAction<int>(OnValueChanged);
        dropdown.onValueChanged.AddListener(onValueChangedListener);
        Localization.LanguageChanged += UpdateLanguage;

        UpdateLanguage();
    }

    private void UpdateLanguage() {
        dropdown.onValueChanged.RemoveListener(onValueChangedListener);
        dropdown.value = (int)Localization.language;
        dropdown.onValueChanged.AddListener(onValueChangedListener);
    }

    private void OnValueChanged(int value) {
        Localization.language = (Localization.Language)dropdown.value;
    }
}
