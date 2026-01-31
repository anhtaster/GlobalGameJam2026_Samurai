using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Manages viewport for minimap - shows only a portion of the grid centered on player
    /// </summary>
    public class MinimapViewport : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel fullGridModel; // Full map data
        [SerializeField] private MinimapGridView gridView;
        [SerializeField] private Transform playerTransform;

        [Header("Viewport Settings")]
        [SerializeField] private int viewportWidth = 12;
        [SerializeField] private int viewportHeight = 12;
        [SerializeField] private bool centerOnPlayer = true;

        [Header("Update Settings")]
        [SerializeField] private bool updateEveryFrame = false;
        [SerializeField] private float updateInterval = 0.1f; // Update every 0.1s if not every frame

        private Vector2Int currentViewportCenter;
        private Vector2Int previousViewportCenter;
        private float lastUpdateTime;
        private bool isInitialized = false;

        private void Start()
        {
            // Delay initialization to ensure GridScanner has run first
            Invoke(nameof(DelayedInit), 0.2f);
        }

        private void DelayedInit()
        {
            if (fullGridModel == null)
            {
                Debug.LogError("[MinimapViewport] Full Grid Model is not assigned!");
                return;
            }

            if (gridView == null)
            {
                Debug.LogError("[MinimapViewport] Grid View is not assigned!");
                return;
            }

            if (playerTransform == null)
            {
                Debug.LogError("[MinimapViewport] Player Transform is not assigned!");
                return;
            }

            fullGridModel.Initialize();
            isInitialized = true;
            UpdateViewport(true);
        }

        private void Update()
        {
            if (!isInitialized || playerTransform == null || fullGridModel == null)
                return;

            if (updateEveryFrame)
            {
                UpdateViewport();
            }
            else
            {
                if (Time.time - lastUpdateTime >= updateInterval)
                {
                    UpdateViewport();
                    lastUpdateTime = Time.time;
                }
            }
        }

        /// <summary>
        /// Update viewport to show area around player
        /// </summary>
        private void UpdateViewport(bool forceUpdate = false)
        {
            // Get player grid position
            Vector2Int playerGridPos = GridCoordinateConverter.WorldToGrid(
                playerTransform.position,
                fullGridModel.GridOrigin,
                fullGridModel.CellSize
            );

            // Calculate viewport center
            if (centerOnPlayer)
            {
                currentViewportCenter = playerGridPos;
            }

            // Only update if viewport center changed or forced
            if (!forceUpdate && currentViewportCenter == previousViewportCenter)
                return;

            RefreshViewportCells();
            previousViewportCenter = currentViewportCenter;
        }

        /// <summary>
        /// Refresh all cells in the viewport
        /// </summary>
        private void RefreshViewportCells()
        {
            // Calculate viewport bounds in full map coordinates
            int startX = currentViewportCenter.x - viewportWidth / 2;
            int endY = currentViewportCenter.y + viewportHeight / 2; // Highest Y in full map = top of viewport

            // Update each cell in the grid view
            for (int viewY = 0; viewY < viewportHeight; viewY++) // viewY=0 is TOP of UI
            {
                for (int viewX = 0; viewX < viewportWidth; viewX++)
                {
                    // Calculate position in full map
                    // viewY=0 (top of UI) corresponds to highest fullMapY (north in game)
                    // viewY increases going DOWN in UI, so fullMapY decreases
                    int fullMapX = startX + viewX;
                    int fullMapY = endY - viewY - 1;

                    Vector2Int fullMapPos = new Vector2Int(fullMapX, fullMapY);

                    // Get cell data from full map
                    MinimapCellData cellData = fullGridModel.GetCell(fullMapPos);

                    // Get corresponding view cell (viewport coordinates)
                    MinimapCellView cellView = gridView.GetCellView(viewX, viewY);

                    if (cellView != null && cellData != null)
                    {
                        // Clear any previous highlight/override before updating
                        cellView.SetVisualOverride(false, Color.white);
                        
                        // Update cell view with data from full map
                        cellView.SetCellType(cellData.CellType);
                        cellView.WorldGridPosition = fullMapPos; // Important for coordinate mapping
                    }
                    else if (cellView != null)
                    {
                        // Clear any previous highlight/override
                        cellView.SetVisualOverride(false, Color.white);
                        
                        // Out of bounds or no data - show empty
                        cellView.SetCellType(CellType.Empty);
                        cellView.WorldGridPosition = fullMapPos;
                    }
                }
            }

            // Re-apply yellow highlights for any hidden walls in this viewport
            WallToggleService wallToggleService = FindObjectOfType<WallToggleService>();
            if (wallToggleService != null)
            {
                wallToggleService.RefreshHighlights();
            }
        }

        /// <summary>
        /// Get current viewport bounds in full map coordinates
        /// </summary>
        public RectInt GetViewportBounds()
        {
            int startX = currentViewportCenter.x - viewportWidth / 2;
            int startY = currentViewportCenter.y - viewportHeight / 2;
            return new RectInt(startX, startY, viewportWidth, viewportHeight);
        }

        /// <summary>
        /// Set player transform
        /// </summary>
        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        /// <summary>
        /// Force refresh viewport
        /// </summary>
        [ContextMenu("Force Refresh Viewport")]
        public void ForceRefresh()
        {
            UpdateViewport(true);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || fullGridModel == null)
                return;

            // Draw viewport bounds in scene view
            RectInt bounds = GetViewportBounds();
            Vector3 min = GridCoordinateConverter.GridToWorldCorner(
                new Vector2Int(bounds.xMin, bounds.yMin),
                fullGridModel.GridOrigin,
                fullGridModel.CellSize
            );
            Vector3 max = GridCoordinateConverter.GridToWorldCorner(
                new Vector2Int(bounds.xMax, bounds.yMax),
                fullGridModel.GridOrigin,
                fullGridModel.CellSize
            );

            Vector3 center = (min + max) / 2f;
            Vector3 size = new Vector3(
                bounds.width * fullGridModel.CellSize,
                0.1f,
                bounds.height * fullGridModel.CellSize
            );

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
