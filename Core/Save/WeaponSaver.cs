using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseWeapon))]
public class WeaponSaver : Saver {
    [System.Serializable]
    private struct WeaponData {
        public bool isAvailable;
        public int maxAmmo, ammo;
    }

    public override void OnLoad(object data) {
        var weaponData = (WeaponData)data;
        var weapon = GetComponent<BaseWeapon>();
        weapon.isAvailable = weaponData.isAvailable;
        weapon.maxAmmo = weaponData.maxAmmo;
        weapon.ammo = weaponData.ammo;
    }

    public override object OnSave() {
        var weapon = GetComponent<BaseWeapon>();
        return new WeaponData {
            isAvailable = weapon.isAvailable,
            maxAmmo = weapon.maxAmmo,
            ammo = weapon.ammo
        };
    }
}
