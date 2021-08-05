using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class AmmoPack : MonoBehaviour
{
    public PhotonView PV;

    [Header("Single")]
    public WeaponPool weaponPool;
    public OnlineGameTime onlineGameTime;

    [Header("Ammo")]
    public string ammoType;
    int defaultAmmo;
    public int ammoInThisPack;

    [Header("Classes")]
    public TextMeshPro ammoText;
    public OnlineAmmoPackSpawnPoint spawnPoint;

    [Header("Other Classes")]
    public PlayerProperties playerProperties;

    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;
        onlineGameTime = OnlineGameTime.onlineGameTimeInstance;
        defaultAmmo = ammoInThisPack;
        UpdateAmmoText();
    }


    private void OnTriggerEnter(Collider other)
    {

    }

    public void EnablePack()
    {
        ammoInThisPack = defaultAmmo;
        UpdateAmmoText();
        gameObject.SetActive(true);
    }

    void DisableAmmoPack()
    {
        Debug.Log("Disabling ammo pack. Weapon pool: " + WeaponPool.weaponPoolInstance);
        if (!weaponPool)
            weaponPool = WeaponPool.weaponPoolInstance;
        for (int i = 0; i < weaponPool.allAmmoPacks.Count; i++)
            if (weaponPool.allAmmoPacks[i] == gameObject)
            {
                Debug.Log($"Disabling Ammo Pack: {i}");
                playerProperties.allPlayerScripts.weaponPickUp.DisableAmmoPackWithRPC(i);
            }
    }

    int RandomAmmo()
    {
        int ranAmmo = (int)Mathf.Floor(Random.Range(1, (defaultAmmo * 0.8f)));
        return ranAmmo;
    }

    void UpdateAmmoText()
    {
        ammoText.text = ammoInThisPack.ToString();
    }

    public void StartRespawn()
    {
        spawnPoint.StartRespawn();
        gameObject.SetActive(false);
    }
}
