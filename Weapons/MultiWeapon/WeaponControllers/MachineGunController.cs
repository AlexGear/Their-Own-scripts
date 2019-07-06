using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public class MachineGunController : ShootingController
    {
        [SerializeField] float cooldown = 0.1f;
        [SerializeField] Flamethrower flamethrowerPrefab;
        [SerializeField] LightningEmitter lightningEmitterPrefab;

        private Timer cooldownTimer;
        private List<Flamethrower> flamethrowers;
        private List<LightningEmitter> lightningEmitters;
        private bool flamethrowersAreShut = true;

        protected override void Awake()
        {
            base.Awake();
            cooldownTimer = new Timer(cooldown);
            cooldownTimer.SetOnEdge();
            flamethrowers = new List<Flamethrower>();
            lightningEmitters = new List<LightningEmitter>();
        }

        public override void OnWeaponReconfigured()
        {
            ShutFlamethrowers();
        }

        public override void OnUpdateBeingSelected()
        {
            if (!Input.GetButton("Fire"))
            {
                if (!flamethrowersAreShut)
                {
                    ShutFlamethrowers();
                }
                return;
            }
            if (cooldownTimer.Tick())
            {
                Fire();
            }
        }

        private void Fire()
        {
            Magazine magazine = weapon.magazine;
            if (magazine.isEmpty)
            {
                return;
            }
            switch (magazine)
            {
                case BulletMagazine bulletMag:
                    ShootBullets(bulletMag.bulletPool, magazine, weapon.nozzle, weapon.owner);
                    break;

                case FireMagazine fireMag:
                    ThrowFlames(fireMag, weapon.nozzle, weapon.owner);
                    break;

                case ElectricityMagazine electroMag:
                    EmitLightnings(electroMag, weapon.nozzle, weapon.owner);
                    break;
            }
        }

        private void ThrowFlames(FireMagazine magazine, Nozzle nozzle, Unit owner)
        {
            flamethrowersAreShut = false;

            var rays = nozzle.GetRays();
            int count = magazine.TakeAtMost(rays.Count);
            EnsureDevicesNumber(flamethrowers, flamethrowerPrefab, count);
            int i;
            for (i = 0; i < count; i++)
            {
                var flamethrower = flamethrowers[i];
                flamethrower.transform.AlignWithRay(rays[i]);
                flamethrower.StartFlameParticlesOnly();
                flamethrower.ApplyAreaDamage(owner);
            }
            for (; i < flamethrowers.Count; i++)
            {
                flamethrowers[i].Stop();
            }
        }

        private void EmitLightnings(ElectricityMagazine magazine, Nozzle nozzle, Unit owner)
        {
            var rays = nozzle.GetRays();
            int count = magazine.TakeAtMost(rays.Count);
            EnsureDevicesNumber(lightningEmitters, lightningEmitterPrefab, count);
            for (int i = 0; i < count; i++)
            {
                var lightningEmitter = lightningEmitters[i];
                lightningEmitter.transform.AlignWithRay(rays[i]);
                lightningEmitter.Fire(owner);
            }
        }

        private void ShutFlamethrowers()
        {
            foreach (var flamethrower in flamethrowers)
            {
                flamethrower.Stop();
            }
            flamethrowersAreShut = true;
        }
    }
}