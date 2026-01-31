using UnityEngine;

namespace GlobalGameJam
{

    public class MinimapGridViewModel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel gridModel;
        [SerializeField] private MinimapPlayerMarkerView playerMarker;
        [SerializeField] private GridScanner gridScanner;

        [Header("Texture Mode (for large maps - RECOMMENDED)")]
        [SerializeField] private MinimapTextureRenderer textureRenderer;
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private MinimapGhostLayer ghostLayer;
        [SerializeField] private WallToggleService wallToggleService;
        
        [Header("Legacy Mode (for small maps)")]
        [SerializeField] private MinimapGridView gridView;

        [Header("Player Tracking")]
        [SerializeField] private Transform playerTransform;

        [Header("Viewport Settings (Legacy Mode Only)")]
        [SerializeField] private int viewportWidth = 12;
        [SerializeField] private int viewportHeight = 12;

        [Header("Rotation Settings")]
        [SerializeField] private Transform rotationTarget;

        public float CurrentRotationAngle { get; private set; }
        
        private bool useTextureMode = false;

        private void Start()
        {
            // Determine mode based on what's assigned
            useTextureMode = textureRenderer != null;
            
            Initialize();
        }

        public void Initialize()
        {
            // Scan scene first
            if (gridScanner != null)
            {
                Debug.Log("[MinimapGridViewModel] Scanning scene...");
                gridScanner.ScanScene();
            }

            if (useTextureMode)
            {
                InitializeTextureMode();
            }
            else
            {
                InitializeLegacyMode();
            }

            Debug.Log($"[MinimapGridViewModel] Init complete (TextureMode={useTextureMode})");
        }

        /// <summary>
        /// Initialize for texture-based rendering (large maps)
        /// </summary>
        private void InitializeTextureMode()
        {
            // TextureRenderer initializes itself via Start/Invoke
            // Just wire up the references

            if (playerMarker != null && playerTransform != null)
            {
                playerMarker.SetPlayerTransform(playerTransform);
                playerMarker.SetTextureRenderer(textureRenderer, mapContainer);
                playerMarker.SetGridModel(gridModel);
                Debug.Log("[MinimapGridViewModel] Player marker configured for texture mode");
            }

            if (ghostLayer != null && mapContainer != null)
            {
                ghostLayer.SetTextureMode(textureRenderer, mapContainer, gridModel);
                Debug.Log("[MinimapGridViewModel] Ghost layer configured for texture mode");
            }

            if (wallToggleService != null)
            {
                wallToggleService.SetTextureRenderer(textureRenderer);
                Debug.Log("[MinimapGridViewModel] WallToggleService configured for texture mode");
            }
        }

        /// <summary>
        /// Initialize for legacy UI grid (small maps)
        /// </summary>
        private void InitializeLegacyMode()
        {
            if (gridView != null)
            {
                Debug.Log($"[MinimapGridViewModel] Generating grid view ({viewportWidth}x{viewportHeight})...");
                gridView.GenerateGrid(viewportWidth, viewportHeight);
            }
            else
            {
                Debug.LogError("[MinimapGridViewModel] GridView is not assigned!");
            }

            if (playerMarker != null && playerTransform != null)
            {
                playerMarker.SetPlayerTransform(playerTransform);
                playerMarker.SetGridView(gridView);
                Debug.Log("[MinimapGridViewModel] Player marker initialized (legacy mode)");
            }
        }

        private void Update()
        {
            // RotateMinimap(); // Disabled - map rotation feature removed
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
                float playerY = playerTransform.eulerAngles.y;
                target.rotation = Quaternion.Euler(0, 0, playerY);
                CurrentRotationAngle = playerY;
            }
        }

        [ContextMenu("Scan Scene")]
        public void ScanScene()
        {
            if (gridScanner != null)
            {
                gridScanner.ScanScene();
                
                if (useTextureMode && textureRenderer != null)
                {
                    textureRenderer.RenderFullMap();
                }
                else if (gridView != null)
                {
                    gridView.GenerateGrid(viewportWidth, viewportHeight);
                }
            }
        }

        [ContextMenu("Refresh Grid View")]
        public void RefreshGridView()
        {
            if (useTextureMode && textureRenderer != null)
            {
                textureRenderer.RenderFullMap();
            }
            else if (gridView != null)
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

            if (useTextureMode && textureRenderer != null)
            {
                // Update single pixel
                var cell = gridModel.GetCell(gridPos);
                if (cell != null && gridView != null && gridView.ColorConfig != null)
                {
                    Color color = gridView.ColorConfig.GetColorForCellType(newType);
                    textureRenderer.UpdateCell(gridPos.x, gridPos.y, color);
                    textureRenderer.ApplyChanges();
                }
            }
            else if (gridView != null)
            {
                gridView.UpdateCell(gridPos);
            }
        }
    }
}

