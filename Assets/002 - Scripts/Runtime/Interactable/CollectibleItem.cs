using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectibleCard : MonoBehaviour
{
    public enum Type { Card, Toy }

    [SerializeField] Type _type;
    [SerializeField] GameObject _toy;
    [SerializeField] List<GameObject> _cards;
    [SerializeField] string _name;
    [SerializeField] GameObject _foundFx;



    private void Awake()
    {
        if (_type == Type.Card &&
            GameManager.instance.flyingCameraMode == GameManager.FlyingCamera.Disabled &&
            GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            gameObject.SetActive(true);
        }
        else if (_type == Type.Toy && GameManager.instance.gameMode == GameManager.GameMode.Versus)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_type == Type.Card)
        {
            _toy.SetActive(false);

            foreach (var card in _cards)
            {
                card.SetActive(card.name == _name);
            }
        }
        else
        {
            _toy.SetActive(true);

            foreach (var card in _cards)
            {
                card.SetActive(false);
            }
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

            if (p == GameManager.GetRootPlayer() && p.isMine)
            {
                //if (Vector3.Distance(p.transform.position, gameObject.transform.position) > 0.25f) return;

                if (_type == Type.Card)
                    CurrentRoomManager.instance.playerDataCells[0].AddFoundCard(_name);
                else if (_type == Type.Toy)
                    CurrentRoomManager.instance.playerDataCells[0].AddFoundToy(SceneManager.GetActiveScene().buildIndex.ToString());

                _foundFx.SetActive(true);
                foreach (var card in _cards)
                    card.SetActive(false);
                _toy.SetActive(false);

                Destroy(GetComponent<Rigidbody>());
                Destroy(GetComponent<Collider>());
            }
        }
    }
}
