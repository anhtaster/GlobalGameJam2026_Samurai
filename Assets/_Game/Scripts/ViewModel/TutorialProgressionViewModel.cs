using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

namespace GlobalGameJam
{
    public class TutorialProgressionViewModel : MonoBehaviour
    {
        [Header("Model Reference")]
        [SerializeField] private TutorialProgressionModel model;

        [Header("Dependencies")]
        [SerializeField] private TutorialViewModel tutorialViewModel;
        [SerializeField] private GridScanner gridScanner;

        [Header("Minimap Reference")]
        [SerializeField] private GameObject minimapPanel;

        [Header("Follow-up Narrator (after Minimap unlock)")]
        [SerializeField] private string followUpNarratorText = "Look for items marked in blue on your map. They may aid your escape.";
        [SerializeField] private AudioClip followUpNarratorClip;
        [SerializeField] private float followUpNarratorDuration = 5f;

        [Header("Tutorial Wall (appears after MapToggle unlock)")]
        [SerializeField] private GameObject tutorialWall;
        [SerializeField] private string maskLayerNarratorText = "A wall blocks your path. Press E while in map mode to hide it and reveal the way forward.";
        [SerializeField] private AudioClip maskLayerNarratorClip;
        [SerializeField] private float maskLayerNarratorDuration = 6f;
        [SerializeField] private GameObject maskLayerTutorialPanel; // Separate panel for mask layer tutorial shown 10s after MapToggle pickup
        [SerializeField] private GameObject tutorialCheckpoint; // Checkpoint trigger zone (enabled when wall appears)

        // Events
        public event Action OnMinimapUnlocked;
        public event Action OnMapToggleUnlocked;
        public event Action OnGlassesUnlocked;

        // Public accessors
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

            // Auto-find GridScanner if not assigned
            if (gridScanner == null)
            {
                gridScanner = FindFirstObjectByType<GridScanner>();
            }

            // Reset progress at start (for testing/new game)
            model.ResetProgress();

            // Hide minimap initially (chờ nhặt item)
            if (minimapPanel != null)
            {
                minimapPanel.SetActive(false);
            }

            // Hide tutorial wall initially
            if (tutorialWall != null)
            {
                tutorialWall.SetActive(false);
                Debug.Log("[TutorialProgressionViewModel] Tutorial wall disabled initially");
            }

            // Disable tutorial checkpoint initially (enable when wall appears)
            if (tutorialCheckpoint != null)
            {
                tutorialCheckpoint.SetActive(false);
                Debug.Log("[TutorialProgressionViewModel] Tutorial checkpoint disabled initially");
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

                // Hiển thị map 3D object và trigger animation lấy map
                MapController mapController = FindFirstObjectByType<MapController>();
                if (mapController != null)
                {
                    mapController.ShowMapForFirstTime();
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

                // Enable tutorial wall
                if (tutorialWall != null)
                {
                    tutorialWall.SetActive(true);
                    Debug.Log("[TutorialProgressionViewModel] Tutorial wall enabled");
                    
                    // Register wall to minimap
                    if (gridScanner != null)
                    {
                        gridScanner.RegisterWall(tutorialWall);
                    }
                    else
                    {
                        Debug.LogWarning("[TutorialProgressionViewModel] GridScanner not found, tutorial wall won't appear on minimap!");
                    }
                }

                // Enable tutorial checkpoint (now safe to trigger)
                if (tutorialCheckpoint != null)
                {
                    tutorialCheckpoint.SetActive(true);
                    Debug.Log("[TutorialProgressionViewModel] Tutorial checkpoint enabled");
                }

                // Play narrator
                if (tutorialViewModel != null && !string.IsNullOrEmpty(narratorText))
                {
                    tutorialViewModel.PlayNarrative(narratorText, narratorClip, 5f);
                    
                    // Play mask layer narrator after a delay
                    StartCoroutine(PlayMaskLayerNarrator());
                    
                    // Show tutorial panel after 10 seconds
                    StartCoroutine(ShowDelayedTutorialPanel());
                }

                OnMapToggleUnlocked?.Invoke();
            }
        }

        private IEnumerator PlayMaskLayerNarrator()
        {
            // Wait for the unlock narrator to finish (5s + 1s buffer)
            yield return new WaitForSeconds(6f);
            
            // Play mask layer tutorial narrator
            if (tutorialViewModel != null && !string.IsNullOrEmpty(maskLayerNarratorText))
            {
                tutorialViewModel.PlayNarrative(
                    maskLayerNarratorText,
                    maskLayerNarratorClip,
                    maskLayerNarratorDuration
                );
            }
        }

        private IEnumerator ShowDelayedTutorialPanel()
        {
            // Wait 10 seconds
            yield return new WaitForSeconds(10f);
            
            // Show mask layer tutorial panel
            if (maskLayerTutorialPanel != null)
            {
                // Get CanvasGroup component (should be on the panel)
                CanvasGroup panelGroup = maskLayerTutorialPanel.GetComponent<CanvasGroup>();
                
                if (panelGroup != null)
                {
                    // Ensure panel is active but invisible
                    maskLayerTutorialPanel.SetActive(true);
                    panelGroup.alpha = 0f;
                    panelGroup.interactable = true;
                    panelGroup.blocksRaycasts = true;
                    
                    // Fade in
                    panelGroup.DOFade(1f, 0.5f);
                    Debug.Log("[TutorialProgressionViewModel] Mask layer tutorial panel shown (10s delay)");
                }
                else
                {
                    // No CanvasGroup, just show it directly
                    maskLayerTutorialPanel.SetActive(true);
                    Debug.LogWarning("[TutorialProgressionViewModel] Mask layer panel has no CanvasGroup, showing without fade.");
                }
            }
            else
            {
                Debug.LogWarning("[TutorialProgressionViewModel] maskLayerTutorialPanel is not assigned!");
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

                // Trigger scene transition after delay
                StartCoroutine(TriggerSceneTransition());

                OnGlassesUnlocked?.Invoke();
            }
        }

        private IEnumerator TriggerSceneTransition()
        {
            // Wait 18 seconds
            yield return new WaitForSeconds(18f);
            
            Debug.Log("[TutorialProgressionViewModel] Triggering scene transition...");
            
            // Fade to black and load next level
            if (GlobalGameJam.SceneTransitionManager.Instance != null)
            {
                GlobalGameJam.SceneTransitionManager.Instance.FadeAndLoadNextLevel();
            }
            else
            {
                Debug.LogWarning("[TutorialProgressionViewModel] SceneTransitionManager not found! Cannot transition to next level.");
            }
        }
    }
}
