using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Deflection {
    public static Vector2 CalculateDirection(Vector2 shooterPosition, Vector2 targetPosition, 
        float projectileSpeed, Vector2 targetVelocity, int iterations = 3)
    {
        Vector2 lookDirection = (targetPosition - shooterPosition).normalized;
        Vector2 startLookDirection = lookDirection;
        for(int i = 0; i < iterations; i++) {
            float bulletTravelTime = lookDirection.magnitude / projectileSpeed;
            Vector2 deflectionOffset = targetVelocity * bulletTravelTime;
            lookDirection = startLookDirection + deflectionOffset;
        }
        return lookDirection;
    }
}
