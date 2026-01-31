using UnityEngine;
using UnityEngine.InputSystem;

namespace GlobalGameJam
{
    public class MinimapInteractionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridView gridView;
        [SerializeField] private MinimapGridViewModel viewModel;
        [SerializeField] private WallToggleService wallToggleService;
        [SerializeField] private MinimapGhostLayer ghostLayer;
        
        [Header("Player Control")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private MonoBehaviour[] scriptsToDisable;

        [Header("Grid Navigation")]
        [SerializeField] private Vector2Int currentCursorPos;
        [SerializeField] private float navigationCooldown = 0.15f;
        private float nextMoveTime;

        private bool isMapMode = false;

        private void Start()
        {
            if (ghostLayer != null)
                ghostLayer.SetVisible(false);

            if (playerInput == null && (scriptsToDisable == null || scriptsToDisable.Length == 0))
            {
                Debug.LogWarning("[MinimapInteractionController] WARNING: 'Player Input' and 'Scripts To Disable' are empty! The player will NOT stop moving in Map Mode. Please assign them in the Inspector.");
            }
        }

        private void Update()
        {
            HandleInput();
        }

        [Header("Input Settings")]
        [SerializeField] private bool invertInputX = false;
        [SerializeField] private bool invertInputY = false;

        private void HandleInput()
        {
            // Toggle Map Mode
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                ToggleMapMode();
            }

            if (!isMapMode) return;

            // Navigation (WASD)
            if (Time.time >= nextMoveTime)
            {
                Vector2 inputVector = Vector2.zero;

                if (Keyboard.current != null)
                {
                     // Standard WASD mapping to Screen Vectors
                     // W = Up (+Y), S = Down (-Y)
                     // D = Right (+X), A = Left (-X)
                    if (Keyboard.current.wKey.isPressed) inputVector.y = 1;
                    else if (Keyboard.current.sKey.isPressed) inputVector.y = -1;
                    
                    if (Keyboard.current.dKey.isPressed) inputVector.x = 1;
                    else if (Keyboard.current.aKey.isPressed) inputVector.x = -1;
                }

                // Don't allow movement if current region is active (walls already hidden)
                // But still allow E key to toggle it off!
                if (inputVector != Vector2.zero && wallToggleService != null && wallToggleService.IsRegionActive)
                {
                    // Block WASD movement when in "red" state - must press E first to deselect
                    return;
                }

                if (inputVector != Vector2.zero)
                {
                    // Direct screen-relative movement (no rotation)
                    // W = Up, S = Down, A = Left, D = Right
                    // This works regardless of map rotation
                    int moveX = Mathf.RoundToInt(inputVector.x);
                    int moveY = Mathf.RoundToInt(inputVector.y);

                    // Apply Inversions if needed
                    if (invertInputX) moveX = -moveX;
                    if (invertInputY) moveY = -moveY;

                    Debug.Log($"[MinimapInteraction] Input: {inputVector}, Final Move: ({moveX}, {moveY})");
                    
                    if (moveX != 0 || moveY != 0)
                    {
                        MoveCursor(new Vector2Int(moveX, moveY));
                        nextMoveTime = Time.time + navigationCooldown;
                    }
                }
            }

            // Action (E)
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryToggleWall();
            }
        }

        private void ToggleMapMode()
        {
            isMapMode = !isMapMode;

            Debug.Log($"[MinimapInteractionController] Map Mode: {isMapMode}");

            if (isMapMode)
            {
                // Enter Map Mode
                // Disable Player Input
                if (playerInput != null) playerInput.DeactivateInput();
                foreach (var script in scriptsToDisable) if (script != null) script.enabled = false;
                
                // Show Cursor
                if (ghostLayer != null)
                {
                    ghostLayer.SetVisible(true);
                    UpdateCursorVisuals();
                }
            }
            else
            {
                // Exit Map Mode
                // Enable Player Input
                if (playerInput != null) playerInput.ActivateInput();
                foreach (var script in scriptsToDisable) if (script != null) script.enabled = true;

                // Hide Cursor
                if (ghostLayer != null)
                {
                    ghostLayer.SetVisible(false);
                }
            }
        }

        private void MoveCursor(Vector2Int delta)
        {
            if (gridView == null) 
            {
                Debug.LogError("[MinimapInteractionController] GridView is NULL!");
                return;
            }

            Vector2Int newPos = currentCursorPos + delta;
            
            // Constrain to grid bounds
            // Check GridView dimensions first (works for viewport mode)
            if (newPos.x < 0 || newPos.x >= gridView.CurrentWidth ||
                newPos.y < 0 || newPos.y >= gridView.CurrentHeight)
            {
                Debug.Log($"[MinimapInteractionController] Move Blocked: {newPos} is out of bounds. Grid Size: {gridView.CurrentWidth}x{gridView.CurrentHeight}");
                return;
            }

            currentCursorPos = newPos;
            Debug.Log($"[MinimapInteractionController] Moved to {currentCursorPos}");
            UpdateCursorVisuals();
        }

        private void UpdateCursorVisuals()
        {
            if (ghostLayer != null && gridView != null)
            {
                ghostLayer.UpdateCursorPosition(currentCursorPos, gridView.CellUISize);
            }
        }

        private void TryToggleWall()
        {
            if (wallToggleService == null || ghostLayer == null || gridView == null) return;

            // Convert Local Viewport Position (currentCursorPos) to World Grid Position
            MinimapCellView cell = gridView.GetCellView(currentCursorPos);
            
            if (cell != null)
            {
                Vector2Int worldPos = cell.WorldGridPosition;
                Debug.Log($"[MinimapInteractionController] Toggling Region. Cursor: {currentCursorPos} -> World: {worldPos}");

                bool success = wallToggleService.ToggleRegion(worldPos, ghostLayer.MaskSize);
                
                if (success)
                {
                    // Update visual state (Green <-> Red)
                    ghostLayer.SetState(wallToggleService.IsRegionActive);
                }
            }
            else
            {
                Debug.LogWarning($"[MinimapInteractionController] Could not find CellView at cursor: {currentCursorPos}");
            }
        }
    }
}
