using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/WallTile")]
public class WallTile : Tile {
    [SerializeField] Sprite[] sprites;
    [SerializeField] bool isCap = false;

    private static readonly Vector3Int[] offsets = { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };

    private bool IsWall(Vector3Int pos, ITilemap tilemap, bool includeCaps = true) {
        var tile = tilemap.GetTile(pos);
        return tile is WallTile && (includeCaps || !((WallTile)tile).isCap);
    }

    private int CalculateCode(Vector3Int position, ITilemap tilemap) {
        int code = 0;
        for(int i = 0; i < 4; i++) {
            var neighbour = position + offsets[i];
            if(IsWall(neighbour, tilemap)) {
                code |= 1 << i;
            }
        }
        return code;
    }

    private int CalculateCapCode(Vector3Int position, ITilemap tilemap) {
        for(int i = 0; i < 4; i++) {
            var neighbour = position + offsets[i];
            if(IsWall(neighbour, tilemap, false)) {
                return 1 << i;
            }
        }
        return 1;
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap) {
        tilemap.RefreshTile(position);
        for(int i = 0; i < 4; i++) {
            var neighbour = position + offsets[i];
            if(IsWall(neighbour, tilemap)) {
                tilemap.RefreshTile(neighbour);
            }
        }
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        int code = isCap ? CalculateCapCode(position, tilemap) : CalculateCode(position, tilemap);
        tileData.sprite = sprites[code];
        tileData.colliderType = colliderType;
    }
}
