using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIPhotonIdWorldUIWitness : MonoBehaviour
{
    [SerializeField] AiAbstractClass aiAbstractClass;

    // Update is called once per frame
    void Update()
    {
        try
        {
            GetComponent<Text>().text = aiAbstractClass.GetComponent<PhotonView>().ViewID.ToString();
            GetComponent<Text>().text += "\n" + aiAbstractClass.targetPlayer.GetComponent<Player>().nickName;
        }
        catch { }
    }
}
