using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using NaughtyAttributes;

namespace Forest {

    /// <summary>
    /// Ent the Forest Keeper
    /// </summary>
    public class ForestChunks : MonoBehaviour {
        [SerializeField] float chunkSize = 60f;
        [SerializeField, ReadOnly] Chunk[] chunks = new Chunk[0];
        [SerializeField, ReadOnly] Rect chunksRect;
        [SerializeField, ReadOnly] int chunksArrayWidth;
        [SerializeField, ReadOnly] int chunksArrayHeight;

        public static ForestChunks current { get; private set; }

        private void Awake() {
            current = this;
        }

        /// <summary>
        /// Snaps the point <paramref name="original"/> to the bottom left corner of the cell it is within
        /// </summary>
        /// <param name="original">Point to be snapped</param>
        /// <returns></returns>
        public Vector2 SnapToCellOrigin(Vector2 original) {
            return (Vector2)Vector2Int.FloorToInt(original / chunkSize) * chunkSize;
        }

        /// <summary>
        /// Returns the chunk at row <paramref name="i"/> and column <paramref name="j"/> starting at the bottom left
        /// </summary>
        /// <param name="i">Row index starting at the bottom</param>
        /// <param name="j">Column index starting at the left</param>
        public Chunk this[int i, int j] {
            get {
                if(i < 0 || i >= chunksArrayHeight)
                    throw new System.ArgumentOutOfRangeException(nameof(i));
                if(j < 0 || j >= chunksArrayWidth)
                    throw new System.ArgumentOutOfRangeException(nameof(j));

                return chunks[j + i * chunksArrayWidth];
            }
        }

        /// <summary>
        /// Returns the chunk at row index.y and column index.x starting at the bottom left
        /// </summary>
        public Chunk this[Vector2Int index] => this[index.y, index.x];

        /// <summary>
        /// Returns the chunk within which the <paramref name="point"/> is.
        /// If <paramref name="point"/> is outside the chunks scope, <see cref="null"/> is returned.
        /// </summary>
        public Chunk GetChunk(Vector2 point) {
            if(!chunksRect.Contains(point)) {
                return null;
            }
            return this[PointToIndexUnclamped(point)];
        }

        private Vector2Int PointToIndexUnclamped(in Vector2 point) {
            Vector2 relativePoint = point - chunksRect.min;
            return Vector2Int.FloorToInt(relativePoint / chunkSize);
        }

        public IEnumerable<Chunk> GetChunksTouchingRect(Rect rect) {
            Vector2Int minIndex = ClampMinIndex(PointToIndexUnclamped(rect.min));
            Vector2Int maxIndex = ClampMaxIndex(PointToIndexUnclamped(rect.max));
            for(int i = minIndex.y; i <= maxIndex.y; i++) {
                for(int j = minIndex.x; j <= maxIndex.x; j++) {
                    yield return this[i, j];
                }
            }
        }

        private Vector2Int ClampMinIndex(Vector2Int index) {
            if(index.x < 0) index.x = 0;
            if(index.y < 0) index.y = 0;
            return index;
        }

        private Vector2Int ClampMaxIndex(Vector2Int index) {
            if(index.x >= chunksArrayWidth) index.x = chunksArrayWidth - 1;
            if(index.y >= chunksArrayHeight) index.y = chunksArrayHeight - 1;
            return index;
        }

#if UNITY_EDITOR
        [Button]
        private void FillChunks() {
            Forest[] forests = FindObjectsOfType<Forest>().Where(f => f.isActiveAndEnabled).ToArray();
            if(forests.Length == 0) {
                Debug.LogError("No enabled Forests found in the scene", this);
                return;
            }

            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;
            var allSlotDatas = new List<SlotData>();
            foreach(var forest in forests) {
                foreach(var slotData in forest.GetSlotDatas()) {
                    // Copying to ensure that this class stores its own slot datas,
                    // not only the references to the Forests' slot datas
                    SlotData copy = new SlotData(slotData);
                    allSlotDatas.Add(copy);

                    var pos = copy.position;
                    if(pos.x < min.x) min.x = pos.x;
                    if(pos.y < min.y) min.y = pos.y;
                    if(pos.x > max.x) max.x = pos.x;
                    if(pos.y > max.y) max.y = pos.y;
                }
            }

            var minSnapped = SnapToCellOrigin(min);
            var maxSnapped = SnapToCellOrigin(max);
            this.chunksArrayWidth = Mathf.RoundToInt((maxSnapped.x - minSnapped.x) / chunkSize) + 1;
            this.chunksArrayHeight = Mathf.RoundToInt((maxSnapped.y - minSnapped.y) / chunkSize) + 1;
            this.chunksRect = new Rect(minSnapped, new Vector2(chunksArrayWidth, chunksArrayHeight) * chunkSize);
            this.chunks = new Chunk[chunksArrayWidth * chunksArrayHeight];
            for(int i = 0; i < this.chunks.Length; i++) {
                this.chunks[i] = new Chunk() {
                    slotDatas = new SlotData[0]
                };
            }
            foreach(var slotData in allSlotDatas) {
                Chunk chunk = GetChunk(slotData.position);
                chunk.slotDatas = chunk.slotDatas.Append(slotData).ToArray();
            }
        }

#endif

#if UNITY_EDITOR
        private static GUIStyle _labelStyle;
        private static GUIStyle labelStyle {
            get {
                if(_labelStyle != null) {
                    return _labelStyle;
                }
                _labelStyle = new GUIStyle();
                _labelStyle.normal.textColor = Color.white;
                _labelStyle.contentOffset = new Vector2(6, -19);
                return _labelStyle;
            }
        }

        private void OnDrawGizmosSelected() {
            for(int i = 0; i <= chunksArrayHeight; i++) {
                float y = chunksRect.y + i * chunkSize;
                for(int j = 0; j <= chunksArrayWidth; j++) {
                    float x = chunksRect.x + j * chunkSize;

                    if(!SceneViewHelper.IsRectVisible(new Rect(x, y, chunkSize, chunkSize)))
                        continue;
                    
                    Handles.color = new Color(1, 1, 1, 0.6f);
                    // Vertical
                    Handles.DrawLine(new Vector3(x, chunksRect.y), new Vector3(x, chunksRect.y + chunksArrayHeight * chunkSize));
                    // Horizontal
                    Handles.DrawLine(new Vector3(chunksRect.x, y), new Vector3(chunksRect.x + chunksArrayWidth * chunkSize, y));

                    if(i < chunksArrayHeight && j < chunksArrayWidth) {
                        Color color = (i + j) % 2 == 1 ? Color.blue : Color.red;
                        Handles.color = color;
                        labelStyle.normal.textColor = color;

                        Handles.Label(new Vector3(x, y), $"[{i}; {j}]", labelStyle);

                        // Checkers-like painting
                        foreach(var slotData in this[i, j].slotDatas) {
                            // The \ element of the cross
                            Handles.DrawLine(slotData.position + Vector3.up + Vector3.left, slotData.position + Vector3.down + Vector3.right);
                            // The / element of the cross
                            Handles.DrawLine(slotData.position + Vector3.up + Vector3.right, slotData.position + Vector3.down + Vector3.left);
                        }
                    }
                }
            }
        }
#endif
    }

}