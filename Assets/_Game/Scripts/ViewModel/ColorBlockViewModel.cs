using System;
using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// ViewModel: Business logic for color block management
    /// Handles exclusive selection and state persistence
    /// </summary>
    public class ColorBlockViewModel
    {
        // Events for view to subscribe
        public event Action<int> OnColorBlockChanged; // Parameter: active block index (0=none, 1=red, 2=green, 3=blue)
        public event Action OnAllBlocksHidden;

        // State
        private int currentActiveBlock = 0; // 0=none, 1=red, 2=green, 3=blue
        private int savedState = 0; // Saved state when glasses are taken off
        private readonly ColorBlockModel model;

        /// <summary>
        /// Current active block (0=none, 1=red, 2=green, 3=blue)
        /// </summary>
        public int CurrentActiveBlock => currentActiveBlock;

        public ColorBlockViewModel(ColorBlockModel model)
        {
            this.model = model;
            if (model != null)
            {
                currentActiveBlock = model.DefaultActiveBlock;
                savedState = currentActiveBlock;
            }
        }

        /// <summary>
        /// Select a specific color block (exclusive selection)
        /// </summary>
        /// <param name="blockIndex">1=red, 2=green, 3=blue</param>
        public void SelectBlock(int blockIndex)
        {
            if (blockIndex < 1 || blockIndex > 3)
            {
                Debug.LogWarning($"[ColorBlockViewModel] Invalid block index: {blockIndex}. Must be 1, 2, or 3.");
                return;
            }

            // Toggle behavior: if clicking same block, deselect it
            if (currentActiveBlock == blockIndex)
            {
                currentActiveBlock = 0;
                Debug.Log($"[ColorBlockViewModel] Deselected block {blockIndex}");
            }
            else
            {
                currentActiveBlock = blockIndex;
                Debug.Log($"[ColorBlockViewModel] Selected block {blockIndex}");
            }

            // Update saved state
            savedState = currentActiveBlock;

            // Notify view
            OnColorBlockChanged?.Invoke(currentActiveBlock);
        }

        /// <summary>
        /// Hide all blocks (called when glasses are taken off)
        /// Preserves current state for later restoration
        /// </summary>
        public void HideAllBlocks()
        {
            Debug.Log($"[ColorBlockViewModel] Hiding all blocks. Saved state: {currentActiveBlock}");
            savedState = currentActiveBlock;
            OnAllBlocksHidden?.Invoke();
        }

        /// <summary>
        /// Restore previously saved state (called when glasses are put back on)
        /// </summary>
        public void RestoreSavedState()
        {
            Debug.Log($"[ColorBlockViewModel] Restoring saved state: {savedState}");
            currentActiveBlock = savedState;
            OnColorBlockChanged?.Invoke(currentActiveBlock);
        }

        /// <summary>
        /// Get the saved state without changing current state
        /// </summary>
        public int GetSavedState() => savedState;
    }
}
