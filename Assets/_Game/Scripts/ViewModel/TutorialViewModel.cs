using UnityEngine;
using System.Collections;

public class TutorialViewModel : MonoBehaviour
{
    public static TutorialViewModel Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private TutorialView view;
    [SerializeField] private AudioSource audioSource;

    private Coroutine currentNarrativeCoroutine;

    private void Awake()
    {
        // Singleton Implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (view == null)
        {
            view = GetComponent<TutorialView>();
            if (view == null)
            {
                Debug.LogError("[TutorialViewModel] TutorialView reference is missing!");
            }
        }
    }

    public void PlayNarrative(string text, AudioClip clip, float displayDuration = 3f)
    {
        Debug.Log($"[TutorialViewModel] PlayNarrative called with text: '{text}'");
        
        // Stop previous narrative if still running
        if (currentNarrativeCoroutine != null)
        {
            StopCoroutine(currentNarrativeCoroutine);
            Debug.Log("[TutorialViewModel] Stopped previous narrative");
        }
        
        currentNarrativeCoroutine = StartCoroutine(PlayNarrativeRoutine(text, clip, displayDuration));
    }

    private IEnumerator PlayNarrativeRoutine(string text, AudioClip clip, float displayDuration)
    {
        Debug.Log($"[TutorialViewModel] Starting narrative routine for: '{text}'");
        
        // 1. Play Audio
        if (audioSource != null && clip != null)
        {
            audioSource.Stop(); // Stop any previous audio
            audioSource.PlayOneShot(clip);
        }

        // 2. Show Text
        if (view != null)
        {
            view.ShowSubtitle(text);
            Debug.Log("[TutorialViewModel] Subtitle shown");
        }
        else
        {
            Debug.LogError("[TutorialViewModel] View is null!");
        }

        // 3. Wait
        // If displayDuration is 0, we assume we want to keep it until manually hidden,
        // or we can wait for audio length. For now, let's use the explicit duration.
        // If user wants it to match audio, we could do: 
        // float waitTime = (clip != null) ? clip.length : displayDuration;
        
        yield return new WaitForSeconds(displayDuration);

        // 4. Hide Text
        if (view != null)
        {
            view.HideSubtitle();
        }
    }

    public void ShowTutorialPanel(float duration)
    {
        StartCoroutine(ShowPanelRoutine(duration));
    }

    private IEnumerator ShowPanelRoutine(float duration)
    {
        if (view != null)
        {
            view.ShowPanel();
            yield return new WaitForSeconds(duration);
            view.HidePanel();
        }
    }

    /// <summary>
    /// Show the tutorial panel (stays visible until manually hidden)
    /// </summary>
    public void ShowPanel()
    {
        if (view != null)
        {
            view.ShowPanel();
        }
    }

    /// <summary>
    /// Hide the tutorial panel
    /// </summary>
    public void HidePanel()
    {
        if (view != null)
        {
            view.HidePanel();
        }
    }
}
