using Photon.Pun;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overshield : MonoBehaviour
{
    public int tts { get { return _tts; } }

    [SerializeField] int _tts;








    private void Start()
    {
        try { NetworkGameManager.instance.overshield = this; } catch { }
        try
        {
            if (GameManager.instance.gameType == GameManager.GameType.Retro || GameManager.instance.gameType == GameManager.GameType.Swat)
                gameObject.SetActive(false);
        }
        catch { }

        if (GameManager.instance.gameType != GameManager.GameType.Retro && GameManager.instance.gameType != GameManager.GameType.Swat)
        {

            //GameTime.instance.OnGameTimeElapsedChanged -= OnGameTimeChanged;
            //GameTime.instance.OnGameTimeElapsedChanged += OnGameTimeChanged;
        }
    }




    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ManCannon {other}");
        if (other.gameObject.activeInHierarchy)
        {
            try
            {
                if (other.GetComponent<PlayerCapsule>())
                    if (other.transform.root.GetComponent<Player>() &&
                        !other.transform.root.GetComponent<Player>().isDead &&
                        !other.transform.root.GetComponent<Player>().isRespawning)
                    {
                        Debug.Log($"ManCannon LAUNCH!");
                        if (other.transform.root.GetComponent<Player>().isMine)
                            NetworkGameManager.instance.LootOvershield(other.transform.root.GetComponent<Player>().photonId);
                    }
            }
            catch (System.Exception e) { Debug.LogWarning(e); }
        }
    }
}
