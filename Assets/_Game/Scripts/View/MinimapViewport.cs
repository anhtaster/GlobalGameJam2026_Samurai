using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// View component: applies viewport data from ViewModel to the grid UI
    /// </summary>
    public class MinimapViewport : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridViewModel viewModel;
        [SerializeField] private MinimapGridView gridView;

        private int currentWidth = -1;
        private int currentHeight = -1;

        private void OnEnable()
        {
            if (viewModel == null)
            {
                viewModel = FindFirstObjectByType<MinimapGridViewModel>();
            }

            if (gridView == null)
            {
                gridView = GetComponentInParent<MinimapGridView>();
            }

            if (viewModel == null || gridView == null)
            {
                Debug.LogError("[MinimapViewport] Missing ViewModel or GridView reference!");
                return;
            }

            viewModel.ViewportDataChanged += ApplyViewportData;
            viewModel.ForceRefresh();
        }

        private void OnDisable()
        {
            if (viewModel != null)
            {
                viewModel.ViewportDataChanged -= ApplyViewportData;
            }
        }
        private void ApplyViewportData(MinimapViewportData data)
        {
            if (gridView == null || data == null)
                return;

            gridView.UpdateCells(data);
            currentWidth = data.Width;
            currentHeight = data.Height;
        }
    }
}
