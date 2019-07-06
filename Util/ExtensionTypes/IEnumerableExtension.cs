using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IEnumerableExtension {
    public static T ClosestTo<T>(this IEnumerable<T> source, Vector2 point, 
        System.Func<T, Vector2> positionSelector, System.Predicate<T> filter = null) 
    {
        if(source == null)
            return default;

        T closest = default;
        float minSqrDist = float.PositiveInfinity;
        foreach(var entry in source) {
            if(filter != null && !filter(entry))
                continue;

            Vector2 entryPoint = positionSelector(entry);
            float sqrDist = (point - entryPoint).sqrMagnitude;
            if(sqrDist < minSqrDist) {
                minSqrDist = sqrDist;
                closest = entry;
            }
        }
        return closest;
    }
}
