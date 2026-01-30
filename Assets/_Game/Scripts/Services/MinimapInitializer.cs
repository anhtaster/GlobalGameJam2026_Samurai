using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Call this from Loading Screen to initialize minimap before entering gameplay
    /// </summary>
    public class MinimapInitializer : MonoBehaviour
    {
        [SerializeField] private MinimapGridViewModel viewModel;

        /// <summary>
        /// Call this during loading screen
        /// </summary>
        public void InitializeMinimap()
        {
            if (viewModel != null)
            {
                viewModel.InitializeWithScan();
            }
            else
            {
                Debug.LogError("[MinimapInitializer] ViewModel not assigned!");
            }
        }

        /// <summary>
        /// For testing - call from Inspector or Start if no loading screen
        /// </summary>
        [ContextMenu("Initialize Now")]
        public void InitializeNow()
        {
            InitializeMinimap();
        }
    }
}
