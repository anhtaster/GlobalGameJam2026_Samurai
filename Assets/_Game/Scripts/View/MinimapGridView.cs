using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GlobalGameJam
{
    public class MinimapGridView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel gridModel;
        [SerializeField] private MinimapColorConfig colorConfig;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GridLayoutGroup gridLayout;

        [Header("Settings")]
        [SerializeField] private float cellUISize = 30f;

        private Dictionary<Vector2Int, MinimapCellView> cellViews = new Dictionary<Vector2Int, MinimapCellView>();

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
        /// Generate the minimap grid UI
        /// </summary>
        public void GenerateGrid()
        {
            if (gridModel == null)
            {
                Debug.LogError("[MinimapGridView] GridModel is not assigned!");
                return;
            }

            GenerateGrid(gridModel.GridWidth, gridModel.GridHeight);
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

            // Initialize grid model if assigned
            if (gridModel != null)
            {
                gridModel.Initialize();
            }

            // Clear existing cells
            ClearGrid();

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

            Debug.Log($"[MinimapGridView] Generated {cellViews.Count} cell views ({width}x{height})");
        }

        private void SetupGridLayout()
        {
            if (gridModel != null)
            {
                SetupGridLayout(gridModel.GridWidth);
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
        /// Create cell view with grid model data
        /// </summary>
        private void CreateCellView(int x, int y)
        {
            Vector2Int gridPos = new Vector2Int(x, y);
            MinimapCellData cellData = gridModel.GetCell(gridPos);

            if (cellData == null)
                return;

            // Instantiate cell prefab
            GameObject cellObj = Instantiate(cellPrefab, transform);
            cellObj.name = $"Cell_{x}_{y}";

            // Get and initialize cell view component
            MinimapCellView cellView = cellObj.GetComponent<MinimapCellView>();
            if (cellView == null)
            {
                cellView = cellObj.AddComponent<MinimapCellView>();
            }

            cellView.Initialize(cellData, colorConfig);

            // Store reference
            cellViews[gridPos] = cellView;
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
            MinimapCellView cellView = GetCellView(gridPos);
            if (cellView != null)
            {
                cellView.SetVisualOverride(active, color);
            }
        }
    }
}
