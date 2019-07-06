using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float followTime = 0.08f;

    [Range(0, 1)] public float cursorWeight = 1f;
    public Transform additionalTarget = null;
    [Range(0, 1)] public float additionalTargetWeight = 0f;

    private Transform mainCharTransform;
    private MapScript map;
    private Camera mainCamera;
    private Transform[] perspecitveCameras;
    private IEnumerator cameraZoomRoutine = null;
    private float originalSize;
    private float closePerspectiveZ;
    private Vector2 smoothDampVelocity;

    public static CameraFollow current { get; private set; }

    private void Awake() {
        current = this;
        mainCamera = GetComponent<Camera>();
        perspecitveCameras = FindPerspectiveCamerasInChildren();
        originalSize = mainCamera.orthographicSize;
        closePerspectiveZ = -transform.localPosition.z;
    }

    void Start() {
        map = MapScript.current;
        mainCharTransform = MainCharacter.current.transform;

        ApplyPos(ClampPos(mainCharTransform.position));
    }

    private Transform[] FindPerspectiveCamerasInChildren() {
        return GetComponentsInChildren<Camera>().Where(c => !c.orthographic)
            .Select(c => c.transform).ToArray();
    }

    public void ResetZoom() {
        ZoomCamera(originalSize);
    }

    public void ZoomCamera(float desiredSize) {
        if(cameraZoomRoutine != null) {
            StopCoroutine(cameraZoomRoutine);
        }
        cameraZoomRoutine = ZoomCoroutine(desiredSize);
        StartCoroutine(cameraZoomRoutine);
    }

    public bool IsPointSeen(Vector2 point, Vector2 margin = default(Vector2)) {
        Vector2 cameraHalfSize = GetCameraHalfSize();
        Vector2 position = transform.position;

        Vector2 bottomLeft = position - cameraHalfSize - margin;
        Vector2 topRight = position + cameraHalfSize + margin;

        Rect availableFieldOfView = new Rect(bottomLeft, topRight - bottomLeft);
        return availableFieldOfView.Contains(point);
    }

    public Vector2 GetMaxRectSize() => GetCameraHalfSize() * 4;

    private IEnumerator ZoomCoroutine(float desiredSize) {
        while(true) {
            float currentSize = mainCamera.orthographicSize;
            if(Mathf.Approximately(desiredSize, currentSize)) {
                yield break;
            }
            mainCamera.orthographicSize = Mathf.Lerp(currentSize, desiredSize, Time.unscaledDeltaTime * zoomSpeed);
            UpdatePerspectiveCameras();

            yield return null;
        }
    }

    private void UpdatePerspectiveCameras() {
        float factor = 1f - mainCamera.orthographicSize / originalSize;
        var perspectivePos = new Vector3(0, 0, closePerspectiveZ * factor);
        foreach(var cam in perspecitveCameras) {
            cam.localPosition = perspectivePos;
        }
    }

    private Vector2 GetCameraTargetPos() {
        if(additionalTarget == null) {
            return mainCharTransform.position;
        }
        Vector3 toAddTarget = additionalTarget.position - mainCharTransform.position;
        return mainCharTransform.position + additionalTargetWeight * toAddTarget;
    }

    private Vector2 GetCursorShiftedPos(Vector2 original, Vector2 cursorWorldPos) {
        return original + (cursorWorldPos - original) * cursorWeight * 0.47f;
    }

    private Vector2 GetCameraHalfSize() {
        return new Vector2(mainCamera.orthographicSize * mainCamera.aspect, mainCamera.orthographicSize);
    }

    private Vector2 ClampPos(Vector2 original) {
        Vector2 cameraHalfSize = GetCameraHalfSize();
        Vector2 mapMin = map.mapOrigin + cameraHalfSize;
        Vector2 mapMax = map.mapOrigin + map.mapSize - cameraHalfSize;
        return original.Clamp(mapMin, mapMax);
    }

    private void ApplyPos(Vector2 pos) {
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

	void LateUpdate() {
        Vector2 target = GetCameraTargetPos();
        Vector2 cursorShifted = GetCursorShiftedPos(target, MainCharacter.current.mousePosition);
        Vector2 final = ClampPos(cursorShifted);
        
        Vector2 result = Vector2.SmoothDamp(transform.position, final, ref smoothDampVelocity, followTime, 
                                            float.PositiveInfinity, Time.unscaledDeltaTime);
        ApplyPos(result);
    }
}
