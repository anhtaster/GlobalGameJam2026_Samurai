using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Makes a platform invisible but still physically there (collidable)
    /// Attach this to each platform or platform group
    /// The platform will only be visible when wearing matching colored glasses
    /// </summary>
    public class ColoredPlatform : MonoBehaviour
    {
        [Header("Platform Color")]
        [Tooltip("Which color glasses will make this platform visible")]
        public GlassColor platformColor = GlassColor.Red;

        [Header("Visibility Settings")]
        [SerializeField] private bool startVisible = false;
        [Tooltip("If true, platform is visible at start. If false, invisible until glasses worn.")]

        private Renderer[] renderers;
        private bool isCurrentlyVisible;

        void Awake()
        {
            // Get all renderers on this platform and its children
            renderers = GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                Debug.LogWarning($"[ColoredPlatform] No renderers found on {gameObject.name}! Platform won't be visible.");
            }

            // Set initial visibility
            SetVisibility(startVisible);
        }

        /// <summary>
        /// Show this platform (make it visible)
        /// </summary>
        public void Show()
        {
            SetVisibility(true);
        }

        /// <summary>
        /// Hide this platform (make it invisible but still collidable)
        /// </summary>
        public void Hide()
        {
            SetVisibility(false);
        }

        /// <summary>
        /// Set visibility state
        /// </summary>
        private void SetVisibility(bool visible)
        {
            isCurrentlyVisible = visible;

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }

            Debug.Log($"[ColoredPlatform] {gameObject.name} ({platformColor}) is now {(visible ? "VISIBLE" : "INVISIBLE")}");
        }

        /// <summary>
        /// Check if this platform is currently visible
        /// </summary>
        public bool IsVisible => isCurrentlyVisible;

        /// <summary>
        /// Get the color of this platform
        /// </summary>
        public GlassColor PlatformColor => platformColor;
    }
}