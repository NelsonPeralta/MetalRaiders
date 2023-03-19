using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
	public TMP_Text text;

	public RoomInfo info;

	public void SetUp(RoomInfo _info)
	{
		info = _info;
		text.text = _info.Name;

		//Debug.Log(info.CustomProperties["gamemode"]);
		//try { text.text += $" {info.CustomProperties["gamemode"]}"; } catch { }
	}

	public void OnClick()
	{
		Launcher.instance.JoinRoom(info);
	}
}