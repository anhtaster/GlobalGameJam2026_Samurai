using UnityEngine;
using System.Collections.Generic;

namespace GlobalGameJam
{

    [CreateAssetMenu(fileName = "MinimapGridModel", menuName = "GlobalGameJam/Minimap/Grid Model")]
    public class MinimapGridModel : ScriptableObject
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 9;
        [SerializeField] private int gridHeight = 9;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Grid Data")]
        [SerializeField] private List<MinimapCellData> cells = new List<MinimapCellData>();

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float CellSize => cellSize;
        public Vector3 GridOrigin => gridOrigin;

        public bool IsInitialized => gridArray != null;

        private MinimapCellData[,] gridArray;

        public void Initialize()
        {
            gridArray = new MinimapCellData[gridWidth, gridHeight];

            foreach (var cell in cells)
            {
                if (IsValidGridPosition(cell.GridPosition))
                {
                    gridArray[cell.GridPosition.x, cell.GridPosition.y] = cell;
                }
            }
        }

        public void EnsureInitialized()
        {
            if (gridArray == null)
            {
                Initialize();
            }
        }

        public bool HasAnyNonEmptyCells()
        {
            if (gridArray == null)
                return false;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cell = gridArray[x, y];
                    if (cell != null && cell.CellType != CellType.Empty)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void SetupGrid(int width, int height, float size, Vector3 origin)
        {
            gridWidth = width;
            gridHeight = height;
            cellSize = size;
            gridOrigin = origin;

            cells.Clear();
            gridArray = new MinimapCellData[gridWidth, gridHeight];

            // Initialize all cells as Empty
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cellData = new MinimapCellData(new Vector2Int(x, y), CellType.Empty);
                    gridArray[x, y] = cellData;
                    cells.Add(cellData);
                }
            }
        }

        public MinimapCellData GetCell(Vector2Int gridPos)
        {
            if (!IsValidGridPosition(gridPos))
                return null;

            if (gridArray == null)
                return null;

            return gridArray[gridPos.x, gridPos.y];
        }

        public MinimapCellData GetCell(int x, int y)
        {
            return GetCell(new Vector2Int(x, y));
        }

        public void SetCell(Vector2Int gridPos, CellType type, GameObject worldObject = null)
        {
            if (!IsValidGridPosition(gridPos))
                return;

            if (gridArray == null)
            {
                Debug.LogWarning("[MinimapGridModel] GridArray not initialized! Call SetupGrid() first.");
                return;
            }

            // WALL DEDUPLICATION: Skip if trying to place wall on existing wall
            // This handles all junction types: L-corners, T-junctions, crosses
            // First wall registered at a position wins
            var existingCell = gridArray[gridPos.x, gridPos.y];
            if (type == CellType.Wall && existingCell.CellType == CellType.Wall)
            {
                return; // Skip - cell already has a wall
            }

            gridArray[gridPos.x, gridPos.y].SetCellType(type);
            gridArray[gridPos.x, gridPos.y].SetWorldObject(worldObject);
        }

        public bool IsValidGridPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
        }

        public List<MinimapCellData> GetCellsInRect(Vector2Int start, Vector2Int end)
        {
            List<MinimapCellData> result = new List<MinimapCellData>();

            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var cell = GetCell(x, y);
                    if (cell != null)
                    {
                        result.Add(cell);
                    }
                }
            }

            return result;
        }

        public void Clear()
        {
            cells.Clear();
            gridArray = null;
        }
    }
}
