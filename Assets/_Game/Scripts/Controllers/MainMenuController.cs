using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform[] menuButtons; // Nút Play, Quit...
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(1f, 1f, 1f, 0.4f);

    private int currentIndex = 0;
    private bool isActive = false;

    public void SetMenuActive(bool active)
    {
        isActive = active;
        this.gameObject.SetActive(active);
        if (active) 
        {
            currentIndex = 0;
            UpdateVisuals();
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

        if (Keyboard.current.enterKey.wasPressedThisFrame)
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
            menuButtons[i].localScale = isSelected ? Vector3.one * 1.1f : Vector3.one;
        }
    }

    void ExecuteSelection()
    {
        if (currentIndex == 0) // Nút Play/Continue
        {
            Debug.Log("<color=blue>[Main Menu]</color> Quay lại màn chơi.");
            SetMenuActive(false);
            
            // Gọi Master Controller để tắt Pause hoàn toàn và resume game
            var master = FindFirstObjectByType<GlobalGameJam.MinimapInteractionController>();
            if (master != null) master.SetPauseMode(false);
        }
        // Thêm logic cho nút Quit Game nếu cần
    }
}