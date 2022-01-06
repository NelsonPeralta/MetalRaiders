using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitScreenManager : MonoBehaviour
{
    public int numberOfPlayers = 1;
    public MyPlayerManager pManager;
    public bool usePlayerCounter = false;

    private void Awake()
    {
        if (usePlayerCounter)
        {
            numberOfPlayers = StaticVariables.numberOfPlayers;
        }
    }

    public void SetCameras()
    {
        if (numberOfPlayers == 1)
        {
            if (pManager.allPlayers[1])
                pManager.allPlayers[1].SetActive(false);
            if (pManager.allPlayers[2])
                pManager.allPlayers[2].SetActive(false);
            if (pManager.allPlayers[3])
                pManager.allPlayers[3].SetActive(false);


            pManager.allPlayers[0].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0, 0, 1, 1);
            pManager.allPlayers[0].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0, 0, 1, 1);
            pManager.allPlayers[0].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0, 0, 1, 1);

            SetUIs();
        }
        else if (numberOfPlayers == 2)
        {
            Debug.Log("2 Players");
            pManager.allPlayers[2].SetActive(false);
            pManager.allPlayers[3].SetActive(false);

            pManager.allPlayers[0].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
            pManager.allPlayers[0].GetComponent<PlayerProperties>().gunCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
            pManager.allPlayers[0].GetComponent<PlayerProperties>().deathCamera.rect = new Rect(0, 0.5f, 1, 0.5f);

            pManager.allPlayers[1].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0, 0, 1, .5f);
            pManager.allPlayers[1].GetComponent<PlayerProperties>().gunCamera.rect = new Rect(0, 0, 1, .5f);
            pManager.allPlayers[1].GetComponent<PlayerProperties>().deathCamera.rect = new Rect(0, 0, 1, .5f);

            SetUIs();

        }
        else if (numberOfPlayers == 3)
        {
            pManager.allPlayers[3].SetActive(false);

            pManager.allPlayers[0].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
            pManager.allPlayers[0].GetComponent<PlayerProperties>().gunCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
            pManager.allPlayers[0].GetComponent<PlayerProperties>().deathCamera.rect = new Rect(0, 0.5f, 1, 0.5f);

            pManager.allPlayers[1].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0, 0, 0.5f, .5f);
            pManager.allPlayers[1].GetComponent<PlayerProperties>().gunCamera.rect = new Rect(0, 0, 0.5f, .5f);
            pManager.allPlayers[1].GetComponent<PlayerProperties>().deathCamera.rect = new Rect(0, 0, 0.5f, .5f);

            pManager.allPlayers[2].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0.5f, 0, 0.5f, .5f);
            pManager.allPlayers[2].GetComponent<PlayerProperties>().gunCamera.rect = new Rect(0.5f, 0, 0.5f, .5f);
            pManager.allPlayers[2].GetComponent<PlayerProperties>().deathCamera.rect = new Rect(0.5f, 0, 0.5f, .5f);

            SetUIs();
        }
        else if (numberOfPlayers == 4)
        {
            pManager.allPlayers[0].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
            pManager.allPlayers[0].GetComponent<PlayerProperties>().gunCamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
            pManager.allPlayers[0].GetComponent<PlayerProperties>().deathCamera.rect = new Rect(0, 0.5f, 0.5f, 0.5f);

            pManager.allPlayers[1].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            pManager.allPlayers[1].GetComponent<PlayerProperties>().gunCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            pManager.allPlayers[1].GetComponent<PlayerProperties>().deathCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);

            pManager.allPlayers[2].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0, 0, 0.5f, 0.5f);
            pManager.allPlayers[2].GetComponent<PlayerProperties>().gunCamera.rect = new Rect(0, 0, 0.5f, 0.5f);
            pManager.allPlayers[2].GetComponent<PlayerProperties>().deathCamera.rect = new Rect(0, 0, 0.5f, 0.5f);

            pManager.allPlayers[3].GetComponent<PlayerProperties>().mainCamera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
            pManager.allPlayers[3].GetComponent<PlayerProperties>().gunCamera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
            pManager.allPlayers[3].GetComponent<PlayerProperties>().deathCamera.rect = new Rect(0.5f, 0, 0.5f, 0.5f);

            SetUIs();
        }
    }

    void SetUIs()
    {
        if (numberOfPlayers == 1)
        {
            Transform uiComponentsTopLeft = pManager.allPlayers[0].GetComponent<AllPlayerScripts>().playerUIComponents.topLeft;
            Transform uiComponentsTopMiddle = pManager.allPlayers[0].GetComponent<AllPlayerScripts>().playerUIComponents.topMiddle;
            Transform uiComponentsTopRight = pManager.allPlayers[0].GetComponent<AllPlayerScripts>().playerUIComponents.topRight;

            Transform uiComponentsBottomLeft = pManager.allPlayers[0].GetComponent<AllPlayerScripts>().playerUIComponents.bottomLeft;

            uiComponentsTopLeft.localScale = new Vector3(1, 1, 0);
            uiComponentsTopLeft.localPosition = new Vector3(0, 0, 0);
            uiComponentsTopMiddle.localScale = new Vector3(1, 1, 0);
            uiComponentsTopMiddle.localPosition = new Vector3(0, 0, 0);
            uiComponentsTopRight.localScale = new Vector3(1, 1, 0);
            uiComponentsTopRight.localPosition = new Vector3(0, 0, 0);

            uiComponentsBottomLeft.localScale = new Vector3(1, 1, 0);
            uiComponentsBottomLeft.localPosition = new Vector3(0, 0, 0);

        }
        else if (numberOfPlayers == 2)
        {


            foreach (GameObject player in pManager.allPlayers)
            {
                Transform uiComponentsTopLeft = player.GetComponent<AllPlayerScripts>().playerUIComponents.topLeft;
                Transform uiComponentsTopMiddle = player.GetComponent<AllPlayerScripts>().playerUIComponents.topMiddle;
                Transform uiComponentsTopRight = player.GetComponent<AllPlayerScripts>().playerUIComponents.topRight;

                Transform uiComponentsCenter = player.GetComponent<AllPlayerScripts>().playerUIComponents.center;

                Transform uiComponentsBottomLeft = player.GetComponent<AllPlayerScripts>().playerUIComponents.bottomLeft;
                Transform uiComponentsBottomRight = player.GetComponent<AllPlayerScripts>().playerUIComponents.bottomRight;

                if (player == pManager.allPlayers[0])
                {
                    uiComponentsTopLeft.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(-250, 150, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(0, 150, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(250, 150, 0);

                    uiComponentsCenter.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsCenter.localPosition = new Vector3(0, 305, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(-250, 400, 0);
                    uiComponentsBottomRight.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsBottomRight.localPosition = new Vector3(250, 400, 0);
                }
                else if (player == pManager.allPlayers[1])
                {
                    uiComponentsTopLeft.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(-250, 150 - 540, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(0, 150 - 540, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(250, 150 - 540, 0);

                    uiComponentsCenter.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsCenter.localPosition = new Vector3(0, 305 - 540, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(-250, 400 - 540, 0);
                    uiComponentsBottomRight.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsBottomRight.localPosition = new Vector3(250, 400 - 540, 0);
                }

                /*
                uiComponentsTopLeft.localScale = new Vector3(0.75f, 0.75f, 0);
                uiComponentsTopLeft.localPosition = new Vector3(-250, -125, 0);
                uiComponentsTopMiddle.localScale = new Vector3(0.75f, 0.75f, 0);
                uiComponentsTopMiddle.localPosition = new Vector3(0, -125, 0);
                uiComponentsTopRight.localScale = new Vector3(0.75f, 0.75f, 0);
                uiComponentsTopRight.localPosition = new Vector3(250, -125, 0);

                uiComponentsBottomLeft.localScale = new Vector3(0.75f, 0.75f, 0);
                uiComponentsBottomLeft.localPosition = new Vector3(-250, 125, 0);
                */
            }
        }
        else if (numberOfPlayers == 3)
        {
            foreach (GameObject player in pManager.allPlayers)
            {
                Transform uiComponentsTopLeft = player.GetComponent<AllPlayerScripts>().playerUIComponents.topLeft;
                Transform uiComponentsTopMiddle = player.GetComponent<AllPlayerScripts>().playerUIComponents.topMiddle;
                Transform uiComponentsTopRight = player.GetComponent<AllPlayerScripts>().playerUIComponents.topRight;

                Transform uiComponentsCenter = player.GetComponent<AllPlayerScripts>().playerUIComponents.center;

                Transform uiComponentsBottomLeft = player.GetComponent<AllPlayerScripts>().playerUIComponents.bottomLeft;
                Transform uiComponentsBottomRight = player.GetComponent<AllPlayerScripts>().playerUIComponents.bottomRight;

                if (player == pManager.allPlayers[0])
                {
                    uiComponentsTopLeft.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(-250, 150, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(0, 150, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(250, 150, 0);

                    uiComponentsCenter.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsCenter.localPosition = new Vector3(0, 305, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(-250, 400, 0);
                    uiComponentsBottomRight.localScale = new Vector3(0.75f, 0.75f, 0);
                    uiComponentsBottomRight.localPosition = new Vector3(250, 400, 0);

                }
                else if (player == pManager.allPlayers[1])
                {
                    uiComponentsTopLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(-500, 150 - 425, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(-500, 150 - 425, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(-500, 150 - 425, 0);

                    uiComponentsCenter.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsCenter.localPosition = new Vector3(-500, -250, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(-500, 400 - 675, 0);
                    uiComponentsBottomRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomRight.localPosition = new Vector3(-500, 400 - 675, 0);
                }
                else if (player == pManager.allPlayers[2])
                {
                    uiComponentsTopLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(450, 150 - 425, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(450, 150 - 425, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(450, 150 - 425, 0);

                    uiComponentsCenter.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsCenter.localPosition = new Vector3(450, -250, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(450, 400 - 675, 0);
                    uiComponentsBottomRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomRight.localPosition = new Vector3(450, 400 - 675, 0);
                }
            }

            /*
            Transform uiComponentsTopLeftP1 = pManager.allPlayers[0].GetComponent<ChildManager>().FindChildWithTagScript("UI Components Top-Left").GetComponent<Transform>();
            Transform uiComponentsTopMiddleP1 = pManager.allPlayers[0].GetComponent<ChildManager>().FindChildWithTagScript("UI Components Top-Middle").GetComponent<Transform>();
            Transform uiComponentsTopRightP1 = pManager.allPlayers[0].GetComponent<ChildManager>().FindChildWithTagScript("UI Components Top-Right").GetComponent<Transform>();

            Transform uiComponentsBottomLeftP1 = pManager.allPlayers[0].GetComponent<ChildManager>().FindChildWithTagScript("UI Components Bottom-Left").GetComponent<Transform>();



            
            uiComponentsTopLeftP1.localScale = new Vector3(0.75f, 0.75f, 0);
            uiComponentsTopLeftP1.localPosition = new Vector3(-250, -125, 0);
            uiComponentsTopMiddleP1.localScale = new Vector3(0.75f, 0.75f, 0);
            uiComponentsTopMiddleP1.localPosition = new Vector3(0, -125, 0);
            uiComponentsTopRightP1.localScale = new Vector3(0.75f, 0.75f, 0);
            uiComponentsTopRightP1.localPosition = new Vector3(250, -125, 0);

            uiComponentsBottomLeftP1.localScale = new Vector3(0.75f, 0.75f, 0);
            uiComponentsBottomLeftP1.localPosition = new Vector3(-250, 125, 0);

            foreach (GameObject player in pManager.allPlayers)
            {
                if(player != pManager.allPlayers[0])
                {
                    Transform uiComponentsTopLeft = player.GetComponent<ChildManager>().FindChildWithTagScript("UI Components Top-Left").GetComponent<Transform>();
                    Transform uiComponentsTopMiddle = player.GetComponent<ChildManager>().FindChildWithTagScript("UI Components Top-Middle").GetComponent<Transform>();
                    Transform uiComponentsTopRight = player.GetComponent<ChildManager>().FindChildWithTagScript("UI Components Top-Right").GetComponent<Transform>();

                    Transform uiComponentsBottomLeft = player.GetComponent<ChildManager>().FindChildWithTagScript("UI Components Bottom-Left").GetComponent<Transform>();

                    uiComponentsTopLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(0, 0, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(0, 0, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(0, 0, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(0, 0, 0);
                }
            }
            */

        }
        else if (numberOfPlayers == 4)
        {
            foreach (GameObject player in pManager.allPlayers)
            {
                Transform uiComponentsTopLeft = player.GetComponent<AllPlayerScripts>().playerUIComponents.topLeft;
                Transform uiComponentsTopMiddle = player.GetComponent<AllPlayerScripts>().playerUIComponents.topMiddle;
                Transform uiComponentsTopRight = player.GetComponent<AllPlayerScripts>().playerUIComponents.topRight;

                Transform uiComponentsCenter = player.GetComponent<AllPlayerScripts>().playerUIComponents.center;

                Transform uiComponentsBottomLeft = player.GetComponent<AllPlayerScripts>().playerUIComponents.bottomLeft;
                Transform uiComponentsBottomRight = player.GetComponent<AllPlayerScripts>().playerUIComponents.bottomRight;

                if (player == pManager.allPlayers[0])
                {
                    uiComponentsTopLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(-500, 275, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(-500, 275, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(-500, 275, 0);

                    uiComponentsCenter.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsCenter.localPosition = new Vector3(-480, 275, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(-500, 250, 0);
                    uiComponentsBottomRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomRight.localPosition = new Vector3(-500, 250, 0);
                }
                else if (player == pManager.allPlayers[1])
                {
                    uiComponentsTopLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(450, 275, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(450, 275, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(450, 275, 0);

                    uiComponentsCenter.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsCenter.localPosition = new Vector3(480, 275, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(450, 250, 0);
                    uiComponentsBottomRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomRight.localPosition = new Vector3(450, 250, 0);
                }
                else if (player == pManager.allPlayers[2])
                {
                    uiComponentsTopLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(-500, 150 - 425, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(-500, 150 - 425, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(-500, 150 - 425, 0);

                    uiComponentsCenter.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsCenter.localPosition = new Vector3(-480, -275, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(-500, 400 - 675, 0);
                    uiComponentsBottomRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomRight.localPosition = new Vector3(-500, 400 - 675, 0);
                }
                else if (player == pManager.allPlayers[3])
                {
                    uiComponentsTopLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopLeft.localPosition = new Vector3(450, 150 - 425, 0);
                    uiComponentsTopMiddle.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopMiddle.localPosition = new Vector3(450, 150 - 425, 0);
                    uiComponentsTopRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsTopRight.localPosition = new Vector3(450, 150 - 425, 0);

                    uiComponentsCenter.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsCenter.localPosition = new Vector3(480, -275, 0);

                    uiComponentsBottomLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomLeft.localPosition = new Vector3(450, 400 - 675, 0);
                    uiComponentsBottomRight.localScale = new Vector3(0.5f, 0.5f, 0);
                    uiComponentsBottomRight.localPosition = new Vector3(450, 400 - 675, 0);
                }

                /*
                uiComponentsTopLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                uiComponentsTopLeft.localPosition = new Vector3(0, 0, 0);
                uiComponentsTopMiddle.localScale = new Vector3(0.5f, 0.5f, 0);
                uiComponentsTopMiddle.localPosition = new Vector3(0, 0, 0);
                uiComponentsTopRight.localScale = new Vector3(0.5f, 0.5f, 0);
                uiComponentsTopRight.localPosition = new Vector3(0, 0, 0);

                uiComponentsBottomLeft.localScale = new Vector3(0.5f, 0.5f, 0);
                uiComponentsBottomLeft.localPosition = new Vector3(0, 0, 0);
                */
            }
        }
    }
}
