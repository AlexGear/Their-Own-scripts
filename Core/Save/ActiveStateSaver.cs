using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveStateSaver : Saver {
    public override void OnLoad(object data) {
        this.gameObject.SetActive((bool)data);
    }

    public override object OnSave() {
        return this.gameObject.activeSelf;
    }
}
