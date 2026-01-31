using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Collections;

public class IntroCutscene : MonoBehaviour
{
    [Header("Visual Effects")]
    [Tooltip("Reference to the Global Volume containing DepthOfField override")]
    public Volume globalVolume;
    [Tooltip("Duration for the blur to clear completely")]
    public float blurClearDuration = 3.0f;

    [System.Serializable]
    public struct NarrativeData
    {
        [TextArea] public string text;
        public AudioClip voiceClip;
        [Tooltip("Time to wait before showing the next line (includes text display time)")]
        public float duration;
    }

    [Header("Narrative Sequence")]
    public System.Collections.Generic.List<NarrativeData> narrativeSequence;
    
    [Header("Timing")]
    public float tutorialDisplayDuration = 10.0f;

    private DepthOfField _depthOfField;

    [Header("Player Control")]
    public StarterAssets.FirstPersonController playerController;

    private void Start()
    {
        SetupBlur();
        StartCoroutine(PlayIntroSequence());
    }

    private void SetupBlur()
    {
        if (globalVolume != null)
        {
            if (globalVolume.profile.TryGet(out _depthOfField))
            {
                _depthOfField.active = true;
                _depthOfField.focusDistance.value = 0.1f;
                Debug.Log("[IntroCutscene] DepthOfField found and set to blur.");
            }
            else
            {
                Debug.LogWarning("[IntroCutscene] DepthOfField NOT found in Global Volume Profile!");
            }
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        // Disable Player Input
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("[IntroCutscene] Player Input Disabled.");
        }
        else
        {
            // Try to find it if not assigned
            playerController = FindObjectOfType<StarterAssets.FirstPersonController>();
            if (playerController != null)
            {
                 playerController.enabled = false;
                 Debug.Log("[IntroCutscene] Player Input Found & Disabled.");
            }
            else
            {
                Debug.LogWarning("[IntroCutscene] FirstPersonController NOT found! Player might move.");
            }
        }

        // --- Step 1: Clear the Blur ---
        if (_depthOfField != null)
        {
            Debug.Log("[IntroCutscene] Clearing Blur...");
            DOTween.To(() => _depthOfField.focusDistance.value, x => _depthOfField.focusDistance.value = x, 10f, blurClearDuration)
                .SetEase(Ease.OutSine);
        }

        yield return new WaitForSeconds(blurClearDuration);

        // --- Step 2: Use ViewModel for Narrative Sequence ---
        Debug.Log($"[IntroCutscene] Starting Narrative Sequence. Count: {(narrativeSequence != null ? narrativeSequence.Count : 0)}");
        
        if (TutorialViewModel.Instance != null && narrativeSequence != null && narrativeSequence.Count > 0)
        {
            foreach (var line in narrativeSequence)
            {
                Debug.Log($"[IntroCutscene] Playing line: '{line.text}' for {line.duration}s");
                // Play current line
                TutorialViewModel.Instance.PlayNarrative(line.text, line.voiceClip, line.duration);
                
                // Wait for the duration of this line before playing the next
                yield return new WaitForSeconds(line.duration + 0.5f); 
            }
        }
        else
        {
            Debug.LogError("[IntroCutscene] TutorialViewModel Instance is MISSING or Sequence is Empty!");
        }

        // Delay before showing Tutorial Panel
        yield return new WaitForSeconds(0.5f);

        // --- Step 3: Trigger Tutorial Panel ---
        if (TutorialViewModel.Instance != null)
        {
            Debug.Log("[IntroCutscene] Triggering Tutorial Panel via ViewModel...");
            TutorialViewModel.Instance.ShowTutorialPanel(tutorialDisplayDuration);
        }

        // Enable Player Input
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("[IntroCutscene] Player Input Enabled.");
        }

        Debug.Log("[IntroCutscene] Sequence Completed.");
    }
}
