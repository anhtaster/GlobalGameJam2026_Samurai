using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Debug utility to test if GridScanner is working properly
    /// Attach this to GridScanner GameObject to see debug logs
    /// </summary>
    public class GridScannerDebugger : MonoBehaviour
    {
        [SerializeField] private GridScanner gridScanner;
        [SerializeField] private MinimapGridModel gridModel;

        [Header("Debug Visualization")]
        [SerializeField] private bool logGridData = false;
        [SerializeField] private bool drawDebugCubes = false;
        [SerializeField] private float debugCubeHeight = 0.5f;

        private void Start()
        {
            // Debug logging disabled by default to prevent console spam
            // Uncomment to enable debug logging
            // Invoke(nameof(DebugGridData), 0.5f);
        }

        private void DebugGridData()
        {
            if (gridModel == null)
            {
                Debug.LogError("[DebugTool] GridModel is not assigned!");
                return;
            }

            // Initialize if needed
            gridModel.Initialize();

            int floorCount = 0;
            int wallCount = 0;
            int emptyCount = 0;

            Debug.Log("=== GRID SCAN RESULTS ===");
            Debug.Log($"Grid Size: {gridModel.GridWidth}x{gridModel.GridHeight}");
            Debug.Log($"Cell Size: {gridModel.CellSize}");
            Debug.Log($"Grid Origin: {gridModel.GridOrigin}");

            for (int x = 0; x < gridModel.GridWidth; x++)
            {
                for (int y = 0; y < gridModel.GridHeight; y++)
                {
                    var cell = gridModel.GetCell(x, y);
                    if (cell != null)
                    {
                        switch (cell.CellType)
                        {
                            case CellType.Floor:
                                floorCount++;
                                if (logGridData)
                                    Debug.Log($"  [{x},{y}] = FLOOR (Object: {cell.WorldObject?.name ?? "null"})");
                                break;
                            case CellType.Wall:
                                wallCount++;
                                if (logGridData)
                                    Debug.Log($"  [{x},{y}] = WALL (Object: {cell.WorldObject?.name ?? "null"})");
                                break;
                            case CellType.Empty:
                                emptyCount++;
                                break;
                        }
                    }
                }
            }

            Debug.Log($"=== SUMMARY ===");
            Debug.Log($"Total Cells: {gridModel.GridWidth * gridModel.GridHeight}");
            Debug.Log($"Floor Cells: {floorCount}");
            Debug.Log($"Wall Cells: {wallCount}");
            Debug.Log($"Empty Cells: {emptyCount}");

            if (floorCount == 0 && wallCount == 0)
            {
                Debug.LogWarning("⚠️ NO CELLS DETECTED! Check:");
                Debug.LogWarning("  1. Did you tag your ProBuilder objects with 'Floor' and 'Wall' tags?");
                Debug.LogWarning("  2. Is Grid Origin positioned correctly?");
                Debug.LogWarning("  3. Is Cell Size correct?");
            }
            else
            {
                Debug.Log($"✅ Grid scan successful! Detected {floorCount + wallCount} objects.");
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugCubes || gridModel == null || !Application.isPlaying)
                return;

            gridModel.Initialize();

            for (int x = 0; x < gridModel.GridWidth; x++)
            {
                for (int y = 0; y < gridModel.GridHeight; y++)
                {
                    var cell = gridModel.GetCell(x, y);
                    if (cell != null && cell.CellType != CellType.Empty)
                    {
                        Vector3 cellCenter = GridCoordinateConverter.GridToWorld(
                            new Vector2Int(x, y),
                            gridModel.GridOrigin,
                            gridModel.CellSize
                        );
                        cellCenter.y = debugCubeHeight;

                        // Draw different colors
                        Gizmos.color = cell.CellType == CellType.Floor 
                            ? new Color(0, 0, 0, 0.5f) 
                            : new Color(1, 1, 1, 0.7f);
                        
                        Gizmos.DrawCube(cellCenter, new Vector3(gridModel.CellSize * 0.8f, 0.2f, gridModel.CellSize * 0.8f));
                        
                        // Draw wire frame
                        Gizmos.color = cell.CellType == CellType.Floor ? Color.gray : Color.white;
                        Gizmos.DrawWireCube(cellCenter, new Vector3(gridModel.CellSize, 0.2f, gridModel.CellSize));
                    }
                }
            }
        }

        [ContextMenu("Force Debug Grid Data")]
        public void ForceDebug()
        {
            DebugGridData();
        }
    }
}
