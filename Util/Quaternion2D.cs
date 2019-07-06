using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Quaternion2D
{
    public static Quaternion LookRotation(Vector2 forward2D)
    {
        return Quaternion.LookRotation(Vector3.forward, forward2D);
    }
}
