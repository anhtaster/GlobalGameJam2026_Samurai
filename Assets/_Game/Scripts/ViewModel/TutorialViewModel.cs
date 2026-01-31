using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class TutorialViewModel : MonoBehaviour
{
    public static TutorialViewModel Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private TutorialView view;
    [SerializeField] private AudioSource audioSource;

    private Coroutine currentNarrativeCoroutine;
    private bool isNarrativePlaying = false;

    // Event để notify khi narrator bị skip - các class khác có thể listen
    public event Action OnNarrativeSkipped;

    public bool IsNarrativePlaying => isNarrativePlaying;

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

    private void Update()
    {
        // Tab key để skip narrator hiện tại - sử dụng New Input System
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame && isNarrativePlaying)
        {
            SkipNarrative();
        }
    }

    /// <summary>
    /// Skip narrator hiện tại và trigger event để chuyển sang narrator tiếp theo
    /// </summary>
    public void SkipNarrative()
    {
        if (!isNarrativePlaying) return;

        Debug.Log("[TutorialViewModel] Skipping current narrative...");

        // Stop coroutine hiện tại
        if (currentNarrativeCoroutine != null)
        {
            StopCoroutine(currentNarrativeCoroutine);
            currentNarrativeCoroutine = null;
        }

        // Stop audio hiện tại
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Ẩn subtitle và skip hint ngay lập tức
        if (view != null)
        {
            view.HideSubtitle();
            view.HideSkipHint();
        }

        isNarrativePlaying = false;

        // Trigger event để notify các class khác (ví dụ IntroCutscene) chuyển sang narrator tiếp theo
        OnNarrativeSkipped?.Invoke();
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
        
        isNarrativePlaying = true;
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

        // 2. Show Text and Skip Hint
        if (view != null)
        {
            view.ShowSubtitle(text);
            view.ShowSkipHint(); // Hiện "Press Tab to skip"
            Debug.Log("[TutorialViewModel] Subtitle and skip hint shown");
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

        // 4. Hide Text and Skip Hint
        if (view != null)
        {
            view.HideSubtitle();
            view.HideSkipHint();
        }
        
        isNarrativePlaying = false;
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
