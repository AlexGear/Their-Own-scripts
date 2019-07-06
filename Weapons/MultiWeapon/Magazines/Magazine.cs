using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public abstract class Magazine : Part
    {
        public abstract int ammos { get; }
        public abstract void Add(int ammos);
        public abstract bool TryTake(int requiredAmmos);
        public abstract int TakeAtMost(int requiredAmmos);

        public bool isEmpty => ammos <= 0;

        public override string ToString() => $"{name} ({ammos})";
    }
}
