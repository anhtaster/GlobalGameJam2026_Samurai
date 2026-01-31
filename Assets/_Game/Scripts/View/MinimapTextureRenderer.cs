using UnityEngine;
using UnityEngine.UI;

namespace GlobalGameJam
{
    /// <summary>
    /// Renders the minimap grid data to a Texture2D for optimal performance.
    /// Designed for large maps (700x350+) where UI-based rendering would be too slow.
    /// </summary>
    public class MinimapTextureRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel gridModel;
        [SerializeField] private MinimapColorConfig colorConfig;
        [SerializeField] private RawImage displayImage;

        [Header("Texture Settings")]
        [SerializeField] private FilterMode filterMode = FilterMode.Point;
        [SerializeField] private int pixelsPerCell = 1;
        
        [Header("Orientation (check to flip axis)")]
        [SerializeField] private bool flipX = false;
        [SerializeField] private bool flipY = true; // Usually needed for Unity coordinate system
        [SerializeField] private bool swapXY = false; // Swap X and Y axes

        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = Color.yellow;

        private Texture2D mapTexture;
        private Color32[] pixelBuffer; // Reusable buffer to avoid allocations
        private bool isDirty = false;

        public Texture2D MapTexture => mapTexture;
        public int TextureWidth => gridModel != null ? gridModel.GridWidth * pixelsPerCell : 0;
        public int TextureHeight => gridModel != null ? gridModel.GridHeight * pixelsPerCell : 0;

        private void Start()
        {
            // Delay to ensure GridScanner has populated the model
            Invoke(nameof(InitializeTexture), 0.3f);
        }

        /// <summary>
        /// Initialize the texture with grid data
        /// </summary>
        public void InitializeTexture()
        {
            if (gridModel == null)
            {
                Debug.LogError("[MinimapTextureRenderer] GridModel not assigned!");
                return;
            }

            if (colorConfig == null)
            {
                Debug.LogError("[MinimapTextureRenderer] ColorConfig not assigned!");
                return;
            }

            gridModel.Initialize();

            int width = gridModel.GridWidth * pixelsPerCell;
            int height = gridModel.GridHeight * pixelsPerCell;

            // Create texture (use RGBA32 for best compatibility)
            mapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            mapTexture.filterMode = filterMode;
            mapTexture.wrapMode = TextureWrapMode.Clamp;

            // Initialize pixel buffer
            pixelBuffer = new Color32[width * height];

            // Render initial map
            RenderFullMap();

            // Assign to display
            if (displayImage != null)
            {
                displayImage.texture = mapTexture;
            }

            Debug.Log($"[MinimapTextureRenderer] Initialized {width}x{height} texture");
        }

        /// <summary>
        /// Render the entire map to texture (call once at start or when map changes significantly)
        /// </summary>
        public void RenderFullMap()
        {
            if (mapTexture == null || gridModel == null || colorConfig == null)
                return;

            int gridWidth = gridModel.GridWidth;
            int gridHeight = gridModel.GridHeight;

            // Fill pixel buffer
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var cell = gridModel.GetCell(x, y);
                    Color32 color;

                    if (cell != null)
                    {
                        color = colorConfig.GetColorForCellType(cell.CellType);
                    }
                    else
                    {
                        color = colorConfig.GetColorForCellType(CellType.Empty);
                    }

                    // Fill pixels for this cell (supports pixelsPerCell > 1)
                    SetCellPixels(x, y, color);
                }
            }

            // Apply all pixels at once (much faster than SetPixel per cell)
            mapTexture.SetPixels32(pixelBuffer);
            mapTexture.Apply();
        }

        /// <summary>
        /// Update a single cell's pixels (for dynamic changes like hiding walls)
        /// </summary>
        public void UpdateCell(int gridX, int gridY, Color32 color)
        {
            if (mapTexture == null || pixelBuffer == null)
                return;

            if (gridX < 0 || gridX >= gridModel.GridWidth || 
                gridY < 0 || gridY >= gridModel.GridHeight)
                return;

            SetCellPixels(gridX, gridY, color);
            isDirty = true;
        }

        /// <summary>
        /// Update a cell to show as highlighted (e.g., hidden wall)
        /// </summary>
        public void SetCellHighlight(int gridX, int gridY, bool highlighted)
        {
            if (gridModel == null || colorConfig == null)
                return;

            var cell = gridModel.GetCell(gridX, gridY);
            Color32 color;

            if (highlighted)
            {
                color = highlightColor;
            }
            else if (cell != null)
            {
                color = colorConfig.GetColorForCellType(cell.CellType);
            }
            else
            {
                color = colorConfig.GetColorForCellType(CellType.Empty);
            }

            UpdateCell(gridX, gridY, color);
        }

        /// <summary>
        /// Apply pending texture changes (call in LateUpdate or after batch updates)
        /// </summary>
        public void ApplyChanges()
        {
            if (isDirty && mapTexture != null)
            {
                mapTexture.SetPixels32(pixelBuffer);
                mapTexture.Apply();
                isDirty = false;
            }
        }

        private void LateUpdate()
        {
            // Auto-apply any pending changes
            ApplyChanges();
        }

        /// <summary>
        /// Set pixels for a single grid cell (handles pixelsPerCell > 1)
        /// </summary>
        private void SetCellPixels(int gridX, int gridY, Color32 color)
        {
            int gridWidth = gridModel.GridWidth;
            int gridHeight = gridModel.GridHeight;
            int textureWidth = gridWidth * pixelsPerCell;
            int textureHeight = gridHeight * pixelsPerCell;

            // Apply coordinate transformations
            int transformedX = gridX;
            int transformedY = gridY;

            // Swap X and Y if needed
            if (swapXY)
            {
                int temp = transformedX;
                transformedX = transformedY;
                transformedY = temp;
                // Also swap dimensions for texture calculation
                textureWidth = gridHeight * pixelsPerCell;
            }

            // Apply flips
            if (flipX)
            {
                transformedX = (swapXY ? gridHeight : gridWidth) - 1 - transformedX;
            }
            if (flipY)
            {
                transformedY = (swapXY ? gridWidth : gridHeight) - 1 - transformedY;
            }

            for (int py = 0; py < pixelsPerCell; py++)
            {
                for (int px = 0; px < pixelsPerCell; px++)
                {
                    int pixelX = transformedX * pixelsPerCell + px;
                    int pixelY = transformedY * pixelsPerCell + py;
                    int index = pixelY * textureWidth + pixelX;

                    if (index >= 0 && index < pixelBuffer.Length)
                    {
                        pixelBuffer[index] = color;
                    }
                }
            }
        }

        /// <summary>
        /// Convert grid position to UV coordinates (0-1 range)
        /// </summary>
        public Vector2 GridToUV(Vector2Int gridPos)
        {
            if (gridModel == null)
                return Vector2.zero;

            float u = (gridPos.x + 0.5f) / gridModel.GridWidth;
            float v = (gridPos.y + 0.5f) / gridModel.GridHeight;
            return new Vector2(u, v);
        }

        /// <summary>
        /// Convert grid position to local UI position within the RawImage
        /// </summary>
        public Vector2 GridToLocalPosition(Vector2Int gridPos)
        {
            if (displayImage == null)
                return Vector2.zero;

            RectTransform rect = displayImage.rectTransform;
            Vector2 uv = GridToUV(gridPos);

            // Convert UV to local position (assuming pivot is center)
            float localX = (uv.x - 0.5f) * rect.rect.width;
            float localY = (uv.y - 0.5f) * rect.rect.height;

            return new Vector2(localX, localY);
        }

        private void OnDestroy()
        {
            // Clean up texture
            if (mapTexture != null)
            {
                Destroy(mapTexture);
                mapTexture = null;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Refresh Texture")]
        private void EditorRefreshTexture()
        {
            if (Application.isPlaying)
            {
                RenderFullMap();
            }
        }
#endif
    }
}
