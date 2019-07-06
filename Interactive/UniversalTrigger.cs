using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UniversalTrigger : MonoBehaviour {
    [SerializeField] UnityEvent onTriggered;
    [SerializeField] float delay = 0;
    [SerializeField] bool deactivateGameObject = true;

    bool triggered = false;

    void OnTriggerEnter2D(Collider2D collision) {
        if(triggered)
            return;

        if(collision.CompareTag("Player")) {
            triggered = true;
            Invoke(nameof(Trigger), delay);
        }
    }

    private void Trigger() {
        onTriggered.Invoke();
        if(deactivateGameObject) {
            this.gameObject.SetActive(false);
        }
    }
}
