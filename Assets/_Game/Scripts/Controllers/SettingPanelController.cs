using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SettingPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsPanel;
    public TextMeshProUGUI[] menuItems; // Kéo các Text (Light, SFX, Music) vào đây
    public Slider[] sliders;           // Kéo các Slider tương ứng vào đây
    public Button[] actionButtons;

    [Header("Visual Settings")]
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(1f, 1f, 1f, 0.4f); // Làm mờ khi không chọn

    [Header("Control Settings")]
    public float sliderSpeed = 0.5f;

    private int currentIndex = 0;
    private bool isOpen = false;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        UpdateSelectionVisuals();
    }

    void Update()
    {
        if (!isOpen) return;

        // Tổng số mục: Sliders + Buttons
        int totalItems = menuItems.Length + actionButtons.Length;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex - 1 + totalItems) % totalItems;
            UpdateSelectionVisuals();
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex + 1) % totalItems;
            UpdateSelectionVisuals();
        }

        // Chỉ cho phép chỉnh Slider nếu đang chọn ở các index đầu
        if (currentIndex < menuItems.Length)
        {
            HandleSliderAdjustment();
        }
        
        // Nhấn Enter để thực hiện lệnh nếu đang chọn Button
        if (Keyboard.current.enterKey.wasPressedThisFrame && currentIndex >= menuItems.Length)
        {
            int buttonIndex = currentIndex - menuItems.Length;
            actionButtons[buttonIndex].onClick.Invoke();
        }
    }

    public void SetPanelActive(bool active)
    {
        isOpen = active;
        settingsPanel.SetActive(active);
        
        if (active) 
        {
            currentIndex = 0; // Reset về mục đầu tiên khi mở
            UpdateSelectionVisuals(); // Cập nhật màu sắc ngay lập tức
            
            // Đảm bảo Time.timeScale bằng 0 để khớp với logic Pause
            Time.timeScale = 0f; 
        }
    }

    void ToggleSettings()
    {
        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);

        // Đồng bộ với trạng thái Pause toàn cục của bạn
        PausePanelController.IsPaused = isOpen;
        
        // Dừng thời gian nếu mở Setting
        Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen) UpdateSelectionVisuals();
    }

    void HandleSliderAdjustment()
    {
        float move = 0;
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            move = -sliderSpeed * Time.unscaledDeltaTime;
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            move = sliderSpeed * Time.unscaledDeltaTime;

        if (move != 0)
        {
            sliders[currentIndex].value = Mathf.Clamp01(sliders[currentIndex].value + move);
        }
    }

    void UpdateSelectionVisuals()
    {
        // 1. Cập nhật cho Sliders/Text
        for (int i = 0; i < menuItems.Length; i++)
        {
            bool isSelected = (i == currentIndex);
            menuItems[i].color = isSelected ? selectedColor : unselectedColor;
            menuItems[i].transform.localScale = isSelected ? Vector3.one * 1.1f : Vector3.one;
        }

        // 2. Cập nhật cho Buttons (Credit/Quit)
        for (int i = 0; i < actionButtons.Length; i++)
        {
            int actualIndex = i + menuItems.Length;
            bool isSelected = (actualIndex == currentIndex);
            
            // Làm sáng nút bằng cách đổi màu ColorBlock của Button
            ColorBlock cb = actionButtons[i].colors;
            cb.normalColor = isSelected ? selectedColor : unselectedColor;
            actionButtons[i].colors = cb;
            
            // Hoặc phóng to nút
            actionButtons[i].transform.localScale = isSelected ? Vector3.one * 1.1f : Vector3.one;
        }
    }

    public void BackToPauseMenu()
    {
        // 1. Ẩn chính nó và tắt logic nhận phím mũi tên
        this.SetPanelActive(false); 
        
        // 2. Báo cho Master Controller để quay lại Pause
        var master = FindFirstObjectByType<GlobalGameJam.MinimapInteractionController>();
        if (master != null)
        {
            master.ExitSettingToPause();
        }
    }
}