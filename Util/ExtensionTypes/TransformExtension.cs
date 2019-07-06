using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
    public static void AlignWithRay(this Transform transform, Ray ray)
    {
        transform.SetPositionAndRotation(ray.origin, Quaternion2D.LookRotation(ray.direction));
    }
}
