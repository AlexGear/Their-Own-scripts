using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Xray : MonoBehaviour {
    [SerializeField] Collider2D trigger;
    [SerializeField] Renderer[] targetRenderers;
    [SerializeField] float transitionSpeed = 1.5f;
    [SerializeField] bool playerOnly = false;
    [SerializeField] GameObject enableObjectOnXray = null;
    
    private const float xrayedAlpha = 0;

    private bool xrayActive = false;
    private List<Collider2D> touching = new List<Collider2D>();

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player") || (!playerOnly && collision.CompareTag("XrayTrigger"))) {
            touching.Add(collision);
            if(!xrayActive) {
                xrayActive = true;
                StopAllCoroutines();
                for(int i = 0; i < targetRenderers.Length; i++) {
                    StartCoroutine(XrayTransitionDown(targetRenderers[i]));
                }
            }
        }
    }

    void Update() {
        if(!xrayActive) {
            return;
        }
        touching.RemoveAll(x => x == null || !x.isActiveAndEnabled || !x.IsTouching(trigger));
        if(!touching.Any()) {
            xrayActive = false;
            StopAllCoroutines();
            for(int i = 0; i < targetRenderers.Length; i++) {
                StartCoroutine(XrayTransitionUp(targetRenderers[i]));
            }
        }
    }

    private IEnumerator XrayTransitionDown(Renderer target) {
        if(enableObjectOnXray != null) {
            enableObjectOnXray.SetActive(true);
        }
        while(!Mathf.Approximately(target.material.color.a, xrayedAlpha)) {
            Color color = target.material.color;
            color.a = Mathf.MoveTowards(color.a, xrayedAlpha, transitionSpeed * Time.deltaTime);
            target.material.color = color;

            yield return null;
        }
    }

    private IEnumerator XrayTransitionUp(Renderer target) {
        while(!Mathf.Approximately(target.material.color.a, 1f)) {
            Color color = target.material.color;
            color.a = Mathf.MoveTowards(color.a, 1f, transitionSpeed * Time.deltaTime);
            target.material.color = color;

            yield return null;
        }
        if(enableObjectOnXray != null) {
            enableObjectOnXray.SetActive(false);
        }
    }
}
