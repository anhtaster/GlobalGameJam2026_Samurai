using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

namespace GlobalGameJam
{
    /// <summary>
    /// Controller: Manages visibility of ColoredPlatform objects based on glasses color
    /// Automatically finds all ColoredPlatform components in the scene
    /// </summary>
    public class ColorGroupController : MonoBehaviour
    {
        [Header("Model Reference")]
        [SerializeField] private ColorBlockModel model;

        [Header("Auto-Find Platforms")]
        [SerializeField] private bool autoFindPlatforms = true;
        [Tooltip("Automatically find all ColoredPlatform components in scene")]

        [Header("Manual Platform Assignment (Optional)")]
        [SerializeField] private List<ColoredPlatform> manualPlatformList = new List<ColoredPlatform>();
        [Tooltip("Only used if Auto Find Platforms is disabled")]

        [Header("Minimap References")]
        [SerializeField] private MinimapTextureRenderer minimapTextureRenderer;
        [SerializeField] private GridScanner gridScanner;

        [Header("Glasses Integration")]
        [SerializeField] private GlassesController glassesController;

        [Header("Behavior Mode")]
        [SerializeField] private ColorBlockMode blockMode = ColorBlockMode.GlassColorMatch;
        [Tooltip("GlassColorMatch: Platforms automatically match glass color\nManualToggle: Use 1/2/3 keys to manually toggle platforms")]

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private List<ColoredPlatform> allPlatforms = new List<ColoredPlatform>();
        private ColorBlockViewModel viewModel;
        private GlassColor currentGlassColor = GlassColor.Red;

        void Start()
        {
            // Find all colored platforms
            FindAllPlatforms();

            // Hide all platforms initially
            HideAllPlatforms();

            // Find GlassesController if not assigned
            if (glassesController == null)
            {
                glassesController = FindFirstObjectByType<GlassesController>();
                if (glassesController == null)
                {
                    Debug.LogWarning("[ColorGroupController] GlassesController not found!");
                }
            }

            // Create ViewModel from Model if available
            if (model != null)
            {
                viewModel = new ColorBlockViewModel(model);
                viewModel.OnColorBlockChanged += HandleColorBlockChanged;
                viewModel.OnAllBlocksHidden += HandleAllBlocksHidden;
            }
        }

        void OnDestroy()
        {
            if (viewModel != null)
            {
                viewModel.OnColorBlockChanged -= HandleColorBlockChanged;
                viewModel.OnAllBlocksHidden -= HandleAllBlocksHidden;
            }
        }

        void Update()
        {
            // Only handle manual input in ManualToggle mode
            if (blockMode == ColorBlockMode.ManualToggle)
            {
                HandleInput();
            }
        }

        /// <summary>
        /// Find all ColoredPlatform components in the scene
        /// </summary>
        private void FindAllPlatforms()
        {
            allPlatforms.Clear();

            if (autoFindPlatforms)
            {
                // Find all ColoredPlatform components in scene
                ColoredPlatform[] foundPlatforms = FindObjectsByType<ColoredPlatform>(FindObjectsSortMode.None);
                allPlatforms.AddRange(foundPlatforms);

                if (showDebugLogs)
                {
                    int redCount = allPlatforms.Count(p => p.PlatformColor == GlassColor.Red);
                    int greenCount = allPlatforms.Count(p => p.PlatformColor == GlassColor.Green);
                    int blueCount = allPlatforms.Count(p => p.PlatformColor == GlassColor.Blue);
                    
                    Debug.Log($"[ColorGroupController] Auto-found {allPlatforms.Count} platforms: " +
                             $"{redCount} Red, {greenCount} Green, {blueCount} Blue");
                }
            }
            else
            {
                // Use manually assigned list
                allPlatforms.AddRange(manualPlatformList);

                if (showDebugLogs)
                {
                    Debug.Log($"[ColorGroupController] Using {allPlatforms.Count} manually assigned platforms");
                }
            }
        }

        /// <summary>
        /// Handle keyboard input for manual platform selection
        /// </summary>
        private void HandleInput()
        {
            if (viewModel == null) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Must be wearing glasses
            if (glassesController == null || !glassesController.IsWearingGlasses)
            {
                return;
            }

            // Number keys 1, 2, 3 for platform selection
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                viewModel.SelectBlock(1);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                viewModel.SelectBlock(2);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                viewModel.SelectBlock(3);
            }
        }

        /// <summary>
        /// Handle color block changes from ViewModel (ManualToggle mode)
        /// </summary>
        private void HandleColorBlockChanged(int activeBlock)
        {
            if (blockMode == ColorBlockMode.GlassColorMatch)
            {
                return; // Controlled by glass color instead
            }

            // Hide all first
            HideAllPlatforms();

            // Show only the active color
            GlassColor colorToShow = activeBlock switch
            {
                1 => GlassColor.Red,
                2 => GlassColor.Green,
                3 => GlassColor.Blue,
                _ => GlassColor.Red
            };

            if (activeBlock > 0)
            {
                ShowPlatformsOfColor(colorToShow);
            }

            RefreshMinimap();
        }

        /// <summary>
        /// Handle all blocks hidden event
        /// </summary>
        private void HandleAllBlocksHidden()
        {
            HideAllPlatforms();
            RefreshMinimap();
        }

        /// <summary>
        /// Show platforms of a specific color
        /// </summary>
        private void ShowPlatformsOfColor(GlassColor color)
        {
            int count = 0;
            foreach (ColoredPlatform platform in allPlatforms)
            {
                if (platform != null && platform.PlatformColor == color)
                {
                    platform.Show();
                    count++;
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[ColorGroupController] Showing {count} {color} platforms");
            }
        }

        /// <summary>
        /// Hide all platforms
        /// </summary>
        private void HideAllPlatforms()
        {
            foreach (ColoredPlatform platform in allPlatforms)
            {
                if (platform != null)
                {
                    platform.Hide();
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[ColorGroupController] All platforms hidden");
            }
        }

        /// <summary>
        /// Refresh minimap
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

        // ========== PUBLIC API: Called by GlassesController ==========

        /// <summary>
        /// Called when glasses are put on with specific color
        /// </summary>
        public void OnGlassesPutOn(GlassColor glassColor)
        {
            currentGlassColor = glassColor;

            if (showDebugLogs)
            {
                Debug.Log($"[ColorGroupController] Glasses ON - Color: {glassColor}, Mode: {blockMode}");
            }

            if (blockMode == ColorBlockMode.GlassColorMatch)
            {
                // Automatically show platforms matching glass color
                HideAllPlatforms();
                ShowPlatformsOfColor(glassColor);
            }
            else if (blockMode == ColorBlockMode.ManualToggle)
            {
                // Restore previously saved state
                if (viewModel != null)
                {
                    viewModel.RestoreSavedState();
                }
            }

            RefreshMinimap();
        }

        /// <summary>
        /// Backward compatibility
        /// </summary>
        public void OnGlassesPutOn()
        {
            GlassColor color = glassesController != null ? glassesController.CurrentGlassColor : GlassColor.Red;
            OnGlassesPutOn(color);
        }

        /// <summary>
        /// Called when glasses are taken off
        /// </summary>
        public void OnGlassesPutOff()
        {
            if (showDebugLogs)
            {
                Debug.Log("[ColorGroupController] Glasses OFF - hiding all platforms");
            }

            HideAllPlatforms();

            if (viewModel != null)
            {
                viewModel.HideAllBlocks();
            }

            RefreshMinimap();
        }

        /// <summary>
        /// Called when glass color changes while wearing glasses
        /// </summary>
        public void OnGlassColorChanged(GlassColor newColor)
        {
            currentGlassColor = newColor;

            if (showDebugLogs)
            {
                Debug.Log($"[ColorGroupController] Glass color changed to {newColor}");
            }

            if (blockMode == ColorBlockMode.GlassColorMatch)
            {
                // Automatically switch to matching platforms
                HideAllPlatforms();
                ShowPlatformsOfColor(newColor);
                RefreshMinimap();
            }
        }

        /// <summary>
        /// Manual refresh - useful for debugging
        /// </summary>
        [ContextMenu("Refresh Platform List")]
        public void RefreshPlatformList()
        {
            FindAllPlatforms();
            Debug.Log($"[ColorGroupController] Refreshed: Found {allPlatforms.Count} platforms");
        }
    }

    public enum ColorBlockMode
    {
        GlassColorMatch,
        ManualToggle
    }
}