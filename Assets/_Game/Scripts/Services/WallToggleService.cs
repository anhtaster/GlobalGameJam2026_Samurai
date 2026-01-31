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
        
        private bool isRegionActive = false;
        private bool useTextureMode = false;
        private Color highlightColor = Color.yellow;

        public bool IsRegionActive => isRegionActive;

        private void Start()
        {
            useTextureMode = textureRenderer != null;
            
            if (gridView != null && gridView.ColorConfig != null)
            {
                highlightColor = gridView.ColorConfig.HighlightColor;
            }
        }

        public bool ToggleRegion(Vector2Int centerPos, int maskSize)
        {
            if (gridModel == null) return false;

            if (isRegionActive)
            {
                if (CanRestoreWalls())
                {
                    RestoreWalls();
                    return true;
                }
                return false;
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
                    SetCellHighlight(cell.GridPosition, true);
                }
            }

            if (useTextureMode && textureRenderer != null)
            {
                textureRenderer.ApplyChanges();
            }
            
            isRegionActive = true;
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

            foreach (var pos in hiddenPositions)
            {
                SetCellHighlight(pos, false);
            }

            if (useTextureMode && textureRenderer != null)
            {
                textureRenderer.ApplyChanges();
            }

            hiddenWalls.Clear();
            hiddenPositions.Clear();
            isRegionActive = false;
        }

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
