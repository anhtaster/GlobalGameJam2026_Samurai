using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Trigger zone that plays the mask layer tutorial when player enters.
    /// Should be placed near the tutorial wall.
    /// </summary>
    public class TutorialCheckpointTrigger : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TutorialViewModel tutorialViewModel;
        [SerializeField] private GameObject maskLayerTutorialPanel;

        [Header("Narrator Settings")]
        [SerializeField] private string narratorText = "A wall blocks your path. Press E while in map mode to hide it and reveal the way forward.";
        [SerializeField] private AudioClip narratorClip;
        [SerializeField] private float narratorDuration = 6f;

        [Header("Panel Delay")]
        [SerializeField] private float panelDelay = 4f; // Show panel after narrator finishes

        private bool hasTriggered = false;

        private void Start()
        {
            // Auto-find TutorialViewModel if not assigned
            if (tutorialViewModel == null)
            {
                tutorialViewModel = FindFirstObjectByType<TutorialViewModel>();
            }

            // Hide panel initially
            if (maskLayerTutorialPanel != null)
            {
                maskLayerTutorialPanel.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            // Check if player entered
            if (other.CompareTag("Player"))
            {
                hasTriggered = true;
                Debug.Log("[TutorialCheckpointTrigger] Player entered checkpoint - triggering mask layer tutorial");

                // Play narrator
                if (tutorialViewModel != null && !string.IsNullOrEmpty(narratorText))
                {
                    tutorialViewModel.PlayNarrative(narratorText, narratorClip, narratorDuration);
                }

                // Show tutorial panel after delay
                if (maskLayerTutorialPanel != null)
                {
                    Invoke(nameof(ShowTutorialPanel), panelDelay);
                }

                // Disable trigger after use
                gameObject.SetActive(false);
            }
        }

        private void ShowTutorialPanel()
        {
            if (maskLayerTutorialPanel != null)
            {
                maskLayerTutorialPanel.SetActive(true);
                Debug.Log("[TutorialCheckpointTrigger] Tutorial panel shown");
            }
        }
    }
}
