using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [Header("Panels")]
    public MainMenuController mainMenuPanel;
    public SettingPanelController settingPanel;
    public CreditPanelController creditPanel;
    public PausePanelController pausePanel; // Đổi thành script để gọi hàm Reset

    // Enum để track panel trước đó
    private enum PreviousPanel { None, MainMenu, Pause }
    private PreviousPanel previousPanel = PreviousPanel.None;

    // Static property để các script khác biết có panel nào đang mở
    public static bool IsAnyPanelOpen { get; private set; } = false;

    private void Start()
    {
        // Gọi hàm này để đóng tất cả các panel khác và chỉ bật Main Menu
        OpenMainMenu();
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenMainMenu()
    {
        Debug.Log("[UIController] OpenMainMenu() called");
        CloseAll(); // Tắt tất cả các panel khác (Settings, Credits, Pause)
        if (mainMenuPanel != null)
        {
            mainMenuPanel.gameObject.SetActive(true);
            mainMenuPanel.SetMenuActive(true); // QUAN TRỌNG: Để bật biến isActive nhận phím bấm
            Debug.Log("[UIController] Main Menu Panel opened");
        }
        IsAnyPanelOpen = true; // Đánh dấu có panel đang mở
    }

    public void OpenSettings(bool fromPause)
    {
        Debug.Log($"[UIController] OpenSettings(fromPause={fromPause}) called");
        // Lưu lại panel trước đó
        previousPanel = fromPause ? PreviousPanel.Pause : PreviousPanel.MainMenu;
        Debug.Log($"[UIController] Previous panel set to: {previousPanel}");
        
        CloseAll();
        // Bật parent GameObject trước
        if (settingPanel != null)
        {
            settingPanel.gameObject.SetActive(true);
            Debug.Log("[UIController] Setting Panel parent GameObject enabled");
        }
        // SetPanelActive sẽ tự động bật GameObject bên trong
        settingPanel.SetPanelActive(true);
        Debug.Log("[UIController] Setting Panel opened");
        IsAnyPanelOpen = true; // Đánh dấu có panel đang mở
    }

    public void OpenCredits()
    {
        Debug.Log("[UIController] OpenCredits() called");
        // Lưu lại panel trước đó (Credit chỉ mở từ Main Menu)
        previousPanel = PreviousPanel.MainMenu;
        
        CloseAll();
        // Bật parent GameObject trước
        if (creditPanel != null)
        {
            creditPanel.gameObject.SetActive(true);
            Debug.Log("[UIController] Credit Panel parent GameObject enabled");
        }
        // SetPanelActive sẽ tự động bật GameObject bên trong
        creditPanel.SetPanelActive(true);
        Debug.Log("[UIController] Credit Panel opened");
        IsAnyPanelOpen = true; // Đánh dấu có panel đang mở
        Debug.Log($"[UIController] IsAnyPanelOpen set to TRUE (Credit Panel)");
    }

    public void OpenPauseMenu()
    {
        Debug.Log("[UIController] OpenPauseMenu() called");
        CloseAll();
        pausePanel.gameObject.SetActive(true);
        pausePanel.ResetMenu(); // Gọi hàm reset để bật isActive/IsPaused
        Debug.Log("[UIController] Pause Panel opened");
        IsAnyPanelOpen = true; // Đánh dấu có panel đang mở
    }

    public void CloseSettingsAndReturnToPrevious()
    {
        Debug.Log($"[UIController] CloseSettingsAndReturnToPrevious() called, previousPanel={previousPanel}");
        
        // Tắt Setting Panel trước khi quay về
        if (settingPanel != null)
        {
            settingPanel.SetPanelActive(false);
            settingPanel.gameObject.SetActive(false);
            Debug.Log("[UIController] Setting Panel closed");
        }
        
        // Quay về panel trước đó
        if (previousPanel == PreviousPanel.Pause)
        {
            Debug.Log("[UIController] Returning to Pause Panel");
            OpenPauseMenu();
        }
        else
        {
            Debug.Log("[UIController] Returning to Main Menu");
            OpenMainMenu();
        }
        
        previousPanel = PreviousPanel.None;
    }

    public void CloseCreditAndReturnToPrevious()
    {
        Debug.Log("[UIController] CloseCreditAndReturnToPrevious() called");
        
        // Tắt Credit Panel trước khi quay về
        if (creditPanel != null)
        {
            creditPanel.SetPanelActive(false);
            creditPanel.gameObject.SetActive(false);
            Debug.Log("[UIController] Credit Panel closed");
        }
        
        // Credit chỉ mở từ Main Menu
        Debug.Log("[UIController] Returning to Main Menu from Credit");
        OpenMainMenu();
        previousPanel = PreviousPanel.None;
    }

    public void CloseAll()
    {
        Debug.Log("[UIController] CloseAll() called - closing Main Menu and Pause only");
        mainMenuPanel.SetMenuActive(false);
        // KHÔNG tắt Setting và Credit Panel - chúng tự quản lý
        if(pausePanel != null) pausePanel.gameObject.SetActive(false);
        
        // Mặc định Resume thời gian, các panel mở sau sẽ tự dừng lại nếu cần
        Time.timeScale = 1f;
        IsAnyPanelOpen = false; // Đánh dấu không còn panel nào mở
        Debug.Log("[UIController] Main Menu and Pause closed, IsAnyPanelOpen=false");
    }
}