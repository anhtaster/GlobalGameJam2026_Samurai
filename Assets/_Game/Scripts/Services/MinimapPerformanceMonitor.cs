using UnityEngine;
using TMPro;

namespace GlobalGameJam
{
    /// <summary>
    /// Optional UI display for minimap performance metrics
    /// Attach to UI Text to show real-time performance
    /// </summary>
    public class MinimapPerformanceMonitor : MonoBehaviour
    {
        [SerializeField] private MinimapGridViewModel viewModel;
        [SerializeField] private TextMeshProUGUI performanceText;
        [SerializeField] private float updateInterval = 1f;

        private float timer;

        private void Update()
        {
            if (viewModel == null || performanceText == null)
                return;

            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                performanceText.text = $"Minimap: {viewModel.GetPerformanceStats()}";
                timer = 0f;
            }
        }
    }
}
