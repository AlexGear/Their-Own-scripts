using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientLightScript : MonoBehaviour {
    void Start() {
        Vector3 mapPos = MapScript.current.mapCenter;
        transform.position = new Vector3(mapPos.x, mapPos.y, mapPos.z + 2f);
        transform.localScale = MapScript.current.mapSize - new Vector2(5f, 5f);
    }
}
