using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPackUICounters : MonoBehaviour
{
    PlayerManager pManager;
    
    [Header("Ammo Packs")]
    public GameObject[] smallAmmoPacks = new GameObject[4]; // 1Power, 2 Heavy, 4 Small standard map
    public GameObject[] heavyAmmoPacks = new GameObject[4];
    public GameObject[] powerAmmoPacks = new GameObject[4];

    [Header("Grenade Ammo Packs")]
    public GameObject[] grenadeAmmoPacks = new GameObject[4];

    private void Start()
    {
        pManager = GameObject.FindGameObjectWithTag("Player Manager").GetComponent<PlayerManager>();

        AssignCamerasToAmmoPackTextScript();
    }

    public void AssignCamerasToAmmoPackTextScript()
    {
        foreach (GameObject child in smallAmmoPacks)
        {
            child.GetComponent<AmmoPackText>().playerCamera1 = pManager.allPlayers[0].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera2 = pManager.allPlayers[1].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera3 = pManager.allPlayers[2].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera4 = pManager.allPlayers[3].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
        }

        foreach (GameObject child in heavyAmmoPacks)
        {
            child.GetComponent<AmmoPackText>().playerCamera1 = pManager.allPlayers[0].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera2 = pManager.allPlayers[1].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera3 = pManager.allPlayers[2].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera4 = pManager.allPlayers[3].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
        }

        foreach (GameObject child in powerAmmoPacks)
        {
            child.GetComponent<AmmoPackText>().playerCamera1 = pManager.allPlayers[0].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera2 = pManager.allPlayers[1].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera3 = pManager.allPlayers[2].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera4 = pManager.allPlayers[3].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
        }

        foreach (GameObject child in grenadeAmmoPacks)
        {
            child.GetComponent<AmmoPackText>().playerCamera1 = pManager.allPlayers[0].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera2 = pManager.allPlayers[1].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera3 = pManager.allPlayers[2].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
            child.GetComponent<AmmoPackText>().playerCamera4 = pManager.allPlayers[3].GetComponent<ChildManager>().FindChildWithTagScript("Main Camera").GetComponent<Camera>();
        }
    }

    private void Update()
    {
        
    }
}
