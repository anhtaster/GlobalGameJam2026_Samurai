using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GlobalGameJam
{
    public class MinimapGridView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapColorConfig colorConfig;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GridLayoutGroup gridLayout;
        [SerializeField] private MinimapGridModel gridModel;

        [Header("Settings")]
        [SerializeField] private float cellUISize = 30f;

        private Dictionary<Vector2Int, MinimapCellView> cellViews = new Dictionary<Vector2Int, MinimapCellView>();
        private int currentWidth = -1;
        private int currentHeight = -1;

        public MinimapGridModel GridModel => gridModel;
        public MinimapColorConfig ColorConfig => colorConfig;

        public int CurrentWidth { get; private set; }
        public int CurrentHeight { get; private set; }
        public float CellUISize => cellUISize;

        private void Awake()
        {
            if (gridLayout == null)
            {
                gridLayout = GetComponent<GridLayoutGroup>();
            }
        }

        /// <summary>
        /// Generate grid with custom size (for viewport mode)
        /// </summary>
        public void GenerateGrid(int width, int height)
        {
            if (cellPrefab == null)
            {
                Debug.LogError("[MinimapGridView] Cell Prefab is not assigned!");
                return;
            }

            if (currentWidth == width && currentHeight == height && cellViews.Count == width * height)
            {
                return;
            }

            // Clear existing cells
            ClearGrid();

            currentWidth = width;
            currentHeight = height;

            // Setup grid layout with custom size
            SetupGridLayout(width);

            CurrentWidth = width;
            CurrentHeight = height;

            // Create cell views with custom size
            // Important: Store cells by VISUAL position (viewX, viewY) not grid position
            // viewY=0 is TOP row, viewY=height-1 is BOTTOM row
            for (int viewY = 0; viewY < height; viewY++) // Top to bottom
            {
                for (int viewX = 0; viewX < width; viewX++) // Left to right
                {
                    CreateCellViewSimple(viewX, viewY);
                }
            }

            // Debug.Log($"[MinimapGridView] Generated {cellViews.Count} cell views ({width}x{height})");
        }

        public void EnsureGrid(int width, int height)
        {
            GenerateGrid(width, height);
        }

        public void UpdateCells(MinimapViewportData data)
        {
            if (data == null)
                return;

            if (currentWidth != data.Width || currentHeight != data.Height || cellViews.Count != data.Width * data.Height)
            {
                GenerateGrid(data.Width, data.Height);
            }

            foreach (var cellView in cellViews.Values)
            {
                cellView.SetVisualOverride(false, Color.white); 
            }

            // Batch update to reduce overhead
            for (int viewY = 0; viewY < data.Height; viewY++)
            {
                for (int viewX = 0; viewX < data.Width; viewX++)
                {
                    MinimapCellView cellView = GetCellView(viewX, viewY);
                    if (cellView != null)
                    {
                        CellType newType = data.GetCellType(viewX, viewY);
                        // Only update if cell type changed to reduce SetCellType calls
                        if (cellView.GetCellData()?.CellType != newType)
                        {
                            cellView.SetCellType(newType);
                        }
                    }
                }
            }
        }

        private void SetupGridLayout(int columnCount)
        {
            if (gridLayout != null)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = columnCount;
                gridLayout.cellSize = new Vector2(cellUISize, cellUISize);
                gridLayout.spacing = Vector2.zero;
                gridLayout.padding = new RectOffset(0, 0, 0, 0);
            }
            else
            {
                Debug.LogError("[MinimapGridView] GridLayout is NULL!");
            }
        }

        /// <summary>
        /// Create empty cell view (for viewport mode - data assigned later)
        /// </summary>
        private void CreateCellViewSimple(int x, int y)
        {
            Vector2Int gridPos = new Vector2Int(x, y);

            // Instantiate cell prefab
            GameObject cellObj = Instantiate(cellPrefab, transform);
            cellObj.name = $"ViewportCell_{x}_{y}";

            // Get and initialize cell view component
            MinimapCellView cellView = cellObj.GetComponent<MinimapCellView>();
            if (cellView == null)
            {
                cellView = cellObj.AddComponent<MinimapCellView>();
            }

            // Create empty cell data for now (viewport will update it)
            MinimapCellData emptyCellData = new MinimapCellData(gridPos, CellType.Empty);
            cellView.Initialize(emptyCellData, colorConfig);

            // Store reference
            cellViews[gridPos] = cellView;
        }

        public void ClearGrid()
        {
            foreach (var cellView in cellViews.Values)
            {
                if (cellView != null)
                {
                    Destroy(cellView.gameObject);
                }
            }

            cellViews.Clear();
            currentWidth = -1;
            currentHeight = -1;
        }

        public MinimapCellView GetCellView(Vector2Int gridPos)
        {
            cellViews.TryGetValue(gridPos, out MinimapCellView cellView);
            return cellView;
        }

        public MinimapCellView GetCellView(int x, int y)
        {
            return GetCellView(new Vector2Int(x, y));
        }

        public void UpdateCell(Vector2Int gridPos)
        {
            MinimapCellView cellView = GetCellView(gridPos);
            if (cellView != null)
            {
                cellView.UpdateVisual();
            }
        }

        public void UpdateAllCells()
        {
            foreach (var cellView in cellViews.Values)
            {
                if (cellView != null)
                {
                    cellView.UpdateVisual();
                }
            }
        }

        public void SetCellSize(float size)
        {
            cellUISize = size;
            if (gridLayout != null)
            {
                gridLayout.cellSize = new Vector2(size, size);
            }
        }

        private void OnDestroy()
        {
            ClearGrid();
        }

        public void SetCellColorOverride(Vector2Int gridPos, bool active, Color color)
        {
            // First, try direct lookup (Local Coordinate)
            MinimapCellView cellView = GetCellView(gridPos);
            
            // If not found, it might be a World Coordinate (from WallToggleService). 
            // Iterate to find the matching Viewport Cell.
            if (cellView == null)
            {
                foreach (var view in cellViews.Values)
                {
                    if (view.WorldGridPosition == gridPos)
                    {
                        cellView = view;
                        break;
                    }
                }
            }

            if (cellView != null)
            {
                cellView.SetVisualOverride(active, color);
            }
            else
            {
                // Commented out log to avoid spam if target is outside Viewport
                // Debug.LogWarning($"[MinimapGridView] SetCellColorOverride failed: No View found for {gridPos}");
            }
        }
    }
}
