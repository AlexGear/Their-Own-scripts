using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootDropper : MonoBehaviour {
    [SerializeField] Loot loot;
    [SerializeField] bool dropOnOwnerDeath = true;
    [SerializeField, Range(0, 1)] float _chance = 1;

    private Unit owner;

    public float chance {
        get { return _chance; }
        set { _chance = value; }
    }

    public void DropLoot(Vector2 position) {
        Instantiate(loot.gameObject, position, Quaternion.identity);
    }

    public void DropLoot() {
        DropLoot(transform.position);
    }

    private void Awake() {
        if(!dropOnOwnerDeath) {
            return;
        }
        owner = GetComponent<Unit>();
        if(owner == null) {
            Debug.LogWarning("dropOnOwnerDeath==true, but no Unit script was found");
            return;
        }
        owner.Died += OnOwnerDied;
    }

    private void OnOwnerDied(Unit u) {
        if(Random.value < chance) {
            DropLoot();
        }
    }

    private void OnDestroy() {
        if(owner != null) {
            owner.Died -= OnOwnerDied;
        }
    }
}
