using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NextLevelLoadTrigger : MonoBehaviour {
    [SerializeField] string nextLevel;

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            SaveSystem.instance.PassToNextSceneAndSave(nextLevel);
        }
    }
}
