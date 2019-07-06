using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public class HookController : WeaponController
    {
        [SerializeField] Hook hookPrefab;
        
        private List<Hook> hooks = new List<Hook>();

        public override void OnWeaponReconfigured()
        {
            ReleaseHooks();
        }

        public override void OnUpdateBeingSelected()
        {
            if (Input.GetButtonDown("Fire"))
            {
                if (hooks.TrueForAll(h => h.isRetracted))
                {
                    LaunchHooks(weapon.nozzle);
                }
            }
            else if (Input.GetButtonDown("Secondary Fire"))
            {
                ReleaseHooks();
            }
        }

        private void LaunchHooks(Nozzle nozzle)
        {
            var rays = nozzle.GetRays();
            EnsureDevicesNumber(hooks, hookPrefab, rays.Count, parentIsThis: false);
            for (int i = 0; i < rays.Count; i++)
            {
                var hook = hooks[i];
                var ray = rays[i];
                hook.transform.AlignWithRay(ray);
                hook.Launch(this.transform, ray.direction);
            }
        }

        private void ReleaseHooks()
        {
            foreach (var hook in hooks)
            {
                hook.StartRetracting();
            }
        }
    }
}