using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action OnLevelStart;
    public event Action OnLevelComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loads the specified level by name asynchronously.
    /// </summary>
    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadLevelRoutine(levelName));
    }

    private IEnumerator LoadLevelRoutine(string levelName)
    {
        // Optional: Trigger specific transition animation here if needed later (e.g. fade out)
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);

        // Prevent scene from activating until we are ready (optional, good for loading screens)
        // asyncLoad.allowSceneActivation = false; 

        while (!asyncLoad.isDone)
        {
            // Update loading bar here if needed
            yield return null;
        }

        OnLevelStart?.Invoke();
    }

    /// <summary>
    /// Call this when the level objectives are met.
    /// </summary>
    public void CompleteLevel()
    {
        OnLevelComplete?.Invoke();
        Debug.Log("Level Complete!");
    }

    /// <summary>
    /// Reloads the current active scene.
    /// </summary>
    public void RestartLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadLevel(currentScene);
    }
}
