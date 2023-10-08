using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Melee : MonoBehaviour
{
    public PlayerController pController;
    public Player player;

    [Header("Players in Melee Zone")]
    public List<HitPoints> hitPointsInMeleeZone;

    [Header("Components")]
    bool meleeReady = true;
    public GameObject meleeIndicator;
    public GameObject knifeGameObject;
    public AudioSource audioSource;
    public AudioClip knifeSuccessSound;
    public AudioClip knifeFailSound;





    public Movement movement { get { return _movement; } }




    [SerializeField] Movement _movement;

    float _maxDis; // Does NOT take into account the radius of player character controller


    private void Start()
    {
        _maxDis = GetComponent<BoxCollider>().size.z;

        player.OnPlayerDeath -= OnPlayerDeadth_Delegate;
        player.OnPlayerDeath += OnPlayerDeadth_Delegate;

        meleeIndicator.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.GetComponent<HitPoints>() || this.player.isDead || this.player.isRespawning || other.transform == player.transform)
            return;

        HitPoints hp = other.GetComponent<HitPoints>();

        if (!hitPointsInMeleeZone.Contains(hp))
        {
            hitPointsInMeleeZone.Add(hp);
            player.OnPlayerDeath -= OnForeignPlayerDeath_Delegate;
            player.OnPlayerDeath += OnForeignPlayerDeath_Delegate;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        try
        {
            hitPointsInMeleeZone.Remove(other.GetComponent<HitPoints>());
        }
        catch { }
    }

    int _pushForce;
    public void Knife()
    {

        if (hitPointsInMeleeZone.Count > 0)
        {
            StartCoroutine(EnableMeleeIndicator());

            for (int i = 0; i < hitPointsInMeleeZone.Count; i++)
            {
                HitPoints hp = hitPointsInMeleeZone[i];
                if (hp.hitPoints <= 0 || hp.isDead || !hp.gameObject.activeInHierarchy)
                    hitPointsInMeleeZone.Remove(hp);
                else
                {
                    if (player.isMine)
                    {
                        audioSource.clip = knifeSuccessSound;
                        audioSource.volume = 1;
                        audioSource.spatialBlend = 0.9f;

                        audioSource.Play();

                        Vector3 dir = (hp.transform.position - player.transform.position);

                        print("Melee");
                        print(_maxDis);

                        if (hp.meleeMagnetism)
                        {

                            Vector3 targetPostition = new Vector3(hp.transform.position.x,
                                                        movement.transform.position.y,
                                                        hp.transform.position.z);
                            movement.transform.LookAt(targetPostition);

                            if (Vector3.Distance(hp.transform.position, movement.transform.position) > _maxDis) _pushForce = 200;
                            else _pushForce = 100;

                            Debug.Log(Vector3.Distance(hp.transform.position, movement.transform.position));
                            movement.Push(hp.transform.position - movement.transform.position, _pushForce, PushSource.Melee, true);
                        }

                        try
                        {
                            hp.hitboxes[0].GetComponent<ActorHitbox>().Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactDir: dir);
                        }
                        catch { }

                        try
                        {
                            hp.hitboxes[0].GetComponent<PlayerHitbox>().Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactDir: dir);
                        }
                        catch { }
                        //playerToDamage.Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactDir: dir);
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
        hitPointsInMeleeZone.Remove(player.GetComponent<HitPoints>());
    }

    void OnPlayerDeadth_Delegate(Player player)
    {
        hitPointsInMeleeZone.Clear();
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
