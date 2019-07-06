using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootMedicine : Loot {
    [SerializeField] private int health;

    protected override void PickUp() {
        if (mainCharacter.health != mainCharacter.maxHealth) {
            base.PickUp();
            mainCharacter.health += health;
        }
    }
}
