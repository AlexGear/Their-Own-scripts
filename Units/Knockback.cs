using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour {
    private Rigidbody2D rb;
    private IEnumerator flightCoroutine = null;
    private UnityEngine.AI.NavMeshAgent navAgent;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public void FromEpicenter(Vector2 epicenter, float force, float duration) {
        Vector2 forceVector = (rb.position - epicenter).normalized * force;
        rb.velocity = Vector2.zero;
        rb.AddForce(forceVector, ForceMode2D.Impulse);
        rb.simulated = true;
        navAgent.enabled = false;
        StartFlailAnimation();

        if(flightCoroutine != null) {
            StopCoroutine(flightCoroutine);
        }
        flightCoroutine = FlightCoroutine(duration);
        StartCoroutine(flightCoroutine);
    }

    private IEnumerator FlightCoroutine(float duration) {
        float timer = 0;
        while(timer < duration) {
            timer += Time.deltaTime;
            yield return null;
        }
        rb.simulated = false;
        StopFlailAnimation();
        navAgent.enabled = true;

        flightCoroutine = null;
    }

    private void StartFlailAnimation() {

    }

    private void StopFlailAnimation() {

    }
}
