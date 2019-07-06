using System.Collections.Generic;
using UnityEngine;

public class CommonWeapon : BaseWeapon {
    [SerializeField] private uint bulletsPerShot = 1;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] ParticleSystem shellParticles;

    public float bulletSpeed => bullet.GetComponent<Ammo>().GetSpeed();

    private GameObjectPool bulletPool;
    private Animator muzzleFlashAnimator;
    private static readonly int shotHash = Animator.StringToHash("Shot");

    protected override void Start() {
        base.Start();
        bulletPool = GameObjectPool.Get(bullet);
        muzzleFlashAnimator = muzzleFlash.GetComponent<Animator>();
    }

    protected override void Fire() {
        base.Fire();
        ReleaseBullet();
        if (bulletsPerShot > 1) {
            for (int i = 0; i < bulletsPerShot - 1; i++) {
                Invoke(nameof(ReleaseBullet), Random.Range(0f, 0.06f));
            }
        }
        if(muzzleFlashAnimator != null) {
            muzzleFlashAnimator.SetTrigger(shotHash);
        }
        if (shotAudioSource != null) {
            shotAudioSource.PlayOneShot(shotAudioSource.clip);
        }
        if(shellParticles != null) {
            shellParticles.Emit(1);
        }
    }

    private void ReleaseBullet() {
        float deviation = Random.Range(-scatter, scatter);
        Quaternion rotation = bulletStart.rotation * Quaternion.AngleAxis(deviation, Vector3.forward);
        var bulletObject = bulletPool.Take(bulletStart.position, rotation);
        var ammo = bulletObject.GetComponent<Ammo>();
        ammo.shooter = owner;
    }
}