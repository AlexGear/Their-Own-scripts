using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour {
    private bool playerIsNear = false;
    
    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            playerIsNear = true;
            PlayerEntered();
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            playerIsNear = false;
            PlayerExited();
        }
    }

    protected virtual void PlayerEntered() {
        // TODO: show hint
    }

    protected virtual void PlayerExited() {
        // TODO: hide hint
    }

    private void Update() {
        if(playerIsNear && MainCharacter.current.inputEnabled && Input.GetButtonDown("Use")) {
            OnUsePressed();
        }
    }

    protected abstract void OnUsePressed();
}
