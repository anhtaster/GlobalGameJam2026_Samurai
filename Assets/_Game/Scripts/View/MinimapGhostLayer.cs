using UnityEngine;
using UnityEngine.UI;

namespace GlobalGameJam
{
    public class MinimapGhostLayer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maskSize = 9;
        [SerializeField] private Color maskColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color activeColor = new Color(1, 0, 0, 0.3f);
        
        [Header("Texture Mode (for large maps)")]
        [SerializeField] private MinimapTextureRenderer textureRenderer;
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private MinimapGridModel gridModel;
        
        [Header("Legacy Mode (for small maps)")]
        [SerializeField] private MinimapGridView gridView;

        private RectTransform ghostRect;
        private Image ghostImage;
        private Vector2Int currentGridPos;
        private bool useTextureMode = false;

        public int MaskSize => maskSize;
        public Vector2Int CurrentGridPos => currentGridPos;

        private void Awake()
        {
            InitializeGhostVisuals();
        }

        private void Start()
        {
            useTextureMode = textureRenderer != null && mapContainer != null;
        }

        private void InitializeGhostVisuals()
        {
            GameObject ghostObj = new GameObject("GhostCursor_9x9");
            ghostObj.transform.SetParent(transform, false);
            
            ghostRect = ghostObj.AddComponent<RectTransform>();
            ghostImage = ghostObj.AddComponent<Image>();
            ghostImage.color = maskColor;
            ghostImage.raycastTarget = false;

            LayoutElement layoutElement = ghostObj.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            ghostObj.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            if (ghostImage != null)
            {
                ghostImage.gameObject.SetActive(visible);
                if (visible && ghostRect != null)
                {
                    ghostRect.SetAsLastSibling();
                }
            }
        }

        public void SetState(bool isActiveRegion)
        {
            if (ghostImage != null)
                ghostImage.color = isActiveRegion ? activeColor : maskColor;
        }

        /// <summary>
        /// Update cursor position using cell size (legacy mode)
        /// </summary>
        public void UpdateCursorPosition(Vector2Int centerGridPos, float cellSize)
        {
            if (ghostRect == null) return;

            currentGridPos = centerGridPos;
            float size = maskSize * cellSize;
            ghostRect.sizeDelta = new Vector2(size, size);

            if (useTextureMode)
            {
                UpdatePositionTexture(centerGridPos);
            }
            else
            {
                UpdatePositionLegacy(centerGridPos);
            }
        }

        /// <summary>
        /// Update cursor position for texture mode
        /// </summary>
        private void UpdatePositionTexture(Vector2Int gridPos)
        {
            if (mapContainer == null || gridModel == null) return;

            // Calculate position on texture
            float u = (gridPos.x + 0.5f) / gridModel.GridWidth;
            float v = (gridPos.y + 0.5f) / gridModel.GridHeight;

            u = Mathf.Clamp01(u);
            v = Mathf.Clamp01(v);

            Rect containerRect = mapContainer.rect;
            float localX = (u - 0.5f) * containerRect.width;
            float localY = (v - 0.5f) * containerRect.height;

            ghostRect.position = mapContainer.TransformPoint(new Vector3(localX, localY, 0));

            // Scale cursor size based on map container to cell ratio
            float cellPixelSize = containerRect.width / gridModel.GridWidth;
            float cursorSize = maskSize * cellPixelSize;
            ghostRect.sizeDelta = new Vector2(cursorSize, cursorSize);
        }

        /// <summary>
        /// Update cursor position for legacy UI mode
        /// </summary>
        private void UpdatePositionLegacy(Vector2Int gridPos)
        {
            if (gridView == null) return;

            MinimapCellView cell = gridView.GetCellView(gridPos);
            if (cell != null)
            {
                ghostRect.position = cell.transform.position;
            }
        }

        public void SetTextureMode(MinimapTextureRenderer renderer, RectTransform container, MinimapGridModel model)
        {
            textureRenderer = renderer;
            mapContainer = container;
            gridModel = model;
            useTextureMode = renderer != null && container != null;
        }

        public void SetGridView(MinimapGridView view)
        {
            gridView = view;
            useTextureMode = false;
        }
    }
}

