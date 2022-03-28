using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Melee : MonoBehaviour
{
    public PlayerController pController;
    public Player pProperties;

    [Header("Players in Melee Zone")]
    public List<Player> playersInMeleeZone;

    [Header("Components")]
    bool meleeReady = true;
    public GameObject meleeIndicator;
    public GameObject knifeGameObject;
    public AudioSource audioSource;
    public AudioClip knifeSuccessSound;
    public AudioClip knifeFailSound;


    private void Start()
    {
        meleeIndicator.SetActive(false);
    }
    public void Knife()
    {

        if (playersInMeleeZone.Count > 0)
        {
            StartCoroutine(EnableMeleeIndicator());

            for (int i = 0; i < playersInMeleeZone.Count; i++)
            {
                Player playerToDamage = playersInMeleeZone[i];
                if (playerToDamage.hitPoints < pProperties.meleeDamage)
                    RemoveCorrespondingPlayer(playerToDamage.gameObject);

                playerToDamage.Damage((int)pProperties.meleeDamage, false, pProperties.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        RemoveNullIndexes();
        if (other.GetComponent<Player>() && other.gameObject != pProperties.gameObject)
            playersInMeleeZone.Add(other.GetComponent<Player>());
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
