using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GametypeInfo : MonoBehaviour
{
    public GameSettings gameSettings;
    public string gametype;
    
    public void chooseGametype()
    {
            Log.Print(() =>"Clicke on \n GamteTypeInfo");
            if (gametype == "Slayer")
                gameSettings.toggleSlayer();
            else if (gametype == "Team Slayer")
                gameSettings.toggleTeamSlayer();
        
    }
}
