using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class TilemapToSpriteConverter : MonoBehaviour {
#if UNITY_EDITOR
    [SerializeField] Sprite resultSprite;
    [SerializeField] float pixelsPerUnit = 100;
    [SerializeField] bool generate = false;
    [SerializeField] bool generateAs = false;
    [SerializeField] private string path = null;

    void Awake() {
        generate = false;
        generateAs = false;
    }

    string ChoosePath() {
        return EditorUtility.SaveFilePanelInProject("Save Generated Tilemap Sprite", "Tilemap Sprite", "png", "Save Generated Tilemap Sprite", "Assets/Pictures");
    }

    void Update () {
        if(generateAs) {
            generateAs = false;
            string p = ChoosePath();

            if(string.IsNullOrEmpty(p))
                return;

            path = p;
            generate = true;
        }
        if(generate) {
            generate = false;

            if(string.IsNullOrEmpty(path)) {
                string p = ChoosePath();

                if(string.IsNullOrEmpty(p))
                    return;

                path = p;
            }

            Tilemap tilemap = GetComponent<Tilemap>();
            if(tilemap != null) {
                Generate(tilemap);
            }
            else {
                Debug.LogWarning("No Tilemap attached");
            }
        }
	}

    private void Generate(Tilemap tilemap) {
        int tileCount = tilemap.GetUsedTilesCount();
        TileBase[] tiles = new TileBase[tileCount];
        tilemap.GetUsedTilesNonAlloc(tiles);
        
        tilemap.CompressBounds();
        Vector2 tilemapSize = tilemap.localBounds.size;
        Vector2Int textureSize = Vector2Int.RoundToInt(tilemapSize * pixelsPerUnit);
		Vector2Int powerOf2Size = new Vector2Int(Mathf.NextPowerOfTwo(textureSize.x), Mathf.NextPowerOfTwo(textureSize.y));
        Texture2D texture = new Texture2D(powerOf2Size.x, powerOf2Size.y, TextureFormat.ARGB32, false);
        Color zeroColor = new Color(0, 0, 0, 0);
        for(int x = 0; x < powerOf2Size.x; x++) {
            for(int y = 0; y < powerOf2Size.y; y++) {
                texture.SetPixel(x, y, zeroColor);
            }
        }

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for(int x = 0; x < bounds.size.x; x++) {
            for(int y = 0; y < bounds.size.y; y++) {
                TileBase tileBase = allTiles[x + y * bounds.size.x];
                if(tileBase == null) {
                    continue;
                }
                Tile tile = tileBase as Tile;
                if(tile == null) {
                    continue;
                }

                Sprite tileSprite = tilemap.GetSprite(new Vector3Int(x + bounds.x, y + bounds.y, 0));
                Vector2 tileSize = tileSprite.bounds.size;
                tileSize.x /= transform.lossyScale.x;
                tileSize.y /= transform.lossyScale.y;
                Vector2Int tileSizePixels = Vector2Int.RoundToInt(tileSize * pixelsPerUnit);

                Vector2 pivot = tileSprite.pivot;
                int tileSpriteWidth = Mathf.RoundToInt(tileSprite.rect.xMax);
                int tileSpriteHeight = Mathf.RoundToInt(tileSprite.rect.yMax);
                pivot.x *= tileSize.x / tileSpriteWidth;
                pivot.y *= tileSize.y / tileSpriteHeight;

                Vector2 tilePos = tilemap.CellToLocal(new Vector3Int(x, y, 0));
                Vector2 tileOrigin = tilePos;

                tileOrigin.x *= textureSize.x / tilemapSize.x;
                tileOrigin.y *= textureSize.y / tilemapSize.y;
                Vector2Int tileOriginPixels = Vector2Int.RoundToInt(tileOrigin);

                //Color[] pixels = tile.sprite.texture.GetPixels();
                //texture.SetPixels(tileOriginPixels.x, tileOriginPixels.y, tileSizePixels.x, tileSizePixels.y, pixels);

                Texture2D spriteTex = tileSprite.texture;

                for(int px = 0; px < tileSizePixels.x; px++) {
                    for(int py = 0; py < tileSizePixels.y; py++) {
                        int tx = px * tileSpriteWidth / tileSizePixels.x;
                        int ty = py * tileSpriteHeight / tileSizePixels.y;
                        Color pixel = spriteTex.GetPixel(tx, ty);
                        texture.SetPixel(tileOriginPixels.x + px, tileOriginPixels.y + py, pixel);
                    }
                }
            }
        }

        texture.Apply();
        Rect rect = new Rect(Vector2.zero, textureSize);
        resultSprite = Sprite.Create(texture, rect, (Vector2)textureSize / 2f, pixelsPerUnit);

        CreateAsset();
    }

    public void CreateAsset() {
        System.IO.File.WriteAllBytes(path, resultSprite.texture.EncodeToPNG());
        AssetDatabase.Refresh();
        AssetDatabase.AddObjectToAsset(resultSprite, path);
        AssetDatabase.SaveAssets();

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        ti.spritePixelsPerUnit = resultSprite.pixelsPerUnit;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();

        resultSprite = AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
    }
#endif
}
