using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CollectibleCard : MonoBehaviour
{
    [SerializeField] List<GameObject> _cards;
    [SerializeField] string _name;
    [SerializeField] GameObject _foundFx;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var card in _cards)
        {
            card.SetActive(card.name == _name);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>() || other.gameObject.transform.root.GetComponent<Player>())
        {
            Player p = other.gameObject.GetComponent<Player>(); if (p == null) p = other.gameObject.transform.root.GetComponent<Player>();

            if (p == GameManager.GetRootPlayer())
            {
                CurrentRoomManager.instance.extendedPlayerData[0].AddFoundCard(_name);

                _foundFx.SetActive(true);
                foreach (var card in _cards)
                    card.SetActive(false);

                Destroy(GetComponent<Rigidbody>());
                Destroy(GetComponent<Collider>());
            }
        }
    }
}
