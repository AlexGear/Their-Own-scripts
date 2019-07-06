using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CachedVisibilityChecker {
    private struct CheckQuery : System.IEquatable<CheckQuery> {
        public Vector2Int equalityGridPoint;
        public Unit unit;
        public bool straightLine;

        public bool Equals(CheckQuery other) {
            return equalityGridPoint == other.equalityGridPoint && unit == other.unit && straightLine == other.straightLine;
        }

        public override int GetHashCode() => equalityGridPoint.GetHashCode();
    }
    private class CheckResult {
        public bool visible;
        public float timeStamp;
        public bool hasExpired => Time.time - timeStamp > factValidityTime;
    }
    private static Dictionary<CheckQuery, CheckResult> checkFacts;
    private const float factValidityTime = 0.2f;
    private const float equalityGridCellSize = 0.5f;

    private static int calls;
    private const int callsToCollectGarbage = 100;

    static CachedVisibilityChecker() {
        checkFacts = new Dictionary<CheckQuery, CheckResult>();
        calls = 0;
    }

    public static bool IsUnitVisibleFrom(Unit unit, Vector2 point, int obstacleMask, bool straightLine = false) {
        if(unit == null) {
            return false;
        }
        var gridPoint = Vector2Int.FloorToInt(point / equalityGridCellSize);
        var query = new CheckQuery { unit = unit, equalityGridPoint = gridPoint, straightLine = straightLine };

        CheckResult checkResult;
        if(!checkFacts.TryGetValue(query, out checkResult) || checkResult.hasExpired) {
            checkResult = new CheckResult {
                visible = query.unit.IsVisibleFromPoint(point, obstacleMask, straightLine),
                timeStamp = Time.time
            };
            checkFacts[query] = checkResult;
        }

        if(++calls >= callsToCollectGarbage) {
            calls = 0;
            CollectGarbage();
        }

        return checkResult.visible;
    }

    private static void CollectGarbage() {
        var expiredFactKeys = new List<CheckQuery>();
        foreach(var fact in checkFacts) {
            if(fact.Value.hasExpired) {
                expiredFactKeys.Add(fact.Key);
            }
        }
        foreach(var expiredFactKey in expiredFactKeys) {
            checkFacts.Remove(expiredFactKey);
        }
    }
}
