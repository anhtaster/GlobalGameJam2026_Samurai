using UnityEngine;
using UnityEngine.InputSystem;

namespace GlobalGameJam
{
    public class MinimapInteractionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MinimapGridModel gridModel;
        [SerializeField] private WallToggleService wallToggleService;
        [SerializeField] private MinimapGhostLayer ghostLayer;
        [SerializeField] private MapController mapController;
        [SerializeField] private ColorGroupController colorGroupController;
        
        [Header("Texture Mode")]
        [SerializeField] private MinimapTextureRenderer textureRenderer;
        [SerializeField] private RectTransform mapContainer;


        
        [Header("Legacy Mode (optional)")]
        [SerializeField] private MinimapGridView gridView;
        [SerializeField] private MinimapGridViewModel viewModel;
        
        [Header("Player Control")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private MonoBehaviour[] scriptsToDisable;

        [Header("Grid Navigation")]
        [SerializeField] private Vector2Int currentCursorPos;
        [SerializeField] private float navigationCooldown = 0.15f;
        private float nextMoveTime;

        [Header("Input Settings")]
        [SerializeField] private bool invertInputX = false;
        [SerializeField] private bool invertInputY = false;

        [Header("Pause Panel")]
        [SerializeField] private GameObject pausePanel; 
        private bool isPaused = false;

        [Header("Setting Panel")]
        [SerializeField] private SettingPanelController settingController;
        private bool isSetting = false;

        private bool isMapMode = false;
        private bool useTextureMode = false;

        private void Start()
        {
            if (mapController == null)
                mapController = GetComponent<MapController>();

            if (mapController == null)
                mapController = GetComponentInChildren<MapController>();

            if (mapController == null)
                mapController = GetComponentInParent<MapController>();

            if (mapController == null)
                mapController = FindFirstObjectByType<MapController>();

            if (colorGroupController == null)
                colorGroupController = FindFirstObjectByType<ColorGroupController>();

            useTextureMode = textureRenderer != null;

            if (settingController == null)
            {
                settingController = GetComponentInChildren<SettingPanelController>(true);
            }

            
            if (ghostLayer != null)
                ghostLayer.SetVisible(false);

            if (playerInput == null && (scriptsToDisable == null || scriptsToDisable.Length == 0))
            {
            }

            
            // Initialize cursor to center of grid
            if (gridModel != null)
            {
                currentCursorPos = new Vector2Int(gridModel.GridWidth / 2, gridModel.GridHeight / 2);
            }
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (!isSetting && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetPauseMode(!isPaused);
            }

            if (isPaused || isSetting) return;
            HandleInput();
        }

        public void SetPauseMode(bool enabled)
        {
            if (isPaused == enabled) return;
            isPaused = enabled;

            // Cập nhật biến static để Script Menu biết đường mà chạy
            PausePanelController.IsPaused = enabled; 

            if (pausePanel != null) pausePanel.SetActive(isPaused);

            if (isPaused)
            {
                if (playerInput != null) playerInput.DeactivateInput();
                foreach (var script in scriptsToDisable) if (script != null) script.enabled = false;
                Time.timeScale = 0f;

                // Ép Menu cập nhật vị trí mũi tên ngay khi vừa mở
                var menu = pausePanel.GetComponent<PausePanelController>();
                if (menu != null) menu.ResetMenu(); 
            }
            else
            {
                if (playerInput != null) playerInput.ActivateInput();
                foreach (var script in scriptsToDisable) if (script != null) script.enabled = true;
                Time.timeScale = 1f;
            }
}

        private void HandleInput()
        {
            if (Keyboard.current != null)
            {
                // Toggle on each press
                if (Keyboard.current.mKey.wasPressedThisFrame)
                {
                    SetMapMode(!isMapMode);
                }

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    if (isMapMode)
                        TryToggleWall();
                }
            }

            if (!isMapMode) return;

            // Navigation (WASD)
            if (Time.time >= nextMoveTime)
            {
                Vector2 inputVector = Vector2.zero;

                if (Keyboard.current != null)
                {
                    if (Keyboard.current.upArrowKey.isPressed) inputVector.y = 1;
                    else if (Keyboard.current.downArrowKey.isPressed) inputVector.y = -1;
                    
                    if (Keyboard.current.rightArrowKey.isPressed) inputVector.x = 1;
                    else if (Keyboard.current.leftArrowKey.isPressed) inputVector.x = -1;
                }

                // Block movement if walls already hidden
                if (inputVector != Vector2.zero && wallToggleService != null && wallToggleService.IsRegionActive)
                {
                    return;
                }

                if (inputVector != Vector2.zero)
                {
                    int moveX = Mathf.RoundToInt(inputVector.x);
                    int moveY = Mathf.RoundToInt(inputVector.y);

                    if (invertInputX) moveX = -moveX;
                    if (invertInputY) moveY = -moveY;
                    
                    if (moveX != 0 || moveY != 0)
                    {
                        MoveCursor(new Vector2Int(moveX, moveY));
                        nextMoveTime = Time.time + navigationCooldown;
                    }
                }
            }

        }

        private void SetMapMode(bool enabled)
        {
            if (isMapMode == enabled) return;
            isMapMode = enabled;

            if (isMapMode)
            {
                if (mapController != null) mapController.OpenMapExternal();
                if (playerInput != null) playerInput.DeactivateInput();
                foreach (var script in scriptsToDisable) if (script != null) script.enabled = false;
                
                if (ghostLayer != null)
                {
                    ghostLayer.SetVisible(true);
                    UpdateCursorVisuals();
                }
            }
            else
            {
                if (mapController != null) mapController.CloseMapExternal();
                if (playerInput != null) playerInput.ActivateInput();
                foreach (var script in scriptsToDisable) if (script != null) script.enabled = true;

                if (ghostLayer != null)
                {
                    ghostLayer.SetVisible(false);
                }
            }
        }

        private void MoveCursor(Vector2Int delta)
        {
            if (gridModel == null) 
            {
                return;
            }

            Vector2Int newPos = currentCursorPos + delta;
            
            // Constrain to grid bounds using GridModel
            if (newPos.x < 0 || newPos.x >= gridModel.GridWidth ||
                newPos.y < 0 || newPos.y >= gridModel.GridHeight)
            {
                return;
            }

            currentCursorPos = newPos;
            UpdateCursorVisuals();
        }

        private void UpdateCursorVisuals()
        {
            if (ghostLayer == null) return;

            if (useTextureMode && mapContainer != null && gridModel != null)
            {
                // Texture mode: calculate cell size from container
                float cellSize = mapContainer.rect.width / gridModel.GridWidth;
                ghostLayer.UpdateCursorPosition(currentCursorPos, cellSize);
            }
            else if (gridView != null)
            {
                // Legacy mode
                ghostLayer.UpdateCursorPosition(currentCursorPos, gridView.CellUISize);
            }
        }

        private void TryToggleWall()
        {
            if (wallToggleService == null || ghostLayer == null || gridModel == null) return;

            // In texture mode, cursor position IS the world position
            Vector2Int worldPos = currentCursorPos;

            bool success = wallToggleService.ToggleRegion(worldPos, ghostLayer.MaskSize);
            
            if (success)
            {
                ghostLayer.SetState(wallToggleService.IsRegionActive);
            }
        }

        // private void ToggleSettingMode()
        // {
        //     isSetting = !isSetting;
            
        //     // Gọi hàm thực thi bên script Setting
        //     if (settingController != null)
        //     {
        //         // Chúng ta sẽ sửa hàm ToggleSettings bên kia để nhận tham số trực tiếp
        //         settingController.SetPanelActive(isSetting);
        //     }

        //     // Khóa/Mở khóa các script điều khiển người chơi & góc nhìn
        //     if (isSetting)
        //     {
        //         if (playerInput != null) playerInput.DeactivateInput();
        //         foreach (var script in scriptsToDisable) if (script != null) script.enabled = false;
                
        //         Time.timeScale = 0f;
        //         Cursor.lockState = CursorLockMode.None;
        //         Cursor.visible = true;
        //     }
        //     else
        //     {
        //         // Chỉ kích hoạt lại nếu cũng không bị Pause
        //         if (!isPaused)
        //         {
        //             if (playerInput != null) playerInput.ActivateInput();
        //             foreach (var script in scriptsToDisable) if (script != null) script.enabled = true;
                    
        //             Time.timeScale = 1f;
        //             Cursor.lockState = CursorLockMode.Locked;
        //             Cursor.visible = false;
        //         }
        //     }
        // }

        private void ToggleSettingMode()
        {
            isSetting = !isSetting;
            
            if (settingController != null)
            {
                settingController.SetPanelActive(isSetting);
            }

            // Quan trọng: Khi đang Setting, phải chặn HandleInput() của Gameplay
            if (isSetting)
            {
                if (playerInput != null) playerInput.DeactivateInput();
                Time.timeScale = 0f;
            }
        }

        public void EnterSettingFromPause()
        {
            isSetting = true;   // Kích hoạt chế độ Setting
            isPaused = false;    // Tạm thời tắt cờ IsPaused để logic Update của Controller không nhận Esc nữa
            
            if (pausePanel != null) pausePanel.SetActive(false);
            
            if (settingController != null)
            {
                // Gọi SetPanelActive(true) để bật cả GameObject và biến isOpen bên kia
                settingController.SetPanelActive(true);
            }
        }

        public void ExitSettingToPause()
        {
            isSetting = false; // Tắt cờ Setting
            isPaused = true;   // Bật lại cờ Pause để phím Esc hoạt động lại

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                // Reset lại vị trí mũi tên ở Menu Pause
                var menu = pausePanel.GetComponent<PausePanelController>();
                if (menu != null) menu.ResetMenu();
            }
        }
    }
}

