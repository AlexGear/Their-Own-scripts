using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Follower : MonoBehaviour {
    [SerializeField] private GameObject leader;
    [SerializeField] private bool needRotate;

	void Start() {
        LateUpdate();
    }

    void LateUpdate() {
        if (leader == null)
            return;
        transform.position = leader.transform.position;
        if (needRotate)
            transform.rotation = leader.transform.rotation;
    }
}
