using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashAttack : MonoBehaviour
{
    public int damage;
    public float constantForceSpeed;
    public Transform explosionPrefab;    

    [Header("Audio")]
    public AudioSource impactSound;
    bool damageDealt;

    private void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * constantForceSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.GetComponent<PlayerProperties>() != null && other.gameObject.layer != 23)
        {
            PlayerProperties hitPlayerProperties = other.gameObject.GetComponent<PlayerProperties>();

            if (!damageDealt)
            {
                hitPlayerProperties.BleedthroughDamage(damage, false, 99);
                damageDealt = true;
                Destroy(gameObject);
            }
        }
    }
}
