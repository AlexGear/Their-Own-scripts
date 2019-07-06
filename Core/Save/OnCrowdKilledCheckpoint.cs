using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class OnCrowdKilledCheckpoint : BaseCheckpoint {
    [SerializeField] List<Unit> crowd;

    void Awake() {
        foreach(var unit in crowd) {
            if(unit != null) {
                unit.Died += OnUnitDied;
            }
        }
    }

    private void OnUnitDied(Unit unit) {
        if(crowd.Contains(unit)) {
            crowd.Remove(unit);
            unit.Died -= OnUnitDied;
            if(!crowd.Any(u => u.isActiveAndEnabled)) {
                OnCrowdKilled();
            }
        }
    }

    protected virtual void OnCrowdKilled() {
        this.gameObject.SetActive(false);
        Save();
    }
}
