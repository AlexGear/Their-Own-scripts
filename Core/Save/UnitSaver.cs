using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitSaver : Saver {
    [System.Serializable]
    private struct UnitData {
        public float health, maxHealth;
    }

    public override void OnLoad(object data) {
        var unitData = (UnitData)data;
        var unit = GetComponent<Unit>();
        unit.maxHealth = unitData.maxHealth;
        unit.health = unitData.health;
    }

    public override object OnSave() {
        var unit = GetComponent<Unit>();
        return new UnitData {
            maxHealth = unit.maxHealth,
            health = unit.health
        };
    }
}
