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

    int RandomAmmo()
    {
        int ranAmmo = (int)Mathf.Floor(Random.Range(1, (defaultAmmo * 0.8f)));
        return ranAmmo;
    }

    void UpdateAmmoText()
    {
        ammoText.text = ammoInThisPack.ToString();
    }
}
