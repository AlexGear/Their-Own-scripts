using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class TilemapNavigationObstacle : MonoBehaviour {
    private enum CombineKind { None = 0, Single, Vertical, Horizontal }
    private struct TileObstacle {
        public CombineKind combineKind;
        public NavMeshObstacle obstacle;
    }
    private float obstacleHeight = 150;

    void Start() {
        Tilemap tilemap = GetComponent<Tilemap>();
        Vector3 cellSize = tilemap.cellSize;
        cellSize.Scale(transform.lossyScale);

        Quaternion rotation = Quaternion.AngleAxis(-90, transform.right) * transform.rotation;

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);
        TileObstacle[] tileObstacles = new TileObstacle[(bounds.size.x + 1) * (bounds.size.y + 1)];

        int width = bounds.size.x, height = bounds.size.y;
        for(int x = 0; x < width; x++) {
            int ox = x + 1;
            for(int y = 0; y < height; y++) {
                int oy = y + 1;

                Tile tile = (allTiles[x + y * width] as Tile);
                if(tile == null || tile.colliderType == Tile.ColliderType.None) {
                    continue;
                }

                int oi = ox + oy * width;
                TileObstacle combineCandidate = tileObstacles[oi - 1];      // on the left of current tile
                CombineKind kind = combineCandidate.combineKind;
                if(kind == CombineKind.Single || kind == CombineKind.Horizontal) {
                    tileObstacles[oi - 1].combineKind = CombineKind.Horizontal;
                    tileObstacles[oi] = HorizontalCombine(combineCandidate, cellSize.x);
                    continue;
                }

                combineCandidate = tileObstacles[oi - width];  // on the top of current tile
                kind = combineCandidate.combineKind;
                if(kind == CombineKind.Single || kind == CombineKind.Vertical) {
                    tileObstacles[oi - width].combineKind = CombineKind.Vertical;
                    tileObstacles[oi] = VerticalCombine(combineCandidate, cellSize.y);
                    continue;
                }

                var obstacleObject = new GameObject(this.name + " Nav Obstacle");
                obstacleObject.isStatic = this.gameObject.isStatic;
                obstacleObject.transform.parent = NavigationMain.current.transform;
                Vector3 position = tilemap.CellToWorld(new Vector3Int(bounds.x + x, bounds.y + y, 0));
                obstacleObject.transform.SetPositionAndRotation(new Vector3(position.x + cellSize.x / 2f, position.y + cellSize.y / 2f, 0), rotation);

                var nmObstacle = obstacleObject.AddComponent<NavMeshObstacle>();
                nmObstacle.carving = true;
                nmObstacle.size = new Vector3(cellSize.x, obstacleHeight, cellSize.y);

                tileObstacles[oi].obstacle = nmObstacle;
                tileObstacles[oi].combineKind = CombineKind.Single;
            }
        }
        Destroy(this);
    }

    private TileObstacle HorizontalCombine(TileObstacle with, float cellWidth) {
        TileObstacle obstacle;
        obstacle.combineKind = CombineKind.Horizontal;
        obstacle.obstacle = with.obstacle;
        obstacle.obstacle.center += new Vector3(cellWidth / 2f, 0, 0);
        obstacle.obstacle.size += new Vector3(cellWidth, 0, 0);
        return obstacle;
    }

    private TileObstacle VerticalCombine(TileObstacle with, float cellHeight) {
        TileObstacle obstacle;
        obstacle.combineKind = CombineKind.Vertical;
        obstacle.obstacle = with.obstacle;
        obstacle.obstacle.center += new Vector3(0, 0, cellHeight / 2f);
        obstacle.obstacle.size += new Vector3(0, 0, cellHeight);
        return obstacle;
    }
}
