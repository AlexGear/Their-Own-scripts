using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CutsceneTrigger : MonoBehaviour {
    [SerializeField] Cutscene cutscene;

    private Coroutine playCoroutine = null;

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            playCoroutine = StartCoroutine(PlayAfterSavesLoadingIsCompleted());
        }
    }

    private IEnumerator PlayAfterSavesLoadingIsCompleted() {
        yield return null;
        cutscene.Play();
        this.gameObject.SetActive(false);
    }

    void OnDisable() {
        if(playCoroutine != null) {
            StopCoroutine(playCoroutine);
        }
    }
}
