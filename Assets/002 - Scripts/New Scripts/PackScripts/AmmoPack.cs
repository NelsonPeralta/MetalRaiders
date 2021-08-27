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
    public bool randomAmmo;
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
        if (!randomAmmo)
            defaultAmmo = ammoInThisPack;
        else
            defaultAmmo = GetRandomAmmo();
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

    int GetRandomAmmo()
    {
        int ranAmmo = (int)Mathf.Floor(Random.Range(1, (defaultAmmo * 0.8f)));
        return ranAmmo;
    }

    void UpdateAmmoText()
    {
        ammoText.text = ammoInThisPack.ToString();
    }

    public void SetRandomAmmoAsDefault()
    {
        defaultAmmo = GetRandomAmmo();
        UpdateAmmoText();
    }

    [PunRPC]
    public void SetRandomAmmoAsDefault_RPC()
    {
        defaultAmmo = GetRandomAmmo();
        UpdateAmmoText();
    }
}
