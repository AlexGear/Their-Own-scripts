using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigun : CommonWeapon {
    [SerializeField] AudioClip torsionStartSound;
    [SerializeField] AudioClip torsionIdleSound;
    [SerializeField] AudioSource torsionAudioSource;
    [SerializeField] AudioSource shootingAudioSource;

    void OnEnable() {
        shootingAudioSource.mute = true;
        torsionAudioSource.clip = torsionStartSound;
        torsionAudioSource.Play();
        StartCoroutine(IdleSound());
    }

    private IEnumerator IdleSound() {
        yield return new WaitForSeconds(torsionStartSound.length - 0.05f);
        torsionAudioSource.clip = torsionIdleSound;
        torsionAudioSource.Play();
    }

    protected override void Update() {
        base.Update();
        bool isShooting = isTriggerPressed && ammo > 0 && isFireLineUnlocked;
        shootingAudioSource.mute = !isShooting;
    }
}
