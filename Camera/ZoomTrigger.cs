using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomTrigger : MonoBehaviour {
    [SerializeField] float zoomCameraSize = 6f;

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            CameraFollow.current.ZoomCamera(zoomCameraSize);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            CameraFollow.current.ResetZoom();
        }
    }

    void OnDisable() {
        if(CameraFollow.current != null) {
            CameraFollow.current.ResetZoom();
        }
    }
}
