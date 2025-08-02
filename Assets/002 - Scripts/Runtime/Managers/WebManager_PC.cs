using UnityEngine;
using UnityEngine.Networking;
using System;
using Photon.Pun;
using System.Collections;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Timers;
using System.Linq;
using Steamworks;

public partial class WebManager
{
    IEnumerator Login_Coroutine(string steamid, string username, string password)
    {
        Debug.Log("LOGIN COROUTINE");
        WWWForm form = new WWWForm();
        string m = "login";
        if (password.Equals("steam")) m = "loginwithsteam";
        form.AddField("service", m);
        form.AddField("steamid", steamid);
        form.AddField("username", username);
        form.AddField("password", password);
        Debug.Log($"Password: {password} {m}");


        PhotonNetwork.NickName = SteamUser.GetSteamID().m_SteamID.ToString();
        CurrentRoomManager.instance.playerDataCells[0].steamId = (long)SteamUser.GetSteamID().m_SteamID;
        CurrentRoomManager.instance.playerDataCells[0].steamName = SteamFriends.GetPersonaName();
        CurrentRoomManager.instance.playerDataCells[0].occupied = true;
        CurrentRoomManager.instance.playerDataCells[0].local = true;



        //using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/unity/database.php", form))
        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                string jsonarray = www.downloadHandler.text;


                PlayerDatabaseAdaptor.PlayerLoginData pld = PlayerDatabaseAdaptor.PlayerLoginData.CreateFromJSON(jsonarray);
                pda.playerLoginData = pld;
                try
                {
                    //StartCoroutine(Login_Coroutine_Set_Online_Stats(pda.steamid));
                    StartCoroutine(GetPlayerExtendedPublicData_Coroutine(pda.steamid));
                    //StartCoroutine(Login_Coroutine_Set_PvP_Stats(pda.steamid));
                    //StartCoroutine(Login_Coroutine_Set_PvE_Stats(pda.steamid));

                    GameManager.instance.playerDataRetrieved = true;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    MenuManager.Instance.OpenErrorMenu($"Failed to fetch stats: {e}");
                }
            }
        }
    }

    IEnumerator GetPlayerExtendedPublicData_Coroutine(long playerid)
    {
        //Debug.Log($"GET PLAYER EXTENDED PUBLIC DATA {playerNamePlateInstance != null} {playerid}");
        //playerNamePlateInstance.playerDataCell = CurrentRoomManager.GetDataCellWithSteamIdAndRewiredId(playerid, 0);



        // DISCLAIMER
        // PlayerDatabaseAdaptor has authority on the data put into the PlayerListItem. Check var pda.playerBasicOnlineStats

        //PlayerDatabaseAdaptor pda = new PlayerDatabaseAdaptor();
        //if (pli)
        //    pda.playerListItem = pli;
        WWWForm form = new WWWForm();
        form.AddField("service", "getplayerpublicdata");
        form.AddField("playerid", playerid.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log($"GET PLAYER EXTENDED PUBLIC DATA RESULTS {playerid}");
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                string jsonarray = www.downloadHandler.text;

                Debug.Log(jsonarray);
                PlayerDatabaseAdaptor.PlayerExtendedPublicData pepd = PlayerDatabaseAdaptor.PlayerExtendedPublicData.CreateFromJSON(jsonarray);

                CurrentRoomManager.instance.AddExtendedPlayerDataCell(pepd); // ERROR IF NULL, BREAKS GAME

                //try
                //{


                //    if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                //    {
                //        playerNamePlateInstance.playerDataCell.team = GameManager.Team.Red;
                //        playerNamePlateInstance.UpdateColorPalette();
                //    }
                //}
                //catch (Exception e) { Debug.LogWarning(e); }
            }
        }
    }


    //IEnumerator GetForeignPlayerExtendedPublicData_Coroutine(string steamid, PlayerNamePlate pli = null)
    //{
    //    Debug.Log("GetPlayerExtendedPublicData_Coroutine");
    //    // DISCLAIMER
    //    // PlayerDatabaseAdaptor has authority on the data put into the PlayerListItem. Check var pda.playerBasicOnlineStats

    //    PlayerDatabaseAdaptor pda = new PlayerDatabaseAdaptor();
    //    //if (pli)
    //    //    pda.playerListItem = pli;
    //    WWWForm form = new WWWForm();
    //    form.AddField("service", "getplayerpublicdata");
    //    form.AddField("steamid", steamid);

    //    using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
    //    {
    //        yield return www.SendWebRequest();

    //        if (www.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.Log(www.error);
    //        }
    //        else
    //        {
    //            Debug.Log(www.result);
    //            Debug.Log(www.downloadHandler.text);

    //            string jsonarray = www.downloadHandler.text;

    //            Debug.Log(jsonarray);
    //            PlayerDatabaseAdaptor.PlayerExtendedPublicData pepd = PlayerDatabaseAdaptor.PlayerExtendedPublicData.CreateFromJSON(jsonarray);
    //            try
    //            {
    //                CurrentRoomManager.instance.AddExtendedPlayerDataCell(pepd);
    //            }
    //            catch (Exception e) { Debug.LogWarning(e); }

    //            try
    //            {
    //                pli.playerDataCell = CurrentRoomManager.GetDataCellWithDatabaseIdAndRewiredId(pepd.player_id, 0);
    //            }
    //            catch (Exception e) { Debug.LogWarning(e); }

    //            try
    //            {
    //                //pda.player = PlayerDatabaseAdaptor.PlayerLoginData.CreateFromJSON(jsonarray);

    //                //StartCoroutine(Login_Coroutine_Set_Online_Stats(pda.id, pda));
    //                //StartCoroutine(Login_Coroutine_Set_PvP_Stats(pda.id, pda));
    //                //StartCoroutine(Login_Coroutine_Set_PvE_Stats(pda.id, pda));

    //                //var d = new Dictionary<string, PlayerDatabaseAdaptor>(GameManager.instance.roomPlayerData);
    //                //if (!d.ContainsKey(pda.username))
    //                //{
    //                //    //Debug.Log($"Adding key {}")
    //                //    d.Add(pda.username, pda);
    //                //}
    //                //else
    //                //    d[pda.username] = pda;

    //                //GameManager.instance.roomPlayerData = d;

    //                //if (pli)
    //                //    pli.pda = pda;


    //                //Launcher.instance.ShowPlayerMessage("Fetched player extended data successfully!");
    //                //if (!pli)
    //                //    MenuManager.Instance.OpenMenu("online title");
    //            }
    //            catch (Exception e)
    //            {
    //                Debug.LogWarning(e);
    //                if (www.downloadHandler.text.Contains("wrong credentials"))
    //                {
    //                    Launcher.instance.OnCreateRoomFailed(0, "Wrong credentials");
    //                    Launcher.instance.loginButton.SetActive(true);
    //                }
    //            }
    //        }
    //    }
    //}


    //IEnumerator SaveArmorData_Coroutine(string newDataString)
    //{
    //    int xpAndCreditGain = Random.Range(160, 240);

    //    long playerId = pda.steamid;
    //    int newLevel = pda.playerBasicOnlineStats.level;
    //    int newXp = pda.playerBasicOnlineStats.xp + xpAndCreditGain;
    //    int newCredits = pda.playerBasicOnlineStats.credits + xpAndCreditGain;
    //    int dbXpToLevel = 0;

    //    if (PlayerProgressionManager.playerLevelToXpDic.ContainsKey(pda.playerBasicOnlineStats.level + 1))
    //    {
    //        dbXpToLevel = PlayerProgressionManager.playerLevelToXpDic[pda.playerBasicOnlineStats.level + 1];

    //        if (dbXpToLevel < newXp)
    //            newLevel = pda.playerBasicOnlineStats.level + 1;
    //    }



    //    WWWForm form = new WWWForm();
    //    form.AddField("service", "SaveBasicOnlineStats");

    //    form.AddField("playerId", playerId.ToString());
    //    form.AddField("newLevel", newLevel);
    //    form.AddField("newXp", newXp);
    //    form.AddField("newCredits", newCredits);

    //    using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
    //    {
    //        yield return www.SendWebRequest();

    //        if (www.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.Log(www.error);
    //        }
    //        else
    //        {
    //            Debug.Log(www.result);
    //            Debug.Log(www.downloadHandler.text);

    //            if (www.downloadHandler.text.Contains("Could not save swarm stats"))
    //            {
    //                Debug.LogError("Could not save swarm stats");

    //            }
    //            else if (www.downloadHandler.text.Contains("Swarm stats saved"))
    //            {
    //                Debug.Log("Swarm stats saved successfully");
    //            }
    //        }
    //    }
    //    StartCoroutine(Login_Coroutine_Set_PvE_Stats(playerId));
    //}

    IEnumerator SaveXp_Coroutine(PlayerSwarmMatchStats onlinePlayerSwarmScript = null, PlayerMultiplayerMatchStats playerMultiplayerStats = null, List<long> winPlayers = null, bool swarmGameWon = false)
    {
        {
            int xpAndCreditGain = PlayerProgressionManager.xpGainPerMatch;
            int honorGained = PlayerProgressionManager.honorGainPerMatch;

            if (swarmGameWon)
            {
                honorGained = 1;

            }
            else if (winPlayers != null)
            {
                if (winPlayers.Contains(CurrentRoomManager.GetLocalPlayerData(0).playerExtendedPublicData.player_id))
                    xpAndCreditGain = (int)(1.15f * xpAndCreditGain);
                else
                    honorGained--;

                if (GameManager.instance.gameMode != GameManager.GameMode.Versus)
                    honorGained = 0;

                if (honorGained < 0) honorGained = 0;
            }


            DateTime today = DateTime.Now;
            if (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday)
            {
                honorGained++;
                xpAndCreditGain *= 2;
            }









            Debug.Log($"SaveBasicOnlineStats_Coroutine. Xp: {pda.playerBasicOnlineStats.xp} -> {pda.playerBasicOnlineStats.xp + xpAndCreditGain}");

            long playerId = pda.steamid;
            int newLevel = pda.playerBasicOnlineStats.level;
            int newXp = pda.playerBasicOnlineStats.xp + xpAndCreditGain;
            int newCredits = pda.playerBasicOnlineStats.credits + xpAndCreditGain;
            int newHonor = pda.playerBasicOnlineStats.honor;
            int minXpToLevelUp = 999999999;

            if (PlayerProgressionManager.playerLevelToXpDic.ContainsKey(pda.playerBasicOnlineStats.level + 1))
                minXpToLevelUp = PlayerProgressionManager.playerLevelToXpDic[pda.playerBasicOnlineStats.level + 1];


            newHonor += honorGained;


            PlayerProgressionManager.Rank rank = PlayerProgressionManager.GetClosestAndNextRank(pda.playerBasicOnlineStats.honor)[0];



            if (newXp >= minXpToLevelUp)
            {
                Debug.Log("LEVEL UP");
                newLevel = pda.playerBasicOnlineStats.level + 1;
            }
            GameManager.instance.carnageReport = new CarnageReport(rank, pda.level, pda.xp, xpAndCreditGain, pda.honor, honorGained, newXp >= minXpToLevelUp && pda.level < 50, newLevel);

            WWWForm form = new WWWForm();
            form.AddField("service", "SaveBasicOnlineStats");

            form.AddField("playerId", playerId.ToString());
            form.AddField("newLevel", newLevel);
            form.AddField("newXp", newXp);
            form.AddField("newCredits", newCredits);
            form.AddField("newHonor", newHonor);


            using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    Debug.Log("SaveBasicOnlineStats_Coroutine");
                    Debug.Log(www.result);
                    Debug.Log(www.downloadHandler.text);

                    if (www.result.ToString().Contains("uccess"))
                        if (xpAndCreditGain > 0)
                            //GameManager.GetRootPlayer().GetComponent<KillFeedManager>().EnterNewFeed($"<color=\"yellow\">Gained {xpAndCreditGain} Xp and Cuckbucks");
                            //else
                            //    GameManager.GetRootPlayer().GetComponent<KillFeedManager>().EnterNewFeed($"{www.result}");
                            if (newXp >= minXpToLevelUp)
                            {
                                //GameManager.GetRootPlayer().GetComponent<KillFeedManager>().EnterNewFeed($"<color=\"yellow\">LEVEL UP! ({newLevel})");
                            }


                    if (www.downloadHandler.text.Contains("Could not save swarm stats"))
                    {
                        Debug.LogError("Could not save swarm stats");

                    }
                    else if (www.downloadHandler.text.Contains("Swarm stats saved"))
                    {
                        Debug.Log("Swarm stats saved successfully");
                    }
                }
            }
            StartCoroutine(Login_Coroutine_Set_Online_Stats(playerId));

            StartCoroutine(Login_Coroutine_Set_PvE_Stats(playerId));
            StartCoroutine(Login_Coroutine_Set_PvP_Stats(playerId));
        }
    }

    IEnumerator SaveSwarmStats_Coroutine(PlayerSwarmMatchStats onlinePlayerSwarmScript)
    {
        long playerId = pda.steamid;
        int newKills = pda.GetPvEKills() + onlinePlayerSwarmScript.kills;
        int newDeaths = pda.GetPvEDeaths() + onlinePlayerSwarmScript.deaths;
        int newHeadshots = pda.GetPvEHeadshots() + onlinePlayerSwarmScript.headshots;
        int newHighestScore = pda.GetPvEHighestPoints();
        if (onlinePlayerSwarmScript.GetTotalPoints() > newHighestScore)
            newHighestScore = onlinePlayerSwarmScript.GetTotalPoints();

        Debug.Log("bababooey " + pda.GetPvEHighestPoints() + " " + onlinePlayerSwarmScript.GetTotalPoints() + " " + newHighestScore + "\nXp to Level: ");

        //GameManager.instance.GetMyPlayer().GetComponent<PlayerUI>().killFeedManager.EnterNewFeed("Saving");


        WWWForm form = new WWWForm();
        form.AddField("service", "SaveSwarmStats");
        form.AddField("playerId", playerId.ToString());
        form.AddField("newKills", newKills);
        form.AddField("newDeaths", newDeaths);
        form.AddField("newHeadshots", newHeadshots);
        form.AddField("newHighestPoints", newHighestScore);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save swarm stats"))
                {
                    Debug.LogError("Could not save swarm stats");

                }
                else if (www.downloadHandler.text.Contains("Swarm stats saved"))
                {
                    Debug.Log("Swarm stats saved successfully");
                }
            }
        }
    }

    IEnumerator SaveMultiplayerStats_Coroutine(PlayerMultiplayerMatchStats playerMultiplayerStats)
    {
        long playerId = pda.steamid;
        int newKills = pda.GetPvPKills() + playerMultiplayerStats.kills;
        int newDeaths = pda.GetPvPDeaths() + playerMultiplayerStats.deaths;
        int newHeadshots = pda.GetPvPHeadshots() + playerMultiplayerStats.headshots;
        int newMeleeKills = pda.PvPMeleeKills + playerMultiplayerStats.meleeKills;
        int newGrenadeKills = pda.PvPGrenadeKills + playerMultiplayerStats.grenadeKills;

        WWWForm form = new WWWForm();
        form.AddField("service", "SaveMultiplayerStats");
        form.AddField("playerId", playerId.ToString());
        form.AddField("newKills", newKills);
        form.AddField("newDeaths", newDeaths);
        form.AddField("newHeadshots", newHeadshots);
        form.AddField("newMeleeKills", newMeleeKills);
        form.AddField("newGrenadeKills", newGrenadeKills);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save swarm stats"))
                {
                    Debug.LogError("Could not save swarm stats");

                }
                else if (www.downloadHandler.text.Contains("Swarm stats saved"))
                {
                    Debug.Log("Swarm stats saved successfully");
                }
            }
        }
    }


    IEnumerator Register_Coroutine(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "register");
        form.AddField("username", username);
        form.AddField("password", password);

        //using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/unity/database.php", form))
        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log(www.result);
                //Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Duplicate"))
                {
                    Launcher.instance.OnCreateRoomFailed(0, "That username already exists");
                }
                else
                {
                    Launcher.instance.ShowPlayerMessage("Registered successfully!");
                }
            }
        }
    }



    IEnumerator Login_Coroutine_Set_Online_Stats(long playerId, PlayerDatabaseAdaptor _pda = null)
    {
        Debug.Log("Login_Coroutine_Set_Online_Stats");
        WWWForm form = new WWWForm();
        form.AddField("service", "getBasicOnlineData");
        form.AddField("playerId", playerId.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                string jsonarray = www.downloadHandler.text;



                try
                {
                    PlayerDatabaseAdaptor.PlayerCommonData pd = PlayerDatabaseAdaptor.PlayerCommonData.CreateFromJSON(jsonarray);
                    if (_pda == null)
                        pda.playerBasicOnlineStats = pd;
                    else
                        _pda.playerBasicOnlineStats = pd;

                    if (www.result.ToString().Contains("Success"))
                    {
                        Debug.Log("Login_Coroutine_Set_Online_Stats SUCCESS");
                        Debug.Log(pda.playerBasicOnlineStats.armor_color_palette);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    if (www.downloadHandler.text.Contains("Could not fetch pvp stats"))
                    {
                        Launcher.instance.OnCreateRoomFailed(0, "Could not fetch pvp stats");
                    }
                }
            }
        }
    }

    IEnumerator Login_Coroutine_Set_PvP_Stats(long playerId, PlayerDatabaseAdaptor _pda = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "getBasicPvPStats");
        form.AddField("playerId", playerId.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log(www.result);
                //Debug.Log(www.downloadHandler.text);

                string jsonarray = www.downloadHandler.text;

                try
                {
                    PlayerDatabaseAdaptor.PlayerBasicPvPStats pd = PlayerDatabaseAdaptor.PlayerBasicPvPStats.CreateFromJSON(jsonarray);

                    if (_pda == null)
                        pda.SetPlayerBasicPvPStats(pd);
                    else
                        _pda.SetPlayerBasicPvPStats(pd);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    if (www.downloadHandler.text.Contains("Could not fetch pvp stats"))
                    {
                        Launcher.instance.OnCreateRoomFailed(0, "Could not fetch pvp stats");
                    }
                }
            }
        }
    }

    IEnumerator Login_Coroutine_Set_PvE_Stats(long playerId, PlayerDatabaseAdaptor _pda = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "getBasicPvEStats");
        form.AddField("playerId", playerId.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                string jsonarray = www.downloadHandler.text;

                try
                {
                    PlayerDatabaseAdaptor.PlayerBasicPvEStats pd = PlayerDatabaseAdaptor.PlayerBasicPvEStats.CreateFromJSON(jsonarray);

                    if (_pda == null)
                        pda.SetPlayerBasicPvEStats(pd);
                    else
                        _pda.SetPlayerBasicPvEStats(pd);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    if (www.downloadHandler.text.Contains("Could not fetch pve stats"))
                    {
                        Launcher.instance.OnCreateRoomFailed(0, "Could not fetch pve stats");
                    }
                }
            }
        }
    }











    // Armory
    public IEnumerator SaveUnlockedArmorStringData_Coroutine(PlayerArmorPiece playerArmorPiece)
    {
        pda.unlockedArmorDataString += $"-{playerArmorPiece.entity}-";
        //pda.unlockedArmorDataString.Replace("\n\n", "\n");
        pda.credits -= playerArmorPiece.cost;

        WWWForm form = new WWWForm();
        form.AddField("service", "SaveUnlockedArmorStringData");
        form.AddField("playerId", pda.steamid.ToString());

        form.AddField("newUnlockedArmorStringData", pda.unlockedArmorDataString);
        form.AddField("newPlayerCredits", pda.credits);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save UnlockedArmorStringData"))
                {
                    Debug.LogError("Could not save UnlockedArmorStringData");

                }
                else if (www.downloadHandler.text.Contains("UnlockedArmorStringData saved successfully"))
                {
                    Debug.Log("UnlockedArmorStringData saved successfully");
                }
            }

            ArmoryManager.instance.OnArmorBuy_Delegate();
        }
    }

    [Tooltip("FOR ACHIEVEMENTS ONLY")]
    public static IEnumerator UnlockArmorPiece_Coroutine(string _armorPieceCodename)
    {
        Debug.Log("UnlockArmorPiece_Coroutine");
        webManagerInstance.pda.unlockedArmorDataString += $"-{_armorPieceCodename}-";
        //pda.unlockedArmorDataString.Replace("\n\n", "\n");

        WWWForm form = new WWWForm();
        form.AddField("service", "SaveUnlockedArmorStringData");
        form.AddField("playerId", webManagerInstance.pda.ToString());

        form.AddField("newUnlockedArmorStringData", webManagerInstance.pda.unlockedArmorDataString);
        form.AddField("newPlayerCredits", webManagerInstance.pda.credits);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save UnlockedArmorStringData"))
                {
                    Debug.LogError("Could not save UnlockedArmorStringData");

                }
                else if (www.downloadHandler.text.Contains("UnlockedArmorStringData saved successfully"))
                {
                    Debug.Log("UnlockedArmorStringData saved successfully");
                }
            }
        }
    }

    public IEnumerator SaveEquippedArmorStringData_Coroutine(int playerDbId, string data)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "SaveEquippedArmorDataString");
        form.AddField("playerId", playerDbId);

        form.AddField("newEquippedArmorStringData", data);

        Debug.Log(data);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save SaveEquippedArmorDataString"))
                {
                    Debug.LogError("Could not save SaveEquippedArmorDataString");

                }
                else if (www.downloadHandler.text.Contains("SaveEquippedArmorDataString saved successfully"))
                {
                    Debug.Log("SaveEquippedArmorDataString saved successfully");
                }
            }
        }
    }



    public IEnumerator SaveArmorColorPalette_Coroutine(int steamid, string colorName)
    {
        Debug.Log("SaveArmorColorPalette_Coroutine");
        WWWForm form = new WWWForm();
        form.AddField("service", "SaveArmorColorPalette");
        form.AddField("playerId", steamid);

        form.AddField("newArmorColorPaletteData", colorName);


        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save SaveEquippedArmorDataString"))
                {
                    Debug.LogError("Could not save SaveEquippedArmorDataString");

                }
                else if (www.downloadHandler.text.Contains("SaveEquippedArmorDataString saved successfully"))
                {
                    Debug.Log("SaveEquippedArmorDataString saved successfully");
                }
            }
        }
    }
}
