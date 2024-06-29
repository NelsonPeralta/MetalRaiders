using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] TMP_Text _volumeWitness, _sensWitness;
    [SerializeField] Slider _volumeSlider, _camSensSlider;


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
}
