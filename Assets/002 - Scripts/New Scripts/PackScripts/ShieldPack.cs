using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPack : MonoBehaviour
{
    public ChildManager cManager;
    PlayerProperties pProperties;
    public GameObject packFX;

    private void Start()
    {
        cManager = GetComponent<ChildManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            pProperties = other.gameObject.GetComponent<PlayerProperties>();
            packFX = cManager.FindChildWithTagScript("Pack FX");

            if (pProperties.hasShield && pProperties.shieldSlider.value < pProperties.maxShield && pProperties.needsShieldPack)
            {
                StartCoroutine(AllowShieldRecharge());
            }
        }
    }

    IEnumerator AllowShieldRecharge()
    {
        pProperties.needsShieldPack = false;
        packFX.SetActive(false);
        cManager.FindChildWithTagScript("Motion Tracker Icon").SetActive(false);

        yield return new WaitForSeconds(5);

        pProperties.needsShieldPack = true;
        Destroy(this.gameObject);
    }
}
