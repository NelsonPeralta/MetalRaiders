using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public List<Player> players = new List<Player>();
    public enum SpawnPointType { Player, Computer }
    public SpawnPointType spawnPointType;

    [SerializeField] GameObject _witness;
    [SerializeField] int _radius;
    [SerializeField] List<PlayerCapsule> _players = new List<PlayerCapsule>();
    [SerializeField] List<Collider> _colliders = new List<Collider>();

    private void Awake()
    {
        //GetComponent<SphereCollider>().radius = _radius;
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.GetComponent<PlayerCapsule>()) // TODO: Optimize
    //    {
    //        Player player = (Player)other.transform.root.GetComponent<Player>();

    //        if (!players.Contains(player))
    //        {
    //            if (GameManager.instance.isDev)
    //                _witness.SetActive(true);

    //            player.OnPlayerDeath -= OnPLayerDeath;
    //            player.OnPlayerDeath += OnPLayerDeath;
    //            players.Add(player);
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    Debug.Log($"SpawnPoint {other.name}");
    //    if (other.GetComponent<PlayerCapsule>() && players.Contains(other.transform.root.GetComponent<Player>()))
    //    {
    //        other.transform.root.GetComponent<Player>().OnPlayerDeath -= OnPLayerDeath;
    //        players.Remove(other.transform.root.GetComponent<Player>());

    //        if (players.Count == 0 && GameManager.instance.isDev)
    //        {
    //            GameObject wit = gameObject.transform.Find("Witness").gameObject;
    //            wit.SetActive(false);
    //        }
    //    }
    //}

    //void OnPLayerDeath(Player p)
    //{
    //    try
    //    {
    //        p.OnPlayerDeath -= OnPLayerDeath;
    //        players.Remove(p);
    //    }
    //    catch { }

    //    //if (players.Count == 0)
    //    //{
    //    //    GameObject wit = gameObject.transform.Find("Witness").gameObject;
    //    //    wit.SetActive(false);
    //    //}
    //}



    float _delay = 0.2f;
    private void Update()
    {
        if (_delay > 0)
        {

            _delay -= Time.deltaTime;
            if (_delay <= 0)
            {
                try
                {
                    _players = Physics.OverlapSphere(transform.position, 25, 7).ToList().Select(collider => collider.GetComponent<PlayerCapsule>()).Where(g=>g!= null).ToList();
                }
                catch { }
                //foreach (var hitCollider in _colliders)
                //{
                //    if (hitCollider.GetComponent<PlayerCapsule>() && players.Contains(hitCollider.transform.root.GetComponent<Player>()))
                //    {
                //        players.Remove(other.transform.root.GetComponent<Player>());

                //        if (players.Count == 0 && GameManager.instance.isDev)
                //        {
                //            GameObject wit = gameObject.transform.Find("Witness").gameObject;
                //            wit.SetActive(false);
                //        }
                //    }
                //}


                _delay = 0.2f;
            }
        }
    }
}