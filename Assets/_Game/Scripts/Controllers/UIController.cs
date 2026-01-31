using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject pausePanel;

    [Header("Resolution")]
    public TMP_Dropdown resDropdown;
    private Resolution[] resolutions;

    [Header("Audio")]
    public AudioMixer mainMixer;

    void Start()
    {
        // Lấy danh sách Resolution hỗ trợ từ phần cứng
        resolutions = Screen.resolutions;
        resDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resDropdown.AddOptions(options);
        resDropdown.value = currentResIndex;
        resDropdown.RefreshShownValue();
    }

    // Hàm đổi độ phân giải (Gán vào OnValueChanged của Dropdown)
    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, FullScreenMode.Windowed);
    }

    // --- Điều hướng Menu ---
    public void PlayGame() => SceneManager.LoadScene("GameScene");
    
    public void OpenSettings() 
    {
        settingsPanel.SetActive(true);
        if(mainMenuPanel != null) mainMenuPanel.SetActive(false);
    }

    public void CloseSettings() 
    {
        settingsPanel.SetActive(false);
        // Nếu đang ở MainMenu thì hiện lại MainMenu, nếu đang Pause thì thôi
        if (SceneManager.GetActiveScene().name == "MainMenuScene") 
            mainMenuPanel.SetActive(true);
    }

    public void QuitGame() => Application.Quit();

    // Hàm này gán vào OnValueChanged của Slider Music
    public void SetMusicVolume(float value)
    {
        // Công thức logarit để âm thanh giảm mượt mà (value từ 0.0001 đến 1)
        mainMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
    }

    // Hàm này gán vào OnValueChanged của Slider SFX
    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVol", Mathf.Log10(value) * 20);
    }
}