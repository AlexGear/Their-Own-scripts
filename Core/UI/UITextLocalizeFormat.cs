using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextLocalizeFormat : UITextLocalize {
    [SerializeField] string[] formatArgs;

    public override void UpdateText() {
        string ls = Localization.ByID(localizedID);
        GetComponent<Text>().text = string.Format(ls, formatArgs);
    }
}
