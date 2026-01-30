using UnityEngine;

namespace GlobalGameJam
{

    public static class GridCoordinateConverter
    {

        public static Vector2Int WorldToGrid(Vector3 worldPosition, Vector3 gridOrigin, float cellSize)
        {
            Vector3 localPos = worldPosition - gridOrigin;
            int x = Mathf.FloorToInt(localPos.x / cellSize);
            int z = Mathf.FloorToInt(localPos.z / cellSize);
            return new Vector2Int(x, z);
        }

        public static Vector3 GridToWorld(Vector2Int gridPosition, Vector3 gridOrigin, float cellSize)
        {
            float x = gridOrigin.x + (gridPosition.x + 0.5f) * cellSize;
            float z = gridOrigin.z + (gridPosition.y + 0.5f) * cellSize;
            return new Vector3(x, gridOrigin.y, z);
        }

        public static Vector3 GridToWorldCorner(Vector2Int gridPosition, Vector3 gridOrigin, float cellSize)
        {
            float x = gridOrigin.x + gridPosition.x * cellSize;
            float z = gridOrigin.z + gridPosition.y * cellSize;
            return new Vector3(x, gridOrigin.y, z);
        }

        public static bool IsWorldPositionInGrid(Vector3 worldPosition, Vector3 gridOrigin, float cellSize, int gridWidth, int gridHeight)
        {
            Vector2Int gridPos = WorldToGrid(worldPosition, gridOrigin, cellSize);
            return gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.y >= 0 && gridPos.y < gridHeight;
        }

        public static Vector2Int ClampGridPosition(Vector2Int position, int gridWidth, int gridHeight)
        {
            int x = Mathf.Clamp(position.x, 0, gridWidth - 1);
            int y = Mathf.Clamp(position.y, 0, gridHeight - 1);
            return new Vector2Int(x, y);
        }
    }
}
