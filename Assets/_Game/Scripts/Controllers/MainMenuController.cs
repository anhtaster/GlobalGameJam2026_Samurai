using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform[] menuButtons; // Thứ tự: 0-Play, 1-Setting, 2-Credit, 3-Quit
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("Navigation Panels")]
    public GameObject settingsPanel;
    public GameObject creditPanel;

    private int currentIndex = 0;
    private bool isActive = false;

    void Start()
    {
        SetMenuActive(true);
    }



    public void SetMenuActive(bool active)
    {
        isActive = active;
        this.gameObject.SetActive(active);
        if (active) 
        {
            currentIndex = 0;
            UpdateVisuals();
            Time.timeScale = 0f; 
        }
    }

    void Update()
    {
        if (!isActive) return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex - 1 + menuButtons.Length) % menuButtons.Length;
            UpdateVisuals();
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            currentIndex = (currentIndex + 1) % menuButtons.Length;
            UpdateVisuals();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ExecuteSelection();
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var text = menuButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            bool isSelected = (i == currentIndex);
            
            if (text != null) text.color = isSelected ? selectedColor : unselectedColor;
            menuButtons[i].localScale = isSelected ? Vector3.one * 1.15f : Vector3.one;
        }
    }

    void ExecuteSelection()
    {
        // Lấy tên button hiện tại để xác định action
        var currentButton = menuButtons[currentIndex];
        var buttonText = currentButton.GetComponentInChildren<TextMeshProUGUI>();
        string buttonName = buttonText != null ? buttonText.text.ToLower() : currentButton.name.ToLower();

        // Debug chi tiết
        Debug.Log($"=== BUTTON DEBUG ===");
        Debug.Log($"Button GameObject name: {currentButton.name}");
        Debug.Log($"Button Text: {(buttonText != null ? buttonText.text : "NO TEXT")}");
        Debug.Log($"Button selected: {buttonName} (index: {currentIndex})");
        Debug.Log($"===================");

        // Xác định action dựa trên tên button thay vì index
        if (buttonName.Contains("play"))
        {
            // PLAY
            Debug.Log("[MainMenuController] Executing PLAY action");
            UIController.Instance.CloseAll();
            var master = FindFirstObjectByType<GlobalGameJam.MinimapInteractionController>();
            if (master != null) master.StartGame();
        }
        else if (buttonName.Contains("setting"))
        {
            // SETTING
            Debug.Log("[MainMenuController] Executing SETTING action");
            UIController.Instance.OpenSettings(false);
        }
        else if (buttonName.Contains("credit"))
        {
            // CREDIT
            Debug.Log("[MainMenuController] Executing CREDIT action");
            UIController.Instance.OpenCredits();
        }
        else if (buttonName.Contains("quit") || buttonName.Contains("exit"))
        {
            // QUIT
            Debug.Log("[MainMenuController] Executing QUIT action");
            Debug.Log("Quitting Game...");
            Application.Quit();
        }
        else
        {
            Debug.LogWarning($"Unknown button: {buttonName}");
        }
    }

    private void PlayGame()
    {
        SetMenuActive(false);
        var master = FindFirstObjectByType<GlobalGameJam.MinimapInteractionController>();
        if (master != null) master.SetPauseMode(false); // Resume game
    }

    private void OpenSettings()
    {
        if (settingsPanel != null)
        {
            isActive = false; // Tạm tắt nhận phím ở MainMenu
            settingsPanel.SetActive(true);
            var setCtrl = settingsPanel.GetComponentInParent<SettingPanelController>();
            if (setCtrl != null) setCtrl.SetPanelActive(true);
        }
    }

    private void OpenCredits()
    {
        if (creditPanel != null)
        {
            this.SetMenuActive(false); // Ẩn Main Menu và tắt isActive
            
            var creditCtrl = creditPanel.GetComponent<CreditPanelController>();
            if (creditCtrl != null) 
            {
                creditCtrl.SetPanelActive(true);
            }
        }
    }

    private void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}