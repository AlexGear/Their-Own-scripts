using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CompositeCollider2D))]
public class CompositeShield : MonoBehaviour {
    public static CompositeShield current { get; private set; }

    private CompositeCollider2D compositeCollider;

    private void Awake() {
        current = this;
        compositeCollider = GetComponent<CompositeCollider2D>();
    }

    public bool OverlapPoint(Vector2 point) => compositeCollider.OverlapPoint(point);
}
