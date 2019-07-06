using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NavigationMain : MonoBehaviour {
    [SerializeField] Transform standardFloor;
    [SerializeField] Transform shieldHoldersFloor;

#if UNITY_EDITOR
    private static NavigationMain _current = null;
    public static NavigationMain current => _current != null ? _current : (_current = FindObjectOfType<NavigationMain>());
#else
    public static NavigationMain current { get; private set; }
#endif
    public float standardAgentsZ => standardFloor.position.z;
    public float shieldHoldersZ => shieldHoldersFloor.position.z;

#if !UNITY_EDITOR
    void Awake() {
        current = this;
    }
#endif

    void Start() {
        var map = MapScript.current;
#if UNITY_EDITOR
        if(!Application.isPlaying && _map == null) {
            _map = FindObjectOfType<MapScript>();
        }
#endif
        var scale = new Vector3(map.mapSize.x, map.mapSize.y, 1);
        standardFloor.localScale = scale;
        shieldHoldersFloor.localScale = scale;
	}

#if UNITY_EDITOR
    private MapScript _map = null;
    private Vector2 prevMapSize = Vector2.negativeInfinity;
    private Vector2 prevMapCenter = Vector2.negativeInfinity;

    void Update() {
        if(Application.isPlaying) {
            return;
        }
        if(_map == null) {
            _map = FindObjectOfType<MapScript>();
        }
        if(prevMapSize != _map.mapSize || prevMapCenter != _map.mapCenter) {
            prevMapSize = _map.mapSize;
            prevMapCenter = _map.mapCenter;
            
            standardFloor.position = new Vector3(_map.mapCenter.x, _map.mapCenter.y, standardAgentsZ);
            standardFloor.localScale = _map.mapSize;

            shieldHoldersFloor.position = new Vector3(_map.mapCenter.x, _map.mapCenter.y, shieldHoldersZ);
            shieldHoldersFloor.localScale = _map.mapSize;
        }
    }
#endif
}
