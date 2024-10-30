using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagBannerAutoChangeForPlayer : MonoBehaviour
{
    [SerializeField] GameObject _redBanner, _blueBanner;


    private void OnEnable()
    {
        if (transform.root.GetComponent<Player>())
        {
            _redBanner.SetActive(GameManager.instance.gameType == GameManager.GameType.CTF && transform.root.GetComponent<Player>().team == GameManager.Team.Blue);
            _blueBanner.SetActive(GameManager.instance.gameType == GameManager.GameType.CTF && transform.root.GetComponent<Player>().team == GameManager.Team.Red);
        }

        if (transform.root.GetComponent<PlayerCameraSplitScreenBehaviour>())
        {
            _redBanner.SetActive(GameManager.instance.gameType == GameManager.GameType.CTF && transform.root.GetComponent<PlayerCameraSplitScreenBehaviour>().player.team == GameManager.Team.Blue);
            _blueBanner.SetActive(GameManager.instance.gameType == GameManager.GameType.CTF && transform.root.GetComponent<PlayerCameraSplitScreenBehaviour>().player.team == GameManager.Team.Red);
        }
    }
}
