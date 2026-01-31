using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class TutorialView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private CanvasGroup tutorialPanel;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private void Start()
    {
        // Ensure everything is hidden on start
        if (subtitleText != null)
        {
            subtitleText.alpha = 0f;
            subtitleText.text = "";
            subtitleText.gameObject.SetActive(false);
        }

        if (tutorialPanel != null)
        {
            tutorialPanel.alpha = 0f;
            tutorialPanel.interactable = false;
            tutorialPanel.blocksRaycasts = false;
            tutorialPanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Displays the subtitle text with a fade-in animation.
    /// </summary>
    public void ShowSubtitle(string text)
    {
        if (subtitleText != null)
        {
            // Kill any existing tweens on this text to prevent conflicts
            subtitleText.DOKill();
            
            // Reset state
            subtitleText.text = text;
            subtitleText.alpha = 0f;
            subtitleText.gameObject.SetActive(true);

            // Animate
            subtitleText.DOFade(1f, fadeDuration);
        }
    }

    /// <summary>
    /// Hides the subtitle text with a fade-out animation.
    /// </summary>
    public void HideSubtitle()
    {
        if (subtitleText != null)
        {
            // Kill any existing tweens first
            subtitleText.DOKill();
            
            subtitleText.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                subtitleText.text = "";
                subtitleText.gameObject.SetActive(false);
            });
        }
    }

    /// <summary>
    /// Shows the main tutorial panel.
    /// </summary>
    public void ShowPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.gameObject.SetActive(true);
            tutorialPanel.alpha = 0f;
            tutorialPanel.DOFade(1f, fadeDuration);
            tutorialPanel.interactable = true;
            tutorialPanel.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// Hides the main tutorial panel.
    /// </summary>
    public void HidePanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                tutorialPanel.gameObject.SetActive(false);
                tutorialPanel.interactable = false;
                tutorialPanel.blocksRaycasts = false;
            });
        }
    }
}
