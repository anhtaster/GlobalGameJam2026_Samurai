using UnityEngine;
using UnityEngine.UI;

namespace GlobalGameJam
{

    [RequireComponent(typeof(Image))]
    public class MinimapCellView : MonoBehaviour
    {
        [SerializeField] private Image cellImage;

        private MinimapCellData cellData;
        private MinimapColorConfig colorConfig;
        
        // Cached RectTransform for performance (avoid GetComponent every frame)
        private RectTransform cachedRectTransform;
        public RectTransform CachedRectTransform => cachedRectTransform;

        private void Awake()
        {
            if (cellImage == null)
            {
                cellImage = GetComponent<Image>();
            }
            // Cache RectTransform once at Awake
            cachedRectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(MinimapCellData data, MinimapColorConfig config)
        {
            cellData = data;
            colorConfig = config;

            UpdateVisual();
        }

        private bool isOverrideActive = false;
        private Color currentOverrideColor;

        public void UpdateVisual()
        {
            if (cellData == null || colorConfig == null || cellImage == null)
                return;

            if (isOverrideActive)
            {
                cellImage.color = currentOverrideColor;
            }
            else
            {
                Color cellColor = colorConfig.GetColorForCellType(cellData.CellType);
                cellImage.color = cellColor;
            }
            
            // Don't disable/enable image - it causes GridLayoutGroup to rebuild
            // Instead, use transparent color for Empty cells (set in ColorConfig)
        }

        public void SetCellType(CellType type)
        {
            if (cellData == null)
                return;

            cellData.SetCellType(type);
            UpdateVisual();
        }

        public Vector2Int GetGridPosition()
        {
            return cellData?.GridPosition ?? Vector2Int.zero;
        }

        public MinimapCellData GetCellData()
        {
            return cellData;
        }

        // Added to track actual world coordinate in Viewport Mode
        public Vector2Int WorldGridPosition { get; set; }

        public void SetVisualOverride(bool active, Color overrideColor)
        {
            isOverrideActive = active;
            currentOverrideColor = overrideColor;
            
            UpdateVisual();
        }
    }
}
