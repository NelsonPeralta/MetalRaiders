﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    [Header("Other Scripts")]
    public PlayerInventory pInventory;
    public PlayerWeaponSwapping weaponPickUp;
    public WeaponProperties wProperties;
    public GameObject ActiveCrosshair;
    public CameraScript cameraScript;

    [Space(20)]
    [Header("Crosshairs")]
    public List<Crosshair> crosshairList = new List<Crosshair>();

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
        weaponPickUp.OnWeaponPickup += UpdateReticule_Delegate;
        StartCoroutine(LateStart_Coroutine());
    }

    IEnumerator LateStart_Coroutine()
    {
        yield return new WaitForEndOfFrame();
        UpdateReticule();
    }

    void UpdateReticule_Delegate(PlayerWeaponSwapping weaponPickUp)
    {
        UpdateReticule();
    }
    public void UpdateReticule()
    {
        foreach(Crosshair c in crosshairList)
        {
            if (c.weaponReticule != pInventory.activeWeapon.reticuleType)
                c.gameObject.SetActive(false);
            else
                c.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
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
        foreach(Crosshair c in crosshairList)
        {
            if(pInventory.activeWeapon.reticuleType == c.weaponReticule)
            {
                c.redReticuleVersion.SetActive(true);
            }
        }
    }

    public void DeactivateRedCrosshair()
    {
        foreach (Crosshair c in crosshairList)
        {
            if (pInventory.activeWeapon.reticuleType == c.weaponReticule)
            {
                c.redReticuleVersion.SetActive(false);
            }
        }
    }
    public void FindComponents()
    {
        //Debug.Log(pInventory.activeWeapIs);
        //pInventory = GameObject.FindGameObjectWithTag("Player Inventory").GetComponent<PlayerInventoryManager>();
    }


}