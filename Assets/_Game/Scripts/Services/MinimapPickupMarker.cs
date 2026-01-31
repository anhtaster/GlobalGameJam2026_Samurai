using UnityEngine;
using System.Collections.Generic;

namespace GlobalGameJam
{
    /// <summary>
    /// Manages pickup markers on the minimap. Renders them as blue dots on the map texture.
    /// </summary>
    public class MinimapPickupMarker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapTextureRenderer textureRenderer;
        [SerializeField] private MinimapGridModel gridModel;
        
        [Header("Marker Settings")]
        [SerializeField] private Color pickupColor = Color.cyan; // Blue color for pickups

        private Dictionary<Vector2Int, bool> pickupMarkers = new Dictionary<Vector2Int, bool>();

        /// <summary>
        /// Register a pickup location to show on minimap
        /// </summary>
        public void RegisterPickup(Vector3 worldPosition)
        {
            if (gridModel == null)
            {
                Debug.LogWarning("[MinimapPickupMarker] GridModel not assigned!");
                return;
            }

            // Convert world position to grid position
            Vector2Int gridPos = WorldToGrid(worldPosition);
            
            if (!pickupMarkers.ContainsKey(gridPos))
            {
                pickupMarkers[gridPos] = true;
               UpdateMarker(gridPos, true);
                Debug.Log($"[MinimapPickupMarker] Registered pickup at grid {gridPos}");
            }
        }

        /// <summary>
        /// Remove a pickup marker (when picked up)
        /// </summary>
        public void RemovePickup(Vector3 worldPosition)
        {
            if (gridModel == null) return;

            Vector2Int gridPos = WorldToGrid(worldPosition);
            
            if (pickupMarkers.ContainsKey(gridPos))
            {
                pickupMarkers.Remove(gridPos);
                UpdateMarker(gridPos, false);
            }
        }

        private void UpdateMarker(Vector2Int gridPos, bool show)
        {
            if (textureRenderer == null) return;

            if (show)
            {
                textureRenderer.UpdateCell(gridPos.x, gridPos.y, pickupColor);
            }
            else
            {
                // Restore original cell color
                var cell = gridModel.GetCell(gridPos);
                if (cell != null)
                {
                    var colorConfig = FindObjectOfType<MinimapColorConfig>();
                    if (colorConfig != null)
                    {
                        Color32 originalColor = colorConfig.GetColorForCellType(cell.CellType);
                        textureRenderer.UpdateCell(gridPos.x, gridPos.y, originalColor);
                    }
                }
            }
            
            textureRenderer.ApplyChanges();
        }

        private Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            Vector3 localPos = worldPosition - gridModel.GridOrigin;
            int gridX = Mathf.RoundToInt(localPos.x / gridModel.CellSize);
            int gridY = Mathf.RoundToInt(localPos.z / gridModel.CellSize);
            return new Vector2Int(gridX, gridY);
        }
    }
}
