using UnityEngine;
using UnityEngine.InputSystem;

namespace GlobalGameJam
{
    public class MinimapInteractionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridView gridView;
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

                if (inputVector != Vector2.zero)
                {
                    // Apply Rotation Correction
                    // If the Grid is rotated, we need to rotate our input vector to match the Grid's local axes.
                    // Grid Delta = Inverse(GridRotation) * ScreenInput
                    
                    float gridRotationz = 0f;
                    if (gridView != null && gridView.transform.parent != null) 
                    {
                        // Try to find the rotation. MinimapGridViewModel rotates the "container".
                        // Logic: We need the Z rotation of the GridView itself (or its container).
                        // Let's approximate by checking GridView's LossyScale or Rotation?
                        // UI rotation is usually z-axis.
                        gridRotationz = gridView.transform.eulerAngles.z; 
                    }

                    // Rotate input by -gridRotationz
                    Quaternion rotation = Quaternion.Euler(0, 0, -gridRotationz);
                    Vector3 rotatedInput = rotation * new Vector3(inputVector.x, inputVector.y, 0);

                    // Round to nearest integer direction
                    int moveX = Mathf.RoundToInt(rotatedInput.x);
                    int moveY = Mathf.RoundToInt(rotatedInput.y);

                    // Apply Inversions
                    if (invertInputX) moveX = -moveX;
                    if (invertInputY) moveY = -moveY;

                    // IMPORTANT: Coordinate System Adjustment
                    // If Visual Up (Y+) corresponds to Grid Index DECREASE (Y-), we normally flip Y.
                    // But "Rotated Input" logic tries to align Screen Up to Grid Up. 
                    // Let's assume standard behavior first, and rely on Invert Flags for final tweaking.
                    
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
