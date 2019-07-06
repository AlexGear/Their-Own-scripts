using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TransformRandomizer : MonoBehaviour {
#if UNITY_EDITOR
    [Header("Position Z")]
    [SerializeField] bool randomizePositionZ = false;
    [SerializeField] float minZ = 0;
    [SerializeField] float maxZ = 0;

    [Header("Rotation")]
    [SerializeField] bool randomizeRotation = true;
    [SerializeField] float minAngle = 0;
    [SerializeField] float maxAngle = 360;
    [SerializeField] bool localRotation = true;

    [Header("Scale")]
    [SerializeField] bool randomizeScale = true;
    [SerializeField] Vector3 minScale;
    [SerializeField] Vector3 maxScale;
    [SerializeField] bool scaleAlongAllAxesTogether = true;

    [Header("Randomize")]
    [SerializeField] bool randomizeChildren = true;

    void Reset() {
        minScale = transform.localScale * 0.9f;
        maxScale = transform.localScale * 1.1f;
    }
    
    [ContextMenu("Randomize")]
    private void Randomize() {
        if(randomizePositionZ) RandomizePosition();
        if(randomizeRotation) RandomizeRotation();
        if(randomizeScale) RandomizeScale();

        if(randomizeChildren) {
            foreach(var randomizer in GetComponentsInChildren<TransformRandomizer>()) {
                if(randomizer == this) {
                    continue;
                }
                randomizer.Randomize();
            }
        }
    }

    private void RandomizePosition() {
        Vector3 position = transform.position;
        position.z = Random.Range(minZ, maxZ);
        transform.position = position;
    }

    private void RandomizeRotation() {
        float angle = Random.Range(minAngle, maxAngle);
        if(localRotation) {
            transform.localEulerAngles = new Vector3(0, 0, angle);
        }
        else {
            transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }

    private void RandomizeScale() {
        if(scaleAlongAllAxesTogether) {
            float t = Random.Range(0f, 1f);
            transform.localScale = Vector3.Lerp(minScale, maxScale, t);
        }
        else {
            float x = Random.Range(minScale.x, maxScale.x);
            float y = Random.Range(minScale.y, maxScale.y);
            float z = Random.Range(minScale.z, maxScale.z);
            transform.localScale = new Vector3(x, y, z);
        }
    }
#endif
}
