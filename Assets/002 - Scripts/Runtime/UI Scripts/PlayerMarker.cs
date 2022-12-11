using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerMarker : MonoBehaviour
{
    public enum Color { Red, Green }

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

        if (GameManager.instance.gameType.ToString().Contains("Team"))
        {
            Debug.Log(GameManager.instance.gameType.ToString());
            Debug.Log(GameManager.instance.onlineTeam.ToString());

            if (!player.isMine)
            {
                if (color == Color.Red)
                {
                    Debug.Log(player.team.ToString());
                    Debug.Log(GameManager.GetMyPlayer().team.ToString());

                    if (player.team == GameManager.GetMyPlayer().team)
                        gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log(player.team.ToString());
                    Debug.Log(GameManager.GetMyPlayer().team.ToString());
                }
            }
            else
            {
                if (color == Color.Red)
                    gameObject.SetActive(false);
            }
        }
        else
        {
            if (color == Color.Green)
                gameObject.SetActive(false);
        }
    }

    public void OnPLayerDeath_Delegate(Player player)
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void OnPlayerRespawn_Delegate(Player player)
    {
        GetComponent<MeshRenderer>().enabled = true;
    }
}
