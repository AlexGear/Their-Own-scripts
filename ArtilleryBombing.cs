using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtilleryBombing : MonoBehaviour {
    [SerializeField] GameObject explosionPreparePrefab;
    [SerializeField] float prepareDuration = 2f;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float minInterval = 2f;
    [SerializeField] float maxInterval = 8f;

    private bool isOnTrigger = false;

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            isOnTrigger = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            isOnTrigger = false;
        }
    }

    IEnumerator Start() {
        while(true) {
            if(MainCharacter.current.isDead)
                yield break;

            if(isOnTrigger) {
                Vector2 position = MainCharacter.current.position;
                Vector2 velocity = MainCharacter.current.velocityTrend;
                position += velocity * prepareDuration;
                position += Random.insideUnitCircle * 1.8f;

                Instantiate(explosionPreparePrefab, position, Quaternion.identity);
                yield return new WaitForSeconds(prepareDuration);
                var explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
                explosion.GetComponent<Explosion>()?.Explode();
            }

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval) - prepareDuration);
        }
    }
}
