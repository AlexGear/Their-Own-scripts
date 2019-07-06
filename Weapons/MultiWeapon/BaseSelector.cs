using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    [System.Serializable]
    public abstract class BaseSelector
    {
        public abstract int selectedIndex { get; set; }
        public abstract IReadOnlyList<Part> GetParts();
    }
}