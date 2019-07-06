using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UITextLocalize : MonoBehaviour {
    [SerializeField] protected string localizedID;

#if UNITY_EDITOR
    void Reset() {
        localizedID = GetComponent<Text>().text;
    }
#endif

    void Awake() {
        UpdateText();
        Localization.LanguageChanged += UpdateText;
    }

    void OnDestroy() {
        Localization.LanguageChanged -= UpdateText;
    }

    public virtual void UpdateText() {
        GetComponent<Text>().text = Localization.ByID(localizedID);
    }
}
