using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Forest {

#if UNITY_EDITOR
    public static class TilemapOutlineGenerator {
        private static readonly Color32 empty = new Color32(0, 0, 0, 0);
        private static readonly Color32 solid = new Color32(255, 255, 255, 255);

        delegate void GenerateOutlineDelegate(Texture2D texture, Rect rect,
            float detail, byte alphaTolerance, bool holeDetection, out Vector2[][] paths);

        static TilemapOutlineGenerator() {
            // "GenerateOutline" method has "internal" accessibility level
            // so we need to use reflection to gain access to it
            var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            MethodInfo methodInfo = typeof(UnityEditor.Sprites.SpriteUtility).GetMethod("GenerateOutline", flags);
            GenerateOutline = (GenerateOutlineDelegate)Delegate.CreateDelegate(typeof(GenerateOutlineDelegate), methodInfo);
        }

        private readonly static GenerateOutlineDelegate GenerateOutline;

        /// <summary>
        /// Generates outline paths around the tilemap tiles.
        /// </summary>
        /// <returns></returns>
        public static Vector2[][] Generate(Tilemap tilemap, float detailLevel = 1f, bool holeDetection = true) {
            // detailScale is used to increase texture resolution and thus the level of details
            const int detailScale = 8;
            Texture2D texture = TilemapToTexture(tilemap, detailScale, out Rect rect);
            Vector2[][] paths;
            GenerateOutline(texture, rect, detailLevel, alphaTolerance: 254, holeDetection, out paths);

            // The paths points' coords are in the texture pixel space relative to its center
            // so we need to transform them into the world space
            Vector2 offset = (Vector2Int)tilemap.cellBounds.min + rect.center / detailScale;
            for(int j = 0; j < paths.Length; j++) {
                Vector2[] path = paths[j];
                for(int i = 0; i < path.Length; i++) {
                    path[i] = tilemap.LocalToWorld(tilemap.CellToLocalInterpolated(path[i] / detailScale + offset));
                }
                // Looping the path
                Array.Resize(ref paths[j], path.Length + 1);
                path = paths[j];
                path[path.Length - 1] = path[0];
            }
            return paths;
        }

        public static Texture2D TilemapToTexture(Tilemap tilemap, int detailScale, out Rect rect) {
            Vector3Int size = tilemap.cellBounds.size * detailScale;
            rect = new Rect(0, 0, size.x, size.y);
            Color32[] pixels = new Color32[size.x * size.y];
            Vector3Int min = tilemap.cellBounds.min;

            for(int y = 0; y < size.y; y++) {
                for(int x = 0; x < size.x; x++) {
                    TileBase tile = tilemap.GetTile(min + new Vector3Int(x / detailScale, y / detailScale, 0));
                    pixels[x + y * size.x] = (tile == null ? empty : solid);
                }
            }

            Texture2D texture = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
            texture.anisoLevel = 0;
            texture.filterMode = FilterMode.Point;
            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }
    }
#endif

}