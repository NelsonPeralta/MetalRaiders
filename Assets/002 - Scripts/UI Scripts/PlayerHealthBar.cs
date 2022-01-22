using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : PlayerBar
{
    public GameObject healthSliderGO;
    public Slider healthSlider;

    public override void OnPlayerDamaged_Delegate(Player player)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        healthSlider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value >= (healthSlider.maxValue * 0.6f))
            healthSliderGO.gameObject.GetComponent<Image>().color = new Color32(0, 255, 0, 255); // Green
        else if (healthSlider.value <= (healthSlider.maxValue * 0.25f))
            healthSliderGO.gameObject.GetComponent<Image>().color = new Color32(255, 0, 0, 255); // Red
        else
            healthSliderGO.gameObject.GetComponent<Image>().color = new Color32(255, 255, 0, 255); // Yellow
    }
}
