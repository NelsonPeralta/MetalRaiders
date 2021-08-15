using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Melee : MonoBehaviour
{
    public PlayerController pController;
    public PlayerProperties pProperties;

    [Header("Players in Melee Zone")]
    public List<PlayerProperties> playersInMeleeZone;

    [Header("Components")]
    bool meleeReady = true;
    public GameObject meleeIndicator;
    public GameObject knifeGameObject;
    public AudioSource audioSource;
    public AudioClip knifeSound;


    private void Start()
    {
        meleeIndicator.SetActive(false);
    }
    private void Update()
    {

        if (pController.player.GetButtonDown("Melee") && meleeReady && pProperties.PV.IsMine)
        {
            StartCoroutine(DisableMelee());
            if (playersInMeleeZone.Count > 0)
            {
                StartCoroutine(EnableMeleeIndicator());

                for (int i = 0; i < playersInMeleeZone.Count; i++)
                {
                    PlayerProperties playerToDamage = playersInMeleeZone[i];
                    if (playerToDamage.Health < pProperties.meleeDamage)
                        RemoveCorrespondingPlayer(playerToDamage.gameObject);

                    playerToDamage.Damage((int)pProperties.meleeDamage, false, pProperties.GetComponent<PhotonView>().ViewID);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        RemoveNullIndexes();
        if (other.GetComponent<PlayerProperties>() && other.gameObject != pProperties.gameObject)
            playersInMeleeZone.Add(other.GetComponent<PlayerProperties>());
    }

    private void OnTriggerExit(Collider other)
    {
        RemoveCorrespondingPlayer(other.gameObject);
        RemoveNullIndexes();
    }

    void RemoveCorrespondingPlayer(GameObject playerGameObject)
    {
        if (!playerGameObject)
            return;
            for (int i = 0; i < playersInMeleeZone.Count; i++)
                if (playersInMeleeZone[i].gameObject && playersInMeleeZone[i].gameObject == playerGameObject)
                    playersInMeleeZone[i] = null;
    }

    void RemoveNullIndexes()
    {
        for (int i = 0; i < playersInMeleeZone.Count; i++)
            if (!playersInMeleeZone[i])
                playersInMeleeZone.RemoveAt(i);
    }

    IEnumerator EnableMeleeIndicator()
    {
        meleeIndicator.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        meleeIndicator.SetActive(false);
    }
    IEnumerator DisableMelee()
    {
        meleeReady = false;
        yield return new WaitForSeconds(0.5f);
        meleeReady = true;
    }
}
