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

    [Header("Other")]
    bool meleeReady = true;
    public GameObject knifeGameObject;
    public AudioSource audioSource;
    public AudioClip knifeSound;

    private void Update()
    {
        //Collider[] colliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale);

        //foreach (Collider hit in colliders)
        //{
        //    if(hit.gameObject.GetComponent<AIHitbox>() != null)
        //    {
        //        var AIHitbox = hit.gameObject.GetComponent<AIHitbox>();

        //        if (pController.player.GetButtonDown("Melee") && !pController.isMeleeing)
        //        {
        //            AIHitbox.UpdateAIHealthMelee(pProperties.meleeDamage, pProperties.gameObject);
        //        }
        //    }



        //    if (hit.gameObject.GetComponent<PlayerHitbox>() != null)
        //    {
        //        var playerHitbox = hit.gameObject.GetComponent<PlayerHitbox>();
        //        var playerInZone = playerHitbox.player.GetComponent<PlayerProperties>();
        //        //Debug.Log($"Player in Melee zone: {playerInZone.PV.ViewID}");

        //        if (pController.player.GetButtonDown("Melee") && playerInZone.gameObject != pProperties.gameObject)
        //        {
        //            //playerHitbox.player.GetComponent<PlayerProperties>().BleedthroughDamage(pProperties.meleeDamage, false, pProperties.playerRewiredID);

        //            playerInZone.SetHealth((int)pProperties.meleeDamage, false, 0);
        //        }
        //    }

        //}

        if (pController.player.GetButtonDown("Melee") && meleeReady && pProperties.PV.IsMine)
        {
            StartCoroutine(DisableMelee());
            if (playersInMeleeZone.Count > 0)
                for (int i = 0; i < playersInMeleeZone.Count; i++)
                {
                    PlayerProperties playerToDamage = playersInMeleeZone[i];
                    if (playerToDamage.Health < pProperties.meleeDamage)
                        RemoveCorrespondingPlayer(playerToDamage.gameObject);

                    playerToDamage.Damage((int)pProperties.meleeDamage, false, pProperties.GetComponent<PhotonView>().ViewID);
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

    IEnumerator DisableMelee()
    {
        meleeReady = false;
        yield return new WaitForSeconds(0.5f);
        meleeReady = true;
    }
}
