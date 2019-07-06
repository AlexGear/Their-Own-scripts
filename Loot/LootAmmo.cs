using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootAmmo : Loot {
    [SerializeField] private AmmoType ammoType;
    [SerializeField] private int value;

    protected override void PickUp() {
        int currentAmmo = mainCharacter.ammoBag.GetAmmo(ammoType);
        if (currentAmmo != mainCharacter.ammoBag.GetMaxAmmo(ammoType)) {
            base.PickUp();
            mainCharacter.ammoBag.SetAmmo(ammoType, currentAmmo + value);
        }
    }
}
