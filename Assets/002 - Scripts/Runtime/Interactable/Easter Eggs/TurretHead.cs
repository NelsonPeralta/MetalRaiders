using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretHead : MonoBehaviour
{
    public Player player;

    public HitPoints hitPoints;
    [SerializeField] Transform _pivot, _bulletSpawnPoint;
    [SerializeField] GameObject _muzzleFlash;
    [SerializeField] GameObject _projectile;

    float _cooldown, _ttl, _findNewTargetCooldown;
    Vector3 _tarPos = new Vector3(0, -1, 0);

    private void OnEnable()
    {
        _findNewTargetCooldown = 2;
        _ttl = 30;
        _cooldown = 1.9f;
    }

    private void Update()
    {
        FindNewTarget();

        try { if (hitPoints.GetComponent<Actor>().hitPoints <= 0) hitPoints = null; } catch { }


        if (!hitPoints) return;
        try
        {
            _tarPos = new Vector3(hitPoints.transform.position.x,
               hitPoints.transform.position.y - 1,
               hitPoints.transform.position.z);
            _pivot.transform.LookAt(_tarPos);
        }
        catch { }





        if (_cooldown > 0)
        {
            _cooldown -= Time.deltaTime;

            if (_cooldown <= 0)
            {
                _muzzleFlash.SetActive(true);
                GetComponent<AudioSource>().Play();
                GameObject g = Instantiate(_projectile, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation);
                g.GetComponent<ExplosiveProjectile>().player = player;

                _cooldown = 1.9f;
            }
            else if (_cooldown <= 1f)
            {
                _muzzleFlash.SetActive(false);
            }
        }

        if (_ttl > 0)
        {
            _ttl -= Time.deltaTime;

            if (_ttl <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }



    void FindNewTarget()
    {
        // If we are in a room and we are not the Host, stop
        if (PhotonNetwork.InRoom) if (!PhotonNetwork.IsMasterClient) return;
        if (hitPoints) return;

        Log.Print(() =>"Finding new Target Transform");

        if (NetworkSwarmManager.instance)
        {
            int pid = NetworkSwarmManager.instance.GetRandomAliveActorId();
            if (pid > 0)
            {
                NetworkSwarmManager.instance.SetTurretTarget(transform.position, pid);
            }
        }

        _findNewTargetCooldown = 2;
    }
}
