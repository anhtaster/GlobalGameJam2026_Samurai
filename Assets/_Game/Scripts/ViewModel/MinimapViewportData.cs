using UnityEngine;

namespace GlobalGameJam
{
    public class MinimapViewportData
    {
        public int Width { get; }
        public int Height { get; }
        public RectInt Bounds { get; }
        public Vector2Int PlayerViewPosition { get; set; }
        public CellType PlayerCellType { get; set; }

        private readonly CellType[] cellTypes;

        public MinimapViewportData(int width, int height, RectInt bounds)
        {
            Width = width;
            Height = height;
            Bounds = bounds;
            cellTypes = new CellType[width * height];
            PlayerViewPosition = new Vector2Int(-1, -1);
            PlayerCellType = CellType.Empty;
        }

        public CellType GetCellType(int viewX, int viewY)
        {
            if (viewX < 0 || viewX >= Width || viewY < 0 || viewY >= Height)
                return CellType.Empty;

            return cellTypes[viewY * Width + viewX];
        }

        public void SetCellType(int viewX, int viewY, CellType type)
        {
            if (viewX < 0 || viewX >= Width || viewY < 0 || viewY >= Height)
                return;

            cellTypes[viewY * Width + viewX] = type;
        }
    }
}
