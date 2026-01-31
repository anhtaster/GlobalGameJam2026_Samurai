using UnityEngine;
using TMPro;

public class MainMenuDebugger : MonoBehaviour
{
    public MainMenuController mainMenuController;

    void Start()
    {
        if (mainMenuController == null)
        {
            mainMenuController = FindFirstObjectByType<MainMenuController>();
        }

        // Debug thứ tự buttons
        var buttonsField = typeof(MainMenuController).GetField("menuButtons", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (buttonsField != null)
        {
            var buttons = (RectTransform[])buttonsField.GetValue(mainMenuController);
            
            Debug.Log("=== MAIN MENU BUTTONS ORDER ===");
            for (int i = 0; i < buttons.Length; i++)
            {
                var text = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                string buttonName = text != null ? text.text : buttons[i].name;
                Debug.Log($"Index {i}: {buttonName}");
            }
            Debug.Log("===============================");
        }
    }
}
