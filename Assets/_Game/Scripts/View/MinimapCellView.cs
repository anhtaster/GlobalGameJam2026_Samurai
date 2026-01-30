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

        private void Awake()
        {
            if (cellImage == null)
            {
                cellImage = GetComponent<Image>();
            }
        }

        public void Initialize(MinimapCellData data, MinimapColorConfig config)
        {
            cellData = data;
            colorConfig = config;

            UpdateVisual();
        }

        public void UpdateVisual()
        {
            if (cellData == null || colorConfig == null || cellImage == null)
                return;

            Color cellColor = colorConfig.GetColorForCellType(cellData.CellType);
            cellImage.color = cellColor;
            
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
    }
}
