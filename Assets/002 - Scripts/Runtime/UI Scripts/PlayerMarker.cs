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

    private void Awake()
    {
        player.OnPlayerTeamChanged += OnPlayerTeamDelegate;
    }
    private void Start()
    {
        player.OnPlayerDeath += OnPLayerDeath_Delegate;
        player.OnPlayerRespawned += OnPlayerRespawn_Delegate;
    }

    public void OnPlayerTeamDelegate(Player player)
    {
        if (GameManager.instance.teamMode.ToString().Contains("Classic"))
        {
            Debug.Log("Player Marker");
            if (!player.isMine)
            {
                if (color == Color.Green)
                {
                    if (player.team != GameManager.GetRootPlayer().team)
                        gameObject.SetActive(false);
                }else if(color == Color.Red)
                {
                    if (player.team == GameManager.GetRootPlayer().team)
                        gameObject.SetActive(false);
                }
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
        Debug.Log("OnPLayerDeath_Delegate");
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void OnPlayerRespawn_Delegate(Player player)
    {
        GetComponent<MeshRenderer>().enabled = true;
    }
}
