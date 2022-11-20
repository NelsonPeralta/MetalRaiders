using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Melee : MonoBehaviour
{
    public PlayerController pController;
    public Player player;

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
        player.OnPlayerDeath -= OnPlayerDeadth_Delegate;
        player.OnPlayerDeath += OnPlayerDeadth_Delegate;

        meleeIndicator.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.GetComponent<Player>() || this.player.isDead || this.player.isRespawning)
            return;

        Player player = (Player)other.GetComponent<Player>();

        if (!playersInMeleeZone.Contains(player) && player != this.player)
        {
            playersInMeleeZone.Add(player);
            player.OnPlayerDeath -= OnForeignPlayerDeath_Delegate;
            player.OnPlayerDeath += OnForeignPlayerDeath_Delegate;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        try
        {
            playersInMeleeZone.Remove(other.GetComponent<Player>());
        }
        catch { }
    }

    public void Knife()
    {

        if (playersInMeleeZone.Count > 0)
        {
            StartCoroutine(EnableMeleeIndicator());

            for (int i = 0; i < playersInMeleeZone.Count; i++)
            {
                Player playerToDamage = playersInMeleeZone[i];
                if (playerToDamage.hitPoints < player.meleeDamage || playerToDamage.isDead || playerToDamage.isRespawning)
                    playersInMeleeZone.Remove(playerToDamage);
                else
                    playerToDamage.Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee");
            }
        }
    }

    void OnForeignPlayerDeath_Delegate(Player player)
    {
        playersInMeleeZone.Remove(player);
    }

    void OnPlayerDeadth_Delegate(Player player)
    {
        playersInMeleeZone.Clear();
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
