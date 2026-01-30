using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// ScriptableObject for minimap color configuration
    /// </summary>
    [CreateAssetMenu(fileName = "MinimapColorConfig", menuName = "GlobalGameJam/Minimap/Color Config")]
    public class MinimapColorConfig : ScriptableObject
    {
        [Header("Cell Colors")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0f); // Transparent dark
        [SerializeField] private Color floorColor = Color.black;
        [SerializeField] private Color wallColor = Color.white;

        [Header("Player Marker")]
        [SerializeField] private Color playerMarkerColor = Color.red;

        [Header("Interactive Colors (Future)")]
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color maskedColor = new Color(1f, 1f, 1f, 0.3f); // Semi-transparent white

        public Color EmptyColor => emptyColor;
        public Color FloorColor => floorColor;
        public Color WallColor => wallColor;
        public Color PlayerMarkerColor => playerMarkerColor;
        public Color HighlightColor => highlightColor;
        public Color MaskedColor => maskedColor;

        /// <summary>
        /// Get color based on cell type
        /// </summary>
        public Color GetColorForCellType(CellType cellType)
        {
            return cellType switch
            {
                CellType.Floor => floorColor,
                CellType.Wall => wallColor,
                CellType.Empty => emptyColor,
                _ => emptyColor
            };
        }
    }
}
