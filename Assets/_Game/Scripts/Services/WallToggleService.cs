using UnityEngine;
using System.Collections.Generic;

namespace GlobalGameJam
{
    public class WallToggleService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel gridModel;
        [SerializeField] private MinimapGridView gridView;

        [Header("Safety Settings")]
        [SerializeField] private LayerMask unsafeLayers; // Player layer
        [SerializeField] private Vector3 checkHalfExtents = new Vector3(0.4f, 1f, 0.4f); // Slightly smaller than cell

        private List<GameObject> hiddenWalls = new List<GameObject>();
        // Keep track of hidden positions for visual restoration
        private List<Vector2Int> hiddenPositions = new List<Vector2Int>();
        
        private Vector2Int activeRegionCenter;
        private bool isRegionActive = false;

        public bool IsRegionActive => isRegionActive;

        private void Awake()
        {
            if (gridView == null)
            {
                gridView = FindObjectOfType<MinimapGridView>();
                if (gridView == null)
                {
                    Debug.LogError("[WallToggleService] Could not find MinimapGridView!");
                }
            }
        }

        public bool ToggleRegion(Vector2Int centerPos, int maskSize)
        {
            if (isRegionActive)
            {
                // Try to restore
                if (CanRestoreWalls())
                {
                    RestoreWalls();
                    return true; // Action successful (Restored)
                }
                else
                {
                    Debug.LogWarning("Cannot restore walls! Player is inside.");
                    // TODO: Play error sound or visual feedback
                    return false; // Action failed
                }
            }
            else
            {
                // Hide new region
                HideRegion(centerPos, maskSize);
                return true; // Action successful (Hidden)
            }
        }

        private void HideRegion(Vector2Int centerPos, int maskSize)
        {
            int halfSize = maskSize / 2;
            Vector2Int start = centerPos - new Vector2Int(halfSize, halfSize);
            Vector2Int end = centerPos + new Vector2Int(halfSize, halfSize);

            List<MinimapCellData> cells = gridModel.GetCellsInRect(start, end);
            hiddenPositions.Clear();

            // Check if we have yellow color in config, else default yellow
            Color highlightColor = Color.yellow;
            if (gridView != null && gridView.ColorConfig != null)
            {
                highlightColor = gridView.ColorConfig.HighlightColor; 
            }

            foreach (var cell in cells)
            {
                if (cell.CellType == CellType.Wall && cell.WorldObject != null)
                {
                    cell.WorldObject.SetActive(false);
                    hiddenWalls.Add(cell.WorldObject);
                    hiddenPositions.Add(cell.GridPosition); // Store for restore

                    // Visual Visual Override
                    if (gridView != null)
                    {
                        gridView.SetCellColorOverride(cell.GridPosition, true, highlightColor);
                    }
                }
            }

            activeRegionCenter = centerPos;
            isRegionActive = true;
            Debug.Log($"[WallToggleService] Hidden {hiddenWalls.Count} walls at {centerPos}");
        }

        private bool CanRestoreWalls()
        {
            foreach (var wall in hiddenWalls)
            {
                if (wall == null) continue;

                // Check physics at the wall's position
                if (Physics.CheckBox(wall.transform.position, checkHalfExtents, wall.transform.rotation, unsafeLayers))
                {
                    return false; // Obstruction found!
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
            if (gridView != null)
            {
                foreach (var pos in hiddenPositions)
                {
                    gridView.SetCellColorOverride(pos, false, Color.white);
                }
            }

            hiddenWalls.Clear();
            hiddenPositions.Clear();
            isRegionActive = false;
            Debug.Log("[WallToggleService] Walls restored.");
        }
    }
}
