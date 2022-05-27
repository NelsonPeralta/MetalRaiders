using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OnlineMapChooser : MonoBehaviour
{
    public int levelIndex;
    public Launcher launcherInstance;
    public Sprite publicMapPreview;
    public Sprite mapPreview;
    private void Start()
    {
        if (levelIndex == 0)
            levelIndex = 1;
        launcherInstance = Launcher.instance;
    }

    public void ChooseThisMap()
    {
        launcherInstance.levelToLoadIndex = levelIndex;
        if (mapPreview)
            publicMapPreview = mapPreview;
    }
}
