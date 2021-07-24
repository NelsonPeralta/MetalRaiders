using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

// Metal LFS

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager playerManagerInstance;
    public List<PlayerProperties> allPlayers = new List<PlayerProperties>();
	PhotonView PV;
	GameObject controller;

	void Awake()
	{
		PV = GetComponent<PhotonView>();

        if (playerManagerInstance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        playerManagerInstance = this;
    }

	void Start()
	{
		if(PV.IsMine)
		{
			CreateController();
		}
	}

	void CreateController()
	{
		Transform spawnpoint = SpawnManager.spawnManagerInstance.GetGenericSpawnpoint();
		controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Online Player V10"), spawnpoint.position + new Vector3(0, 2, 0), spawnpoint.rotation, 0, new object[] { PV.ViewID });
        //allPlayers.Add(controller.GetComponent<PlayerProperties>());
	}

	public void Die()
	{
		PhotonNetwork.Destroy(controller);
		CreateController();
	}
}