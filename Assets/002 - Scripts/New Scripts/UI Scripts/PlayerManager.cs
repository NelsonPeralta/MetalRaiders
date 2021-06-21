using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

// Metal LFS

public class PlayerManager : MonoBehaviour
{
	PhotonView PV;

	GameObject controller;

	void Awake()
	{
		PV = GetComponent<PhotonView>();
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
		Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
		controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player V10"), spawnpoint.position + new Vector3(0, 2, 0), spawnpoint.rotation, 0, new object[] { PV.ViewID });
	}

	public void Die()
	{
		PhotonNetwork.Destroy(controller);
		CreateController();
	}
}