using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlotManager : MonoBehaviour
{
    public bool player0Joined;
    public bool mapChooserOpen;
    public GameObject mapChooser;
    public GameObject pressStartWhenReadyInformer;

    public void openMapChooser()
    {
        mapChooser.SetActive(true);
        mapChooserOpen = true;
        pressStartWhenReadyInformer.SetActive(false);
    }

    public void closeMapChooser()
    {
        mapChooser.GetComponent<MapChooser>().mapChosen = false;
        mapChooser.SetActive(false);
        mapChooserOpen = false;
        pressStartWhenReadyInformer.SetActive(true);
    }
}
