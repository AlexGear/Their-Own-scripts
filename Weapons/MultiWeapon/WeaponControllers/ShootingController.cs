using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public abstract class ShootingController : WeaponController
    {
        [SerializeField] protected float scatter = 5;

        protected void ShootBullets(GameObjectPool pool, Magazine magazine, Nozzle nozzle, Unit owner, int cost = 1)
        {
            var rays = nozzle.GetRays();
            int ammosTaken = magazine.TakeAtMost(rays.Count * cost);
            int count = Mathf.CeilToInt((float)ammosTaken / cost);

            for (int i = 0; i < count; i++)
            {
                ReleaseBullet(rays[i], pool, owner);
            }
        }

        protected void ReleaseBullet(Ray ray, GameObjectPool pool, Unit owner)
        {
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, ray.direction);
            float deviation = Random.Range(-scatter, scatter);
            rotation = Quaternion.AngleAxis(deviation, Vector3.forward) * rotation;

            var bulletObject = pool.Take(ray.origin, rotation);
            var ammo = bulletObject.GetComponent<Ammo>();
            ammo.shooter = owner;
        }
    }
}