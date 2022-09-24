using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPack : MonoBehaviour
{
    Player pProperties;
    public GameObject packFX;

    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "player")
        {
            pProperties = other.gameObject.GetComponent<Player>();

            //if (pProperties.hasShield && pProperties.shieldSlider.value < pProperties.maxShield && pProperties.needsShieldPack)
            //{
            //    StartCoroutine(AllowShieldRecharge());
            //}
        }
    }

    IEnumerator AllowShieldRecharge()
    {
        //pProperties.needsShieldPack = false;
        packFX.SetActive(false);

        yield return new WaitForSeconds(5);

        //pProperties.needsShieldPack = true;
        Destroy(this.gameObject);
    }
}
