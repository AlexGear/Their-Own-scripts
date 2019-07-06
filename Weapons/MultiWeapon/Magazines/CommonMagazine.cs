using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public class CommonMagazine : Magazine
    {
        [SerializeField] int _maxAmmos;
        [SerializeField] int _ammos;

        public override int ammos => _ammos;

        public override void Add(int ammos)
        {
            _ammos += ammos;
            if (_ammos > _maxAmmos)
            {
                _ammos = _maxAmmos;
            }
        }

        public override int TakeAtMost(int requiredAmmos)
        {
            if (requiredAmmos < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(requiredAmmos), "Cannot be negative");
            }

            int taken = Mathf.Min(requiredAmmos, _ammos);
            _ammos -= taken;
            return taken;
        }

        public override bool TryTake(int requiredAmmos)
        {
            if (requiredAmmos < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(requiredAmmos), "Cannot be negative");
            }

            if (_ammos < requiredAmmos)
            {
                return false;
            }
            _ammos -= requiredAmmos;
            return true;
        }

        public override string ToString() => $"{name} ({ammos}/{_maxAmmos})";
    }
}