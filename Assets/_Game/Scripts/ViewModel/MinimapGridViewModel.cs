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

        private void Start()
        {
            Initialize();
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

        [ContextMenu("Refresh Grid View")]
        public void RefreshGridView()
        {
            if (gridView != null)
            {
                gridView.GenerateGrid(viewportWidth, viewportHeight);
            }
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
    }
}
