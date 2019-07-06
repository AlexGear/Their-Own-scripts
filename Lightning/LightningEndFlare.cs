using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEndFlare : MonoBehaviour {
    [SerializeField] private float sparkChance = 0.2f;
    [SerializeField] private float scaleVariation = 0.5f;

    private MeshRenderer lightMeshRenderer;
    private ParticleSystem sparkParticle;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

	void Awake() {
        originalScale = transform.localScale;
        lightMeshRenderer = GetComponent<MeshRenderer>();
        sparkParticle = GetComponentInChildren<ParticleSystem>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

    public void Show() {
        lightMeshRenderer.enabled = true;
        spriteRenderer.enabled = true;
        transform.localScale = originalScale * Random.Range(1f - scaleVariation, 1f + scaleVariation);
        if(Random.value <= sparkChance) {
            sparkParticle.Play();
        }
    }

    public void Hide() {
        if(lightMeshRenderer != null) {
            lightMeshRenderer.enabled = false;
        }
        if(spriteRenderer != null) {
            spriteRenderer.enabled = false;
        }
    }
}
