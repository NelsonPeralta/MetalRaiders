using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    ChildManager cManager;
    public GameObject healthSliderGO;
    public Slider healthSlider;

    // Start is called before the first frame update
    void Start()
    {
        cManager = GetComponent<ChildManager>();
        healthSliderGO = cManager.FindChildWithTagScript("Health Bar Fill").gameObject;
        healthSlider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value > 60)
        {
            healthSliderGO.gameObject.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
        }

        if (healthSlider.value < 60 && healthSlider.value >= 25)
        {
            healthSliderGO.gameObject.GetComponent<Image>().color = new Color32(255, 255, 0, 255);
        }

        if (healthSlider.value < 25)
        {
            healthSliderGO.gameObject.GetComponent<Image>().color = new Color32(255, 0, 0, 255);
        }
    }
}
