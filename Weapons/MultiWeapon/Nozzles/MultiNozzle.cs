using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public class MultiNozzle : Nozzle
    {
        [SerializeField, Range(1, 9)] int nozzleNumber = 7;
        [SerializeField] float angle = 150;

        private Ray[] rays;

        protected override void Awake()
        {
            base.Awake();
            rays = new Ray[nozzleNumber];
        }

        public override IReadOnlyList<Ray> GetRays()
        {
#if UNITY_EDITOR
            if (nozzleNumber != rays.Length)
            {
                rays = new Ray[nozzleNumber];
            }
#endif
            Vector3 origin = transform.position;
            Vector3 dir = transform.up;

            dir = Quaternion.AngleAxis(-angle * 0.5f, Vector3.forward) * dir;
            Quaternion deltaRot = Quaternion.AngleAxis(angle / (nozzleNumber - 1), Vector3.forward);

            rays[0] = new Ray(origin, dir);
            for (int i = 1; i < nozzleNumber; i++)
            {
                dir = deltaRot * dir;
                rays[i] = new Ray(origin, dir);
            }

            return rays;
        }
    }
}