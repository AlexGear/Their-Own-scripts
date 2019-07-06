using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationObstacleLive : NavigationObstacle {
    [SerializeField] float updateInterval = 0.3f;

#if UNITY_EDITOR
    [NaughtyAttributes.Button("Convert To NavMeshModifierVolume")]
    protected override void ConvertToNavMeshModifierVolume() {
        base.ConvertToNavMeshModifierVolume();
    }
#endif

    void Awake() {
        StartCoroutine(UpdateCoroutine());
	}

    private IEnumerator UpdateCoroutine() {
        while(true) {
            yield return new WaitForSeconds(updateInterval);
            if(transform.hasChanged) {
                transform.hasChanged = false;
                SetPosition(transform.position);
                SetRotation(transform.rotation);
            }
        }
    }
}
