using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    public Player player { get { return _player; } set { _player = value; } }
    public int force { get { return _force; } set { _force = value; } }
    public bool useConstantForce { get { return _useConstantForce; } set { _useConstantForce = value; } }

    [SerializeField] Player _player;
    [SerializeField] int _force;
    [SerializeField] bool _useConstantForce;
    [SerializeField] Transform explosionPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (!useConstantForce)
            GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * force);
    }

    // Update is called once per frame
    void Update()
    {
        if (useConstantForce)
            GetComponent<Rigidbody>().AddForce
                (gameObject.transform.forward * force);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 9)
            Explosion();
    }

    void Explosion()
    {
        Transform e = Instantiate(explosionPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);
        e.GetComponent<Explosion>().player = player;
        gameObject.SetActive(false);
    }
}
