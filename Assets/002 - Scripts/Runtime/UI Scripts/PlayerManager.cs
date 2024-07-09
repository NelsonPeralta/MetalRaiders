using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

// Metal LFS

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager playerManagerInstance;
    public List<Player> allPlayers = new List<Player>();
	public PhotonView PV;
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
		//Transform spawnpoint = SpawnManager.spawnManagerInstance.GetRandomSafeSpawnPoint();
		//controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Online Player V10"), spawnpoint.position + new Vector3(0, 2, 0), spawnpoint.rotation, 0, new object[] { PV.ViewID });
        //allPlayers.Add(controller.GetComponent<PlayerProperties>());
	}

	public void Die()
	{
		PhotonNetwork.Destroy(controller);
		CreateController();
	}

    public Player GetPlayerWithGivenPhotonId(int id)
    {
        for (int i = 0; i < allPlayers.Count; i++)
            if (allPlayers[i].GetComponent<PhotonView>().ViewID == id)
                return allPlayers[i];
        return null;
    }

    private void OnDestroy()
    {
        playerManagerInstance = null;
    }
}