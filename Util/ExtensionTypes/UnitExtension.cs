using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitExtension {
    public static bool Is(this Unit unit) {
        return unit != null && !unit.isDead && unit.isActiveAndEnabled;
    }

    public static TUnit ClosestTo<TUnit>(this IEnumerable<TUnit> units, Vector2 point, 
        System.Predicate<TUnit> filter = null) where TUnit : Unit 
    {
        return units.ClosestTo(point, u => u.position, filter);
    }
}
