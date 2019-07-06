using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragileWindow : Unit {
    [SerializeField] private ParticleSystem shardsParticles;
    [SerializeField] private int brokenLayer;
    [SerializeField] private int brokenOrderInLayer;
    [SerializeField] private Sprite brokenSprite;
    [SerializeField] private AudioSource breakingAudio;

    protected override void OnDied() {
        this.gameObject.layer = brokenLayer;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = brokenSprite;
        spriteRenderer.sortingOrder = brokenOrderInLayer;
        shardsParticles.Play();
        breakingAudio.PlayOneShot(breakingAudio.clip);
        Destroy(this);
    }
}
