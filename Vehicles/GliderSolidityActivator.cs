using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GliderSolidityActivator : MonoBehaviour {
    [SerializeField] GameObject solidityObject;
    [SerializeField] float activateAtHeight = 3f;

    private void Update() {
        float height = -transform.position.z;
        bool active = height < activateAtHeight;
        if(solidityObject != null) {
            solidityObject.SetActive(active);
        }
    }
}
