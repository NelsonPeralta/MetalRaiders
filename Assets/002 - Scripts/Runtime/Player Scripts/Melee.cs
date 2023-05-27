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





    public Movement movement { get { return _movement; } }




    [SerializeField] Movement _movement;

    float _maxDis;


    private void Start()
    {
        _maxDis = GetComponent<BoxCollider>().size.z;

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
                if (playerToDamage.hitPoints <= 0 || playerToDamage.isDead || playerToDamage.isRespawning)
                    playersInMeleeZone.Remove(playerToDamage);
                else
                {
                    if (player.isMine)
                    {
                        audioSource.clip = knifeSuccessSound;
                        audioSource.volume = 1;
                        audioSource.spatialBlend = 0.9f;

                        audioSource.Play();

                        Vector3 dir = (playerToDamage.transform.position - player.transform.position);

                        print(_maxDis);
                        Debug.Log(Vector3.Distance(playerToDamage.transform.position, movement.transform.position));
                        movement.Push(playerToDamage.transform.position - movement.transform.position, 200, PushSource.Melee, true);
                        playerToDamage.Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactDir: dir);
                    }
                }
            }
        }
        else
        {
            audioSource.clip = knifeFailSound;
            audioSource.volume = 0.5f;
            audioSource.spatialBlend = 1;

            audioSource.Play();
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
