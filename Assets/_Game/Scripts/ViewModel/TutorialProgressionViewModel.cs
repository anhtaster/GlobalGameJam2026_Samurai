using UnityEngine;
using System;
using System.Collections;

namespace GlobalGameJam
{
    public class TutorialProgressionViewModel : MonoBehaviour
    {
        [Header("Model Reference")]
        [SerializeField] private TutorialProgressionModel model;

        [Header("Dependencies")]
        [SerializeField] private TutorialViewModel tutorialViewModel;

        [Header("Minimap Reference")]
        [SerializeField] private GameObject minimapPanel;

        [Header("Follow-up Narrator (after Minimap unlock)")]
        [SerializeField] private string followUpNarratorText = "Look for items marked in blue on your map. They may aid your escape.";
        [SerializeField] private AudioClip followUpNarratorClip;
        [SerializeField] private float followUpNarratorDuration = 5f;

        // Events
        public event Action OnMapModelUnlocked;
        public event Action OnMinimapUnlocked;
        public event Action OnMapToggleUnlocked;
        public event Action OnGlassesUnlocked;

        // Public accessors
        public bool IsMapModelUnlocked => model != null && model.isMapModelUnlocked;
        public bool IsMinimapUnlocked => model != null && model.isMinimapUnlocked;
        public bool IsMapToggleUnlocked => model != null && model.isMapToggleUnlocked;
        public bool IsGlassesUnlocked => model != null && model.isGlassesUnlocked;

        private void Start()
        {
            if (model == null)
            {
                Debug.LogError("[TutorialProgressionViewModel] Model is not assigned!");
                return;
            }

            // Reset progress at start (for testing/new game)
            model.ResetProgress();

            // Hide minimap initially
            if (minimapPanel != null)
            {
                minimapPanel.SetActive(false);
            }
        }

        public void UnlockMinimap(string narratorText = null, AudioClip narratorClip = null)
        {
            if (model == null) return;

            if (!model.isMinimapUnlocked)
            {
                model.isMinimapUnlocked = true;
                Debug.Log("[TutorialProgressionViewModel] Minimap Unlocked!");

                // Show minimap panel
                if (minimapPanel != null)
                {
                    minimapPanel.SetActive(true);
                }

                // Play narrator
                if (tutorialViewModel != null && !string.IsNullOrEmpty(narratorText))
                {
                    tutorialViewModel.PlayNarrative(narratorText, narratorClip, 5f);
                    
                    // Play follow-up narrator line
                    StartCoroutine(PlayFollowUpNarrator());
                }

                Debug.Log("[TutorialProgressionViewModel] Invoking OnMinimapUnlocked event...");
                OnMinimapUnlocked?.Invoke();
                Debug.Log($"[TutorialProgressionViewModel] OnMinimapUnlocked invoked. Subscriber count: {(OnMinimapUnlocked != null ? OnMinimapUnlocked.GetInvocationList().Length : 0)}");
            }
        }

        public void UnlockMapModel(string narratorText = null, AudioClip narratorClip = null)
        {
            if (model == null) return;

            if (!model.isMapModelUnlocked)
            {
                model.isMapModelUnlocked = true;
                Debug.Log("[TutorialProgressionViewModel] Map Model Unlocked!");

                // Show minimap panel when map is picked up
                if (minimapPanel != null)
                {
                    minimapPanel.SetActive(true);
                    Debug.Log("[TutorialProgressionViewModel] Minimap panel shown with map pickup");
                }

                // Play narrator
                if (tutorialViewModel != null && !string.IsNullOrEmpty(narratorText))
                {
                    tutorialViewModel.PlayNarrative(narratorText, narratorClip, 5f);
                }

                OnMapModelUnlocked?.Invoke();
            }
        }

        private IEnumerator PlayFollowUpNarrator()
        {
            // Wait for the first narrator to finish (5s duration + 1s buffer)
            yield return new WaitForSeconds(6f);
            
            // Play second narrator line
            if (tutorialViewModel != null && !string.IsNullOrEmpty(followUpNarratorText))
            {
                tutorialViewModel.PlayNarrative(
                    followUpNarratorText,
                    followUpNarratorClip,
                    followUpNarratorDuration
                );
            }
        }

        public void UnlockMapToggle(string narratorText = null, AudioClip narratorClip = null)
        {
            if (model == null) return;

            // Require minimap first
            if (!model.isMinimapUnlocked)
            {
                Debug.LogWarning("[TutorialProgressionViewModel] Cannot unlock Map Toggle without Minimap!");
                return;
            }

            if (!model.isMapToggleUnlocked)
            {
                model.isMapToggleUnlocked = true;
                Debug.Log("[TutorialProgressionViewModel] Map Toggle Unlocked!");

                // Play narrator
                if (tutorialViewModel != null && !string.IsNullOrEmpty(narratorText))
                {
                    tutorialViewModel.PlayNarrative(narratorText, narratorClip, 5f);
                }

                OnMapToggleUnlocked?.Invoke();
            }
        }

        public void UnlockGlasses(string narratorText = null, AudioClip narratorClip = null)
        {
            if (model == null) return;

            if (!model.isGlassesUnlocked)
            {
                model.isGlassesUnlocked = true;
                Debug.Log("[TutorialProgressionViewModel] Glasses Unlocked!");

                // Play narrator
                if (tutorialViewModel != null && !string.IsNullOrEmpty(narratorText))
                {
                    tutorialViewModel.PlayNarrative(narratorText, narratorClip, 5f);
                }

                OnGlassesUnlocked?.Invoke();
            }
        }
    }
}
