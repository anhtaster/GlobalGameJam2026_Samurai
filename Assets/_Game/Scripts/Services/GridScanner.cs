using UnityEngine;
using System.Collections.Generic;

namespace GlobalGameJam
{
    /// <summary>
    /// Service to scan ProBuilder objects in the scene and generate minimap grid data
    /// </summary>
    public class GridScanner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel gridModel;

        [Header("Scan Settings")]
        [SerializeField] private string floorTag = "Floor";
        [SerializeField] private string wallTag = "Wall";
        [SerializeField] private string[] extraColorTags = { "R", "G", "B" };
        [SerializeField] private LayerMask scanLayerMask = -1; // Scan all layers by default

        [Header("Grid Configuration")]
        [SerializeField] private int gridWidth = 50;
        [SerializeField] private int gridHeight = 50;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Runtime Settings")]
        [SerializeField] private bool scanOnStart = true;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoFloorColor = new Color(0, 0, 0, 0.3f);
        [SerializeField] private Color gizmoWallColor = new Color(1, 1, 1, 0.5f);

        private void Start()
        {
            if (scanOnStart)
            {
                // Debug.Log("[GridScanner] Auto-scanning scene on Start...");
                ScanScene();
            }
        }

        /// <summary>
        /// Scan the scene and populate grid model
        /// </summary>
        public void ScanScene()
        {
            if (gridModel == null)
            {
                Debug.LogError("[GridScanner] GridModel is not assigned!");
                return;
            }

            // Setup grid
            gridModel.SetupGrid(gridWidth, gridHeight, cellSize, gridOrigin);
            // Debug.Log($"[GridScanner] Grid initialized: {gridWidth}x{gridHeight}, Cell Size: {cellSize}");

            // Find all objects with Floor tag
            GameObject[] floorObjects = GameObject.FindGameObjectsWithTag(floorTag);

            foreach (var obj in floorObjects)
            {
                RegisterObject(obj, CellType.Floor);
            }

            // Find all objects with Wall tag
            GameObject[] wallObjects = GameObject.FindGameObjectsWithTag(wallTag);

            foreach (var obj in wallObjects)
            {
                RegisterObject(obj, CellType.Wall);
            }

            foreach (string tag in extraColorTags)
            {
                GameObject[] extraObjects = GameObject.FindGameObjectsWithTag(tag);
                foreach (var obj in extraObjects)
                {
                    RegisterObject(obj, CellType.Wall); 
                }
            }

            // Post-process: thin wall corners to single cells
            ThinWallCorners();

            // Debug.Log($"[GridScanner] Scan done: {floorObjects.Length} floors, {wallObjects.Length} walls");
        }

        /// <summary>
        /// Dynamically register a single wall GameObject to the grid (for tutorial or runtime spawning)
        /// </summary>
        public void RegisterWall(GameObject wallObject)
        {
            if (wallObject == null)
            {
                Debug.LogWarning("[GridScanner] Cannot register null wall object!");
                return;
            }

            if (gridModel == null)
            {
                Debug.LogError("[GridScanner] GridModel is not assigned!");
                return;
            }

            // Register the wall
            RegisterObject(wallObject, CellType.Wall);
            
            // Update texture renderer if available
            MinimapTextureRenderer textureRenderer = FindFirstObjectByType<MinimapTextureRenderer>();
            if (textureRenderer != null)
            {
                // Get the wall's grid positions
                Bounds bounds = GetObjectBounds(wallObject);
                Vector2Int minGrid = GridCoordinateConverter.WorldToGrid(bounds.min, gridOrigin, cellSize);
                Vector2Int maxGrid = GridCoordinateConverter.WorldToGrid(bounds.max, gridOrigin, cellSize);

                // Normalize for walls (same logic as in RegisterObject)
                int widthX = maxGrid.x - minGrid.x;
                int widthY = maxGrid.y - minGrid.y;
                
                if (widthX >= widthY)
                {
                    maxGrid.y = minGrid.y; // Horizontal wall
                }
                else
                {
                    maxGrid.x = minGrid.x; // Vertical wall
                }

                // Update all cells covered by this wall on the texture
                for (int x = minGrid.x; x <= maxGrid.x; x++)
                {
                    for (int y = minGrid.y; y <= maxGrid.y; y++)
                    {
                        if (gridModel.IsValidGridPosition(new Vector2Int(x, y)))
                        {
                            var cell = gridModel.GetCell(x, y);
                            if (cell != null)
                            {
                                MinimapColorConfig colorConfig = FindFirstObjectByType<MinimapColorConfig>();
                                if (colorConfig != null)
                                {
                                    textureRenderer.UpdateCell(x, y, colorConfig.GetColorForCellType(cell.CellType));
                                }
                            }
                        }
                    }
                }
                
                textureRenderer.ApplyChanges();
            }

            Debug.Log($"[GridScanner] Wall '{wallObject.name}' registered to minimap at position {wallObject.transform.position}");
        }

        /// <summary>
        /// Register a GameObject to the grid
        /// </summary>
        
        private void RegisterObject(GameObject obj, CellType cellType)
        {
            if (obj == null) return;

            // Get bounds of the object
            Bounds bounds = GetObjectBounds(obj);
            
            // Debug log for walls
            if (cellType == CellType.Wall)
            {
                // Debug.Log($"[GridScanner] WALL: {obj.name} | WorldPos: {obj.transform.position} | Bounds: center={bounds.center}, size={bounds.size}");
            }
            
            // Calculate min and max grid positions covered by this object
            Vector3 minWorld = bounds.min;
            Vector3 maxWorld = bounds.max;
            
            Vector2Int minGrid = GridCoordinateConverter.WorldToGrid(minWorld, gridOrigin, cellSize);
            Vector2Int maxGrid = GridCoordinateConverter.WorldToGrid(maxWorld, gridOrigin, cellSize);
            
            // For WALLS: Normalize to 1 cell thickness, keep full length
            // Deduplication in SetCell will prevent overlaps at junctions
            if (cellType == CellType.Wall && obj.CompareTag(wallTag))
            {
                int widthX = maxGrid.x - minGrid.x;
                int widthY = maxGrid.y - minGrid.y;
                
                if (widthX >= widthY)
                {
                    // Horizontal wall - normalize to single row
                    maxGrid.y = minGrid.y;
                }
                else
                {
                    // Vertical wall - normalize to single column
                    maxGrid.x = minGrid.x;
                }
                
                // Debug.Log($"[GridScanner] WALL: {obj.name} -> Grid [{minGrid.x},{minGrid.y}] to [{maxGrid.x},{maxGrid.y}]");
            }
            
            // Safeguard: limit max cells per object to prevent crashes
            const int MAX_CELLS_PER_OBJECT = 10000;
            int totalCells = (maxGrid.x - minGrid.x + 1) * (maxGrid.y - minGrid.y + 1);
            if (totalCells > MAX_CELLS_PER_OBJECT)
            {
                Debug.LogWarning($"[GridScanner] Object {obj.name} too large ({totalCells} cells), skipping");
                return;
            }
            
            // Register ALL cells that this object covers
            for (int x = minGrid.x; x <= maxGrid.x; x++)
            {
                for (int y = minGrid.y; y <= maxGrid.y; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    if (gridModel.IsValidGridPosition(gridPos))
                    {
                        gridModel.SetCell(gridPos, cellType, obj);
                    }
                }
            }
        }

        /// <summary>
        /// Post-process wall cells to prune 1-cell stubs/tips sticking out of corners.
        /// This fixes "overshoot" overlaps where a wall extends 1 cell past a junction.
        /// </summary>
        private void ThinWallCorners()
        {
            List<Vector2Int> cellsToRemove = new List<Vector2Int>();
            
            // Iterate all cells to find "Stubs"
            // A Stub is a wall cell with exactly 1 neighbor, 
            // where that neighbor is a Corner or Junction (has neighbors in perpendicular axes)
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    var cell = gridModel.GetCell(pos);
                    
                    if (cell == null || cell.CellType != CellType.Wall)
                        continue;
                    
                    // Check neighbors of CURRENT cell
                    List<Vector2Int> myNeighbors = GetWallNeighbors(pos);
                    
                    // If I am an endpoint (1 neighbor)
                    if (myNeighbors.Count == 1)
                    {
                        Vector2Int neighborPos = myNeighbors[0];
                        
                        // Check neighbors of my NEIGHBOR
                        List<Vector2Int> neighborNeighbors = GetWallNeighbors(neighborPos);
                        
                        // Check if neighbor is a "Feature" (Corner, T-Junction, +)
                        // It is a feature if it has neighbors in BOTH X and Y axes
                        // (ignoring me, or including me - doesn't matter, checking axis diversity)
                        
                        bool hasHorizontal = false;
                        bool hasVertical = false;
                        
                        foreach (var n in neighborNeighbors)
                        {
                            if (n.x != neighborPos.x) hasHorizontal = true;
                            if (n.y != neighborPos.y) hasVertical = true;
                        }
                        
                        // If neighbor connects to both axes, it's a Corner/Junction
                        // And since I am an endpoint attached to it, I am likely a "stub" or "overshoot"
                        
                        // CRITICAL FIX: Only prune if the neighbor is a complex junction (T or Cross)
                        // If neighbor has only 2 connections (L-corner or Line), removing me would break the line!
                        // We strictly want to remove "extra" stubs, not parts of the main path.
                        
                        if (hasHorizontal && hasVertical && neighborNeighbors.Count > 2)
                        {
                            // It is a T-junction or Cross (checked by count > 2)
                            // AND it involves both axes (checked by flags)
                            // So I am a safe-to-remove stub.
                            cellsToRemove.Add(pos);
                        }
                    }
                }
            }
            
            // Remove marked cells (convert to Floor as requested, to avoid gaps)
            foreach (var pos in cellsToRemove)
            {
                // We set it to Floor. We pass null for the object because we don't know which floor object is there exactly,
                // but for visualization this is sufficient (it will color it as floor).
                gridModel.SetCell(pos, CellType.Floor, null);
            }
            
            if (cellsToRemove.Count > 0)
            {
                // Debug.Log($"[GridScanner] Pruned {cellsToRemove.Count} wall stubs (converted to Floor)");
            }
        }

        private List<Vector2Int> GetWallNeighbors(Vector2Int pos)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            
            Vector2Int[] dirs = new Vector2Int[] 
            { 
                new Vector2Int(0, 1), 
                new Vector2Int(0, -1), 
                new Vector2Int(1, 0), 
                new Vector2Int(-1, 0) 
            };
            
            foreach (var dir in dirs)
            {
                Vector2Int nPos = pos + dir;
                if (gridModel.IsValidGridPosition(nPos))
                {
                    var cell = gridModel.GetCell(nPos);
                    if (cell != null && cell.CellType == CellType.Wall)
                    {
                        neighbors.Add(nPos);
                    }
                }
            }
            return neighbors;
        }
        
        private bool IsWallAt(int x, int y)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (!gridModel.IsValidGridPosition(pos))
                return false;
            
            var cell = gridModel.GetCell(pos);
            return cell != null && cell.CellType == CellType.Wall;
        }

        /// <summary>
        /// Get bounds of a GameObject (handles MeshRenderer, Collider, etc.)
        /// </summary>
        private Bounds GetObjectBounds(GameObject obj)
        {
            // Try to get MeshRenderer
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                return meshRenderer.bounds;
            }

            // Try to get Collider
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds;
            }

            // If no renderer or collider, use transform position with small bounds
            return new Bounds(obj.transform.position, Vector3.one * cellSize);
        }

        /// <summary>
        /// Set grid origin to current position
        /// </summary>
        [ContextMenu("Set Grid Origin to This Position")]
        public void SetGridOriginToThisPosition()
        {
            gridOrigin = transform.position;
            Debug.Log($"[GridScanner] Grid origin set to {gridOrigin}");
        }

        /// <summary>
        /// Auto-calculate grid origin from scene bounds
        /// </summary>
        [ContextMenu("Auto Calculate Grid Origin")]
        public void AutoCalculateGridOrigin()
        {
            // Find all floor objects to determine bounds
            GameObject[] floorObjects = GameObject.FindGameObjectsWithTag(floorTag);
            
            if (floorObjects.Length == 0)
            {
                Debug.LogWarning("[GridScanner] No floor objects found to calculate grid origin!");
                return;
            }

            // Calculate bounds
            Bounds totalBounds = GetObjectBounds(floorObjects[0]);
            foreach (var obj in floorObjects)
            {
                totalBounds.Encapsulate(GetObjectBounds(obj));
            }

            // Set origin to bottom-left corner
            gridOrigin = totalBounds.min;
            gridOrigin.y = 0; // Keep y at ground level

            Debug.Log($"[GridScanner] Grid origin auto-calculated: {gridOrigin}");
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || gridModel == null) return;

            // Draw grid outline
            Gizmos.color = Color.cyan;
            Vector3 gridSize = new Vector3(gridWidth * cellSize, 0, gridHeight * cellSize);
            Gizmos.DrawWireCube(gridOrigin + gridSize / 2f, gridSize);

            // Draw grid cells if model is initialized
            if (Application.isPlaying && gridModel != null)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        var cell = gridModel.GetCell(x, y);
                        if (cell != null && cell.CellType != CellType.Empty)
                        {
                            Vector3 cellCenter = GridCoordinateConverter.GridToWorld(
                                new Vector2Int(x, y), 
                                gridOrigin, 
                                cellSize
                            );

                            Gizmos.color = cell.CellType == CellType.Floor ? gizmoFloorColor : gizmoWallColor;
                            Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
                        }
                    }
                }
            }
        }
    }
}
