using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Armament
{
    public class SingleNozzle : Nozzle
    {
        public override IReadOnlyList<Ray> GetRays()
        {
            return new[] { new Ray(transform.position, transform.up) };
        }
    }
}