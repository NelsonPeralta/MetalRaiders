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





    public PlayerMovement movement { get { return _movement; } }




    [SerializeField] PlayerMovement _movement;
    [SerializeField] LayerMask _meleeMask;

    float _maxDis; // Does NOT take into account the radius of player character controller


    private void Start()
    {
        _maxDis = GetComponent<BoxCollider>().size.z;
        _maxDis = 4; // changed through manual testing

        player.OnPlayerDeath -= OnPlayerDeath_Delegate;
        player.OnPlayerDeath += OnPlayerDeath_Delegate;

        meleeIndicator.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        //if (this.player.isDead || this.player.isRespawning || other.transform == player.transform || other.transform.root == player.transform)
        //    return;

        //HitPoints hps = null;
        //hps = other.GetComponent<HitPoints>();

        //if (hps == null && other.GetComponent<PlayerCapsule>()) { hps = other.transform.root.GetComponent<HitPoints>(); }

        //if (hps)
        //    if (!hitPointsInMeleeZone.Contains(hps))
        //    {
        //        hitPointsInMeleeZone.Add(hps);
        //        other.GetComponent<Player>().OnPlayerDeath -= OnForeignPlayerDeath_Delegate;
        //        other.GetComponent<Player>().OnPlayerDeath += OnForeignPlayerDeath_Delegate;
        //    }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.player.isDead || this.player.isRespawning || other.transform == player.transform || other.transform.root == player.transform)
            return;
        Debug.Log($"Melee OnTriggerEnter {other.name}");

        HitPoints hps = null;
        hps = other.GetComponent<HitPoints>();

        if (hps == null && other.GetComponent<PlayerCapsule>()) { hps = other.transform.root.GetComponent<HitPoints>(); }
        if (hps == null && other.GetComponent<Hitbox>()) { hps = other.GetComponent<Hitbox>().hitPoints; }

        if (hps)
        {
            Debug.Log($"Melee OnTriggerEnter {other.name}");

            if (!hitPointsInMeleeZone.Contains(hps))
            {
                hitPointsInMeleeZone.Add(hps);
                hps.OnDeath -= OnHitPointsDeath_Delegate;
                hps.OnDeath += OnHitPointsDeath_Delegate;
                //other.GetComponent<Player>().OnPlayerDeath -= OnForeignPlayerDeath_Delegate;
                //other.GetComponent<Player>().OnPlayerDeath += OnForeignPlayerDeath_Delegate;
            }
        }
    }



    HitPoints _tempHb;

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log($"Melee OnTriggerExit {other.name}");

        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            if (other.GetComponent<ActorHitbox>())
            {
                try
                {
                    hitPointsInMeleeZone.Remove(other.GetComponent<ActorHitbox>().hitPoints);
                }
                catch { }
            }



        try
        {
            hitPointsInMeleeZone.Remove(other.GetComponent<HitPoints>());
        }
        catch { }

        try
        {
            if (other.GetComponent<PlayerCapsule>())
                hitPointsInMeleeZone.Remove(other.transform.root.GetComponent<HitPoints>());
        }
        catch { }
    }



    int _pushForce;


    public void PushIfAble()
    {
        if (hitPointsInMeleeZone.Count > 0)
        {
            for (int i = 0; i < hitPointsInMeleeZone.Count; i++)
            {
                HitPoints hp = hitPointsInMeleeZone[i];
                if (player.isMine)
                {
                    print($"Melee. Max dis: ({_maxDis}), cur dis: {Vector3.Distance(hp.transform.position, movement.transform.position)}");
                    print(_maxDis);

                    if (hp.meleeMagnetism)
                    {
                        Vector3 targetPostition = new Vector3(hp.transform.position.x,
                                                    movement.transform.position.y,
                                                    hp.transform.position.z);
                        movement.transform.LookAt(targetPostition);

                        if (Vector3.Distance(hp.transform.position, movement.transform.position) > (_maxDis / 2f)) _pushForce = 200;
                        else _pushForce = 100;

                        player.GetComponent<Rigidbody>().AddForce((hp.transform.position - movement.transform.position).normalized * 10, ForceMode.Impulse);
                        player.movement.blockPlayerMoveInput = 0.5f;
                    }
                }
            }
        }
    }



    public bool MeleeDamage()
    {
        pController.currentlyReloadingTimer = 0;
        pController.CancelReloadCoroutine();

        if (hitPointsInMeleeZone.Count > 0)
        {
            StartCoroutine(EnableMeleeIndicator());

            for (int i = 0; i < hitPointsInMeleeZone.Count; i++)
            {
                HitPoints hp = hitPointsInMeleeZone[i];
                //if (hp.hitPoints <= 0 || hp.isDead || !hp.gameObject.activeInHierarchy)
                //    hitPointsInMeleeZone.Remove(hp);
                //else
                {
                    if (player.isMine)
                    {


                        Vector3 dir = (hp.transform.position - player.transform.position);

                        //print($"Melee. Max dis: ({_maxDis}), cur dis: {Vector3.Distance(hp.transform.position, movement.transform.position)}");
                        //print(_maxDis);

                        //if (hp.meleeMagnetism)
                        //{

                        //    Vector3 targetPostition = new Vector3(hp.transform.position.x,
                        //                                movement.transform.position.y,
                        //                                hp.transform.position.z);
                        //    movement.transform.LookAt(targetPostition);

                        //    if (Vector3.Distance(hp.transform.position, movement.transform.position) > (_maxDis / 2f)) _pushForce = 200;
                        //    else _pushForce = 100;

                        //    player.GetComponent<Rigidbody>().AddForce((hp.transform.position - movement.transform.position).normalized * 10, ForceMode.Impulse);
                        //    player.movement.blockPlayerMoveInput = 0.5f;
                        //}

                        try
                        {
                            hp.hitboxes[0].GetComponent<ActorHitbox>().Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactPos: hp.transform.position, impactDir: dir);
                        }
                        catch { }

                        try
                        {
                            hp.hitboxes[0].GetComponent<PlayerHitbox>().Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactPos: hp.transform.position, impactDir: dir);
                        }
                        catch { }
                        //playerToDamage.Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactDir: dir);
                        return true;
                    }
                }
            }
        }
        else
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(player.mainCamera.transform.position,
                player.mainCamera.transform.TransformDirection(Vector3.forward), out hit, _maxDis / 2, _meleeMask))
            {
                print($"Melee raycast hit: {hit.transform.name}");

                if (hit.transform.gameObject.GetComponent<Rigidbody>())
                {
                    print($"Melee pushing: {hit.transform.name}");
                    hit.transform.gameObject.GetComponent<Rigidbody>().AddForce(player.mainCamera.transform.TransformDirection(Vector3.forward).normalized * 300);

                    GameObjectPool.instance.SpawnWeaponSmokeCollisionObject(hit.point, SoundManager.instance.concretePunchHit);
                    return true;
                }
                else
                    GameObjectPool.instance.SpawnWeaponSmokeCollisionObject(hit.point, SoundManager.instance.concretePunchHit);


                if (hit.transform.gameObject.GetComponent<ExplosiveBarrel>())
                {
                    hit.transform.gameObject.GetComponent<ExplosiveBarrel>().Damage((int)player.meleeDamage, false, player.photonId);
                }


                return true;
            }


        }


        return false;
    }



    void OnHitPointsDeath_Delegate(HitPoints hp)
    {
        Debug.Log($"OnHitPointsDeath_Delegate {hp.name}");
        try
        {
            hitPointsInMeleeZone.Remove(hp);
        }
        catch { }
    }

    void OnPlayerDeath_Delegate(Player player)
    {
        Debug.Log($"OnPlayerDeadth_Delegate {player.name}");
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





    public void PlaySuccClip()
    {
        audioSource.clip = knifeSuccessSound;
        audioSource.Play();
    }

    public void PlayMissClip()
    {
        audioSource.clip = knifeFailSound;
        audioSource.Play();
    }
}
