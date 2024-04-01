using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveOptionsButton : MonoBehaviour
{
    [SerializeField] Slider _volumeSlider, _camSensSlider;


    private void OnEnable()
    {
        _volumeSlider.value = PlayerPrefs.GetFloat("volume");
        _camSensSlider.value = PlayerPrefs.GetFloat("sens");
    }
    public void SaveOptions()
    {
        GameManager.SaveOptions(vol: _volumeSlider.value, sens: _camSensSlider.value);
    }
}
