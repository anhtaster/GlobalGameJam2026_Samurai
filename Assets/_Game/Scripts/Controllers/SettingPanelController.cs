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
        // isOpen bây giờ được quản lý bởi MinimapController gọi sang
        if (!isOpen) return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
            UpdateSelectionVisuals();
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex + 1) % menuItems.Length;
            UpdateSelectionVisuals();
        }

        HandleSliderAdjustment();
    }

    public void SetPanelActive(bool active)
    {
        isOpen = active;
        settingsPanel.SetActive(active);
        if (isOpen) 
        {
            currentIndex = 0;
            UpdateSelectionVisuals();
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
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i == currentIndex)
            {
                menuItems[i].color = selectedColor;
                // Có thể tăng Scale nhẹ để tạo hiệu ứng "sáng lên" rõ hơn
                menuItems[i].transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                menuItems[i].color = unselectedColor;
                menuItems[i].transform.localScale = Vector3.one;
            }
        }
    }
}