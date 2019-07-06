using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Localization {
    [System.Serializable]
    private struct LocalizedString {
        public string id;

        public string en;
        public string ru;

        public string GetLocalized(Language language) {
            string result = null;
            switch(language) {
                case Language.Ru: result = ru; break;
                case Language.En: result = en; break;
            }
            return result ?? en ?? id;
        }
    }

    [System.Serializable]
    private struct LocalizationData {
        public LocalizedString[] localizedStrings;
    }

    public enum Language { En, Ru }

    private static Language defaultLanguage {
        get {
            switch(Application.systemLanguage) {
                case SystemLanguage.Russian: return Language.Ru;
                default: return Language.En;
            }
        }
    }
    private const string playerPrefsKey = "Language";
    private const string localizationJsonFile = "Strings";
    private static Dictionary<string, LocalizedString> localizedStrings;

    private static Language _language;
    public static Language language {
        get { return _language; }
        set {
            if(value == _language) {
                return;
            }
            _language = value;
            PlayerPrefs.SetInt(playerPrefsKey, (int)_language);
            
            LanguageChanged?.Invoke();
        }
    }
    public static event System.Action LanguageChanged;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load() {
        _language = (Language)PlayerPrefs.GetInt(playerPrefsKey, (int)defaultLanguage);

        var localizationAsset = Resources.Load<TextAsset>(localizationJsonFile);
        var data = JsonUtility.FromJson<LocalizationData>(localizationAsset.text);
        localizedStrings = new Dictionary<string, LocalizedString>();
        foreach(var ls in data.localizedStrings) {
            if(ls.id != null) {
                localizedStrings[ls.id] = ls;
            }
        }
    }

    public static string ByID(string id) {
        string result = id;
        LocalizedString ls;
        if(localizedStrings.TryGetValue(id, out ls)) {
            result = ls.GetLocalized(_language);
        }
        return result;
    }
}
