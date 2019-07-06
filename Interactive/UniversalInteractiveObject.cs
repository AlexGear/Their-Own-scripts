using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UniversalInteractiveObject : InteractiveObject {
    [SerializeField] UnityEvent onUsePressed;
    [SerializeField] float delay = 0;
    [SerializeField] bool deactivateGameObject = false;

    protected override void OnUsePressed() {
        Invoke(nameof(Trigger), delay);
    }

    private void Trigger() {
        onUsePressed.Invoke();
        if(deactivateGameObject) {
            this.gameObject.SetActive(false);
        }
    }
}
