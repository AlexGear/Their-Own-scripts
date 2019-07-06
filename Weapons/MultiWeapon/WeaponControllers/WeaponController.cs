using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public abstract class WeaponController : Part
    {
        public abstract void OnUpdateBeingSelected();

        protected void EnsureDevicesNumber<T>(IList<T> devices, T prefab, int number, bool parentIsThis = true) where T : Object
        {
            while (devices.Count < number)
            {
                var instance = parentIsThis ? Instantiate(prefab, this.transform) : Instantiate(prefab);
                devices.Add(instance);
            }
        }
    }
}