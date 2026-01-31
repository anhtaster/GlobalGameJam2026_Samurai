using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GlobalGameJam
{
    public class MinimapGhostLayer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maskSize = 9;
        [SerializeField] private Color maskColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color activeColor = new Color(1, 0, 0, 0.3f); 
        [SerializeField] private MinimapGridView gridView;

        private RectTransform ghostRect;
        private Image ghostImage;
        private Vector2Int currentGridPos;
        private bool isLocked = false;

        public MinimapGridModel GridModel => gridView != null ? gridView.GridModel : null;  

        public int MaskSize => maskSize;

        private void Awake()
        {
            InitializeGhostVisuals();
        }

        private void InitializeGhostVisuals()
        {
            // Create a simple UI overlay for the ghost cursor
            GameObject ghostObj = new GameObject("GhostCursor_9x9");
            ghostObj.transform.SetParent(transform, false);
            
            ghostRect = ghostObj.AddComponent<RectTransform>();
            ghostImage = ghostObj.AddComponent<Image>();
            ghostImage.color = maskColor;
            ghostImage.raycastTarget = false; // Interact through it

            // Prevent this object from affecting the GridLayoutGroup
            LayoutElement layoutElement = ghostObj.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            // Initial hide
            ghostObj.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            if (ghostImage != null)
                ghostImage.gameObject.SetActive(visible);
        }

        public void SetState(bool isActiveRegion)
        {
            isLocked = isActiveRegion; 
            if (ghostImage != null)
                ghostImage.color = isActiveRegion ? activeColor : maskColor;
        }

        public void UpdateCursorPosition(Vector2Int centerGridPos, float cellSize)
        {
            if (ghostRect == null || gridView == null) return;
            if (isLocked || ghostRect == null || gridView == null) return;

            currentGridPos = centerGridPos;

            // Size: 9x9 cells
            float size = maskSize * cellSize;
            ghostRect.sizeDelta = new Vector2(size, size);

            // Position: Convert grid pos to local UI position
            // Assuming GridView centers items or starts top-left. 
            // We need to match the visual alignment of the MinimapGridView.
            // Simplified approach: If MinimapGridView uses a GridLayoutGroup, 
            // we might need to anchor correctly interact with it.
            
            // Allow manual calibration if needed, or ask GridView for position of cell (0,0)
            // Ideally, we place this GhostLayer UNDER the GridView or overlaying it perfectly.
            
            // For now, let's assume we can map grid coord to local position.
            // If the grid is centered:
            // (x - width/2) * cellSize, (y - height/2) * cellSize
            // But we need exact logic. Let's try to latch onto a specific cell's position?
            
            MinimapCellView cell = gridView.GetCellView(centerGridPos);
            if (cell != null)
            {
                ghostRect.position = cell.transform.position;
            }
            else
            {
                Debug.LogWarning($"[MinimapGhostLayer] Could not find cell view at {centerGridPos}. GridView has {gridView.GridModel?.GridWidth}x{gridView.GridModel?.GridHeight} cells.");
            }
        }

        private void LateUpdate()
        {
            if (ghostRect != null)
            {
                ghostRect.SetAsLastSibling();
                
                ghostRect.rotation = Quaternion.identity; 
            }
        }
    }
}
