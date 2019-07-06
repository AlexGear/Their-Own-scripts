using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : BaseCheckpoint {
    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            this.gameObject.SetActive(false);
            if(Time.timeSinceLevelLoad > 1f) {  // prevent saving again when loading right at the checkpoint trigger
                Save();
            }
        }
    }
}
