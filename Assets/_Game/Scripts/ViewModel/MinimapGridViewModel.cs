using UnityEngine;
using System;

namespace GlobalGameJam
{

    public class MinimapGridViewModel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel gridModel;
        [SerializeField] private GridScanner gridScanner;

        [Header("Player Tracking")]
        [SerializeField] private Transform playerTransform;

        [Header("Viewport Settings")]
        [SerializeField] private int viewportWidth = 12;
        [SerializeField] private int viewportHeight = 12;
        [SerializeField] private bool centerOnPlayer = true;

        [Header("Update Settings")]
        [SerializeField] private bool updateEveryFrame = false;
        [SerializeField] private float updateInterval = 0.1f;

        [Header("Performance")]
        [SerializeField] private bool enablePerformanceTracking = false;
        [SerializeField] private float performanceLogInterval = 5f;

        public event Action<MinimapViewportData> ViewportDataChanged;
        public event Action<Vector2Int> PlayerViewPositionChanged;

        private Vector2Int previousPlayerGridPos;
        private float lastUpdateTime;
        private MinimapViewportData cachedViewportData;
        private int updateCount;
        private float performanceTimer;
        private float totalUpdateTime;

        // Start() removed - call InitializeWithScan() manually from Loading Screen
        // Or call Initialize() if grid is already scanned

        private void Update()
        {
            if (playerTransform == null || gridModel == null)
                return;

            if (!updateEveryFrame && Time.time - lastUpdateTime < updateInterval)
                return;

            float startTime = enablePerformanceTracking ? Time.realtimeSinceStartup : 0f;

            Vector2Int currentPlayerGridPos = GridCoordinateConverter.WorldToGrid(
                playerTransform.position,
                gridModel.GridOrigin,
                gridModel.CellSize
            );

            if (!updateEveryFrame && currentPlayerGridPos == previousPlayerGridPos)
                return;

            MinimapViewportData data = GetViewportData(currentPlayerGridPos);
            if (data != null)
            {
                ViewportDataChanged?.Invoke(data);
                PlayerViewPositionChanged?.Invoke(data.PlayerViewPosition);
                previousPlayerGridPos = currentPlayerGridPos;
            }

            lastUpdateTime = Time.time;

            if (enablePerformanceTracking)
            {
                float updateTime = (Time.realtimeSinceStartup - startTime) * 1000f;
                totalUpdateTime += updateTime;
                updateCount++;
                performanceTimer += Time.deltaTime;

                if (performanceTimer >= performanceLogInterval)
                {
                    float avgUpdateTime = totalUpdateTime / updateCount;
                    Debug.Log($"[MinimapViewModel] Performance: Avg update time: {avgUpdateTime:F3}ms, Updates: {updateCount}, FPS impact: ~{avgUpdateTime * 60f / 1000f:F2}%");
                    
                    performanceTimer = 0f;
                    totalUpdateTime = 0f;
                    updateCount = 0;
                }
            }
        }

        /// <summary>
        /// Initialize and scan - call this from Loading Screen to avoid lag in-game
        /// </summary>
        public void InitializeWithScan()
        {
            if (gridScanner != null)
            {
                gridScanner.ScanScene();
            }
            ForceRefresh();
        }

        /// <summary>
        /// Initialize without scanning (if grid already scanned)
        /// </summary>
        public void Initialize()
        {
            ForceRefresh();
        }

        [ContextMenu("Scan Scene")]
        public void ScanScene()
        {
            if (gridScanner != null)
            {
                gridScanner.ScanScene();
                ForceRefresh();
            }
        }

        [ContextMenu("Refresh Grid View")]
        public void RefreshGridView()
        {
            ForceRefresh();
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
            ForceRefresh();
        }

        public void UpdateCell(Vector2Int gridPos, CellType newType)
        {
            if (gridModel != null)
            {
                gridModel.SetCell(gridPos, newType);
            }
            ForceRefresh();
        }

        public MinimapViewportData GetViewportData()
        {
            if (playerTransform == null || gridModel == null)
                return null;

            Vector2Int playerGridPos = GridCoordinateConverter.WorldToGrid(
                playerTransform.position,
                gridModel.GridOrigin,
                gridModel.CellSize
            );

            return GetViewportData(playerGridPos);
        }

        private MinimapViewportData GetViewportData(Vector2Int playerGridPos)
        {
            if (!centerOnPlayer)
                return null;

            int gridWidth = gridModel.GridWidth;
            int gridHeight = gridModel.GridHeight;

            int desiredStartX = playerGridPos.x - viewportWidth / 2;
            int desiredStartY = playerGridPos.y - viewportHeight / 2;

            int maxStartX = Mathf.Max(0, gridWidth - viewportWidth);
            int maxStartY = Mathf.Max(0, gridHeight - viewportHeight);

            int startX = Mathf.Clamp(desiredStartX, 0, maxStartX);
            int startY = Mathf.Clamp(desiredStartY, 0, maxStartY);
            int endY = startY + viewportHeight; // Top of viewport (exclusive)

            RectInt bounds = new RectInt(startX, startY, viewportWidth, viewportHeight);

            MinimapViewportData data = new MinimapViewportData(viewportWidth, viewportHeight, bounds);

            for (int viewY = 0; viewY < viewportHeight; viewY++)
            {
                for (int viewX = 0; viewX < viewportWidth; viewX++)
                {
                    int fullMapX = startX + viewX;
                    int fullMapY = endY - viewY - 1;

                    MinimapCellData cellData = gridModel.GetCell(fullMapX, fullMapY);
                    data.SetCellType(viewX, viewY, cellData != null ? cellData.CellType : CellType.Empty);
                }
            }

            int playerViewX = playerGridPos.x - bounds.xMin;
            int playerViewY = (bounds.yMax - 1) - playerGridPos.y;
            if (playerViewX >= 0 && playerViewX < bounds.width && playerViewY >= 0 && playerViewY < bounds.height)
            {
                data.PlayerViewPosition = new Vector2Int(playerViewX, playerViewY);
            }
            else
            {
                data.PlayerViewPosition = new Vector2Int(-1, -1);
            }

            // Set player cell type for marker visibility logic
            MinimapCellData playerCell = gridModel.GetCell(playerGridPos.x, playerGridPos.y);
            data.PlayerCellType = playerCell != null ? playerCell.CellType : CellType.Empty;

            return data;
        }

        public void ForceRefresh()
        {
            if (gridModel == null || playerTransform == null)
                return;

            MinimapViewportData data = GetViewportData();
            if (data == null)
                return;

            ViewportDataChanged?.Invoke(data);
            PlayerViewPositionChanged?.Invoke(data.PlayerViewPosition);
        }

        /// <summary>
        /// Get current performance statistics
        /// </summary>
        public string GetPerformanceStats()
        {
            if (updateCount == 0)
                return "No performance data yet";

            float avgUpdateTime = totalUpdateTime / updateCount;
            float fpsImpact = avgUpdateTime * 60f / 1000f;
            return $"Avg: {avgUpdateTime:F3}ms, FPS Impact: ~{fpsImpact:F2}%, Updates: {updateCount}";
        }
    }
}
