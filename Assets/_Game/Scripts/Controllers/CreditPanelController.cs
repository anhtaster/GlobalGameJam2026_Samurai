using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class CreditPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject creditPanel;
    public Button backButton; // Nút để quay lại Main Menu

    [Header("Visual Settings")]
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(1f, 1f, 1f, 0.4f);

    private bool isOpen = false;

    // Sự kiện để báo cho MainMenuController biết Credit đã đóng
    public event Action OnCreditClosed;

    void Start()
    {
        if (creditPanel != null) creditPanel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen || Keyboard.current == null) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CloseCredit();
        }
    }

    public void SetPanelActive(bool active)
    {
        Debug.Log($"[CreditPanelController] SetPanelActive({active}) called");
        isOpen = active;
        
        if (creditPanel != null)
        {
            creditPanel.SetActive(active);
            Debug.Log($"[CreditPanelController] creditPanel.SetActive({active}) executed");
            Debug.Log($"[CreditPanelController] creditPanel.activeSelf = {creditPanel.activeSelf}");
            Debug.Log($"[CreditPanelController] creditPanel.activeInHierarchy = {creditPanel.activeInHierarchy}");
            Debug.Log($"[CreditPanelController] this.gameObject.activeSelf = {this.gameObject.activeSelf}");
            Debug.Log($"[CreditPanelController] this.gameObject.activeInHierarchy = {this.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[CreditPanelController] creditPanel is NULL!");
        }
        
        if (active)
        {
            // Highlight nút Back ngay lập tức khi mở
            UpdateVisuals(true);
            Debug.Log("[CreditPanelController] Credit Panel is now active");
        }
        else
        {
            Debug.Log("[CreditPanelController] Credit Panel is now inactive");
        }
    }

    public void CloseCredit()
    {
        Debug.Log("[CreditPanelController] CloseCredit() called");
        // Gọi UIController để quay về panel trước đó
        UIController.Instance.CloseCreditAndReturnToPrevious();
    }

    private void UpdateVisuals(bool isSelected)
    {
        if (backButton == null) return;

        // Phóng to và đổi màu nút Back để người dùng biết là đang chọn nó
        backButton.transform.localScale = isSelected ? Vector3.one * 1.1f : Vector3.one;
        
        var cb = backButton.colors;
        cb.normalColor = isSelected ? selectedColor : unselectedColor;
        backButton.colors = cb;

        // Nếu có text bên trong nút
        var txt = backButton.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.color = isSelected ? selectedColor : unselectedColor;
    }
}