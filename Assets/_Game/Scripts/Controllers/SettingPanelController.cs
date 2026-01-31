using System;
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

    [Header("Audio Sources")]
    public AudioSource musicSource;    // Kéo AudioSource cho Music vào đây
    public AudioSource sfxSource;      // Kéo AudioSource cho SFX vào đây

    [Header("Visual Settings")]
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(1f, 1f, 1f, 0.4f); // Làm mờ khi không chọn

    [Header("Control Settings")]
    public float sliderSpeed = 0.5f;

    [Header("Lighting Settings")]
    [Range(0f, 2f)]
    public float minLightIntensity = 0f;
    [Range(0f, 2f)]
    public float maxLightIntensity = 2f;

    private int currentIndex = 0;
    private bool isOpen = false;
    private float defaultLightIntensity;

    public event Action OnSettingClosed;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        // Lưu giá trị mặc định của Environment Lighting
        defaultLightIntensity = RenderSettings.ambientIntensity;
        
        // Khởi tạo slider listeners
        InitializeSliders();
        
        UpdateSelectionVisuals();
    }

    void InitializeSliders()
    {
        if (sliders == null || sliders.Length < 3) return;

        // Slider 0: Light
        sliders[0].onValueChanged.AddListener(OnLightSliderChanged);
        sliders[0].value = Mathf.InverseLerp(minLightIntensity, maxLightIntensity, RenderSettings.ambientIntensity);

        // Slider 1: SFX
        sliders[1].onValueChanged.AddListener(OnSFXSliderChanged);
        if (sfxSource != null) sliders[1].value = sfxSource.volume;

        // Slider 2: Music
        sliders[2].onValueChanged.AddListener(OnMusicSliderChanged);
        if (musicSource != null) sliders[2].value = musicSource.volume;
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
        Debug.Log($"[SettingPanelController] SetPanelActive({active}) called");
        isOpen = active;
        if (settingsPanel != null) settingsPanel.SetActive(active);
        
        if (active) 
        {
            currentIndex = 0;
            UpdateSelectionVisuals();
            Time.timeScale = 0f;
            Debug.Log("[SettingPanelController] Setting Panel is now active");
        }
        else
        {
            Debug.Log("[SettingPanelController] Setting Panel is now inactive");
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

    public void BackToPreviousMenu()
    {
        Debug.Log("[SettingPanelController] BackToPreviousMenu() called");
        // Gọi UIController để quay về panel trước đó
        UIController.Instance.CloseSettingsAndReturnToPrevious();
    }

    // Callback methods for sliders
    void OnLightSliderChanged(float value)
    {
        // value từ 0 đến 1, map sang minLightIntensity -> maxLightIntensity
        float intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, value);
        RenderSettings.ambientIntensity = intensity;
        Debug.Log($"[SettingPanelController] Light intensity set to: {intensity}");
    }

    void OnSFXSliderChanged(float value)
    {
        // value từ 0 (tắt tiếng) đến 1 (max volume)
        if (sfxSource != null)
        {
            sfxSource.volume = value;
            Debug.Log($"[SettingPanelController] SFX volume set to: {value}");
        }
    }

    void OnMusicSliderChanged(float value)
    {
        // value từ 0 (tắt tiếng) đến 1 (max volume)
        if (musicSource != null)
        {
            musicSource.volume = value;
            Debug.Log($"[SettingPanelController] Music volume set to: {value}");
        }
    }

    // public void BackToPauseMenu()
    // {
    //     // 1. Ẩn chính nó và tắt logic nhận phím mũi tên
    //     this.SetPanelActive(false); 
        
    //     // 2. Báo cho Master Controller để quay lại Pause
    //     var master = FindFirstObjectByType<GlobalGameJam.MinimapInteractionController>();
    //     if (master != null)
    //     {
    //         master.ExitSettingToPause();
    //     }
    // }
}