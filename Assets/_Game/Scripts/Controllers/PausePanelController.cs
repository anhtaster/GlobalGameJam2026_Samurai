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

        // 1. Lấy mục đang chọn
        RectTransform selectedRect = menuOptions[currentIndex];
        
        // 2. Lấy component TextMeshPro để đo độ rộng thực tế của chữ
        var tmpText = selectedRect.GetComponent<TextMeshProUGUI>();
        float actualTextWidth;

        if (tmpText != null)
        {
            // preferredWidth lấy độ rộng thật của chữ, không tính phần trống của khung
            actualTextWidth = tmpText.preferredWidth;
        }
        else
        {
            actualTextWidth = selectedRect.rect.width;
        }

        // 3. Tính toán vị trí dựa trên Scale của chính nó
        float finalWidth = actualTextWidth * selectedRect.localScale.x;
        Vector3 targetPos = selectedRect.localPosition;

        // 4. Cập nhật vị trí mũi tên (Dùng padding để điều chỉnh khoảng cách xa gần tùy ý)
        leftArrow.localPosition = new Vector3(targetPos.x - (finalWidth / 2f) - padding, targetPos.y, targetPos.z);
        rightArrow.localPosition = new Vector3(targetPos.x + (finalWidth / 2f) + padding, targetPos.y, targetPos.z);
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