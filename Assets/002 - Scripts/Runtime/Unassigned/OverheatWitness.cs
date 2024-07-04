using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverheatWitness : MonoBehaviour
{
    [SerializeField] WeaponProperties _weaponProperties;
    [SerializeField] GameObject _sliderHolder;
    [SerializeField] Slider _slider;

    Image _fillImage;


    private void Awake()
    {
        _fillImage = _slider.fillRect.GetComponent<Image>();
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_weaponProperties)
        {
            


            _sliderHolder.SetActive(_weaponProperties.currentOverheat > 0);

            if (_sliderHolder.activeSelf)
            {
                _slider.value = _weaponProperties.currentOverheat;

                if (_slider.value >= (_slider.maxValue * 0.6f))
                    _fillImage.color = Color.red;
                else if (_slider.value <= _slider.maxValue * 0.25f)
                    _fillImage.color = Color.green;
                else
                    _fillImage.color = Color.yellow;


                //if (_slider.value >= (_slider.maxValue * 0.6f))
                //    _fillImage.color = new Color32(0, 255, 0, 255); // Green
                //else if (_slider.value <= _slider.maxValue * 0.25f)
                //    _fillImage.color = new Color32(255, 0, 0, 255); // Red
                //else
                //    _fillImage.color = new Color32(255, 255, 0, 255); // Yellow
            }
        }
        else
            gameObject.SetActive(false);

    }
}
