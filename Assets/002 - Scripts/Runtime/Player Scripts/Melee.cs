using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class Melee : MonoBehaviour
{
    public PlayerController pController;
    public Player player;

    [Header("Players in Melee Zone")]
    public List<HitPoints> hitPointsInMeleeZone = new List<HitPoints>();

    [Header("Components")]
    bool meleeReady = true;
    public GameObject meleeIndicator;
    public GameObject knifeGameObject;
    public AudioSource audioSource;
    public AudioClip knifeSuccessSound;
    public AudioClip knifeFailSound;





    public PlayerMovement movement { get { return _movement; } }




    [SerializeField] PlayerMovement _movement;
    [SerializeField] LayerMask _meleeMask, _obstructionMask;


    List<HitPoints> _pushIfAbleList = new List<HitPoints>();
    List<HitPoints> _damageList = new List<HitPoints>();



    //Vector3 _normalScale = new Vector3(1, 1.5f, 1.5f);
    //Vector3 _swordScale = new Vector3(1, 1.5f, 2.5f);




    private void Start()
    {
        player.OnPlayerDeath -= OnPlayerDeath_Delegate;
        player.OnPlayerDeath += OnPlayerDeath_Delegate;

        meleeIndicator.SetActive(false);
    }


    private void Update()
    {
        //if(player && player.playerInventory.activeWeapon)
        //{
        //    if(player.playerInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sword)
        //    {
        //        transform.localScale = _swordScale;
        //    }
        //    else
        //    {
        //        transform.localScale = _normalScale;
        //    }
        //}
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

        HitPoints hps = null;
        hps = other.GetComponent<HitPoints>();

        if (hps == null && other.GetComponent<PlayerCapsule>()) { hps = other.transform.root.GetComponent<HitPoints>(); }
        if (hps == null && other.GetComponent<Hitbox>()) { hps = other.GetComponent<Hitbox>().hitPoints; }

        if (hps)
        {

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





    Vector3 _lookAtPosition;
    public void PushIfAble()
    {
        if (hitPointsInMeleeZone.Count > 0)
        {
            _pushIfAbleList = new List<HitPoints>(hitPointsInMeleeZone);
            _pushIfAbleList = _pushIfAbleList.OrderBy(x => Vector3.Distance(player.mainCamera.transform.position, x.transform.position)).ToList();

            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic)
                for (int i = _pushIfAbleList.Count; i-- > 0;)
                    if (_pushIfAbleList[i].transform.root.GetComponent<Player>() && _pushIfAbleList[i].transform.root.GetComponent<Player>().team == player.team)
                        _pushIfAbleList.RemoveAt(i);


            //for (int i = 0; i < _pushIfAbleList.Count; i++)
            if (_pushIfAbleList.Count > 0 && player.isMine)
            {
                HitPoints hp = _pushIfAbleList[0];
                Log.Print($"MELEE distance to tar: {Vector3.Distance(hp.transform.position, player.mainCamera.transform.position)}");

                if (hp.meleeMagnetism && Vector3.Distance(hp.transform.position, player.mainCamera.transform.position) <=
                    (Player.MELEE_DAMAGE_DISTANCE + 1
                    + (player.playerInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sword ? 2.5f : 0))
                     + ((GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || player.playerInventory.isHoldingHeavy) ? Mathf.Abs(PlayerCamera.THIRD_PERSON_LOCAL_OFFSET.z) : 0))
                {
                    Log.Print($"Melee PushIfAble. Cur dis: {Vector3.Distance(hp.transform.position, player.mainCamera.transform.position)}");

                    _lookAtPosition = new Vector3(hp.biped.targetTrackingCorrectTarget.transform.position.x,
                                                movement.transform.position.y,
                                                hp.biped.targetTrackingCorrectTarget.transform.position.z);
                    movement.transform.LookAt(_lookAtPosition);

                    player.playerCamera.BlockPlayerCamera(0.3f);
                    player.movement.blockPlayerMoveInput = 0.3f;
                    player.movement.blockedMovementType = PlayerMovement.BlockedMovementType.Other;

                    if (Vector3.Distance(hp.transform.position, player.mainCamera.transform.position) > 1.5f) // arbitrary and tested distance as to which the player should be pushed harder to ensure he closes the distance for the damage to register
                    {
                        if (player.playerInventory.activeWeapon.killFeedOutput != WeaponProperties.KillFeedOutput.Sword)
                            player.GetComponent<Rigidbody>().AddForce((hp.biped.targetTrackingCorrectTarget.transform.position - movement.transform.position).normalized * Player.MELEE_PUSH, ForceMode.Impulse);
                        else
                            player.GetComponent<Rigidbody>().AddForce((hp.biped.targetTrackingCorrectTarget.transform.position - movement.transform.position).normalized * Player.MELEE_PUSH * 2.2f, ForceMode.Impulse);
                    }
                    else
                        player.GetComponent<Rigidbody>().AddForce((hp.biped.targetTrackingCorrectTarget.transform.position - movement.transform.position).normalized * (Player.MELEE_PUSH / 2), ForceMode.Impulse);
                }
            }
        }
    }





    bool _trulyObstructed;
    public bool MeleeDamage(WeaponProperties weaponProperties)
    {
        _trulyObstructed = false;

        pController.currentlyReloadingTimer = 0;
        pController.CancelReloadCoroutine();



        _damageList = new List<HitPoints>(hitPointsInMeleeZone);
        _damageList = _damageList.OrderBy(x => Vector3.Distance(player.mainCamera.transform.position, x.transform.position)).ToList();

        if (player.isMine)
            if (_damageList.Count > 0)
            {
                HitPoints hp = _damageList[0];
                Vector3 dir = (hp.transform.position - player.transform.position);
                RaycastHit hit;

                Log.Print($"Melee Damage cur dis: {Vector3.Distance(hp.transform.position, movement.transform.position)}");




                if (Physics.Raycast(player.mainCamera.transform.position,
        player.mainCamera.transform.TransformDirection(Vector3.forward), out hit, Player.MELEE_DAMAGE_DISTANCE
        + ((GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || player.playerInventory.isHoldingHeavy) ? Mathf.Abs(PlayerCamera.THIRD_PERSON_LOCAL_OFFSET.z) : 0)
        , _obstructionMask))
                {
                    Log.Print($"Melee obstruction {hit.transform.name} " +
                        $" HB Dis{Vector3.Distance(hp.transform.position, movement.transform.position)}" +
                        $" Obs Dis: {hit.distance}");

                    if (hit.distance < Vector3.Distance(hp.transform.position, movement.transform.position))
                        _trulyObstructed = true;
                }

                if (Vector3.Distance(hp.transform.position, player.mainCamera.transform.position) <=
                    (Player.MELEE_DAMAGE_DISTANCE + (player.playerInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sword ? 2 : 0)
                    + ((GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || player.playerInventory.isHoldingHeavy) ? Mathf.Abs(PlayerCamera.THIRD_PERSON_LOCAL_OFFSET.z) : 0))
                    && !_trulyObstructed)
                {

                    Log.Print($"Melee DAMAGE. Cur dis: {Vector3.Distance(hp.transform.position, player.mainCamera.transform.position)}");
                    //PrintOnlyInEditor.Log($"Melee found no true obstruction. Angle: " +
                    //    $"{Vector3.SignedAngle(hp.biped.targetTrackingCorrectTarget.transform.forward, player.transform.position - hp.biped.targetTrackingCorrectTarget.transform.position, Vector3.up)}");

                    if (Mathf.Abs(Vector3.SignedAngle(hp.biped.targetTrackingCorrectTarget.transform.forward, player.transform.position - hp.biped.targetTrackingCorrectTarget.transform.position, Vector3.up)) > 130)
                    {
                        if (player.playerInventory.activeWeapon.killFeedOutput != WeaponProperties.KillFeedOutput.Sword)
                            hp.hitboxes[0].GetComponent<PlayerHitbox>().Damage(999, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactPos: hp.transform.position, impactDir: dir, kfo: WeaponProperties.KillFeedOutput.Assasination);
                        else
                            hp.hitboxes[0].GetComponent<PlayerHitbox>().Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactPos: hp.transform.position, impactDir: dir, kfo: WeaponProperties.KillFeedOutput.Sword);
                    }
                    else
                    {



                        try
                        {
                            hp.hitboxes[0].GetComponent<ActorHitbox>().Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactPos: hp.transform.position, impactDir: dir, kfo: WeaponProperties.KillFeedOutput.Melee);
                        }
                        catch { }



                        if (weaponProperties.killFeedOutput != WeaponProperties.KillFeedOutput.Sword)
                        {

                            try
                            {
                                hp.hitboxes[0].GetComponent<PlayerHitbox>().Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactPos: hp.transform.position, impactDir: dir, kfo: WeaponProperties.KillFeedOutput.Melee);
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                hp.hitboxes[0].GetComponent<PlayerHitbox>().Damage((int)player.meleeDamage, false, player.GetComponent<PhotonView>().ViewID, damageSource: "melee", impactPos: hp.transform.position, impactDir: dir, kfo: WeaponProperties.KillFeedOutput.Sword);
                            }
                            catch { }
                        }
                    }

                    return true;
                }

                return false;
            }
            else
            {
                Log.Print("MeleeDamage None");
                RaycastHit hit;
                // Does the ray intersect any objects excluding the player layer. Ex: weapons, wall, trash cans
                if (Physics.Raycast(player.mainCamera.transform.position,
                    player.mainCamera.transform.TransformDirection(Vector3.forward), out hit, Player.MELEE_DAMAGE_DISTANCE / 2, _meleeMask))
                {
                    Log.Print($"Melee raycast hit: {hit.transform.name}");

                    if (hit.transform.gameObject.GetComponent<Rigidbody>())
                    {
                        Log.Print($"Melee pushing: {hit.transform.name}");
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
                    else if (hit.transform.gameObject.GetComponent<IceChunk>())
                    {
                        hit.transform.gameObject.GetComponent<IceChunk>().Damage((int)player.meleeDamage, false, player.photonId);
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
