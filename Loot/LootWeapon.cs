using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootWeapon : Loot {
    [SerializeField] private string weaponName;

    protected override void Start() {
        base.Start();
        needPressButton = true;
    }

    protected override void PickUp() {
        base.PickUp();
        mainCharacter.DropWeapon(mainCharacter.GetSelectedWeaponNumber());
        mainCharacter.SetWeaponAvailable(weaponName, true);
        mainCharacter.SelectWeapon(weaponName);
    }
}
