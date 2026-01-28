using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sensitivity : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Slider sensitivitySlider;

    private bool isPaused = false;

    private void Start()
    {
        // Load saved sensitivity or use default
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        sensitivitySlider.value = savedSensitivity;
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);

        // Apply the saved sensitivity at startup
        SetSensitivity(savedSensitivity);

        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);

        Time.timeScale = isPaused ? 0 : 1;

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log("Cursor Lock State: " + Cursor.lockState + ", Visible: " + Cursor.visible);
    }


    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}
