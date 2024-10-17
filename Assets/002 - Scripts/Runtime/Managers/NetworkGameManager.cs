using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

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


    // Menu
    #region

    [PunRPC]
    public void SendGameParams(Dictionary<string, string> p = null, bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            Dictionary<string, string> ps = new Dictionary<string, string>();
            print("SendGameParams MASTER");

            if (GameManager.instance.teamMode == GameManager.TeamMode.Classic && SceneManager.GetActiveScene().buildIndex == 0)
                for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    if (CurrentRoomManager.GetLocalPlayerData(i).team == GameManager.Team.None)
                    {
                        print("Correcting Teams because a player joined");
                        ps.Add("reevaluateteams", "");
                    }
                }


            ps.Add("roomtype", CurrentRoomManager.instance.roomType.ToString());


            ps.Add("gamemode", GameManager.instance.gameMode.ToString());
            ps.Add("gametype", GameManager.instance.gameType.ToString());
            ps.Add("leveltoloadindex", Launcher.instance.levelToLoadIndex.ToString());
            ps.Add("teammode", GameManager.instance.teamMode.ToString());
            ps.Add("sprintmode", GameManager.instance.teamMode.ToString());
            //ps.Add("teamdict", string.Join(Environment.NewLine, GameManager.instance.teamDict));
            //ps.Add("teamdict", JsonConvert.SerializeObject(GameManager.instance.teamDict));
            ps.Add("nbLocalPlayersDict", JsonConvert.SerializeObject(CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict));


            //Debug.Log($"SendGameParams {GameManager.instance.teamDict}");

            _pv.RPC("SendGameParams", RpcTarget.AllViaServer, ps, false);
        }
        else if (!caller)
        {
            print("SendGameParams CLIENT");
            if (!PhotonNetwork.IsMasterClient)
            {
                try { GameManager.instance.gameMode = (GameManager.GameMode)System.Enum.Parse(typeof(GameManager.GameMode), p["gamemode"]); } catch (System.Exception e) { Debug.Log(e); }
                try { GameManager.instance.gameType = (GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), p["gametype"]); } catch (System.Exception e) { Debug.Log(e); }
                try { Launcher.instance.levelToLoadIndex = (System.Int16.Parse(p["leveltoloadindex"])); } catch (System.Exception e) { Debug.Log(e); }
                try { GameManager.instance.teamMode = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), p["teammode"]); } catch (System.Exception e) { Debug.Log(e); }
                try { GameManager.instance.sprintMode = (GameManager.SprintMode)System.Enum.Parse(typeof(GameManager.SprintMode), p["sprintmode"]); } catch (System.Exception e) { Debug.Log(e); }


                try
                {
                    CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(p["nbLocalPlayersDict"]);
                    Debug.Log(CurrentRoomManager.instance.playerNicknameNbLocalPlayersDict);
                }
                catch { }
            }

            if (p.ContainsKey("reevaluateteams"))
            {
                GameManager.instance.CreateTeamsBecausePlayerJoined();
                //Launcher.instance.FindMasterClientAndToggleIcon();
            }
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



                foreach (ScriptObjPlayerData pdc in CurrentRoomManager.instance.playerDataCells.Where(item => item.occupied))
                {
                    if (pdc.playerExtendedPublicData.player_id.Equals(kvp.Key))
                    {
                        pdc.team = (GameManager.Team)kvp.Value;
                        //CurrentRoomManager.GetDataCellWithDatabaseId(int.Parse(kvp.Value.NickName), 0).team = t;
                    }
                }


                //CurrentRoomManager.GetDataCellWithDatabaseId(kvp.Key, 0).team = (GameManager.Team)kvp.Value;
            }
            foreach (Transform child in Launcher.instance.namePlatesParent)
            {
                child.GetComponent<PlayerNamePlate>().UpdateColorPalette();
            }

            foreach (Transform child in Launcher.instance.namePlatesParent)
            {
                if (child.GetComponent<PlayerNamePlate>().playerDataCell.team == GameManager.Team.Blue)
                    child.SetAsLastSibling();
            }
        }
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
            print("NetworkGameManager EndGame");

            if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
                MultiplayerManager.instance.EndGame();
            else if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
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
    public void AddPlayerSetCount(bool caller = true)
    {
        if (caller)
        {
            print("_nbPlayersSet");
            _pv.RPC("AddPlayerSetCount", RpcTarget.AllViaServer, false);
        }
        else if (!caller)
        {
            CurrentRoomManager.instance.nbPlayersSet++;
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

    public static void StickGrenadeOnPlayer(int nadeIndex, int playerPhotonId, Vector3 gPos)
    {
        Debug.Log($"NETWORK GAME MANAGER StickGrenadeOnPLayer. of player {playerPhotonId}");
        instance._pv.RPC("StickGrenadeOnPlayer_RPC", RpcTarget.All, nadeIndex, playerPhotonId, gPos);
    }





    IEnumerator UpdateAmmo_Coroutine(int playerPid, int wIndex, bool isSpare, bool isThirdWeapon, int ammo)
    {
        yield return new WaitForEndOfFrame();

        Debug.Log($"UpdateAmmo Is Not Mine");


        if (isThirdWeapon)
        {
            if (!isSpare)
                GameManager.GetPlayerWithPhotonView(playerPid).playerInventory.thirdWeapon.UpdateLoadedAmmo(ammo);
            else
                GameManager.GetPlayerWithPhotonView(playerPid).playerInventory.thirdWeapon.UpdateSpareAmmo(ammo);
        }
        else
        {

            if (!isSpare)
            {
                GameManager.GetPlayerWithPhotonView(playerPid).playerInventory.allWeaponsInInventory[wIndex].GetComponent<WeaponProperties>().UpdateLoadedAmmo(ammo);
            }
            else
            {
                GameManager.GetPlayerWithPhotonView(playerPid).playerInventory.allWeaponsInInventory[wIndex].GetComponent<WeaponProperties>().UpdateSpareAmmo(ammo);
            }
        }
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
        int firstWeapIndex = Array.IndexOf(weap.player.playerInventory.allWeaponsInInventory, weap.player.playerInventory.GetWeaponProperties(weap.codeName).gameObject);
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
    public void DamageExplosiveBarrel(Vector3 position, int val, int playerPid = -999)
    {
        if (!CurrentRoomManager.instance.gameOver)
            _pv.RPC("DamageExplosiveBarrel_RPC", RpcTarget.All, position, val, playerPid);
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
        if (!CurrentRoomManager.instance.gameOver)
            _pv.RPC("DisableAmmoPack_RPC", RpcTarget.All, sp);
    }

    public void EnableGrenadePacks()
    {
        if (!CurrentRoomManager.instance.gameOver)
            _pv.RPC("EnableGrenadePacks_RPC", RpcTarget.All);
    }

    #endregion

    // Multiplayer
    public void AddPlayerPoint(int pid, int whichFlagNeedsToBeReset = -1)
    {
        _pv.RPC("AddPlayerPoint_RPC", RpcTarget.All, pid, whichFlagNeedsToBeReset);
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
        print("SpawnNetworkWeapon_RPC");
        LootableWeapon _firstWeapon = WeaponPool.instance.GetLootableWeapon(GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory[firstWeapIndex].GetComponent<WeaponProperties>().codeName);
        print("SpawnNetworkWeapon_RPC 1");

        if (_firstWeapon != null)
        {
            print($"SPAWING WEAPON");

            _firstWeapon.name = _firstWeapon.name.Replace("(Clone)", "");
            _firstWeapon.transform.position = spp;

            try { _firstWeapon.localAmmo = firstWeapCurrAmmo; } catch (System.Exception e) { Debug.Log(e); }
            try { _firstWeapon.spareAmmo = firstWeapSpareAmmo; } catch (System.Exception e) { Debug.Log(e); }


            _firstWeapon.ttl = _firstWeapon.defaultTtl;
            _firstWeapon.gameObject.SetActive(true);
            _firstWeapon.GetComponent<Rigidbody>().AddForce(fDir * 200);
        }
        else print($"COULD NOT SPAWN WEAPON CORRECTLY");
        print("SpawnNetworkWeapon_RPC 2");
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
    void DamageExplosiveBarrel_RPC(Vector3 sp, int val, int playerPid)
    {
        foreach (ExplosiveBarrel eb in FindObjectsOfType<ExplosiveBarrel>(false).ToList())
        {
            if (eb.spawnPointPosition == sp)
            {
                eb.UpdateLastPlayerWhoDamaged(playerPid);
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
    void AddPlayerPoint_RPC(int pid, int whichFlagNeedsToBeReset)
    {
        Debug.Log("AddPlayerPoint_RPC");


        if (whichFlagNeedsToBeReset > -1 && GameManager.instance.gameType == GameManager.GameType.CTF)
        {
            //GameManager.GetPlayerWithPhotonView(pid).playerInventory.SwitchWeapons_RPC();
            GameManager.GetPlayerWithPhotonView(pid).playerInventory.HideFlag();

            if ((GameManager.Team)whichFlagNeedsToBeReset == GameManager.Team.Red)
            {
                GameManager.instance.redFlag.spawnPoint.SpawnFlagAtStand();

                foreach (Player p in GameManager.GetLocalPlayers()) p.killFeedManager.EnterNewFeed($"<color=#31cff9>Red Flag Captured!");
            }
            else
            {
                foreach (Player p in GameManager.GetLocalPlayers()) p.killFeedManager.EnterNewFeed($"<color=#31cff9>Blue Flag Captured!");
                GameManager.instance.blueFlag.spawnPoint.SpawnFlagAtStand();
            }
        }


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
            GameManager.GetPlayerWithPhotonView(pid).maxOvershieldPoints = 150;



            overshield.gameObject.SetActive(false);
            if (GameTime.instance.timeRemaining > overshield.tts)
            {
                int timeToNextSpawn = overshield.tts - (GameTime.instance.timeElapsed % overshield.tts);
                print($"LootOvershield {overshield.tts} {GameTime.instance.timeElapsed} {GameTime.instance.timeElapsed % overshield.tts} {timeToNextSpawn}");
                StartCoroutine(StartOverShieldRespawn_Coroutine(timeToNextSpawn));
            }


            //int t = overshield.tts;
            //int _time = FindObjectOfType<GameTime>().timeRemaining;

            //int timeLeft = 0;

            //if (_time < t)
            //    timeLeft = t - _time;
            //else
            //    timeLeft = t - (_time % t);
            //Debug.Log(timeLeft);

            //StartCoroutine(StartOverShieldRespawn_Coroutine(timeLeft));
            //overshield.gameObject.SetActive(false);
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
    public void StickGrenadeOnPlayer_RPC(int nadeIndex, int playerPhotonId, Vector3 gPos)
    {
        Debug.Log($"NETWORK GAME MANAGER StickGrenadeOnPLayer. of player {playerPhotonId}");

        GrenadePool.instance.stickyGrenadePool[nadeIndex].GetComponent<ExplosiveProjectile>().TriggerStuckBehaviour(playerPhotonId, gPos);
    }


    [PunRPC]
    public void EquipOddballToPlayer_RPC(int playerPhotonView, bool caller = true)
    {


        if (caller && PhotonNetwork.IsMasterClient)
        {
            _pv.RPC("EquipOddballToPlayer_RPC", RpcTarget.All, playerPhotonView, false);
        }
        else if (!caller)
        {
            GameManager.instance.oddballSkull.DisableOddball();
            GameManager.instance.oddballSkull.PlayBallTakenClip();
            GameManager.GetPlayerWithPhotonView(playerPhotonView).playerInventory.EquipOddball();
        }
    }


    [PunRPC]
    public void EquipFlagToPlayer_RPC(int playerPhotonView, int whichFlagToDisable, bool caller = true)
    {


        if (caller && PhotonNetwork.IsMasterClient)
        {
            _pv.RPC("EquipFlagToPlayer_RPC", RpcTarget.All, playerPhotonView, whichFlagToDisable, false); // no latency from MC side
        }
        else if (!caller)
        {
            print("EquipFlagToPlayer_RPC");

            //GameManager.GetPlayerWithPhotonView(playerPhotonView).playerInventory.HideFlag();

            if (GameManager.GetPlayerWithPhotonView(playerPhotonView).team == GameManager.Team.Red)
            {
                GameManager.instance.blueFlag.scriptRoot.gameObject.SetActive(false);
            }
            else
            {
                GameManager.instance.redFlag.scriptRoot.gameObject.SetActive(false);
            }



            GameManager.GetPlayerWithPhotonView(playerPhotonView).playerInventory.EquipFlag();



            foreach (Player p in GameManager.GetLocalPlayers())
            {
                p.killFeedManager.EnterNewFeed($"<color=#31cff9>{(GameManager.GetPlayerWithPhotonView(playerPhotonView).team == GameManager.Team.Red ? GameManager.Team.Blue : GameManager.Team.Red)} Flag Taken");
            }
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
    public void AskMasterClientToSpawnOddball(Vector3 pos, Vector3 dir, bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            _pv.RPC("AskMasterClientToSpawnOddball", RpcTarget.AllViaServer, pos, dir, false);
        }
        else if (!caller)
        {
            GameManager.instance.oddballSkull.rb.velocity = Vector3.zero;
            GameManager.instance.oddballSkull.rb.angularVelocity = Vector3.zero;


            GameManager.instance.oddballSkull.thisRoot.rotation = Quaternion.identity;
            GameManager.instance.oddballSkull.thisRoot.position = pos;

            GameManager.instance.oddballSkull.thisRoot.gameObject.SetActive(true);
            GameManager.instance.oddballSkull.rb.AddForce(dir * 300);

            GameManager.instance.oddballSkull.PlayBallDroppedClip();
        }
    }

    [PunRPC]
    public void AskMasterClientToSpawnFlag(Vector3 pos, Vector3 dir, GameManager.Team flagTeam, bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            _pv.RPC("AskMasterClientToSpawnFlag", RpcTarget.AllViaServer, pos, dir, flagTeam, false);
        }
        else if (!caller)
        {
            if (flagTeam == GameManager.Team.Blue)
            {
                GameManager.instance.blueFlag.rb.velocity = Vector3.zero;
                GameManager.instance.blueFlag.rb.angularVelocity = Vector3.zero;


                GameManager.instance.blueFlag.scriptRoot.rotation = Quaternion.identity;
                GameManager.instance.blueFlag.scriptRoot.position = pos;

                GameManager.instance.blueFlag.scriptRoot.gameObject.SetActive(true);
                GameManager.instance.blueFlag.rb.mass = 1;
                GameManager.instance.blueFlag.rb.AddForce(dir * 350);

                //GameManager.instance.oddballSkull.PlayBallDroppedClip();
            }
            else
            {
                GameManager.instance.redFlag.rb.velocity = Vector3.zero;
                GameManager.instance.redFlag.rb.angularVelocity = Vector3.zero;


                GameManager.instance.redFlag.scriptRoot.rotation = Quaternion.identity;
                GameManager.instance.redFlag.scriptRoot.position = pos;

                GameManager.instance.redFlag.scriptRoot.gameObject.SetActive(true);
                GameManager.instance.redFlag.rb.mass = 1;
                GameManager.instance.redFlag.rb.AddForce(dir * 350);
            }
        }
    }



    [PunRPC]
    public void UpdateAmmo(int playerPid, int wIndex, int ammo, bool isSpare = false, bool isThirdWeapon = false, bool sender = false)
    {
        if (GameTime.instance.timeElapsed > 5)
        {

            print($"UpdateAmmo for {GameManager.GetPlayerWithPhotonView(playerPid)} {ammo}. Sender: {sender}");
            if (GameManager.GetPlayerWithPhotonView(playerPid).isMine && sender)
            {
                instance._pv.RPC("UpdateAmmo", RpcTarget.All, playerPid, wIndex, ammo, isSpare, isThirdWeapon, false);
            }
            else if (!sender && !GameManager.GetPlayerWithPhotonView(playerPid).isMine)
            {
                StartCoroutine(UpdateAmmo_Coroutine(playerPid, wIndex, isSpare, isThirdWeapon, ammo));
            }
        }
    }



    [PunRPC]
    public void SetPlayerDataCellStartingSpawnPositionIndex(int playerDbId, int rewiredId, int indd, bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            instance._pv.RPC("SetPlayerDataCellStartingSpawnPositionIndex", RpcTarget.AllViaServer, playerDbId, rewiredId, indd, false);
        }
        else if (!caller)
        {
            //print($"{playerDbId} {rewiredId} will get spawn {indd}");

            try
            {
                CurrentRoomManager.GetDataCellWithDatabaseIdAndRewiredId(playerDbId, rewiredId).startingSpawnPosInd = indd;
            }
            catch { Debug.LogError("Could not find data cell and assign starting pos"); }
        }
    }




    [PunRPC]
    public void StartGameButton(bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            instance._pv.RPC("StartGameButton", RpcTarget.AllViaServer, false);
        }
        else if (!caller)
        {
            Launcher.instance.gameCountdownText.gameObject.SetActive(true);
            CurrentRoomManager.instance.roomGameStartCountdown = 7;
            CurrentRoomManager.instance.matchSettingsSet = true;
        }
    }

    [PunRPC]

    public void TriggerPlayerOverheatWeapon(int playerPhotonId, int weaponKillFeedOutputInt, bool leftHand, bool caller = true)
    {
        if (caller && GameManager.GetPlayerWithPhotonView(playerPhotonId).isMine)
            instance._pv.RPC("TriggerPlayerOverheatWeapon", RpcTarget.AllViaServer, playerPhotonId, weaponKillFeedOutputInt, leftHand, false);
        else if (!caller)
        {
            print($"TriggerPlayerOverheatWeapon {leftHand}");
            GameManager.GetPlayerWithPhotonView(playerPhotonId).playerController.Descope();

            foreach (GameObject weaponGo in GameManager.GetPlayerWithPhotonView(playerPhotonId).playerInventory.allWeaponsInInventory)
            {
                if (weaponGo.GetComponent<WeaponProperties>().killFeedOutput == (WeaponProperties.KillFeedOutput)weaponKillFeedOutputInt)
                {
                    if (!leftHand)
                    {
                        weaponGo.GetComponent<WeaponProperties>().TriggerOverheat();

                    }
                    else
                    {
                        weaponGo.GetComponent<WeaponProperties>().leftWeapon.TriggerOverheat();
                    }
                }
            }
            //GameManager.GetPlayerWithPhotonView(playerPhotonId).playerInventory.allWeaponsInInventory[weaponInd].GetComponent<WeaponProperties>().TriggerOverheat();
        }
    }





    (Transform, bool) _reservedSpawnPoint;


    [PunRPC]
    public void AskMasterToReserveSpawnPoint(int playerPhotonId, int controllerID, bool caller = true)
    {
        if (caller)
        {
            instance._pv.RPC("AskMasterToReserveSpawnPoint", RpcTarget.MasterClient, playerPhotonId, controllerID, false);
        }
        else if (!caller && PhotonNetwork.IsMasterClient)
        {
            _reservedSpawnPoint = SpawnManager.spawnManagerInstance.GetRandomSafeSpawnPoint(GameManager.GetPlayerWithPhotonView(playerPhotonId).team);
            instance._pv.RPC("ReserveSpawnPoint_RPC", RpcTarget.All, playerPhotonId, controllerID, _reservedSpawnPoint.Item1.position, _reservedSpawnPoint.Item2);
        }
    }


    [PunRPC]
    public void ReserveSpawnPoint_RPC(int playerPhotonId, int controllerID, Vector3 pos, bool isRandom)
    {
        SpawnManager.spawnManagerInstance.ReserveSpawnPoint(pos);

        foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
        {
            if (p.photonId == playerPhotonId && p.rid == controllerID) p.UpdateReservedSpawnPoint(pos, isRandom);
        }
    }

    [PunRPC]
    public void DisableAndExplodeProjectile(int output, int ind, Vector3 pos, bool caller = true)
    {
        if (caller)
        {
            instance._pv.RPC("DisableAndExplodeProjectile", RpcTarget.AllViaServer, output, ind, pos, false);
        }
        else if (!caller)
        {
            GrenadePool.instance.DisableExplosive((WeaponProperties.KillFeedOutput)output, ind, pos);
        }
    }
}
