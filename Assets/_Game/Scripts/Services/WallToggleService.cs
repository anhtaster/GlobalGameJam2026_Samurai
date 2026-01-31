using UnityEngine;
using System.Collections.Generic;

namespace GlobalGameJam
{
    public class WallToggleService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel gridModel;
        
        [Header("Texture Mode (for large maps)")]
        [SerializeField] private MinimapTextureRenderer textureRenderer;
        
        [Header("Legacy Mode (for small maps)")]
        [SerializeField] private MinimapGridView gridView;

        [Header("Safety Settings")]
        [SerializeField] private LayerMask unsafeLayers;
        [SerializeField] private Vector3 checkHalfExtents = new Vector3(0.4f, 1f, 0.4f);

        private List<GameObject> hiddenWalls = new List<GameObject>();
        private List<Vector2Int> hiddenPositions = new List<Vector2Int>();
        
        private Vector2Int activeRegionCenter;
        private bool isRegionActive = false;
        private bool useTextureMode = false;
        private Color highlightColor = Color.yellow;

        public bool IsRegionActive => isRegionActive;

        private void Awake()
        {
            if (gridModel == null)
            {
                Debug.LogWarning("[WallToggleService] MinimapGridModel not assigned!");
            }
        }

        private void Start()
        {
            useTextureMode = textureRenderer != null;
            
            // Get highlight color from config
            if (gridView != null && gridView.ColorConfig != null)
            {
                highlightColor = gridView.ColorConfig.HighlightColor;
            }
        }

        public bool ToggleRegion(Vector2Int centerPos, int maskSize)
        {
            if (gridModel == null)
            {
                Debug.LogError("[WallToggleService] GridModel is null!");
                return false;
            }

            if (isRegionActive)
            {
                if (CanRestoreWalls())
                {
                    RestoreWalls();
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot restore walls! Player is inside.");
                    return false;
                }
            }
            else
            {
                HideRegion(centerPos, maskSize);
                return true;
            }
        }

        private void HideRegion(Vector2Int centerPos, int maskSize)
        {
            if (gridModel == null) return;

            int halfSize = maskSize / 2;
            Vector2Int start = centerPos - new Vector2Int(halfSize, halfSize);
            Vector2Int end = centerPos + new Vector2Int(halfSize, halfSize);

            List<MinimapCellData> cells = gridModel.GetCellsInRect(start, end);
            if (cells == null) return;
            
            hiddenPositions.Clear();
            hiddenWalls.Clear();

            foreach (var cell in cells)
            {
                if (cell == null) continue;
                
                if (cell.CellType == CellType.Wall && cell.WorldObject != null)
                {
                    cell.WorldObject.SetActive(false);
                    hiddenWalls.Add(cell.WorldObject);
                    hiddenPositions.Add(cell.GridPosition);

                    // Update visual
                    SetCellHighlight(cell.GridPosition, true);
                }
            }

            activeRegionCenter = centerPos;
            isRegionActive = true;
            
            // Apply texture changes
            if (useTextureMode && textureRenderer != null)
            {
                textureRenderer.ApplyChanges();
            }

            Debug.Log($"[WallToggleService] Hidden {hiddenWalls.Count} walls at {centerPos}");
        }

        private bool CanRestoreWalls()
        {
            foreach (var wall in hiddenWalls)
            {
                if (wall == null) continue;
                if (Physics.CheckBox(wall.transform.position, checkHalfExtents, wall.transform.rotation, unsafeLayers))
                {
                    return false;
                }
            }
            return true;
        }

        private void RestoreWalls()
        {
            foreach (var wall in hiddenWalls)
            {
                if (wall != null)
                {
                    wall.SetActive(true);
                }
            }

            // Restore visuals
            foreach (var pos in hiddenPositions)
            {
                SetCellHighlight(pos, false);
            }

            // Apply texture changes
            if (useTextureMode && textureRenderer != null)
            {
                textureRenderer.ApplyChanges();
            }

            hiddenWalls.Clear();
            hiddenPositions.Clear();
            isRegionActive = false;
            Debug.Log("[WallToggleService] Walls restored.");
        }

        /// <summary>
        /// Set highlight for a cell (works for both texture and UI mode)
        /// </summary>
        private void SetCellHighlight(Vector2Int gridPos, bool highlighted)
        {
            if (useTextureMode && textureRenderer != null)
            {
                textureRenderer.SetCellHighlight(gridPos.x, gridPos.y, highlighted);
            }
            else if (gridView != null)
            {
                gridView.SetCellColorOverride(gridPos, highlighted, highlighted ? highlightColor : Color.white);
            }
        }

        /// <summary>
        /// Re-apply highlights after map refresh
        /// </summary>
        public void RefreshHighlights()
        {
            if (!isRegionActive) return;

            foreach (var pos in hiddenPositions)
            {
                SetCellHighlight(pos, true);
            }

            if (useTextureMode && textureRenderer != null)
            {
                textureRenderer.ApplyChanges();
            }
        }

        public void SetTextureRenderer(MinimapTextureRenderer renderer)
        {
            textureRenderer = renderer;
            useTextureMode = renderer != null;
        }

        public void SetGridView(MinimapGridView view)
        {
            gridView = view;
            if (view != null && view.ColorConfig != null)
            {
                highlightColor = view.ColorConfig.HighlightColor;
            }
        }
    }
}

