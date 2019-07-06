using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MainCharacter))]
public class SelectedWeaponSaver : Saver {
    public override void OnLoad(object data) {
        GetComponent<MainCharacter>().SelectWeapon((int)data);
    }

    public override object OnSave() {
        return GetComponent<MainCharacter>().GetSelectedWeaponNumber();
    }
}
