using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfo : MonoBehaviour
{
    public GameSettings gameSettings;
    public string mapName;
    public Sprite mapImage;
    [TextArea]
    public string mapInfo = "";

    public void chooseMap()
    {
        // Multiplayer
        if (mapName == "Pitchfork")
            gameSettings.togglePitchfork();
        else if (mapName == "Testing Room")
            gameSettings.toggleTestingRoom();

        // Swarm
        if (mapName == "Downpoor")
            gameSettings.toggleDownpoor();
        else if (mapName == "Tumbleweed")
            gameSettings.toggleTumbleweed();
    }
}
