using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System;
using Newtonsoft.Json;
using Photon.Realtime;
using Rewired.Editor.Libraries.Ionic.Zlib;
using static UnityEditor.PlayerSettings;

public class NetworkGameManager : MonoBehaviourPunCallbacks
{
    public static NetworkGameManager instance { get { return _instance; } }

    Overshield _overshield;
    PhotonView _pv;

    static NetworkGameManager _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Debug.Log($"NetworkGameManager Awake");
            _pv = GetComponent<PhotonView>();
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void Start()
    {
        Debug.Log($"NetworkGameManager Start");
        //if(instance != null)
        //    Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        NetworkGameManager.instance.SendLocalPlayerDataToMasterClient();

    }


    public Overshield overshield
    {
        get { return _overshield; }
        set { _overshield = value; }
    }

    private void OnDestroy()
    {
        Debug.Log($"NetworkGameManager OnDestroy");
    }

    public override void OnLeftRoom()
    {
        Debug.Log($"NetworkGameManager OnLeftRoom");
        //base.OnLeftRoom();

        //Destroy(gameObject);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        GameManager.instance.connection = GameManager.Connection.Offline;
        base.OnDisconnected(cause);
        Debug.Log($"Disconnected: {cause}");
    }


    // Menu
    #region

    [PunRPC]
    public void SendGameParams(Dictionary<string, string> p = null, bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            Dictionary<string, string> ps = new Dictionary<string, string>();

            ps.Add("roomtype", CurrentRoomManager.instance.roomType.ToString());


            ps.Add("gamemode", GameManager.instance.gameMode.ToString());
            ps.Add("gametype", GameManager.instance.gameType.ToString());
            ps.Add("leveltoloadindex", Launcher.instance.levelToLoadIndex.ToString());
            ps.Add("teammode", GameManager.instance.teamMode.ToString());
            //ps.Add("teamdict", string.Join(Environment.NewLine, GameManager.instance.teamDict));
            //ps.Add("teamdict", JsonConvert.SerializeObject(GameManager.instance.teamDict));
            ps.Add("nbLocalPlayersDict", JsonConvert.SerializeObject(CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict));


            //Debug.Log($"SendGameParams {GameManager.instance.teamDict}");

            _pv.RPC("SendGameParams", RpcTarget.All, ps, false);
        }
        else if (!caller && !PhotonNetwork.IsMasterClient)
        {
            try { GameManager.instance.gameMode = (GameManager.GameMode)System.Enum.Parse(typeof(GameManager.GameMode), p["gamemode"]); } catch (System.Exception e) { Debug.Log(e); }
            try { GameManager.instance.gameType = (GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), p["gametype"]); } catch (System.Exception e) { Debug.Log(e); }
            try { Launcher.instance.levelToLoadIndex = (System.Int16.Parse(p["leveltoloadindex"])); } catch (System.Exception e) { Debug.Log(e); }
            try { GameManager.instance.teamMode = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), p["teammode"]); } catch (System.Exception e) { Debug.Log(e); }
            //try
            //{
            //    GameManager.instance.teamDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(p["teamdict"]);
            //    Debug.Log(GameManager.instance.teamDict);
            //}
            //catch { }

            try
            {
                CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(p["nbLocalPlayersDict"]);
                Debug.Log(CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict);
            }
            catch { }

            //CurrentRoomManager.instance.roomType = JsonConvert.DeserializeObject<CurrentRoomManager.RoomType>(p["roomType"]);


            //NetworkGameManager.instance.SendLocalPlayerDataToMasterClient();

        }
    }

    [PunRPC]

    public void ChangePlayerTeam(Dictionary<int, int> d, bool caller = true)
    {
        if (caller)
        {
            _pv.RPC("ChangePlayerTeam", RpcTarget.AllViaServer, d, false);
        }
        else
        {
            foreach (KeyValuePair<int, int> kvp in d)
            {
                Debug.Log($"Player {kvp.Key} wants to change team: {(GameManager.Team)kvp.Value}");
                CurrentRoomManager.GetPlayerDataWithId(kvp.Key).team = (GameManager.Team)kvp.Value;
            }
            foreach (Transform child in Launcher.instance.namePlatesParent) child.GetComponent<PlayerNamePlate>().UpdateColorPalette();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) { SendLocalPlayerDataToMasterClient(); }
    }


    [PunRPC]
    public void SendLocalPlayerDataToMasterClient(Dictionary<string, int> d = null, bool caller = true)
    {
        if (caller && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("SendLocalPlayerData");
            Dictionary<string, int> _d = new Dictionary<string, int>();

            _d.Add(PhotonNetwork.NickName, GameManager.instance.nbLocalPlayersPreset);
            _pv.RPC("SendLocalPlayerDataToMasterClient", RpcTarget.MasterClient, _d, false);
        }
        else if (!caller && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Received SendLocalPlayerData");
            Debug.Log(d.Keys.First());
            Debug.Log(d[d.Keys.First()]);

            if (CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.ContainsKey(d.Keys.First()))
                CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict[d.Keys.First()] = d[d.Keys.First()];
            else
                CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict.Add(d.Keys.First(), d[d.Keys.First()]);

            CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict = CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict;

            Debug.Log(CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict);

            SendLocalPlayerDataToEveryone();
            //SendGameParams();
        }
    }

    [PunRPC]
    public void SendLocalPlayerDataToEveryone(Dictionary<string, int> d = null, bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("SendLocalPlayerData TO EVERYONE");

            _pv.RPC("SendLocalPlayerDataToEveryone", RpcTarget.All, CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict, false);
        }
        else if (!caller && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("SendLocalPlayerData FROM MASTER CLIENT");

            CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict = d;
        }
    }

    [PunRPC]
    public void EndGame(bool caller = true)
    {
        if (caller)
        {
            Debug.Log("NetworkGameManager EndGame");
            _pv.RPC("EndGame", RpcTarget.AllViaServer, false);
        }
        else if (!caller)
        {
            if (CurrentRoomManager.instance.gameOver) return;

            GameManager.instance.previousScenePayloads.Add(GameManager.PreviousScenePayload.OpenCarnageReport);
            GameManager.instance.previousScenePayloads.Add(GameManager.PreviousScenePayload.ResetPlayerDataCells);

            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                MultiplayerManager.instance.EndGame();
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                SwarmManager.instance.EndGame();
        }

    }

    [PunRPC]
    public void AddPlayerJoinedCount(bool caller = true)
    {
        if (caller)
        {
            _pv.RPC("AddPlayerJoinedCount", RpcTarget.AllViaServer, false);
        }
        else if (!caller)
        {
            CurrentRoomManager.instance.nbPlayersJoined++;
        }

    }

    [PunRPC]
    public void AddPlayerLoadedScene(bool caller = true)
    {
        if (caller)
        {
            _pv.RPC("AddPlayerLoadedScene", RpcTarget.AllViaServer, false);
        }
        else if (!caller)
        {
            CurrentRoomManager.instance.playersLoadedScene++;
        }

    }

    #endregion



    // Methods
    #region
    public void UpdateTeamMode(string tm)
    {
        GameManager.instance.teamMode = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), tm);
    }


    public void UpdateSwarmDifficulty(int ei)
    {
        _pv.RPC("UpdateSwarmDifficulty_RPC", RpcTarget.All, ei);

    }

    public static void StickGrenadeOnPlayer(int nadeIndex, int playerId, Vector3 gPos)
    {
        Debug.Log($"NETWORK GAME MANAGER StickGrenadeOnPLayer. of player {playerId}");
        instance._pv.RPC("StickGrenadeOnPlayer_RPC", RpcTarget.All, nadeIndex, playerId, gPos);
    }

    // Lootable Weapons
    #region
    public void EnableLootableWeapon(Vector3 position)
    {
        Debug.Log("EnableLootableWeapon");
        _pv.RPC("EnableLootableWeapon_RPC", RpcTarget.All, position);
    }
    public void DisableLootableWeapon(Vector3 position)
    {
        _pv.RPC("DisableLootableWeapon_RPC", RpcTarget.All, position);
    }
    public void RelocateLootableWeapon(Vector3 position, Quaternion rotation)
    {
        _pv.RPC("RelocateLootableWeapon_RPC", RpcTarget.All, position, rotation);
    }

    public void UpdateLootableWeaponData(Vector3 spp, Dictionary<string, string> param)
    {
        _pv.RPC("UpdateLootableWeaponData_RPC", RpcTarget.All, spp, param);
    }

    public void AddForceLootableWeapon(Vector3 spp, Vector3 dir)
    {
        _pv.RPC("AddForceLootableWeapon_RPC", RpcTarget.All, spp, dir);
    }

    public static void SpawnNetworkWeapon(WeaponProperties weap, Vector3 spp, Vector3 fDir, int? currAmmo = null, int? spareAmmo = null)
    {
        GameObject[] allWeap = weap.player.playerInventory.allWeaponsInInventory;
        int firstWeapIndex = Array.IndexOf(allWeap, weap.gameObject);
        int firstWeapCurrAmmo = weap.loadedAmmo; int firstWeapSpareAmmo = weap.spareAmmo;

        if (currAmmo != null) firstWeapCurrAmmo = (int)currAmmo;
        if (spareAmmo != null) firstWeapSpareAmmo = (int)spareAmmo;

        if (firstWeapCurrAmmo + firstWeapSpareAmmo > 0)
            NetworkGameManager.instance._pv.RPC("SpawnNetworkWeapon_RPC",
                RpcTarget.All, firstWeapIndex, spp, fDir, firstWeapCurrAmmo, firstWeapSpareAmmo);
    }

    public static void SpawnNetworkWeaponOnPlayerDeath(WeaponProperties firstWeapon, WeaponProperties secondWeapon,
         Vector3 firstWeapSpp, Vector3 firstWeapSdir, Vector3 secondWeapSpp)
    {
        GameObject[] allWeap = firstWeapon.player.playerInventory.allWeaponsInInventory;
        int firstWeapIndex = Array.IndexOf(allWeap, firstWeapon.gameObject);
        int secondtWeapIndex = Array.IndexOf(allWeap, secondWeapon.gameObject);

        int firstWeapCurrAmmo = firstWeapon.loadedAmmo; int secondWeapCurrAmmo = secondWeapon.loadedAmmo;
        int firstWeapSpareAmmo = firstWeapon.spareAmmo; int secondWeapSpareAmmo = secondWeapon.spareAmmo;

        Debug.Log(firstWeapIndex); Debug.Log(secondtWeapIndex);

        FindObjectOfType<NetworkGameManager>()._pv.RPC("SpawnNetworkWeaponOnPlayerDeath_RPC",
            RpcTarget.All, firstWeapIndex, secondtWeapIndex, firstWeapCurrAmmo, firstWeapSpareAmmo,
            secondWeapCurrAmmo, secondWeapSpareAmmo, firstWeapSpp, firstWeapSdir, secondWeapSpp);
    }


    #endregion

    // Explosive Barrel
    #region
    public void DamageExplosiveBarrel(Vector3 position, int val)
    {
        _pv.RPC("DamageExplosiveBarrel_RPC", RpcTarget.All, position, val);
    }

    public void ResetAllExplosiveBarrels()
    {
        _pv.RPC("ResetAllExplosiveBarrels_RPC", RpcTarget.All);
    }

    public void EnableExplosiveBarrel(Vector3 position)
    {
        _pv.RPC("EnableExplosiveBarrel_RPC", RpcTarget.All, position);
    }
    public void RelocateExplosiveBarrel(Vector3 position, Quaternion rotation)
    {
        _pv.RPC("RelocateExplosiveBarrel_RPC", RpcTarget.All, position, rotation);
    }
    #endregion

    public void DamageIceChunk(Vector3 position, int val)
    {
        Debug.Log(val);
        _pv.RPC("DamageIceChunk_RPC", RpcTarget.AllViaServer, position, val);
    }

    public void UpdatePlayerTeam(string t, string pn)
    {
        _pv.RPC("UpdatePlayerTeam_RPC", RpcTarget.All, t, pn);
    }

    public void StartLootableWeaponRespawn(Vector3 v)
    {
        _pv.RPC("StartLootableWeaponRespawn_RPC", RpcTarget.All, v);
    }
    #endregion


    // Explosive Barrel
    #region

    public void DisableAmmoPack(Vector3 sp)
    {
        _pv.RPC("DisableAmmoPack_RPC", RpcTarget.All, sp);
    }

    public void EnableGrenadePacks()
    {
        _pv.RPC("EnableGrenadePacks_RPC", RpcTarget.All);
    }

    #endregion

    // Multiplayer
    public void AddPlayerPoint(int pid)
    {
        _pv.RPC("AddPlayerPoint_RPC", RpcTarget.All, pid);
    }

    public void NextHillLocation()
    {
        _pv.RPC("NextHillLocation_RPC", RpcTarget.All);
    }

    public void AskHostToTriggerInteractableObject(Vector3 pos, int? pid = null)
    {
        if (PhotonNetwork.InRoom)
        {
            _pv.RPC("AskHostToTriggerInteractableObject_RPC", RpcTarget.MasterClient, pos, pid);
        }
        else
        {
            TriggerInteractableObject_RPC(pos, pid);
        }
    }








    // RPCs
    #region


    [PunRPC]
    void UpdateTeamMode_RPC(string t)
    {
        GameManager.TeamMode tm = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), t);

        GameManager.instance.teamMode = tm;
    }
    #endregion

    // Lootable Weapons
    #region
    [PunRPC]
    void DisableLootableWeapon_RPC(Vector3 position)
    {
        foreach (LootableWeapon lw in GameManager.instance.lootableWeapons)
        {
            if (lw.spawnPointPosition == position)
                lw.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    void EnableLootableWeapon_RPC(Vector3 position)
    {
        foreach (LootableWeapon lw in GameManager.instance.lootableWeapons)
        {
            if (lw.spawnPointPosition == position)
            {
                lw.gameObject.SetActive(true);
                lw.transform.position = lw.spawnPointPosition;
                lw.transform.rotation = lw.spawnPointRotation;
            }
        }
    }

    [PunRPC]
    void RelocateLootableWeapon_RPC(Vector3 position, Quaternion rotation)
    {
        foreach (LootableWeapon lw in GameManager.instance.lootableWeapons)
        {
            if (lw.spawnPointPosition == position)
            {
                lw.GetComponent<Rigidbody>().velocity *= 0;

                lw.transform.position = position;
                lw.transform.rotation = rotation;
            }
        }
    }

    [PunRPC]
    void UpdateLootableWeaponData_RPC(Vector3 spp, Dictionary<string, string> param)
    {
        foreach (LootableWeapon lw in GameManager.instance.lootableWeapons)
        {
            if (lw.spawnPointPosition == spp)
            {
                lw.UpdateData(param);
                break;
            }
        }
    }

    [PunRPC]
    void AddForceLootableWeapon_RPC(Vector3 spp, Vector3 dir)
    {
        foreach (LootableWeapon lw in GameManager.instance.lootableWeapons)
        {
            if (lw.spawnPointPosition == spp)
            {
                lw.GetComponent<Rigidbody>().AddForce(dir * 200);
            }
        }
    }

    [PunRPC]
    void SpawnNetworkWeapon_RPC(int firstWeapIndex, Vector3 spp, Vector3 fDir, int firstWeapCurrAmmo, int firstWeapSpareAmmo)
    {
        GameObject[] weapInv = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory;
        //LootableWeapon firstWeapon = Instantiate(weapInv[firstWeapIndex].GetComponent<WeaponProperties>().weaponRessource,
        // spp, Quaternion.identity).GetComponent<LootableWeapon>();

        //try { firstWeapon.name = firstWeapon.name.Replace("(Clone)", ""); } catch (System.Exception e) { Debug.Log(e); }
        //try { firstWeapon.transform.position = spp; } catch (System.Exception e) { Debug.Log(e); }
        //try { firstWeapon.spawnPointPosition = spp; } catch (System.Exception e) { Debug.Log(e); }
        //try { firstWeapon.GetComponent<Rigidbody>().AddForce(fDir * 200); } catch (System.Exception e) { Debug.Log(e); }
        //try { firstWeapon.localAmmo = firstWeapCurrAmmo; } catch (System.Exception e) { Debug.Log(e); }
        //try { firstWeapon.spareAmmo = firstWeapSpareAmmo; } catch (System.Exception e) { Debug.Log(e); }


        {
            LootableWeapon _firstWeapon = WeaponPool.instance.GetLootableWeapon(weapInv[firstWeapIndex].GetComponent<WeaponProperties>().codeName);
            try { _firstWeapon.name = _firstWeapon.name.Replace("(Clone)", ""); } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.transform.position = spp; } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.spawnPointPosition = spp; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.localAmmo = firstWeapCurrAmmo; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.spareAmmo = firstWeapSpareAmmo; } catch (System.Exception e) { Debug.Log(e); }


            try { _firstWeapon.ttl = _firstWeapon.defaultTtl; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.gameObject.SetActive(true); } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.GetComponent<Rigidbody>().AddForce(fDir * 200); } catch (System.Exception e) { Debug.LogWarning(e); }
        }
    }

    [PunRPC]
    void SpawnNetworkWeaponOnPlayerDeath_RPC(int firstWeapIndex, int secondtWeapIndex, int firstWeapCurrAmmo, int firstWeapSpareAmmo,
       int secondWeapCurrAmmo, int secondWeapSpareAmmo, Vector3 firstWeapSpp, Vector3 firstWeapSdir, Vector3 secondWeapSpp)
    {
        Debug.Log($"SpawnNetworkWeaponOnPlayerDeath_RPC {firstWeapSdir} {secondtWeapIndex}");
        GameObject[] weapInv = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory;

        if (firstWeapCurrAmmo + firstWeapSpareAmmo > 0)
        {
            LootableWeapon _firstWeapon = WeaponPool.instance.GetLootableWeapon(weapInv[firstWeapIndex].GetComponent<WeaponProperties>().codeName);
            Debug.Log($"First Weapon: {_firstWeapon.name}");
            try { _firstWeapon.name = _firstWeapon.name.Replace("(Clone)", ""); } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.transform.parent = null; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.transform.position = firstWeapSpp; } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.spawnPointPosition = firstWeapSpp; } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.GetComponent<Rigidbody>().velocity = Vector3.zero; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.localAmmo = firstWeapCurrAmmo; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.spareAmmo = firstWeapSpareAmmo; } catch (System.Exception e) { Debug.Log(e); }


            try { _firstWeapon.ttl = _firstWeapon.defaultTtl; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.gameObject.SetActive(true); } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.GetComponent<Rigidbody>().AddForce(firstWeapSdir * 200); } catch (System.Exception e) { Debug.LogWarning(e); }




            //LootableWeapon firstWeapon = Instantiate(weapInv[firstWeapIndex].GetComponent<WeaponProperties>().weaponRessource,
            // firstWeapSpp, Quaternion.identity).GetComponent<LootableWeapon>();

            //try { firstWeapon.name = firstWeapon.name.Replace("(Clone)", ""); } catch (System.Exception e) { Debug.Log(e); }
            //try { firstWeapon.transform.position = firstWeapSpp; } catch (System.Exception e) { Debug.Log(e); }
            //try { firstWeapon.spawnPointPosition = firstWeapSpp; } catch (System.Exception e) { Debug.Log(e); }
            //try { firstWeapon.GetComponent<Rigidbody>().AddForce(firstWeapSdir * 200); } catch (System.Exception e) { Debug.Log(e); }
            //try { firstWeapon.localAmmo = firstWeapCurrAmmo; } catch (System.Exception e) { Debug.Log(e); }
            //try { firstWeapon.spareAmmo = firstWeapSpareAmmo; } catch (System.Exception e) { Debug.Log(e); }
        }


        if (secondWeapCurrAmmo + secondWeapSpareAmmo > 0)
        {
            LootableWeapon _firstWeapon = WeaponPool.instance.GetLootableWeapon(weapInv[secondtWeapIndex].GetComponent<WeaponProperties>().codeName);
            Debug.Log($"Second Weapon: {_firstWeapon.name}");
            try { _firstWeapon.name = _firstWeapon.name.Replace("(Clone)", ""); } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.transform.position = secondWeapSpp; } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.spawnPointPosition = secondWeapSpp; } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.GetComponent<Rigidbody>().velocity = Vector3.zero; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.localAmmo = secondWeapCurrAmmo; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.spareAmmo = secondWeapSpareAmmo; } catch (System.Exception e) { Debug.Log(e); }

            try { _firstWeapon.ttl = _firstWeapon.defaultTtl; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.gameObject.SetActive(true); } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.GetComponent<Rigidbody>().AddForce(firstWeapSdir * 200); } catch (System.Exception e) { Debug.LogWarning(e); }







            //LootableWeapon secondWeapon = Instantiate(weapInv[secondtWeapIndex].GetComponent<WeaponProperties>().weaponRessource,
            // firstWeapSpp, Quaternion.identity).GetComponent<LootableWeapon>();

            //try { secondWeapon.name = secondWeapon.name.Replace("(Clone)", ""); } catch (System.Exception e) { Debug.Log(e); }
            //try { secondWeapon.transform.position = secondWeapSpp; } catch (System.Exception e) { Debug.Log(e); }
            //try { secondWeapon.spawnPointPosition = secondWeapSpp; } catch (System.Exception e) { Debug.Log(e); }
            //try { secondWeapon.GetComponent<Rigidbody>().AddForce(firstWeapSdir * 200); } catch (System.Exception e) { Debug.Log(e); }
            //try { secondWeapon.localAmmo = secondWeapCurrAmmo; } catch (System.Exception e) { Debug.Log(e); }
            //try { secondWeapon.spareAmmo = secondWeapSpareAmmo; } catch (System.Exception e) { Debug.Log(e); }
        }
    }
    #endregion


    // Explosive Barrel
    #region
    [PunRPC]
    void DamageExplosiveBarrel_RPC(Vector3 sp, int val)
    {
        foreach (ExplosiveBarrel eb in FindObjectsOfType<ExplosiveBarrel>(false).ToList())
        {
            if (eb.spawnPointPosition == sp)
            {
                eb._networkHitPoints = val;
            }
        }
    }

    [PunRPC]
    void ResetAllExplosiveBarrels_RPC()
    {
        foreach (ExplosiveBarrel ic in FindObjectsOfType<ExplosiveBarrel>(true).ToList())
        {
            ic.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ic.transform.position = ic.spawnPointPosition;
            ic.transform.rotation = ic.spawnPointRotation;
            ic.gameObject.SetActive(true);
        }
        Debug.Log("All Barrels Reset");
    }


    [PunRPC]
    void EnableExplosiveBarrel_RPC(Vector3 position)
    {
        foreach (ExplosiveBarrel ic in FindObjectsOfType<ExplosiveBarrel>(true).ToList())
        {
            if (ic.spawnPointPosition == position)
            {
                ic.gameObject.SetActive(true);
                ic.transform.position = ic.spawnPointPosition;
                ic.transform.rotation = ic.spawnPointRotation;
            }
        }
    }

    [PunRPC]
    void RelocateExplosiveBarrel_RPC(Vector3 position, Quaternion rotation)
    {
        foreach (ExplosiveBarrel lw in FindObjectsOfType<ExplosiveBarrel>(true).ToList())
        {
            if (lw.spawnPointPosition == position)
            {
                lw.GetComponent<Rigidbody>().velocity *= 0;
                lw.transform.position = position;
                lw.transform.rotation = rotation;
            }
        }
    }
    #endregion

    [PunRPC]
    void DamageIceChunk_RPC(Vector3 position, int val)
    {
        //foreach (IceChunk ic in FindObjectsOfType<IceChunk>(true).ToList())
        //{
        //    if (ic.transform.position == position)
        //    {
        //        ic._networkHitPoints = val;
        //    }
        //}

        foreach (Hazard ic in GameManager.instance.hazards)
        {
            if (ic.transform.position == position)
            {
                ic.GetComponent<IceChunk>().networkHitPoints = val;
            }
        }
    }

    [PunRPC]
    void UpdatePlayerTeam_RPC(string t, string pn)
    {
        Debug.Log("UpdatePlayerTeam_RPC");
        //foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>().ToList())
        //    if (pms.username == pn)
        //        pms.networkTeam = (GameManager.Team)System.Enum.Parse(typeof(GameManager.Team), t);
    }




    [PunRPC]
    void DisableAmmoPack_RPC(Vector3 sp)
    {
        foreach (NetworkGrenadeSpawnPoint ap in FindObjectsOfType<NetworkGrenadeSpawnPoint>())
            if (ap.spawnPoint == sp)
                ap.enable = false;
    }

    [PunRPC]
    void EnableGrenadePacks_RPC()
    {
        foreach (NetworkGrenadeSpawnPoint ap in FindObjectsOfType<NetworkGrenadeSpawnPoint>())
            ap.enable = true;
    }

    [PunRPC]
    void StartOverShieldRespawn_RPC(int t)
    {
        Debug.Log("sdfaeqwer");
        StartCoroutine(StartOverShieldRespawn_Coroutine(t));
    }

    [PunRPC]
    void StartLootableWeaponRespawn_RPC(Vector3 v)
    {
        Debug.Log("sdfaeqwer");
        MultiplayerManager.instance.StartLootableWeaponRespawn(v);
    }



    // Multiplayer
    [PunRPC]
    void AddPlayerPoint_RPC(int pid)
    {
        Debug.Log("AddPlayerPoint_RPC");
        MultiplayerManager.instance.AddPlayerPoint(pid);
    }

    [PunRPC]
    void NextHillLocation_RPC()
    {
        Debug.Log("NextHillLocation_RPC");
        FindObjectOfType<HillManager>().NextLocation();
    }


    [PunRPC]
    void UpdateSwarmDifficulty_RPC(int ei)
    {
        GameManager.instance.difficulty = (SwarmManager.Difficulty)ei;
    }



    [PunRPC]
    public void LootOvershield(int pid, bool caller = true)
    {
        if (caller)
        {
            _pv.RPC("LootOvershield", RpcTarget.AllViaServer, pid, false);
        }
        else
        {
            GameManager.instance.pid_player_Dict[pid].maxOvershieldPoints = 150;


            int t = overshield.tts;
            int _time = FindObjectOfType<GameTime>().timeRemaining;

            int timeLeft = 0;

            if (_time < t)
                timeLeft = t - _time;
            else
                timeLeft = t - (_time % t);
            Debug.Log(timeLeft);

            StartCoroutine(StartOverShieldRespawn_Coroutine(timeLeft));
            overshield.gameObject.SetActive(false);
        }
    }


    [PunRPC]
    public void UpdateRoomCountdowns(int vetoC, int roomGameStartC, bool caller = true)
    {
        if (PhotonNetwork.IsMasterClient && caller)
        {
            //Debug.Log($"UpdateRoomCountdowns: {vetoC} and {roomGameStartC}");
            _pv.RPC("UpdateRoomCountdowns", RpcTarget.All, vetoC, roomGameStartC, false);
        }
        else if (!PhotonNetwork.IsMasterClient && !caller)
        {
            //Debug.Log($"UpdateRoomCountdowns: {vetoC} and {roomGameStartC}");
            CurrentRoomManager.instance.UpdateCountdowns(vetoC, roomGameStartC);
        }
    }


    [PunRPC]
    public void SendVetoToMaster(int nbVetos, bool caller = true)
    {
        if (caller)
        {
            _pv.RPC("SendVetoToMaster", RpcTarget.MasterClient, nbVetos, false);
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"Received: {nbVetos} vetoes");
            CurrentRoomManager.instance.vetos += nbVetos;

            SendVetoToClients();
        }
    }

    [PunRPC]
    public void SendVetoToClients(int nbVetos = 0, bool caller = true)
    {
        if (caller)
        {
            _pv.RPC("SendVetoToClients", RpcTarget.All, CurrentRoomManager.instance.vetos, false);
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            CurrentRoomManager.instance.vetos = nbVetos;
        }
    }

    [PunRPC]
    public void AskHostToTriggerInteractableObject_RPC(Vector3 pos, int? pid = null)
    {
        _pv.RPC("TriggerInteractableObject_RPC", RpcTarget.All, pos, pid);
    }

    [PunRPC]
    public void TriggerInteractableObject_RPC(Vector3 pos, int? pid = null)
    {
        foreach (InteractableObject eet in FindObjectsOfType<InteractableObject>())
        {
            if (eet.transform.position == pos)
                eet.Trigger(pid);
        }
    }

    IEnumerator StartOverShieldRespawn_Coroutine(int t)
    {
        Debug.Log($"Spawning Overshiled in {t} seconds");
        yield return new WaitForSeconds(t);
        overshield.gameObject.SetActive(true);
    }


    [PunRPC]
    public void StickGrenadeOnPlayer_RPC(int nadeIndex, int playerId, Vector3 gPos)
    {
        Debug.Log($"NETWORK GAME MANAGER StickGrenadeOnPLayer. of player {playerId}");

        GrenadePool.instance.stickyGrenadePool[nadeIndex].GetComponent<ExplosiveProjectile>().TriggerStuckBehaviour(playerId, gPos);
    }


    [PunRPC]
    public void EquipOddballToPlayer_RPC(int playerId, bool caller = true)
    {


        if (caller && PhotonNetwork.IsMasterClient)
        {
            _pv.RPC("EquipOddballToPlayer_RPC", RpcTarget.All, playerId, false);
        }
        else if (!caller)
        {
            GameManager.instance.oddballSkull.DisableOddball();
            GameManager.GetPlayerWithId(playerId).playerInventory.EquipOddball();
        }
    }


    [PunRPC]
    public void AddForceToOddball(float calculatedPower, Vector3 pos, float rad, float up, bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            _pv.RPC("AddForceToOddball", RpcTarget.AllViaServer, calculatedPower, pos, rad, up, false);
        }
        else if (!caller)
        {
            print($"AddForceToOddball {calculatedPower} {rad}");
            //GameManager.instance.oddballSkull.rb.AddForce(dir * calculatedPower, mode: ForceMode.Impulse);
            GameManager.instance.oddballSkull.rb.AddExplosionForce(calculatedPower, pos, rad, 3.0F);
        }
    }

    [PunRPC]
    public void ShowOddball(Vector3 pos, Vector3 dir, bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            _pv.RPC("ShowOddball", RpcTarget.AllViaServer, pos, dir, false);
        }
        else if (!caller)
        {
            GameManager.instance.oddballSkull.rb.velocity = Vector3.zero;
            GameManager.instance.oddballSkull.rb.angularVelocity = Vector3.zero;


            GameManager.instance.oddballSkull.transform.root.rotation = Quaternion.identity;
            GameManager.instance.oddballSkull.transform.root.position = pos;

            GameManager.instance.oddballSkull.transform.root.gameObject.SetActive(true);
            GameManager.instance.oddballSkull.rb.AddForce(dir * 300);
        }
    }


    //public void AskHostToStickGrenade
}
