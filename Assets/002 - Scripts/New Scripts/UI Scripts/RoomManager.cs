using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    void Awake()
    {
        if (Instance)
        {
            Debug.Log("There is a Room Manager Instance");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("Scene Loaded");

        if (scene.buildIndex > 0) // We're in the game scene
        {
            try
            {
                Debug.Log($"Master CLient: {PhotonNetwork.MasterClient}");
                Debug.Log($"{PhotonNetwork.CurrentRoom.CustomProperties["mode"]}");
                string mode = PhotonNetwork.CurrentRoom.CustomProperties["mode"].ToString();

                Debug.Log($"Is there a Player Manager: {PlayerManager.playerManagerInstance}");
                if (!PlayerManager.playerManagerInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
                if (!GameObjectPool.gameObjectPoolInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ObjectPool"), Vector3.zero, Quaternion.identity);
                if (!WeaponPool.weaponPoolInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineWeaponPool"), Vector3.zero + new Vector3(0, 5, 0), Quaternion.identity);
                if (!OnlineGameTime.onlineGameTimeInstance)
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineGameTime"), Vector3.zero + new Vector3(0, -100, 0), Quaternion.identity);
                //if (!OnlineMultiplayerManager.multiplayerManagerInstance && mode == "multiplayer")
                //    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineMultiplayerManager"), Vector3.zero + new Vector3(0, -100, 0), Quaternion.identity);
                if (!OnlineSwarmManager.onlineSwarmManagerInstance && mode == "swarm")
                {
                    //PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineSwarmManager"), Vector3.zero + new Vector3(0, -100, 0), Quaternion.identity);
                    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "OnlineAIPool"), Vector3.zero + new Vector3(0, -100, 0), Quaternion.identity);
                }
            }
            catch
            {

            }
        }
        else
        {
            try
            {
                //if (PlayerManager.playerManagerInstance)
                //    PhotonNetwork.Destroy(PlayerManager.playerManagerInstance.PV);
                //if (GameObjectPool.gameObjectPoolInstance)
                //    PhotonNetwork.Destroy(GameObjectPool.gameObjectPoolInstance.PV);
                //if (WeaponPool.weaponPoolInstance)
                //    PhotonNetwork.Destroy(WeaponPool.weaponPoolInstance.PV);
                //if (OnlineGameTime.onlineGameTimeInstance)
                //    PhotonNetwork.Destroy(OnlineGameTime.onlineGameTimeInstance.PV);
                //if (MultiplayerManager.multiplayerManagerInstance)
                //    PhotonNetwork.Destroy(MultiplayerManager.multiplayerManagerInstance.PV);
            }
            catch
            {

            }
        }
    }
}