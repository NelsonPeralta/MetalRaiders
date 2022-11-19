using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerMarker : MonoBehaviour
{
    public enum Color { Red, Green}

    public Color color;

    [SerializeField] Player player;
    [SerializeField] Player targetPlayer;
    [SerializeField] GameObject model;
    [SerializeField] int controllerIdTarget;

    [SerializeField] Material green;
    [SerializeField] Material red;
    // Start is called before the first frame update
    private void Start()
    {
        player.OnPlayerDeath += OnPLayerDeath_Delegate;
        player.OnPlayerRespawned += OnPlayerRespawn_Delegate;
        //if(controllerIdTarget == 0)
        //{
        //    gameObject.layer = 31 - (7 + targetPlayer.controllerId * 2);
        //    gameObject.layer = 5;
        //}

        if (GameManager.instance.gameType.ToString().Contains("Team"))
        {
            Debug.Log(GameManager.instance.gameType.ToString());
            if (!player.isMine)
            {
                if(color == Color.Green)
                    gameObject.SetActive(false);
                //GetComponent<MeshRenderer>().material = red;
            }
            else
            {
                if (color == Color.Red)
                    gameObject.SetActive(false);
                //GetComponent<MeshRenderer>().material = green;
            }
        }
        else
        {
            if (color == Color.Green)
                gameObject.SetActive(false);
            //GetComponent<MeshRenderer>().material = red;
        }

        //if (GetComponent<MeshRenderer>().material = green)
        //    gameObject.layer = 5;


    }

    // Update is called once per frame
    void Update()
    {
        //if (targetPlayer)
        //{
        //    gameObject.transform.LookAt(targetPlayer.transform.position);
        //}
    }

    //IEnumerator LookForPlayer()
    //{
    //    yield return new WaitForSeconds(1);

    //    foreach (Player p in FindObjectsOfType<Player>().ToList())
    //    {
    //        if (GameManager.instance.gameType.ToString().Contains("Team"))
    //            if (p != player && p.controllerId == controllerIdTarget)
    //            {
    //                targetPlayer = p;
    //            }
    //    }
    //}

    public void OnPLayerDeath_Delegate(Player player)
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void OnPlayerRespawn_Delegate(Player player)
    {
        GetComponent<MeshRenderer>().enabled = true;
    }
}
