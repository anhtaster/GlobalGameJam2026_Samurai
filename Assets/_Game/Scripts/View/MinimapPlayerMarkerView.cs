using UnityEngine;
using UnityEngine.UI;

namespace GlobalGameJam
{
    public class MinimapPlayerMarkerView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image markerImage;
        [SerializeField] private Transform playerTransform;
        
        [Header("Texture Mode (for large maps)")]
        [SerializeField] private MinimapTextureRenderer textureRenderer;
        [SerializeField] private RectTransform mapContainer;
        
        [Header("Legacy Mode (for small maps)")]
        [SerializeField] private MinimapGridView gridView;

        [Header("Orientation (must match TextureRenderer)")]
        [SerializeField] private bool flipX = false;
        [SerializeField] private bool flipY = true;

        [Header("Settings")]
        [SerializeField] private MinimapColorConfig colorConfig;
        [SerializeField] private float smoothSpeed = 10f;
        [SerializeField] private bool smoothMovement = true;

        private Vector2Int currentGridPosition;
        private Vector2Int previousGridPosition;
        private RectTransform markerRect;
        private MinimapGridModel gridModel;
        private Vector3 targetPosition;
        private bool useTextureMode = false;

        private void Awake()
        {
            if (markerImage == null)
            {
                markerImage = GetComponent<Image>();
            }
            markerRect = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (colorConfig != null && markerImage != null)
            {
                markerImage.color = colorConfig.PlayerMarkerColor;
            }

            useTextureMode = textureRenderer != null;
            
            if (!useTextureMode && gridView != null)
            {
                gridModel = gridView.GridModel;
            }
        }

        private void Update()
        {
            if (playerTransform == null)
                return;

            if (gridModel == null)
            {
                if (gridView != null && gridView.GridModel != null)
                {
                    gridModel = gridView.GridModel;
                }
                else
                {
                    return;
                }
            }

            UpdatePlayerMarker();
        }

        private void UpdatePlayerMarker()
        {
            currentGridPosition = GridCoordinateConverter.WorldToGrid(
                playerTransform.position,
                gridModel.GridOrigin,
                gridModel.CellSize
            );

            if (useTextureMode)
            {
                UpdateMarkerPositionTexture();
            }
            else
            {
                if (currentGridPosition != previousGridPosition)
                {
                    UpdateMarkerPositionLegacy();
                    previousGridPosition = currentGridPosition;
                }
            }
        }

        /// <summary>
        /// Update marker position for texture-based rendering
        /// </summary>
        private void UpdateMarkerPositionTexture()
        {
            if (textureRenderer == null || mapContainer == null || markerRect == null)
                return;

            // Calculate sub-cell position
            Vector3 localPos = playerTransform.position - gridModel.GridOrigin;
            float gridX = localPos.x / gridModel.CellSize;
            float gridZ = localPos.z / gridModel.CellSize;

            // Convert to UV (0-1 range)
            float u = gridX / gridModel.GridWidth;
            float v = gridZ / gridModel.GridHeight;

            // Apply flip transforms (MUST MATCH TextureRenderer settings!)
            if (flipX)
            {
                u = 1f - u;
            }
            if (flipY)
            {
                v = 1f - v;
            }

            // Clamp to valid range
            u = Mathf.Clamp01(u);
            v = Mathf.Clamp01(v);

            // Convert UV to local position within map container
            Rect containerRect = mapContainer.rect;
            float localX = (u - 0.5f) * containerRect.width;
            float localY = (v - 0.5f) * containerRect.height;

            targetPosition = mapContainer.TransformPoint(new Vector3(localX, localY, 0));

            if (smoothMovement)
            {
                markerRect.position = Vector3.Lerp(
                    markerRect.position,
                    targetPosition,
                    Time.deltaTime * smoothSpeed
                );
            }
            else
            {
                markerRect.position = targetPosition;
            }
        }

        /// <summary>
        /// Legacy update for UI grid mode
        /// </summary>
        private void UpdateMarkerPositionLegacy()
        {
            if (gridView == null) return;

            MinimapCellView cellView = gridView.GetCellView(currentGridPosition);
            if (cellView != null)
            {
                RectTransform targetRect = cellView.CachedRectTransform;
                if (targetRect != null && markerRect != null)
                {
                    if (smoothMovement)
                    {
                        markerRect.position = Vector3.Lerp(
                            markerRect.position,
                            targetRect.position,
                            Time.deltaTime * smoothSpeed
                        );
                    }
                    else
                    {
                        markerRect.position = targetRect.position;
                    }
                }
            }
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        public void SetGridView(MinimapGridView view)
        {
            gridView = view;
            if (view != null)
            {
                gridModel = view.GridModel;
            }
        }

        public void SetTextureRenderer(MinimapTextureRenderer renderer, RectTransform container)
        {
            textureRenderer = renderer;
            mapContainer = container;
            useTextureMode = renderer != null;
        }

        public void SetGridModel(MinimapGridModel model)
        {
            gridModel = model;
        }

        public void SetFlip(bool x, bool y)
        {
            flipX = x;
            flipY = y;
        }

        public void SetColorConfig(MinimapColorConfig config)
        {
            colorConfig = config;
            if (markerImage != null && colorConfig != null)
            {
                markerImage.color = colorConfig.PlayerMarkerColor;
            }
        }
    }
}


