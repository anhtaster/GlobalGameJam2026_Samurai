using UnityEngine;
using UnityEngine.InputSystem; // Bắt buộc cho Input System mới

namespace GlobalGameJam
{
    /// <summary>
    /// Controller: Handle input và view updates cho color blocks
    /// Follows MVVM pattern - delegates business logic to ViewModel
    /// </summary>
    public class ColorGroupController : MonoBehaviour
    {
        [Header("Model Reference")]
        [SerializeField] private ColorBlockModel model;

        [Header("Color Blocks (View)")]
        [SerializeField] private GameObject redGroup;   // Block 1
        [SerializeField] private GameObject greenGroup; // Block 2
        [SerializeField] private GameObject blueGroup;  // Block 3

        [Header("Minimap References")]
        [SerializeField] private MinimapTextureRenderer minimapTextureRenderer;
        [SerializeField] private GridScanner gridScanner;

        [Header("Glasses Integration")]
        [SerializeField] private GlassesController glassesController;

        private ColorBlockViewModel viewModel;

        void Start()
        {
            // Initialize all blocks as hidden
            if (redGroup) redGroup.SetActive(false);
            if (greenGroup) greenGroup.SetActive(false);
            if (blueGroup) blueGroup.SetActive(false);

            // Find GlassesController if not assigned
            if (glassesController == null)
            {
                glassesController = FindFirstObjectByType<GlassesController>();
                if (glassesController == null)
                {
                    Debug.LogWarning("[ColorGroupController] GlassesController not found! Color blocks will not be linked to glasses.");
                }
            }

            // Create ViewModel from Model
            if (model != null)
            {
                viewModel = new ColorBlockViewModel(model);
                
                // Subscribe to ViewModel events
                viewModel.OnColorBlockChanged += HandleColorBlockChanged;
                viewModel.OnAllBlocksHidden += HandleAllBlocksHidden;
            }
            else
            {
                Debug.LogError("[ColorGroupController] ColorBlockModel is not assigned! Please create a ColorBlockModel asset and assign it.");
            }

            // Subscribe to glasses events if available
            SubscribeToGlassesEvents();
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (viewModel != null)
            {
                viewModel.OnColorBlockChanged -= HandleColorBlockChanged;
                viewModel.OnAllBlocksHidden -= HandleAllBlocksHidden;
            }
        }

        void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// Handle keyboard input for color block selection
        /// Only works when wearing glasses
        /// </summary>
        private void HandleInput()
        {
            if (viewModel == null) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Check if wearing glasses - ONLY allow input when wearing glasses
            if (glassesController == null || !glassesController.IsWearingGlasses)
            {
                return; // Silently ignore input when not wearing glasses
            }

            // Number keys 1, 2, 3 for exclusive color block selection
            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                Debug.Log("[ColorGroupController] Key 1 pressed - Red block");
                viewModel.SelectBlock(1);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                Debug.Log("[ColorGroupController] Key 2 pressed - Green block");
                viewModel.SelectBlock(2);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                Debug.Log("[ColorGroupController] Key 3 pressed - Blue block");
                viewModel.SelectBlock(3);
            }
        }

        /// <summary>
        /// Subscribe to glasses events to hide/restore blocks
        /// </summary>
        private void SubscribeToGlassesEvents()
        {
            if (glassesController == null || viewModel == null) return;

            // We need to hook into glasses ViewModel events
            // For now, we'll poll in Update or use a different approach
            // Since GlassesController doesn't expose events, we'll handle in HandleGlassesStateChange
        }

        /// <summary>
        /// Handle color block changes from ViewModel
        /// Updates view (GameObjects) based on active block
        /// </summary>
        private void HandleColorBlockChanged(int activeBlock)
        {
            // Hide all blocks first (exclusive selection)
            if (redGroup) redGroup.SetActive(false);
            if (greenGroup) greenGroup.SetActive(false);
            if (blueGroup) blueGroup.SetActive(false);

            // Show only the active block
            switch (activeBlock)
            {
                case 1:
                    if (redGroup) redGroup.SetActive(true);
                    Debug.Log("[ColorGroupController] Red block VISIBLE");
                    break;
                case 2:
                    if (greenGroup) greenGroup.SetActive(true);
                    Debug.Log("[ColorGroupController] Green block VISIBLE");
                    break;
                case 3:
                    if (blueGroup) blueGroup.SetActive(true);
                    Debug.Log("[ColorGroupController] Blue block VISIBLE");
                    break;
                case 0:
                default:
                    Debug.Log("[ColorGroupController] All blocks HIDDEN");
                    break;
            }

            // Refresh minimap
            RefreshMinimap();
        }

        /// <summary>
        /// Handle all blocks hidden event
        /// </summary>
        private void HandleAllBlocksHidden()
        {
            if (redGroup) redGroup.SetActive(false);
            if (greenGroup) greenGroup.SetActive(false);
            if (blueGroup) blueGroup.SetActive(false);

            Debug.Log("[ColorGroupController] All blocks hidden (glasses off)");
            RefreshMinimap();
        }

        /// <summary>
        /// Refresh minimap after color block changes
        /// </summary>
        private void RefreshMinimap()
        {
            if (gridScanner != null)
            {
                gridScanner.ScanScene();
            }

            if (minimapTextureRenderer != null)
            {
                minimapTextureRenderer.RenderFullMap();
            }
        }

        /// <summary>
        /// Public API: Called by GlassesController when glasses are put on
        /// </summary>
        public void OnGlassesPutOn()
        {
            if (viewModel != null)
            {
                viewModel.RestoreSavedState();
            }
        }

        /// <summary>
        /// Public API: Called by GlassesController when glasses are taken off
        /// </summary>
        public void OnGlassesPutOff()
        {
            if (viewModel != null)
            {
                viewModel.HideAllBlocks();
            }
        }
    }
}