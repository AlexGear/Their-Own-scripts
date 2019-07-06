using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public class CannonController : ChargingShotController
    {
        [SerializeField] LightningEmitter lightningEmitterPrefab;
        private List<LightningEmitter> lightningEmitters = new List<LightningEmitter>();

        private string status = null;

        private void OnGUI()
        {
            if (status != null)
            {
                int old = GUI.skin.box.fontSize;
                GUI.skin.box.fontSize = 25;
                GUI.Box(new Rect(400, 50, 110, 40), status);
                GUI.skin.box.fontSize = old;
            }
        }

        protected override void OnChargingProgress(float t)
        {
            status = $"{Mathf.CeilToInt(t * 100)}%";
        }

        protected override void OnChargingInterrupted()
        {
            status = null;
        }

        protected override void OnFire()
        {
            status = null;

            Magazine magazine = weapon.magazine;
            if (magazine.isEmpty)
            {
                return;
            }
            switch (magazine)
            {
                case BulletMagazine bulletMag:
                    ShootBullets(bulletMag.heavyBulletPool, magazine, weapon.nozzle, weapon.owner, 20);
                    break;

                case ElectricityMagazine electroMag:
                    EmitLightnings(electroMag, weapon.nozzle, weapon.owner, 35);
                    break;
            }
        }

        private void EmitLightnings(ElectricityMagazine magazine, Nozzle nozzle, Unit owner, int cost = 1)
        {
            var rays = nozzle.GetRays();
            int ammosTaken = magazine.TakeAtMost(rays.Count * cost);
            int count = Mathf.CeilToInt((float)ammosTaken / cost);

            EnsureDevicesNumber(lightningEmitters, lightningEmitterPrefab, count);
            for (int i = 0; i < count; i++)
            {
                var lightningEmitter = lightningEmitters[i];
                lightningEmitter.transform.AlignWithRay(rays[i]);
                lightningEmitter.Fire(owner);
            }
        }
    }
}