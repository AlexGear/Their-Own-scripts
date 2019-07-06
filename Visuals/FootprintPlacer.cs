using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintPlacer : MonoBehaviour {
    [SerializeField] string footprintsName;
    [SerializeField] Transform leftFoot;
    [SerializeField] Transform rightFoot;
    [SerializeField] float placeCooldown = 0.2f;
    private ParticleSystem leftPS, rightPS;
    private float lastTime;

    private void Start() {
        leftPS  = FootprintsMain.current.GetParticleSystem(footprintsName, Foot.Left);
        if(leftPS == null) {
            Debug.LogError($"Couldn't find Left footprints particle system '{footprintsName}'");
        }

        rightPS = FootprintsMain.current.GetParticleSystem(footprintsName, Foot.Right);
        if(rightPS == null) {
            Debug.LogError($"Couldn't find Right footprints particle system '{footprintsName}'");
        }
    }

    public void PlaceLeftFootprint() {
        PlaceFootprint(leftPS, leftFoot);
    }

    public void PlaceRightFootprint() {
        PlaceFootprint(rightPS, rightFoot);
    }

    private void PlaceFootprint(ParticleSystem ps, Transform footTransform) {
        if(ps == null || IsOnCooldown()) {
            return;
        }
        lastTime = Time.time;

        ps.transform.position = (Vector2)footTransform.position; // force zeroing the Z coordinate
        var main = ps.main;
        main.startRotation = footTransform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        ps.Emit(1);
    }

    private bool IsOnCooldown() {
        return Time.time - lastTime < placeCooldown;
    }
}
