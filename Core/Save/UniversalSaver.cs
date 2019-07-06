using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISavable {
    object CaptureSnapshot();
    void RestoreSnapshot(object data);
}

public class UniversalSaver : Saver {
    [SerializeField, TypeRestrict(typeof(ISavable))] Object savable;

    public override void OnLoad(object data) {
        ((ISavable)savable).RestoreSnapshot(data);
    }

    public override object OnSave() {
        return ((ISavable)savable).CaptureSnapshot();
    }
}
