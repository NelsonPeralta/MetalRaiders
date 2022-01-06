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
    public int defaultAmmo;
    [SerializeField] int ammoInThisPack;

    [Header("Classes")]
    public TextMeshPro ammoText;
    public OnlineAmmoPackSpawnPoint onlineAmmoPackSpawnPoint;

    [Header("Other Classes")]
    public PlayerProperties playerProperties;

    private void Start()
    {
        weaponPool = WeaponPool.weaponPoolInstance;
        onlineGameTime = OnlineGameTime.onlineGameTimeInstance;
        ammoInThisPack = GetNewAmmo();
        UpdateAmmoText();
    }


    private void OnTriggerEnter(Collider other)
    {

    }

    public void EnablePack()
    {
        ammoInThisPack = GetNewAmmo();
        UpdateAmmoText();
        gameObject.SetActive(true);
    }

    int GetNewAmmo()
    {
        int newAmmo = 0;
        if (!randomAmmo)
            newAmmo = defaultAmmo;
        else
            newAmmo = (int)Mathf.Floor(Random.Range(1, (defaultAmmo * 0.7f)));
        if (newAmmo <= 0)
            newAmmo = 1;
        return newAmmo;
    }

    void UpdateAmmoText()
    {
        ammoText.text = ammoInThisPack.ToString();
    }

    public void SetRandomAmmoAsDefault()
    {
        randomAmmo = true;
        ammoInThisPack = GetNewAmmo();
        UpdateAmmoText();
    }

    public int GetAmmo()
    {
        return ammoInThisPack;
    }
}
