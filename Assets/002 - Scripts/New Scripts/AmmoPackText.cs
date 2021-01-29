using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoPackText : MonoBehaviour
{
    public Text ammoText1;
    public Text ammoText2;
    public Text ammoText3;
    public Text ammoText4;
    public ChildManager cManager;
    public Canvas canvas;
    public Canvas canvas1;
    public Canvas canvas2;
    public Canvas canvas3;
    public Canvas canvas4;
    public Camera playerCamera1;
    public Camera playerCamera2;
    public Camera playerCamera3;
    public Camera playerCamera4;
    public int ammo;

    public GameObject temp;

    public GameObject sceneManager;

    bool hasFoundTexts = false;

    private void Start()
    {
        cManager = gameObject.GetComponent<ChildManager>();
        canvas1 = cManager.FindChildWithTagScript("Canvas 1").gameObject.GetComponent<Canvas>();
        canvas2 = cManager.FindChildWithTagScript("Canvas 2").gameObject.GetComponent<Canvas>();
        canvas3 = cManager.FindChildWithTagScript("Canvas 3").gameObject.GetComponent<Canvas>();
        canvas4 = cManager.FindChildWithTagScript("Canvas 4").gameObject.GetComponent<Canvas>();

        temp = cManager.FindChildWithTagScript("Ammo Text 1").gameObject;
        ammoText1 = cManager.FindChildWithTagScript("Ammo Text 1").gameObject.GetComponent<Text>();
        ammoText2 = cManager.FindChildWithTagScript("Ammo Text 2").gameObject.GetComponent<Text>();
        ammoText3 = cManager.FindChildWithTagScript("Ammo Text 3").gameObject.GetComponent<Text>();
        ammoText4 = cManager.FindChildWithTagScript("Ammo Text 4").gameObject.GetComponent<Text>();

        sceneManager = GameObject.FindGameObjectWithTag("Scene Manager");

        FindAmmoPack();
    }

    public void UpdateText()
    {
        ammoText1.text = ammo.ToString();
        ammoText2.text = ammo.ToString();
        ammoText3.text = ammo.ToString();
        ammoText4.text = ammo.ToString();
    }

    public void Update()
    {
        canvas1.transform.LookAt(playerCamera1.transform);
        canvas2.transform.LookAt(playerCamera2.transform);
        canvas3.transform.LookAt(playerCamera3.transform);
        canvas4.transform.LookAt(playerCamera4.transform);

        CheckDistanceBetweenCounterAndCamera();
    }

    void FindAmmoPack()
    {
        if(gameObject.GetComponent<SmallAmmoPack>() == true)
        {
            ammo = gameObject.GetComponent<SmallAmmoPack>().ammoInThisPack;
            ammoText1.text = gameObject.GetComponent<SmallAmmoPack>().ammoInThisPack.ToString();
            ammoText2.text = gameObject.GetComponent<SmallAmmoPack>().ammoInThisPack.ToString();
            ammoText3.text = gameObject.GetComponent<SmallAmmoPack>().ammoInThisPack.ToString();
            ammoText4.text = gameObject.GetComponent<SmallAmmoPack>().ammoInThisPack.ToString();
        }
        else if (gameObject.GetComponent<HeavyAmmoPack>() == true)
        {
            ammo = gameObject.GetComponent<HeavyAmmoPack>().ammoInThisPack;
            ammoText1.text = gameObject.GetComponent<HeavyAmmoPack>().ammoInThisPack.ToString();
            ammoText2.text = gameObject.GetComponent<HeavyAmmoPack>().ammoInThisPack.ToString();
            ammoText3.text = gameObject.GetComponent<HeavyAmmoPack>().ammoInThisPack.ToString();
            ammoText4.text = gameObject.GetComponent<HeavyAmmoPack>().ammoInThisPack.ToString();
        }
        else if (gameObject.GetComponent<PowerAmmoPack>() == true)
        {
            ammo = gameObject.GetComponent<PowerAmmoPack>().ammoInThisPack;
            ammoText1.text = gameObject.GetComponent<PowerAmmoPack>().ammoInThisPack.ToString();
            ammoText2.text = gameObject.GetComponent<PowerAmmoPack>().ammoInThisPack.ToString();
            ammoText3.text = gameObject.GetComponent<PowerAmmoPack>().ammoInThisPack.ToString();
            ammoText4.text = gameObject.GetComponent<PowerAmmoPack>().ammoInThisPack.ToString();
        }
        else if (gameObject.GetComponent<GrenadeAmmoPack>() == true)
        {
            ammo = gameObject.GetComponent<GrenadeAmmoPack>().ammoInThisPack;
            ammoText1.text = gameObject.GetComponent<GrenadeAmmoPack>().ammoInThisPack.ToString();
            ammoText2.text = gameObject.GetComponent<GrenadeAmmoPack>().ammoInThisPack.ToString();
            ammoText3.text = gameObject.GetComponent<GrenadeAmmoPack>().ammoInThisPack.ToString();
            ammoText4.text = gameObject.GetComponent<GrenadeAmmoPack>().ammoInThisPack.ToString();
        }
    }

    void CheckDistanceBetweenCounterAndCamera()
    {
        float distance1 = Vector3.Distance(playerCamera1.transform.position, transform.position);
        float distance2 = Vector3.Distance(playerCamera2.transform.position, transform.position);
        float distance3 = Vector3.Distance(playerCamera3.transform.position, transform.position);
        float distance4 = Vector3.Distance(playerCamera4.transform.position, transform.position);

        if(distance1 <= 5)
        {
            canvas1.gameObject.SetActive(true);
        }
        else
        {
            canvas1.gameObject.SetActive(false);
        }

        if (distance2 <= 5)
        {
            canvas2.gameObject.SetActive(true);
        }
        else
        {
            canvas2.gameObject.SetActive(false);
        }

        if (distance3 <= 5)
        {
            canvas3.gameObject.SetActive(true);
        }
        else
        {
            canvas3.gameObject.SetActive(false);
        }

        if (distance4 <= 5)
        {
            canvas4.gameObject.SetActive(true);
        }
        else
        {
            canvas4.gameObject.SetActive(false);
        }
    }

}
