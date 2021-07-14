using System.Collections;
using UnityEngine;
using Photon.Pun;

public class FullyAutomaticFire : MonoBehaviourPun
{
    public AllPlayerScripts allPlayerScripts;
    public CommonFiringActions CommonFiringActions;
    public PhotonView PV;
    public GameObjectPool gameObjectPool;

    [Header("Other Scripts")]
    public int playerRewiredID;
    public PlayerProperties pProperties;
    public PlayerController pController;
    public ThirdPersonScript tPersonController;
    public PlayerInventory pInventory;
    public WeaponProperties wProperties;
    public GeneralWeapProperties gwProperties;

    public float nextFireInterval;
    float fireInterval = 0;

    private bool ThisisShooting = false;
    private bool hasButtonDown = false;

    void Awake()
    {
        gameObjectPool = GameObjectPool.gameObjectPoolInstance;
    }

    [PunRPC]
    public void ShootAuto(Vector3 realPosition, Quaternion realRotation)
    {
        if (wProperties.isFullyAutomatic && !pController.isDualWielding && !pController.isDrawingWeapon)
        {
            Debug.Log("Spawned Bullet and player is : " + wProperties.pController.name);
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Spawn bullet from bullet spawnpoint
            var bullet = gameObjectPool.SpawnPooledBullet();

            if (photonView.IsMine)
            {
                bullet.transform.position = gwProperties.bulletSpawnPoint.transform.position;
                bullet.transform.rotation = gwProperties.bulletSpawnPoint.transform.rotation;
            }
            else
            {
                bullet.transform.position = realPosition;
                bullet.transform.rotation = realRotation;

                bullet.transform.position = gwProperties.bulletSpawnPoint.transform.position;
                bullet.transform.rotation = gwProperties.bulletSpawnPoint.transform.rotation;

            }

            //bullet.transform.position = gwProperties.bulletSpawnPoint.transform.position;
            //bullet.transform.rotation = gwProperties.bulletSpawnPoint.transform.rotation;

            bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = this.allPlayerScripts;
            bullet.gameObject.GetComponent<Bullet>().range = wProperties.range;
            bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
            bullet.gameObject.GetComponent<Bullet>().playerWhoShot = gwProperties.gameObject.GetComponent<PlayerProperties>().gameObject;
            bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
            bullet.gameObject.GetComponent<Bullet>().raycastScript = pProperties.raycastScript;
            bullet.gameObject.GetComponent<Bullet>().crosshairScript = pProperties.cScript;
            SetTeamToBulletScript(bullet.transform);
            bullet.SetActive(true);
        }


        //// BACKUP CODE
        /////
        ///
        //if (!PV.IsMine)
        //    return;
        //if (wProperties.isFullyAutomatic && !pController.isDualWielding && !pController.isDrawingWeapon)
        //{
        //    Debug.Log("Spawned Bullet and player is : " + wProperties.pController.name);
        //    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //    //Spawn bullet from bullet spawnpoint
        //    var bullet = gameObjectPool.SpawnPooledBullet();
        //    bullet.transform.position = gwProperties.bulletSpawnPoint.transform.position;
        //    bullet.transform.rotation = gwProperties.bulletSpawnPoint.transform.rotation;

        //    bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = this.allPlayerScripts;
        //    bullet.gameObject.GetComponent<Bullet>().range = wProperties.range;
        //    bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
        //    bullet.gameObject.GetComponent<Bullet>().playerWhoShot = gwProperties.gameObject.GetComponent<PlayerProperties>().gameObject;
        //    bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
        //    bullet.gameObject.GetComponent<Bullet>().raycastScript = pProperties.raycastScript;
        //    bullet.gameObject.GetComponent<Bullet>().crosshairScript = pProperties.cScript;
        //    SetTeamToBulletScript(bullet.transform);
        //    bullet.SetActive(true);
        //}
    }

    public void Update()
    {
        if (pController != null)
        {
            if (!pController.isDualWielding)
            {
                if (wProperties)
                    nextFireInterval = wProperties.timeBetweenFABullets;

                if (!ThisisShooting)
                {
                    if (pController.isShooting /*|| Script.isShooting*/)
                    {
                        BulletSpawnPoint bsp = gwProperties.bulletSpawnPoint.GetComponent<BulletSpawnPoint>();

                        Debug.Log("Trying to shoot from FAF script");
                        PV.RPC("ShootAuto", RpcTarget.All, bsp.GetRealPosition(), bsp.GetRealRotation()); // Doesnt work
                        //PV.RPC("RPC_Shoot_Projectile_Test", RpcTarget.All, bsp.GetRealPosition(), bsp.GetRealRotation()); // Works

                        StartFiringIntervalCooldown();
                        //FireAuto(false, false);
                        //PV.RPC("FireAuto", RpcTarget.All, false, false);
                    }
                }


                if (pInventory.activeWeapIs == 0)
                    if (pInventory.weaponsEquiped[0])
                        wProperties = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();
                    else
                        ;
                else if (pInventory.activeWeapIs == 1)
                    if (pInventory.weaponsEquiped[1])
                        wProperties = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();

                if (pController.player.GetButtonUp("Shoot"))
                    hasButtonDown = false;
            }
        }

        FireIntervalCooldown();
    }




    [PunRPC]
    public void FireAuto(bool thisIsShootingRight, bool thisIsShootingLeft)
    {
        if (ThisisShooting)
            return;

        // Must only spawn bullet. SFXs must only be called if isMine
        //PV.RPC("ShootAuto", RpcTarget.All);
        //ShootAuto();
        CommonFiringActions.AfterShootingAction(wProperties);
        StartFiringIntervalCooldown();
    }

    [PunRPC]
    void RPC_Shoot_Projectile_Test(Vector3 realPosition, Quaternion realRotation)
    {
        GameObject bullet = gameObjectPool.SpawnPooledBullet();

        if (photonView.IsMine)
        {
            bullet.transform.position = gameObject.transform.position;
            bullet.transform.rotation = gameObject.transform.rotation;
        }
        else
        {
            bullet.transform.position = realPosition;
            bullet.transform.rotation = realRotation;

        }

        //bullet.transform.position = gameObject.transform.position;
        //bullet.transform.rotation = gameObject.transform.rotation;
        bullet.SetActive(true);
    }

    public void SetTeamToBulletScript(Transform bullet)
    {
        // TO DO
        //  Add a variable string with all small caracters for team variable
    }

    IEnumerator Player3PSFiringAnimation()
    {
        tPersonController.anim.Play("Fire");
        yield return new WaitForEndOfFrame();
    }

    void StartFiringIntervalCooldown()
    {
        fireInterval = nextFireInterval;
        ThisisShooting = true;
    }

    void FireIntervalCooldown()
    {
        if (!ThisisShooting)
            return;
        fireInterval -= Time.deltaTime;

        if (fireInterval <= 0)
            ThisisShooting = false;
    }

}
