using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public ChildManager cManager;
    public SplitScreenManager ssManager;

    public GameObject[] allPlayers = new GameObject[4];

    public int playerCount;
    

    private void Start()
    {
        cManager = GetComponent<ChildManager>();
        ssManager = GetComponent<SplitScreenManager>();

        ssManager.pManager = gameObject.GetComponent<PlayerManager>();

        allPlayers = GameObject.FindGameObjectsWithTag("Player");
        playerCount = allPlayers.Length;

        for (int i = 0; i < playerCount; i++)
        {
            /*
            allPlayers[i].gameObject.GetComponent<FPSControllerLPFP.FpsControllerLPFP>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponent<FPSControllerLPFP.FpsControllerLPFP>().SetPlayerIDInInput();
            */

            allPlayers[i].gameObject.GetComponent<Movement>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponent<Movement>().SetPlayerIDInInput();

            allPlayers[i].gameObject.GetComponent<PlayerController>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponent<PlayerController>().SetPlayerIDInInput();

            allPlayers[i].gameObject.GetComponent<PlayerProperties>().playerRewiredID = i;

            allPlayers[i].gameObject.GetComponentInChildren<CameraScript>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponentInChildren<CameraScript>().SetPlayerIDInInput();


            allPlayers[i].gameObject.GetComponentInChildren<ThirdPersonScript>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponentInChildren<ThirdPersonScript>().notMyFPSController = allPlayers[i].gameObject.GetComponent<FPSControllerLPFP.FpsControllerLPFP>();
            allPlayers[i].gameObject.GetComponentInChildren<ThirdPersonScript>().movement = allPlayers[i].gameObject.GetComponent<Movement>();

            allPlayers[i].gameObject.GetComponent<Tags>().tags[0] = "Player " + i;

            allPlayers[i].gameObject.GetComponent<AllPlayerScripts>().raycastScript.playerRewiredID = i;

            allPlayers[i].gameObject.GetComponentInChildren<FullyAutomaticFire>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponentInChildren<BurstFire>().playerRewiredID = i;
            allPlayers[i].gameObject.GetComponentInChildren<SingleFire>().playerRewiredID = i;

            SetLayers(i); // Watchout for Camera Script
        }
        ssManager.SetCameras();
    }

    void SetLayers(int playerID)
    {
        Camera mainCam = allPlayers[playerID].gameObject.GetComponent<AllPlayerScripts>().playerUIComponents.mainCamera;
        Camera gunCam = allPlayers[playerID].gameObject.GetComponent<AllPlayerScripts>().playerUIComponents.gunCamera;
        GameObject firstPersonModels = allPlayers[playerID].gameObject.GetComponent<PlayerProperties>().firstPersonModels;
        GameObject thirdPersonModels = allPlayers[playerID].gameObject.GetComponent<PlayerProperties>().thirdPersonModels;
        Debug.Log($"Set Layers of Player: {allPlayers[playerID].gameObject.name}");
        Debug.Log($"Player {playerID} REWIRED ID = {allPlayers[playerID].gameObject.GetComponent<PlayerProperties>().playerRewiredID}");

        if (playerID == 0)
        {
            mainCam.cullingMask &= ~(1 << 28);
            gunCam.cullingMask |= (1 << 24);

            foreach (GameObject child in firstPersonModels.GetComponent<ChildManager>().allChildren)
            {
                child.layer = 24;
            }

            foreach (GameObject child in thirdPersonModels.GetComponent<ChildManager>().allChildren)
            {
                child.layer = 28;
            }
        }
        if (playerID == 1)
        {
            mainCam.cullingMask &= ~(1 << 29);
            gunCam.cullingMask |= (1 << 25);

            foreach (GameObject child in firstPersonModels.GetComponent<ChildManager>().allChildren)
            {
                child.layer = 25;
            }

            foreach (GameObject child in thirdPersonModels.GetComponent<ChildManager>().allChildren)
            {
                child.layer = 29;
            }
        }
        if (playerID == 2)
        {
            mainCam.cullingMask &= ~(1 << 30);
            gunCam.cullingMask |= (1 << 26);

            foreach (GameObject child in firstPersonModels.GetComponent<ChildManager>().allChildren)
            {
                child.layer = 26;
            }

            foreach (GameObject child in thirdPersonModels.GetComponent<ChildManager>().allChildren)
            {
                child.layer = 30;
            }
        }
        if (playerID == 3)
        {
            mainCam.cullingMask &= ~(1 << 31);
            gunCam.cullingMask |= (1 << 27);

            foreach (GameObject child in firstPersonModels.GetComponent<ChildManager>().allChildren)
            {
                child.layer = 27;
            }

            foreach (GameObject child in thirdPersonModels.GetComponent<ChildManager>().allChildren)
            {
                child.layer = 31;
            }
        }

        ///Player Layers
        /// 24 = P1 FPS
        /// 25 = P2 FPS
        /// 26 = P3 FPS
        /// 27 = P4 FPS
        /// 
        /// 28 = P1 3PS
        /// 29 = P2 3PS
        /// 30 = P3 3PS
        /// 31 = P4 3PS
        /// cam.cullingMask &= ~(1 << 24); //Disable Layer 24
        /// gunCam.cullingMask |= (1 << 24); //Enable Layer 24
    }
}
