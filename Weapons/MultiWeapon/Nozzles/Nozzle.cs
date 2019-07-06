using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public abstract class Nozzle : Part
    {
        public abstract IReadOnlyList<Ray> GetRays();
    }
}
