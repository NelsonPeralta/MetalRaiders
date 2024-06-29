using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveOptionsButton : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] Slider _volumeSlider, _camSensSlider;


    private void OnEnable()
    {
        _volumeSlider.value = PlayerPrefs.GetFloat("volume");
        _camSensSlider.value = PlayerPrefs.GetFloat("sens");
    }
    public void SaveOptions()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            GameManager.SaveOptions(vol: _volumeSlider.value, sens: _camSensSlider.value);
        }
        else
        {
            _player.playerDataCell.sens = _camSensSlider.value;

            if (_player == GameManager.GetRootPlayer())
                GameManager.SaveOptions(vol: _volumeSlider.value, sens: _camSensSlider.value);

            _player.playerUI.optionsMenuScript.UpdateWitnesses();
        }
    }
}
