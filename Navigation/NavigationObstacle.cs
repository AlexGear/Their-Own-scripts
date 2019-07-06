using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using static UnityEngine.Mathf;


public class NavigationObstacle : MonoBehaviour {
    //private enum ObstacleShape { Box, Circle }
    //[Header("Use BoxCollider for Box & CircleCollider for Circle")]
    //[SerializeField] private ObstacleShape shape;
    //[SerializeField] private bool synchronizeEnabledState = false;

#if UNITY_EDITOR
    [Header("Conversion to NavMeshModifiers")]
    [SerializeField] bool skipIfNotStatic = true;
    [SerializeField] bool tagEditorOnly = true;
    [SerializeField] bool removeThisScript = true;
    [SerializeField] bool removeDisabledColliders = true;
    [SerializeField] bool replaceThisGameObjectIfCollidersDisabledAndNoOtherComponents = true;

    [NaughtyAttributes.Button("Convert To NavMeshModifiers")]
    protected virtual void ConvertToNavMeshModifierVolume() {
        var createdObjs = new List<GameObject>();
        try {
            if(!gameObject.isStatic && skipIfNotStatic) {
                Debug.Log("Skipped " + name, this);
                return;
            }

            var colliders = GetComponents<Collider2D>();

            var components = gameObject.GetComponents<Component>();
            bool noOtherComponents = components.Length == colliders.Length + components.OfType<Transform>().Count() + components.OfType<NavigationObstacle>().Count();
            bool collidersDisabled = colliders.All(c => !c.enabled);
            bool replaceGO = replaceThisGameObjectIfCollidersDisabledAndNoOtherComponents && noOtherComponents && collidersDisabled;

            var parent = replaceGO ? transform.parent : transform;

            for(int i = 0; i < colliders.Length; i++) {
                string name = GenerateObstacleName(colliders, i);
                if(colliders[i] is BoxCollider2D box) {
                    CreateBoxObstacle(parent, name, Vector2.zero, Quaternion.identity, box.size, box.offset);
                }
                else if(colliders[i] is CircleCollider2D circle) {
                    CreateCircleObstacle(parent, name, circle);
                }
                else if(colliders[i] is PolygonCollider2D polygon) {
                    CreatePolygonObstacle(parent, name, polygon);
                }
            }

            if(replaceGO)
                UnityEditor.Undo.DestroyObjectImmediate(this.gameObject);
            else {
                if(removeDisabledColliders) {
                    foreach(var collider in colliders) {
                        if(!collider.enabled) {
                            UnityEditor.Undo.DestroyObjectImmediate(collider);
                        }
                    }
                }
                if(removeThisScript) {
                    UnityEditor.Undo.DestroyObjectImmediate(this);
                }
            }
        }
        catch(System.InvalidOperationException e) 
        when(e.Message.Contains("Destroying a GameObject inside a Prefab instance is not allowed")) {
            Debug.LogError($"Couldn't convert {name}:\n{e.Message}", this);
            foreach(var createdObj in createdObjs) {
                DestroyImmediate(createdObj);
            }
        }

        GameObject CreateEmptyGO(string name, Transform _parent) {
            var go = new GameObject(name);
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Convert To NavMeshModifierVolume");
            go.isStatic = this.gameObject.isStatic;
            if(tagEditorOnly) {
                go.tag = "EditorOnly";
            }
            go.transform.parent = _parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            createdObjs.Add(go);
            return go;
        }

        NavMeshModifierVolume CreateBoxObstacle(Transform parent, string name, Vector2 localPosition, 
            Quaternion localRotation, Vector2 size, Vector2 offset = default) 
        {
            var obstacleObj = CreateEmptyGO(name, parent);
            obstacleObj.transform.localPosition = localPosition;
            obstacleObj.transform.localRotation = localRotation;
            var modifier = obstacleObj.AddComponent<NavMeshModifierVolume>();
            modifier.center = offset;
            modifier.size = new Vector3(size.x, size.y, 500);
            modifier.area = NavMesh.GetAreaFromName("Not Walkable");
            return modifier;
        }

        void CreateCircleObstacle(Transform parent, string name, CircleCollider2D circle) {
            var obstacleObjRoot = CreateEmptyGO(name, parent).transform;
            obstacleObjRoot.localPosition = circle.offset;
            float radius = circle.radius;
            float k = 1 + Sqrt(2);
            float ySize = radius * (1 / k + 0.5f / Sqrt(k / (k - 1)));
            float xSize = ySize * k;
            for(int j = 0; j < 4; j++) {
                var rotation = Quaternion.AngleAxis(j * 45, Vector3.forward);
                CreateBoxObstacle(obstacleObjRoot, name + $" Part {j + 1}", Vector2.zero, rotation, new Vector2(xSize, ySize));
            }
        }

        void CreatePolygonObstacle(Transform parent, string name, PolygonCollider2D polygon) {
            var obstacleObj = CreateEmptyGO(name, parent);
            obstacleObj.transform.localPosition = polygon.offset;

            Vector2[] polygonPoints = polygon.points;
            int[] polygonIndices = new Triangulator(polygonPoints).Triangulate();

            var vertices = new List<Vector3>(polygonPoints.Length * 2);
            var indices = new List<int>();
            // Bottom face
            for(int i = 0; i < polygonPoints.Length; i++) {
                Vector3 point = polygonPoints[i];
                point.z = obstacleObj.transform.InverseTransformPoint(Vector3.zero).z;
                vertices.Add(point);
            }
            indices.AddRange(polygonIndices);
            // Top face
            float shieldHoldersZ = NavigationMain.current.shieldHoldersZ;
            for(int i = 0; i < polygonPoints.Length; i++) {
                Vector3 point = polygonPoints[i];
                point.z = obstacleObj.transform.InverseTransformPoint(new Vector3(0, 0, shieldHoldersZ)).z;
                vertices.Add(point);
            }
            indices.AddRange(polygonIndices.Select(TopIndex));
            // Side faces
            int prevI = polygonPoints.Length - 1;
            for(int i = 0; i < polygonPoints.Length; i++) {
                int[] triangle1 = { BottomIndex(prevI), BottomIndex(i), TopIndex(prevI) };
                int[] triangle2 = { BottomIndex(i), TopIndex(i), TopIndex(prevI) };
                indices.AddRange(triangle1);
                indices.AddRange(triangle2);
                prevI = i;
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
            var collider = obstacleObj.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            var modifier = obstacleObj.AddComponent<NavMeshModifier>();
            modifier.overrideArea = true;
            modifier.area = NavMesh.GetAreaFromName("Not Walkable");

            int BottomIndex(int i) => i;
            int TopIndex(int i) => i + polygonPoints.Length;
        }

    }
#endif

    private static Transform _obstaclesRoot;
    private static Transform obstaclesRoot {
        get {
            if(_obstaclesRoot != null)
                return _obstaclesRoot;
            var obstaclesRootGO = new GameObject("Nav Obstacles");
            obstaclesRootGO.transform.parent = NavigationMain.current.transform;
            return (_obstaclesRoot = obstaclesRootGO.transform);
        }
    }

    private string obstacleName => 
        name + (name.Contains("Nav") || name.Contains("Obstacle") ? "" : " Nav Obstacle");

    private Transform obstacleTransform;
    private float obstacleHeight = 150;

    public void SetPosition(Vector3 position) {
        transform.position = position;
        obstacleTransform.position = new Vector3(position.x, position.y, 0);
    }

    public void SetRotation(Quaternion rotation) {
        transform.rotation = rotation;
        obstacleTransform.rotation = rotation;
    }
    
    private void OnEnable() {
        SetObstaclesActive(true);
    }

    private void OnDisable() {
        SetObstaclesActive(false);
    }

    private void SetObstaclesActive(bool active) {
        if(obstacleTransform == null) return;

        obstacleTransform.gameObject.SetActive(active);
    }

    void Start() {
        Collider2D[] colliders = GetComponents<Collider2D>();

        obstacleTransform = CreateObstaclesTransform();

        for(int i = 0; i < colliders.Length; i++) {
            string name = GenerateObstacleName(colliders, i);
            CreateObstacleObject(colliders[i], name, obstacleTransform);
        }
    }

    private Transform CreateObstaclesTransform() {
        var parentGO = new GameObject(obstacleName);
        parentGO.transform.parent = obstaclesRoot;
        parentGO.transform.SetPositionAndRotation(transform.position, transform.rotation);
        return parentGO.transform;
    }

    private string GenerateObstacleName(Collider2D[] colliders, int i) {
        string name = obstacleName;
        if(colliders.Length > 1)
            name += $" [{i + 1}]";
        switch(colliders[i]) {
            case BoxCollider2D _: name += " Box"; break;
            case CircleCollider2D _: name += " Round"; break;
            case PolygonCollider2D _: name += " Polygon"; break;      
        }
        return name;
    }

    private void CreateObstacleObject(Collider2D collider, string name, Transform parent) {
        if(!(collider is BoxCollider2D || collider is CircleCollider2D)) {
            Debug.LogWarning("Cannot generate Nav Obstacle for collider of type " + collider.GetType().Name);
            return;
        }

        var obstacleObject = new GameObject(name);
        obstacleObject.transform.parent = parent;

        var nmObstacle = obstacleObject.AddComponent<NavMeshObstacle>();
        nmObstacle.carving = true;
        var scale = transform.lossyScale;

        if(collider is BoxCollider2D) {
            var boxCollider = (BoxCollider2D)collider;
            nmObstacle.shape = NavMeshObstacleShape.Box;
            nmObstacle.size = new Vector3(boxCollider.size.x * scale.x, obstacleHeight, boxCollider.size.y * scale.y);
            nmObstacle.center = new Vector3(boxCollider.offset.x * scale.x, 0, boxCollider.offset.y * scale.y);
        }
        else {
            var circleCollider = (CircleCollider2D)collider;
            nmObstacle.shape = NavMeshObstacleShape.Capsule;
            nmObstacle.radius = circleCollider.radius * Mathf.Max(scale.x, scale.y);
            nmObstacle.center = new Vector3(circleCollider.offset.x * scale.x, 0, circleCollider.offset.y * scale.y);
            nmObstacle.height = obstacleHeight;
        }
        obstacleObject.transform.SetPositionAndRotation(transform.position, GetRotationForYUpOrientedObstacle());
    }

    private Quaternion GetRotationForYUpOrientedObstacle() {
        return Quaternion.AngleAxis(-90, transform.right) * transform.rotation;
    }

    void OnDestroy() {
        if(obstacleTransform != null) {
            Destroy(obstacleTransform.gameObject);
        }
    }
}
