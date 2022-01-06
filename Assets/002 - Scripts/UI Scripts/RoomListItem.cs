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
	}

	public void OnClick()
	{
		Launcher.launcherInstance.JoinRoom(info);
	}
}