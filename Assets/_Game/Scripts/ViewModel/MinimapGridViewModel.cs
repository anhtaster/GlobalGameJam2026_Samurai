using System;
using UnityEngine;

namespace GlobalGameJam
{

    public class MinimapGridViewModel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel gridModel;
        [SerializeField] private MinimapGridView gridView;
        [SerializeField] private MinimapPlayerMarkerView playerMarker;
        [SerializeField] private GridScanner gridScanner;

        [Header("Player Tracking")]
        [SerializeField] private Transform playerTransform;

        [Header("Viewport Settings")]
        [SerializeField] private int viewportWidth = 12;
        [SerializeField] private int viewportHeight = 12;

        [Header("Ghost Settings")]
        [SerializeField] private MinimapGhostLayer ghostLayer;

        [Header("Rotation Settings")]
        [SerializeField] private Transform rotationTarget; // Assign the Grid Container here

        public event Action<Vector2Int> PlayerViewPositionChanged;
        public event Action<MinimapViewportData> ViewportDataChanged;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            RotateMinimap();
            HandleGhostInput(); 
        }

        private void RotateMinimap()
        {
            if (playerTransform == null) return;

            Transform target = rotationTarget;
            if (target == null && gridView != null)
            {
                target = gridView.transform;
            }

            if (target != null)
            {
                // Rotate opposite to player to keep North Up? Or follow player?
                // User said "rotate according to player view". 
                // Usually this means Map's Z rotation = Player's Y rotation.
                // So if Player turns 90deg Right, Map turns 90deg Left to keep "Forward" Up?
                // OR Map turns 90deg Right so "Forward" is East?
                // Let's stick to: Map Roation = Player Rotation. 
                // So if Player Y = 90, Map Z = 90.
                float playerY = playerTransform.eulerAngles.y;
                target.rotation = Quaternion.Euler(0, 0, playerY);
            }
        }

        public void Initialize()
        {
            // Scan scene if scanner is assigned
            if (gridScanner != null)
            {
                Debug.Log("[MinimapGridViewModel] Scanning scene...");
                gridScanner.ScanScene();
            }

            // Generate minimap grid UI (viewport mode)
            if (gridView != null)
            {
                Debug.Log($"[MinimapGridViewModel] Generating grid view ({viewportWidth}x{viewportHeight})...");
                gridView.GenerateGrid(viewportWidth, viewportHeight);
            }
            else
            {
                Debug.LogError("[MinimapGridViewModel] GridView is not assigned!");
            }

            // Setup player marker
            if (playerMarker != null && playerTransform != null)
            {
                playerMarker.SetPlayerTransform(playerTransform);
                playerMarker.SetGridView(gridView);
                Debug.Log("[MinimapGridViewModel] Player marker initialized");
            }

            Debug.Log("[MinimapGridViewModel] Initialization complete!");
        }

        [ContextMenu("Scan Scene")]
        public void ScanScene()
        {
            if (gridScanner != null)
            {
                gridScanner.ScanScene();
                if (gridView != null)
                {
                    gridView.GenerateGrid(viewportWidth, viewportHeight);
                }
            }
        }

        public void InitializeWithScan() 
        {
            // Quét cảnh trước rồi mới khởi tạo UI
            ScanScene();
            Initialize();
        }

        [ContextMenu("Refresh Grid View")]
        public void RefreshGridView()
        {
            if (gridView != null)
            {
                gridView.GenerateGrid(viewportWidth, viewportHeight);
            }
        }

        public void ForceRefresh()
        {
            RefreshGridView();
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
            if (playerMarker != null)
            {
                playerMarker.SetPlayerTransform(player);
            }
        }

        public void UpdateCell(Vector2Int gridPos, CellType newType)
        {
            if (gridModel != null)
            {
                gridModel.SetCell(gridPos, newType);
            }

            if (gridView != null)
            {
                gridView.UpdateCell(gridPos);
            }
        }

        public string GetPerformanceStats()
        {
            if (gridModel == null) return "No Model";
            return $"{viewportWidth}x{viewportHeight} Viewport active";
        }

        private Vector2Int GetMouseGridPosition()
        {
            if (gridView == null) return Vector2Int.zero;

            RectTransform rect = gridView.GetComponent<RectTransform>();
            
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out Vector2 localPoint))
            {
                float width = rect.rect.width;
                float height = rect.rect.height;

                int x = Mathf.FloorToInt((localPoint.x + (width / 2)) / gridView.CellUISize);
                int y = Mathf.FloorToInt(((height / 2) - localPoint.y) / gridView.CellUISize);

                x = Mathf.Clamp(x, 0, viewportWidth - 1);
                y = Mathf.Clamp(y, 0, viewportHeight - 1);

                return new Vector2Int(x, y);
            }

            return Vector2Int.zero;
        }

        private void HandleGhostInput()
        {
            if (ghostLayer == null || gridView == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                ghostLayer.SetState(true);
            }

            if (Input.GetMouseButtonUp(0))
            {
                ghostLayer.SetState(false);
            }

            if (!Input.GetMouseButton(0))
            {
                Vector2Int mouseGridPos = GetMouseGridPosition(); 
                ghostLayer.UpdateCursorPosition(mouseGridPos, gridView.CellUISize);
            }
        }
    }
}
