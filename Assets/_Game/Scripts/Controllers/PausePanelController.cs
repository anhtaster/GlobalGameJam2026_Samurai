using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Nếu bạn dùng TextMeshPro

public class PausePanelController : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("UI Elements")]
    public RectTransform[] menuOptions; // Kéo 3 cái Text vào đây
    public RectTransform leftArrow;     // Kéo mũi tên bên trái vào đây
    public RectTransform rightArrow;    // Kéo mũi tên bên phải vào đây

    [Header("Panels")]
    public GameObject pausePanel;    // Panel Pause hiện tại
    public GameObject settingsPanel; // Kéo Setting Panel vào đây
    public GameObject mainMenuPanel;
    
    [Header("Settings")]
    public float padding = 10f; // Khoảng cách từ mũi tên đến mép chữ
    
    private int currentIndex = 0;

    public static bool IsPaused = false;

    void Start()
    {
        UpdateArrows();
    }

    void Update()
    {
        // Quan trọng: Kiểm tra IsPaused static
        if (!IsPaused || Keyboard.current == null) return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex - 1 + menuOptions.Length) % menuOptions.Length;
            UpdateArrows();
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex + 1) % menuOptions.Length;
            UpdateArrows();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ConfirmSelection();
        }
    }

    // Thêm hàm này để MinimapInteractionController gọi tới
    public void ResetMenu()
    {
        Debug.Log("[PausePanelController] ResetMenu() called");
        IsPaused = true; 
        currentIndex = 0;
        this.gameObject.SetActive(true); // Đảm bảo Panel hiện lên
        UpdateArrows();
        Debug.Log("[PausePanelController] Pause menu reset and active");
    }

    void UpdateArrows()
    {
        if (menuOptions == null || menuOptions.Length == 0) return;

        for (int i = 0; i < menuOptions.Length; i++)
        {
            var tmpText = menuOptions[i].GetComponent<TextMeshProUGUI>();
            if (i == currentIndex)
            {
                // Mục được chọn: Sáng và To
                if (tmpText != null) tmpText.color = selectedColor;
                menuOptions[i].localScale = Vector3.one * 1.2f;

                // Di chuyển mũi tên (giữ nguyên logic cũ của bạn)
                float finalWidth = (tmpText != null ? tmpText.preferredWidth : menuOptions[i].rect.width) * menuOptions[i].localScale.x;
                leftArrow.localPosition = new Vector3(menuOptions[i].localPosition.x - (finalWidth / 2f) - padding, menuOptions[i].localPosition.y, 0);
                rightArrow.localPosition = new Vector3(menuOptions[i].localPosition.x + (finalWidth / 2f) + padding, menuOptions[i].localPosition.y, 0);
            }
            else
            {
                // Mục không được chọn: Mờ và Nhỏ
                if (tmpText != null) tmpText.color = unselectedColor;
                menuOptions[i].localScale = Vector3.one;
            }
        }
    }

    void ConfirmSelection()
    {
        Debug.Log($"[PausePanelController] ConfirmSelection() called, currentIndex={currentIndex}");
        var master = FindFirstObjectByType<GlobalGameJam.MinimapInteractionController>();
        if (master == null) return;

        if (currentIndex == 0) // Resume
        {
            Debug.Log("[PausePanelController] Resume selected");
            master.SetPauseMode(false);
        }
        else if (currentIndex == 1) // Setting
        {
            Debug.Log("[PausePanelController] Setting selected from Pause");
            // Gọi UIController để mở Setting từ Pause
            UIController.Instance.OpenSettings(true);
        }
        else if (currentIndex == 2) // Main Menu
        {
            Debug.Log("[PausePanelController] Main Menu selected from Pause");
            master.ReturnToMainMenu(); // Gọi hàm reset ở Master
            this.gameObject.SetActive(false); 
            if (mainMenuPanel != null) 
            {
                mainMenuPanel.SetActive(true);
                var mainMenuCtrl = mainMenuPanel.GetComponent<MainMenuController>();
                if (mainMenuCtrl != null) mainMenuCtrl.SetMenuActive(true);
            }
        }
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        pausePanel.SetActive(IsPaused);
        
        // Nếu thoát Pause, đảm bảo các panel con cũng đóng
        if (!IsPaused)
        {
            settingsPanel.SetActive(false);
            mainMenuPanel.SetActive(false);
        }

        Time.timeScale = IsPaused ? 0f : 1f;
    }
}