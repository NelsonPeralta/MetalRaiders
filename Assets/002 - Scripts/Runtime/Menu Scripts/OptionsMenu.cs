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
}
