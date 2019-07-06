using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using NaughtyAttributes;

namespace Forest {

    public class Forest : MonoBehaviour {
#if UNITY_EDITOR
        [SerializeField] Tilemap tilemap;
        [SerializeField] bool createOutline = true;
        [SerializeField]
        [Tooltip("Force all plants that have 'hasSwitchableCollider' checked to keep colliders enabled")]
        bool enableAllForestColliders = false;
        [SerializeField, Range(0, 6)] int restrictedAreaRadius = 1;

        #region Outline fields
        [ShowIf(nameof(createOutline))]
        [BoxGroup("Outline Generator")]
        [Range(0, 1)]
        [SerializeField]
        float outlineDetailLevel = 1;

        [ShowIf(nameof(createOutline))]
        [BoxGroup("Outline Generator")]
        [SerializeField]
        bool useOutlineStoppers = true;

        [ShowIf(nameof(createOutline))]
        [BoxGroup("Outline Generator")]
        [MinMaxSlider(1f, 10f)]
        [SerializeField]
        Vector2 outlineBushesInterval = new Vector2(2f, 9f);

        [ShowIf(nameof(createOutline))]
        [BoxGroup("Outline Generator")]
        [MinMaxSlider(-5f, 5f)]
        [SerializeField]
        Vector2 outlineBushesProtrusion = new Vector2(-4.5f, 4.5f);

        [ShowIf(nameof(createOutline))]
        [BoxGroup("Outline Generator")]
        [EnableIf(nameof(EnableEnsureNoGaps))]
        [InfoBox("'Ensure No Gaps' option is unavailable if not each candidate has a Collider2D",
            InfoBoxType.Warning, nameof(NotEachOutlineBushCandidateHasCollider))]
        [InfoBox("'Ensure No Gaps' option is unavailable if 'Intensity' is not 1",
            InfoBoxType.Warning, nameof(OutlineBushesIntensityIsNot1))]
        [SerializeField]
        bool ensureNoGaps = true;

        #region ensureNoGaps validation code

        private bool EnableEnsureNoGaps() {
            return EachOutlineBushCandidateHasCollider() &&
                   OutlineBushesIntensityIs1();
        }

        private bool NotEachOutlineBushCandidateHasCollider() {
            return !EachOutlineBushCandidateHasCollider();
        }

        private bool EachOutlineBushCandidateHasCollider() {
            if(!outlineBushesCandidates.Any()) {
                return false;
            }

            foreach(var candidate in outlineBushesCandidates) {
                if(candidate.FindCollider() == null) {
                    return false;
                }
            }
            return true;
        }

        private bool OutlineBushesIntensityIsNot1() {
            return !OutlineBushesIntensityIs1();
        }

        private bool OutlineBushesIntensityIs1() {
            return Mathf.Approximately(outlineBushesCandidates.GetIntensity(), 1f);
        }

        #endregion
        [ShowIf(nameof(createOutline))]
        [BoxGroup("Outline Generator")]
        [SerializeField]
        CandidateTable outlineBushesCandidates;

        [ShowIf(nameof(createOutline))]
        [BoxGroup("Outline Generator")]
        [SerializeField]
        string outlineBushesTag;
        #endregion

        [SerializeField, HideInInspector]
        Vector2[] rawOutlinePathPoints = new Vector2[0];

        [SerializeField]
        Path outlinePath = new Path();

        // Outline stoppers determining the outline limits
        [SerializeField, HideInInspector]
        Vector2 startStopper;

        [SerializeField, HideInInspector]
        Vector2 endStopper;

        [SerializeField]
        List<GameObject> forestSlotObjects = new List<GameObject>();

        [SerializeField]
        List<GameObject> outlineSlotObjects = new List<GameObject>();

        [SerializeField]
        SlotData[] slotDatas = new SlotData[0];

        public const float stopperRadius = 2f;

        public SlotData[] GetSlotDatas() => slotDatas;

        public IEnumerable<GameObject> GetForestSlotObjects() => forestSlotObjects;

        public IEnumerable<GameObject> GetOutlineSlotObjects() => outlineSlotObjects;

        [Button]
        public void GenerateOutlinePath() {
            if(!createOutline) {
                Debug.Log("createOutline=false. Skipping outline path generation", this);
                return;
            }
            Vector2[][] paths = TilemapOutlineGenerator.Generate(tilemap, outlineDetailLevel);
            if(paths.Length == 0) {
                Debug.LogWarning("It seems like the tilemap is blank. No paths were generated", this);
                return;
            }
            if(paths.Length == 1) {
                rawOutlinePathPoints = paths[0];
                CreateOutlinePath();
                return;
            }
            Debug.Log($"Found more than 1 path: {paths.Length}. Longest path will be used", this);
            rawOutlinePathPoints = GetLongestPath(paths);
            CreateOutlinePath();
        }

        public void CreateOutlinePath() {
            var points = rawOutlinePathPoints;
            if(!useOutlineStoppers) {
                outlinePath = new Path(points);
                return;
            }

            int start = -1;
            for(int i = 0; i < points.Length; i++) {
                if((points[i] - startStopper).CompareLength(stopperRadius) < 0) {
                    start = i;
                    // No break because there might be more points overlapping startStopper,
                    // and we want to find the last of them
                }
            }
            if(start == -1) {
                outlinePath = new Path(points);
                return;
            }

            int end = -1;
            for(int i = start + 1; i != start; i++) {
                if(i == points.Length) i = 0;
                if((points[i] - endStopper).CompareLength(stopperRadius) < 0) {
                    end = i;
                    break;
                    // Break is used because among all the points overlapping the endStopper
                    // we want to choose the first encountered
                }
            }
            if(end == -1) {
                outlinePath = new Path(points);
                return;
            }

            Vector2[] limitedPoints;
            if(start <= end) {
                limitedPoints = new Vector2[end - start + 1];
                System.Array.Copy(points, start, limitedPoints, 0, limitedPoints.Length);
            }
            else {
                int lengthToArrayEnd = points.Length - start;
                int lengthFromArrayStart = end + 1;
                limitedPoints = new Vector2[lengthToArrayEnd + lengthFromArrayStart];
                System.Array.Copy(points, start, limitedPoints, 0, lengthToArrayEnd);
                System.Array.Copy(points, 0, limitedPoints, lengthToArrayEnd, lengthFromArrayStart);
            }

            outlinePath = new Path(limitedPoints);
        }

        private static Vector2[] GetLongestPath(Vector2[][] paths) {
            if(paths == null) return null;

            Vector2[] longest = null;
            float maxLength = 0;
            foreach(Vector2[] path in paths) {
                float length = 0;
                for(int i = 0; i < path.Length - 1; i++)
                    length += Vector2.Distance(path[i], path[i + 1]);

                if(longest == null || length > maxLength) {
                    maxLength = length;
                    longest = path;
                }
            }
            return longest;
        }

        private void Reset() {
            tilemap = GetComponentInChildren<Tilemap>();
            startStopper = transform.position + new Vector3(8, 6);
            endStopper = transform.position + new Vector3(-8, 6);
        }

        [Button("Create Outline Collider")]
        public void CreateOutlineCollider() {
            if(!createOutline) {
                Debug.Log("createOutline=false. Skipping outline collider creation", this);
                return;
            }
            if(rawOutlinePathPoints == null || rawOutlinePathPoints.Length < 3) {
                string reason = rawOutlinePathPoints == null ? "null" : $"Length is less than 3: {rawOutlinePathPoints.Length}";
                Debug.LogError($"The raw outline path points array is invalid ({reason}). Cannot create an outline collider", this);
                return;
            }

            var collider = GetComponent<PolygonCollider2D>();
            if(collider == null) {
                collider = Undo.AddComponent<PolygonCollider2D>(this.gameObject);
                collider.isTrigger = true;
            }
            collider.points = rawOutlinePathPoints.Select(p => (Vector2)transform.InverseTransformPoint(p)).ToArray();

            var rigidbody = GetComponent<Rigidbody2D>();
            if(rigidbody == null) {
                rigidbody = Undo.AddComponent<Rigidbody2D>(this.gameObject);
                rigidbody.bodyType = RigidbodyType2D.Static;
            }
        }

        [Button]
        public void GenerateSlotObjects() {
            GenerateForestSlotObjects(true);
            if(createOutline) {
                GenerateOutlineSlotObjects(true);
            }
        }

        [Button("Objects --> Datas")]
        public void GenerateSlotDatasFromSlotObjects() {
            var forestSlotDatas = forestSlotObjects.Select(
                go => SlotObjectToSlotData(go, enableAllForestColliders, tag: null)
            );

            var outlineSlotDatas = outlineSlotObjects.Select(
                go => SlotObjectToSlotData(go, true, outlineBushesTag)
            );

            slotDatas = forestSlotDatas.Union(outlineSlotDatas).Where(x => x != null).ToArray();


            SlotData SlotObjectToSlotData(GameObject slotObject, bool isSwitchableColliderEnabled, string tag) {
                var plantSlot = slotObject.GetComponent<PlantSlot>();
                if(plantSlot == null) {
                    Debug.LogError($"Slot object '{slotObject.name}' has no PlantSlot attached. Skipping", this);
                    return null;
                }
                var plantItselfTransform = slotObject.transform.GetChild(slotObject.transform.childCount - 1);
                return new SlotData {
                    position = plantItselfTransform.position,
                    rotation = plantItselfTransform.rotation,
                    scale = plantItselfTransform.lossyScale,
                    prefab = plantSlot.plantPrefabReference,
                    slotObjectPrefab = PrefabUtility.GetCorrespondingObjectFromSource(slotObject),
                    hasSwitchableCollider = plantSlot.hasSwitchableCollider,
                    isSwitchableColliderEnabled = isSwitchableColliderEnabled
                };
            }
        }

        [Button]
        public void RemoveSlotObjects() {
            RemoveForestSlotObjects();
            RemoveOutlineSlotObjects();
        }

        [Button("Datas --> Objects")]
        public void RestoreSlotObjectsFromSlotDatas() {
            RestoreSlotObjectsFromSlotDatas(true);
        }

        public void RestoreSlotObjectsFromSlotDatas(bool preservingPrefabConnections) {
            if(slotDatas.Length == 0) {
                Debug.LogError("The slot datas array is empty. Skipping operation", this);
                return;
            }
            RemoveForestSlotObjects();
            RemoveOutlineSlotObjects();
            foreach(SlotData slotData in slotDatas) {
                GameObject slotObject = InstantiateSlotObject(slotData.slotObjectPrefab, slotData.position, preservingPrefabConnections);
                var plantItselfTransform = slotObject.transform.GetChild(slotObject.transform.childCount - 1);
                slotObject.transform.rotation = slotData.rotation;
                slotObject.transform.localScale = DivideVectors(slotData.scale, plantItselfTransform.localScale);
                if(slotData.isSwitchableColliderEnabled) {
                    outlineSlotObjects.Add(slotObject);
                }
                else {
                    forestSlotObjects.Add(slotObject);
                }
            }

            Vector3 DivideVectors(Vector3 a, Vector3 b) {
                return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
            }
        }

        private void GenerateForestSlotObjects(bool preservingPrefabConnections) {
            RemoveForestSlotObjects();

            int skipped = 0;
            var engagedPositions = new HashSet<Vector3Int>();
            foreach(Vector3Int pos in tilemap.cellBounds.allPositionsWithin) {
                ForestTile tile = tilemap.GetTile(pos) as ForestTile;
                if(tile == null) continue;
                
                bool flag = false;
                for(int x = pos.x - restrictedAreaRadius; x <= pos.x + restrictedAreaRadius; x++) {
                    for(int y = pos.y - restrictedAreaRadius; y <= pos.y + restrictedAreaRadius; y++) {
                        if(engagedPositions.Contains(new Vector3Int(x, y, 0))) {
                            flag = true;
                            break;
                        }
                    }
                }
                if(flag) {
                    skipped++;
                    continue;
                }

                float intensity = tile.candidates.GetIntensity();
                float overridenIntensity = 1 - Mathf.Pow(1 - intensity, skipped * intensity);
                GameObject chosen = tile.candidates.GetRandomPrefab(overridenIntensity);
                if(chosen != null) {
                    Vector3 position = tilemap.CellToWorld(pos);
                    GameObject slotObject = InstantiateSlotObject(chosen, position, preservingPrefabConnections);
                    forestSlotObjects.Add(slotObject);

                    skipped = 0;
                    engagedPositions.Add(pos);
                }
                else skipped++;
            }
        }

        private GameObject InstantiateSlotObject(GameObject prefab, Vector3 position, bool preservingPrefabConnection) {
            Transform parent = GetComponentInChildren<Tilemap>().transform;
            GameObject slotObject = preservingPrefabConnection ? 
                (GameObject)PrefabUtility.InstantiatePrefab(prefab) : Instantiate(prefab);
            slotObject.transform.position = position;
            slotObject.transform.parent = parent;

            PlantSlot plantSlot = slotObject.GetComponent<PlantSlot>();
            if(plantSlot != null) {
                plantSlot.Randomize();
            }

            return slotObject;
        }
        
        private void RemoveForestSlotObjects() {
            foreach(GameObject slotObject in forestSlotObjects) {
                DestroyImmediate(slotObject);
            }
            forestSlotObjects.Clear();
        }
        
        private void GenerateOutlineSlotObjects(bool preservingPrefabConnections) {
            RemoveOutlineSlotObjects();

            bool noGaps = EnableEnsureNoGaps() && ensureNoGaps;
            var prevColliders = new List<CircleCollider2D>();
            float length = outlinePath.GetTotalLength();
            float d = 0;
            while(d < length) {
                GameObject prefab = outlineBushesCandidates.GetRandomPrefab();
                GameObject slotObject = null;
                float interval;

                int attempts = 0;
                const int maxAttempts = 300;
                while(true) {
                    if(slotObject != null) {
                        // Slot object from previous attempt
                        DestroyImmediate(slotObject);
                    }
                    if(prevColliders.Count == 0)
                        interval = 0;
                    else if(attempts >= maxAttempts * 0.8f)
                        interval = outlineBushesInterval.x;
                    else
                        interval = Random.Range(outlineBushesInterval.x, outlineBushesInterval.y);

                    float protrusion = Random.Range(outlineBushesProtrusion.x, outlineBushesProtrusion.y);
                    Vector2 position = GetRandomOffsettedPointAlongOutlinePath(d + interval, protrusion);

                    slotObject = InstantiateSlotObject(prefab, position, preservingPrefabConnections);
                    if(!noGaps) {
                        outlineSlotObjects.Add(slotObject);
                        break;
                    }
                    var collider = slotObject.GetComponentInChildren<CircleCollider2D>();
                    if(prevColliders.Count == 0 || prevColliders.Any(pc => pc.Distance(collider).isOverlapped)) {
                        prevColliders.Add(collider);
                        outlineSlotObjects.Add(slotObject);
                        break;
                    }
                    attempts++;
                    if(attempts >= maxAttempts) {
                        DestroyImmediate(slotObject);
                        Debug.LogError($"Failed to generate outline bushes without gaps in {maxAttempts} attempts", this);
                        return;
                    }
                }
                d += interval;
            }

            Vector2 GetRandomOffsettedPointAlongOutlinePath(float distance, float offset) {
                Vector2 position = outlinePath.GetPointAlongPathClamped(distance);
                Vector2 direction = outlinePath.GetDirectionAtPointAlongPathClamped(distance);
                position += Vector2.Perpendicular(direction) * -offset;
                return position;
            }
        }
        
        private void RemoveOutlineSlotObjects() {
            foreach(GameObject slotObject in outlineSlotObjects) {
                DestroyImmediate(slotObject);
            }
            outlineSlotObjects.Clear();
        }

        private void OnDrawGizmosSelected() {
            if(createOutline && outlinePath != null && outlinePath.isValid) {
                Handles.color = Color.red;
                CompareFunction oldZtest = Handles.zTest;
                Handles.zTest = CompareFunction.Always;

                Vector2 prev = outlinePath[0];
                for(int i = 1; i < outlinePath.waypoints.Count; i++) {
                    if(SceneViewHelper.IsPointVisible(outlinePath[i])) {
                        Handles.DrawLine(prev, outlinePath[i]);
                        Handles.DrawSolidDisc(outlinePath[i], Vector3.forward, 0.2f);
                    }
                    prev = outlinePath[i];
                }
                Handles.DrawSolidDisc(outlinePath[0], Vector3.forward, 0.2f);

                Handles.zTest = oldZtest;
            }

            if(forestSlotObjects.Count == 0 || outlineSlotObjects.Count == 0) {
                foreach(var slotData in slotDatas) {
                    Vector2 position = slotData.position;
                    float radius = slotData.scale.x;

                    Vector2 rectSize = Vector2.one * radius * 2;
                    if(!SceneViewHelper.IsRectVisible(new Rect(position - rectSize * 0.5f, rectSize)))
                        continue;

                    if(!slotData.isSwitchableColliderEnabled && forestSlotObjects.Count == 0) {
                        Handles.color = new Color32(34, 139, 34, 40);
                        Handles.DrawSolidDisc(position, Vector3.forward, radius);
                    }
                    else if(slotData.isSwitchableColliderEnabled && outlineSlotObjects.Count == 0) {
                        Handles.color = new Color32(142, 64, 42, 40);
                        Handles.DrawSolidDisc(position, Vector3.forward, radius);
                    }
                }
            }
        }
#endif
    }

}