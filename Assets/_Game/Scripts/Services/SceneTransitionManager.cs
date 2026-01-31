using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace GlobalGameJam
{
    /// <summary>
    /// Handles scene transition with fade effect
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Fade Settings")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 1.5f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("Dependencies")]
        [SerializeField] private LevelManager levelManager;

        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Auto-find LevelManager if not assigned
            if (levelManager == null)
            {
                levelManager = FindFirstObjectByType<LevelManager>();
                if (levelManager == null)
                {
                    Debug.LogWarning("[SceneTransitionManager] LevelManager not found in scene. Will attempt to use static Instance at runtime.");
                }
            }

            // Setup fade image
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
                fadeImage.raycastTarget = false; // Don't block input when invisible
            }
        }

        /// <summary>
        /// Fade to black and load next level
        /// </summary>
        public void FadeAndLoadNextLevel(float delay = 0f)
        {
            StartCoroutine(FadeAndLoadCoroutine(delay, true));
        }

        /// <summary>
        /// Fade to black and load specific level by name
        /// </summary>
        public void FadeAndLoadLevel(string levelName, float delay = 0f)
        {
            StartCoroutine(FadeAndLoadLevelCoroutine(levelName, delay));
        }

        private IEnumerator FadeAndLoadCoroutine(float delay, bool loadNextLevel)
        {
            // Wait for delay
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            // Fade to black
            if (fadeImage != null)
            {
                fadeImage.raycastTarget = true; // Block input during transition
                fadeImage.DOColor(fadeColor, fadeDuration).SetEase(Ease.InOutSine);
                yield return new WaitForSeconds(fadeDuration);
            }

            // Load next level
            if (levelManager != null)
            {
                levelManager.LoadNextLevel();
            }
            else if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] LevelManager not found! Attempting direct scene load.");
                // Fallback: load next scene by build index
                int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                int nextSceneIndex = currentSceneIndex + 1;
                
                if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
                }
                else
                {
                    Debug.LogError("[SceneTransitionManager] No next scene in build settings!");
                }
            }
        }

        private IEnumerator FadeAndLoadLevelCoroutine(string levelName, float delay)
        {
            // Wait for delay
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            // Fade to black
            if (fadeImage != null)
            {
                fadeImage.raycastTarget = true; // Block input during transition
                fadeImage.DOColor(fadeColor, fadeDuration).SetEase(Ease.InOutSine);
                yield return new WaitForSeconds(fadeDuration);
            }

            // Load specific level
            if (levelManager != null)
            {
                levelManager.LoadLevel(levelName);
            }
            else if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(levelName);
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] LevelManager not found!");
            }
        }

        /// <summary>
        /// Fade from black (for scene start)
        /// </summary>
        public void FadeIn()
        {
            if (fadeImage != null)
            {
                fadeImage.color = fadeColor; // Start fully black
                fadeImage.raycastTarget = true;
                fadeImage.DOColor(new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f), fadeDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => fadeImage.raycastTarget = false);
            }
        }
    }
}
