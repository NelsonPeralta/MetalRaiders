using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBullet : MonoBehaviour
{
    public Player player { get { return _player; } set { _player = value; _spawnDir = player.transform.position; } }
    public int speed { get { return _speed; } set { _speed = value * 10; } }
    public int range { get { return _range; } set { _range = value; } }
    public int damage { get { return _damage; } set { _damage = value; } }
    public Vector3 spawnDir { get { return _spawnDir; } }

    [SerializeField] Player _player;
    [SerializeField] int _speed;
    [SerializeField] int _range;
    [SerializeField] int _damage;

    Vector3 _spawnDir;

    private void OnEnable()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward.normalized * speed);
    }
    private void OnDisable()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
    private void Start()
    {
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);

        if (collision.gameObject.GetComponent<Rigidbody>())
            collision.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

        //GameObject genericHit = FindObjectOfType<GameObjectPool>().SpawnPooledGenericHit();
        //genericHit.transform.position = collision.contacts[0].point;
        //genericHit.SetActive(true);

        bool damageDealt = false;
        Vector3 finalHitPoint = collision.contacts[0].point;
        GameObject finalHitObject = collision.gameObject;
        WeaponProperties weaponProperties = player.playerInventory.activeWeapon;
        IDamageable finalHitDamageable = finalHitObject.GetComponent<IDamageable>();




        try
        {
            if (GameManager.instance.gameType.ToString().Contains("Team"))
            {

                if (finalHitObject.GetComponent<PlayerHitbox>())
                {
                    if (finalHitObject.GetComponent<PlayerHitbox>().player.team != player.team)
                    {
                        if (finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<PlayerHitbox>().player.isDead && !finalHitObject.GetComponent<PlayerHitbox>().player.isRespawning)
                        {
                            PlayerHitbox hitbox = finalHitObject.GetComponent<PlayerHitbox>();
                            Player player = hitbox.player.GetComponent<Player>();
                            bool wasHeadshot = false;
                            bool wasNutshot = false;
                            if (weaponProperties.isHeadshotCapable && (hitbox.isHead || hitbox.isNuts))
                            {
                                int maxShieldPoints = player.maxHitPoints - player.maxHealthPoints;

                                if (maxShieldPoints > 0 && (player.hitPoints <= player.maxHealthPoints))
                                    damage = player.maxHealthPoints;

                                if (weaponProperties.reticuleType == WeaponProperties.ReticuleType.Sniper)
                                    damage = (int)(damage * weaponProperties.headshotMultiplier);

                                wasHeadshot = hitbox.isHead;
                                wasNutshot = hitbox.isNuts;

                            }

                            if (player.PV.IsMine)
                            {
                                if (weaponProperties.codeName != null)
                                    finalHitDamageable.Damage(damage, wasHeadshot, player.GetComponent<PhotonView>().ViewID, finalHitPoint, impactDir: spawnDir, damageSource: weaponProperties.codeName, isGroin: wasNutshot);
                                else
                                    finalHitDamageable.Damage(damage, wasHeadshot, player.GetComponent<PhotonView>().ViewID, finalHitPoint, isGroin: wasNutshot);
                            }

                            damageDealt = true;
                        }
                    }
                    else
                    {
                        GameObject genericHit = FindObjectOfType<GameObjectPool>().SpawnPooledGenericHit();
                        genericHit.transform.position = finalHitPoint;
                        genericHit.SetActive(true);

                        damageDealt = true;
                    }
                }
                else
                {
                    try
                    {
                        if (!finalHitObject.GetComponent<PlayerHitbox>())
                            try
                            {
                                finalHitDamageable.Damage(damage, false, player.pid);

                            }
                            catch
                            {
                                finalHitDamageable.Damage(damage);
                            }
                    }
                    catch { }
                    GameObject genericHit = FindObjectOfType<GameObjectPool>().SpawnPooledGenericHit();
                    genericHit.transform.position = finalHitPoint;
                    genericHit.SetActive(true);

                    damageDealt = true;
                }

            }
            else
            {
                if (finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<PlayerHitbox>().player.isDead && !finalHitObject.GetComponent<PlayerHitbox>().player.isRespawning)
                {
                    PlayerHitbox hitbox = finalHitObject.GetComponent<PlayerHitbox>();
                    Player player = hitbox.player.GetComponent<Player>();
                    bool wasHeadshot = false;
                    bool wasNutshot = false;
                    if (weaponProperties.isHeadshotCapable && (hitbox.isHead || hitbox.isNuts))
                    {
                        int maxShieldPoints = player.maxHitPoints - player.maxHealthPoints;

                        if (maxShieldPoints > 0 && (player.hitPoints <= player.maxHealthPoints))
                            damage = player.maxHealthPoints;

                        if (weaponProperties.reticuleType == WeaponProperties.ReticuleType.Sniper)
                            damage = (int)(damage * weaponProperties.headshotMultiplier);

                        wasHeadshot = hitbox.isHead;
                        wasNutshot = hitbox.isNuts;
                    }

                    if (player.PV.IsMine)
                    {
                        if (weaponProperties.codeName != null)
                            finalHitDamageable.Damage(damage, wasHeadshot, player.GetComponent<PhotonView>().ViewID, finalHitPoint, impactDir: spawnDir, weaponProperties.codeName, isGroin: wasNutshot);
                        else
                            finalHitDamageable.Damage(damage, wasHeadshot, player.GetComponent<PhotonView>().ViewID, finalHitPoint, isGroin: wasNutshot);
                    }

                    damageDealt = true;
                }
                else if (!finalHitObject.GetComponent<PlayerHitbox>() && !finalHitObject.GetComponent<CapsuleCollider>() && !finalHitObject.GetComponent<AIHitbox>() && !finalHitObject.GetComponent<CharacterController>())
                {
                    try
                    {
                        if (!finalHitObject.GetComponent<PlayerHitbox>())
                            try
                            {
                                finalHitDamageable.Damage(damage, false, player.pid);

                            }
                            catch
                            {
                                finalHitDamageable.Damage(damage);
                            }
                    }
                    catch { }
                    GameObject genericHit = FindObjectOfType<GameObjectPool>().SpawnPooledGenericHit();
                    genericHit.transform.position = finalHitPoint;
                    genericHit.SetActive(true);

                    damageDealt = true;
                }
            }
            //gameObject.SetActive(false);
        }
        catch { }





        gameObject.SetActive(false);
    }
}
