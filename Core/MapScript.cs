using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapScript : MonoBehaviour {
    [SerializeField] SpriteRenderer mapBounds;

    public static MapScript current { get; private set; }

    public Vector2 mapOrigin => mapBounds.bounds.min;
    public Vector2 mapCenter => mapBounds.bounds.center;
    public Vector2 mapSize => mapBounds.bounds.size;

    void Awake() {
        current = this;
    }
}
