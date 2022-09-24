using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    public static void SavePlayer (MyPlayerInfo player)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player" + player.playerID + "character" + player.characterID + ".nut";
        Debug.Log(path);
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerLocalSaveData data = new PlayerLocalSaveData(player);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerLocalSaveData LoadPlayer(int playerID, int characterID, MyPlayerInfo player)
    {
        string path = Application.persistentDataPath + "/player" + playerID + "character" + characterID + ".nut";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerLocalSaveData data = formatter.Deserialize(stream) as PlayerLocalSaveData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            SavePlayer(player);
            Debug.Log("Save File Created in " + path);
            return null;
        }
    }
}
