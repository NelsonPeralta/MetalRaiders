using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] TMP_Text _volumeWitness, _sensWitness, _vsyncHeader;
    [SerializeField] Slider _volumeSlider, _camSensSlider;

    private void OnEnable()
    {
        UpdateVSyncHeader();
    }


    public void UpdateWitnesses()
    {
        _volumeWitness.text = _volumeSlider.value.ToString();
        _sensWitness.text = _camSensSlider.value.ToString();
    }

    public void ChangeVolumeBtn(int i)
    {
        _volumeSlider.value = Mathf.Clamp(_volumeSlider.value + i, 0, 100);
        _volumeWitness.text = _volumeSlider.value.ToString();
    }


    public void ChangeCamSensBtn(int i)
    {
        _camSensSlider.value = Mathf.Clamp(_camSensSlider.value + i, 0, 10);
        _sensWitness.text = _camSensSlider.value.ToString();
    }

    public void ToggleVSync() // called from TMP button
    {
        if (PlayerPrefs.GetInt("vsyncint") == 0)
        {
            PlayerPrefs.SetInt("vsyncint", 1);
        }
        else if (PlayerPrefs.GetInt("vsyncint") == 1)
        {
            PlayerPrefs.SetInt("vsyncint", 0);
        }

        GameManager.LoadVSyncPrefs();

        UpdateVSyncHeader();
    }

    void UpdateVSyncHeader()
    {
        if (_vsyncHeader)
        {
            if (PlayerPrefs.GetInt("vsyncint") == 0)
            {
                _vsyncHeader.text = "V Sync: Off";
            }
            else if (PlayerPrefs.GetInt("vsyncint") == 1)
            {
                _vsyncHeader.text = "V Sync: On";
            }
        }
    }
}
