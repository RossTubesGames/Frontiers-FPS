using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sensitivity : MonoBehaviour
{
    public Slider sensitivitySlider;

    private void Start()
    {
        // Load saved sensitivity or use default
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        sensitivitySlider.value = savedSensitivity;
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);

        // Apply the saved sensitivity at startup
        SetSensitivity(savedSensitivity);
    }





    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}
