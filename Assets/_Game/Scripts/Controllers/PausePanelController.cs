using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Nếu bạn dùng TextMeshPro

public class PausePanelController : MonoBehaviour
{
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
        if (!IsPaused || Keyboard.current == null) return;

        // Kiểm tra phím Lên (Mũi tên lên hoặc phím W)
        bool upPressed = Keyboard.current.upArrowKey.wasPressedThisFrame || 
                        (Keyboard.current.wKey != null && Keyboard.current.wKey.wasPressedThisFrame);

        // Kiểm tra phím Xuống (Mũi tên xuống hoặc phím S)
        bool downPressed = Keyboard.current.downArrowKey.wasPressedThisFrame || 
                        (Keyboard.current.sKey != null && Keyboard.current.sKey.wasPressedThisFrame);

        bool confirmPressed = Keyboard.current.enterKey.wasPressedThisFrame || 
                             Keyboard.current.spaceKey.wasPressedThisFrame;

        if (upPressed)
        {
            currentIndex = (currentIndex - 1 + menuOptions.Length) % menuOptions.Length;
            UpdateArrows();
        }
        else if (downPressed)
        {
            currentIndex = (currentIndex + 1) % menuOptions.Length;
            UpdateArrows();
        }

        if (confirmPressed)
        {
            ConfirmSelection();
        }
    }

    // Thêm hàm này để MinimapInteractionController gọi tới
    public void ResetMenu()
    {
        currentIndex = 0;
        UpdateArrows();
    }

    void UpdateArrows()
    {
        if (menuOptions == null || menuOptions.Length == 0) return;

        RectTransform selectedText = menuOptions[currentIndex];
        
        // Sử dụng localPosition để đồng bộ tọa độ trong cùng một cha (PausePanel)
        Vector3 targetPos = selectedText.localPosition;
        
        // Lấy chiều rộng thực tế của khung chữ
        float textWidth = selectedText.rect.width * selectedText.localScale.x;

        // Cập nhật vị trí cho 2 mũi tên kẹp hai bên
        leftArrow.localPosition = new Vector3(targetPos.x - (textWidth / 2f) - padding, targetPos.y, targetPos.z);
        rightArrow.localPosition = new Vector3(targetPos.x + (textWidth / 2f) + padding, targetPos.y, targetPos.z);
    }

    void ConfirmSelection()
    {
        // Index 0: Resume
        if (currentIndex == 0) 
        {
            TogglePause(); // Tắt pause và chạy tiếp game
        }
        // Index 1: Setting
        else if (currentIndex == 1) 
        {
            pausePanel.SetActive(false);    // Ẩn pause panel
            settingsPanel.SetActive(true);  // Hiện setting panel
            // Lưu ý: Vẫn giữ IsPaused = true và Time.timeScale = 0
        }
        // Index 2: Main Menu
        else if (currentIndex == 2) 
        {
            pausePanel.SetActive(false);
            mainMenuPanel.SetActive(true);
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