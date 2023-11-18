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
    }
    private void OnTriggerEnter(Collider other)
    {
        //if (other.GetComponent<Player>() && !other.GetComponent<Player>().isDead && !other.GetComponent<Player>().isRespawning)
        //{
        //    if (other.GetComponent<Player>().isMine)
        //    {
        //        NetworkGameManager.instance.LootOvershield(other.GetComponent<Player>().pid);
        //    }
        //}






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
                            NetworkGameManager.instance.LootOvershield(other.transform.root.GetComponent<Player>().pid);
                    }
            }
            catch (System.Exception e) { Debug.LogWarning(e); }
        }
    }
}
