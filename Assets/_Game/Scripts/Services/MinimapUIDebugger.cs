using UnityEngine;
using UnityEngine.UI;

namespace GlobalGameJam
{
    public class MinimapUIDebugger : MonoBehaviour
    {
        [SerializeField] private MinimapGridView gridView;
        [SerializeField] private Canvas canvas;

        [ContextMenu("Debug UI Setup")]
        public void DebugUISetup()
        {
            Debug.Log("=== MINIMAP UI DEBUG ===");

            // Check Canvas
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas != null)
            {
                Debug.Log($"✅ Canvas found: {canvas.name}");
                Debug.Log($"   Render Mode: {canvas.renderMode}");
                Debug.Log($"   Canvas Scaler: {canvas.GetComponent<CanvasScaler>() != null}");
            }
            else
            {
                Debug.LogError("❌ No Canvas found!");
                return;
            }

            // Check GridView
            if (gridView == null)
            {
                Debug.LogError("❌ GridView not assigned!");
                return;
            }

            Debug.Log($"✅ GridView found: {gridView.name}");
            Debug.Log($"   Grid Model assigned: {gridView.GridModel != null}");
            Debug.Log($"   Color Config assigned: {gridView.ColorConfig != null}");

            // Check cell count
            int cellCount = gridView.transform.childCount;
            Debug.Log($"   Cell count in GridContainer: {cellCount}");

            if (cellCount == 0)
            {
                Debug.LogWarning("⚠️ No cells found! GridView.GenerateGrid() not called?");
            }
            else
            {
                Debug.Log($"✅ Found {cellCount} cells");

                // Check first cell
                Transform firstCell = gridView.transform.GetChild(0);
                Image cellImage = firstCell.GetComponent<Image>();
                MinimapCellView cellView = firstCell.GetComponent<MinimapCellView>();

                Debug.Log($"   First cell name: {firstCell.name}");
                Debug.Log($"   Has Image component: {cellImage != null}");
                Debug.Log($"   Has MinimapCellView: {cellView != null}");

                if (cellImage != null)
                {
                    Debug.Log($"   Image color: {cellImage.color}");
                    Debug.Log($"   Image enabled: {cellImage.enabled}");
                }
            }

            // Check GridLayoutGroup
            GridLayoutGroup gridLayout = gridView.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                Debug.Log($"✅ GridLayoutGroup found");
                Debug.Log($"   Cell Size: {gridLayout.cellSize}");
                Debug.Log($"   Constraint Count: {gridLayout.constraintCount}");
            }
            else
            {
                Debug.LogError("❌ GridLayoutGroup not found!");
            }
        }

        [ContextMenu("Force Generate Grid (12x12)")]
        public void ForceGenerateGrid()
        {
            if (gridView != null)
            {
                Debug.Log("Generating 12x12 grid...");
                gridView.GenerateGrid(12, 12);
            }
        }

        [ContextMenu("Test Cell Color")]
        public void TestCellColor()
        {
            if (gridView == null || gridView.ColorConfig == null)
            {
                Debug.LogError("GridView or ColorConfig not assigned!");
                return;
            }

            Debug.Log("=== COLOR CONFIG TEST ===");
            Debug.Log($"Floor Color: {gridView.ColorConfig.FloorColor}");
            Debug.Log($"Wall Color: {gridView.ColorConfig.WallColor}");
            Debug.Log($"Empty Color: {gridView.ColorConfig.EmptyColor}");
        }
    }
}
