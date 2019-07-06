using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LayerSetter : MonoBehaviour {
    [SerializeField, Layer] int layer;

    private void Update() {
        if(gameObject.layer != layer)
            gameObject.layer = layer;
    }
}
