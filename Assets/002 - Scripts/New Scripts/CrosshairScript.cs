using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairScript : MonoBehaviour
{
    [Header("Other Scripts")]
    public PlayerInventory pInventory;
    public WeaponProperties wProperties;
    public GameObject ActiveCrosshair;
    public CameraScript cameraScript;

    [Space(20)]
    [Header("Crosshairs")]
    public GameObject ARCrosshair;
    public GameObject SMGCrosshair;
    public GameObject PistolCrosshair;
    public GameObject ShotgunCrosshair;
    public GameObject SniperCrosshair;
    public GameObject EmptyCrosshair;

    public bool hasFoundComponents = false;
    public bool RRisActive = false;
    public bool friendlyRRisActive = false;


    public void Start()
    {

        if (!hasFoundComponents)
        {
            FindComponents();
            hasFoundComponents = true;
        }

    }

    private void Update()
    {
        //Debug.Log("RR: " + RRisActive);
        if (pInventory != null)
        {
            if (pInventory.activeWeapIs == 0)
            {
                if (pInventory.weaponsEquiped[0] != null)
                {
                    wProperties = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();
                }

                if (pInventory.weaponsEquiped[0] != null)
                {
                    if (pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>().reticule == "AR")
                    {
                        ARCrosshair.SetActive(true);
                        SMGCrosshair.SetActive(false);
                        PistolCrosshair.SetActive(false);
                        ShotgunCrosshair.SetActive(false);
                        SniperCrosshair.SetActive(false);
                        ActiveCrosshair = ARCrosshair;
                    }

                    else if (pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>().reticule == "SMG")
                    {
                        ARCrosshair.SetActive(false);
                        SMGCrosshair.SetActive(true);
                        PistolCrosshair.SetActive(false);
                        ShotgunCrosshair.SetActive(false);
                        SniperCrosshair.SetActive(false);
                        ActiveCrosshair = SMGCrosshair;
                    }

                    else if (pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>().reticule == "Pistol")
                    {
                        ARCrosshair.SetActive(false);
                        SMGCrosshair.SetActive(false);
                        PistolCrosshair.SetActive(true);
                        ShotgunCrosshair.SetActive(false);
                        SniperCrosshair.SetActive(false);
                        ActiveCrosshair = PistolCrosshair;
                    }
                    else if (pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>().reticule == "Shotgun")
                    {
                        ARCrosshair.SetActive(false);
                        SMGCrosshair.SetActive(false);
                        PistolCrosshair.SetActive(false);
                        ShotgunCrosshair.SetActive(true);
                        SniperCrosshair.SetActive(false);
                        ActiveCrosshair = ShotgunCrosshair;
                    }
                    else if (pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>().reticule == "Sniper")
                    {
                        ARCrosshair.SetActive(false);
                        SMGCrosshair.SetActive(false);
                        PistolCrosshair.SetActive(false);
                        ShotgunCrosshair.SetActive(false);
                        SniperCrosshair.SetActive(true);
                        ActiveCrosshair = ShotgunCrosshair;
                    }
                    else if (pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>().reticule == "")
                    {
                        ARCrosshair.SetActive(false);
                        SMGCrosshair.SetActive(false);
                        PistolCrosshair.SetActive(false);
                        ShotgunCrosshair.SetActive(false);
                        SniperCrosshair.SetActive(false);
                        ActiveCrosshair = EmptyCrosshair; ;
                    }
                }
            }


            if (pInventory.activeWeapIs == 1)
            {
                wProperties = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();

                if (pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>().reticule == "AR")
                {
                    ARCrosshair.SetActive(true);
                    SMGCrosshair.SetActive(false);
                    PistolCrosshair.SetActive(false);
                    ShotgunCrosshair.SetActive(false);
                    SniperCrosshair.SetActive(false);
                    ActiveCrosshair = ARCrosshair;
                }

                else if (pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>().reticule == "SMG")
                {
                    ARCrosshair.SetActive(false);
                    SMGCrosshair.SetActive(true);
                    PistolCrosshair.SetActive(false);
                    ShotgunCrosshair.SetActive(false);
                    SniperCrosshair.SetActive(false);
                    ActiveCrosshair = SMGCrosshair;
                }

                else if (pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>().reticule == "Pistol")
                {
                    ARCrosshair.SetActive(false);
                    SMGCrosshair.SetActive(false);
                    PistolCrosshair.SetActive(true);
                    ShotgunCrosshair.SetActive(false);
                    SniperCrosshair.SetActive(false);
                    ActiveCrosshair = PistolCrosshair;
                }
                else if (pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>().reticule == "Shotgun")
                {
                    ARCrosshair.SetActive(false);
                    SMGCrosshair.SetActive(false);
                    PistolCrosshair.SetActive(false);
                    ShotgunCrosshair.SetActive(true);
                    SniperCrosshair.SetActive(false);
                    ActiveCrosshair = ShotgunCrosshair;
                }
                else if (pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>().reticule == "Sniper")
                {
                    ARCrosshair.SetActive(false);
                    SMGCrosshair.SetActive(false);
                    PistolCrosshair.SetActive(false);
                    ShotgunCrosshair.SetActive(false);
                    SniperCrosshair.SetActive(true);
                    ActiveCrosshair = ShotgunCrosshair;
                }
                else if (pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>().reticule == "")
                {
                    ARCrosshair.SetActive(false);
                    SMGCrosshair.SetActive(false);
                    PistolCrosshair.SetActive(false);
                    ShotgunCrosshair.SetActive(false);
                    SniperCrosshair.SetActive(false);
                    ActiveCrosshair = EmptyCrosshair;
                }
            }
        }


        if (RRisActive == true)
        {
            foreach (Transform child in transform)
            {
                if (child.tag == "Crosshairs")
                {
                    foreach (Transform childOFchild in child)
                    {

                        if (childOFchild.gameObject.GetComponent<Image>() != null)
                        {
                            childOFchild.gameObject.GetComponent<Image>().color = new Color32(255, 0, 0, 255);
                        }

                    }
                }
            }
        }
        else if (RRisActive == false)
        {
            if (friendlyRRisActive == false)
            {
                foreach (Transform child in transform)
                {
                    if (child.tag == "Crosshairs")
                    {
                        foreach (Transform childOFchild in child)
                        {

                            if (childOFchild.gameObject.GetComponent<Image>() != null)
                            {
                                childOFchild.gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                            }




                        }
                    }
                }
            }
            else if (friendlyRRisActive == true)
            {
                foreach (Transform child in transform)
                {
                    if (child.tag == "Crosshairs")
                    {
                        foreach (Transform childOFchild in child)
                        {

                            if (childOFchild.gameObject.GetComponent<Image>() != null)
                            {
                                childOFchild.gameObject.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
                            }
                        }
                    }
                }
            }
        }
    }

    public void ActivateRedCrosshair()
    {
        if(ActiveCrosshair.GetComponent<Crosshair>())
            ActiveCrosshair.GetComponent<Crosshair>().redReticuleVersion.SetActive(true);
    }

    public void DeactivateRedCrosshair()
    {
        if (ActiveCrosshair.GetComponent<Crosshair>())
            ActiveCrosshair.GetComponent<Crosshair>().redReticuleVersion.SetActive(false);
    }
    public void FindComponents()
    {
        //Debug.Log(pInventory.activeWeapIs);
        //pInventory = GameObject.FindGameObjectWithTag("Player Inventory").GetComponent<PlayerInventoryManager>();
    }


}
