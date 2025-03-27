using System.Collections.Generic;
using UnityEngine;
// https://answers.unity.com/questions/125049/is-there-any-way-to-view-the-console-in-a-build.html
public class ConsoleToGUI : MonoBehaviour
{
    string myLog = "*begin log";
    string filename = "";
    bool doShow = true;
    int kChars = 1400;


    [SerializeField] List<string> _myLogList = new List<string>();
    int kLines = 65;


    float _LKeyHeldTime;



    private void Start()
    {
        doShow = false;
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.L))
        {
            if (_LKeyHeldTime < 1) _LKeyHeldTime += Time.deltaTime;

            if (_LKeyHeldTime >= 1 && _LKeyHeldTime < 2)
            {
                doShow = !doShow;

                if (doShow)
                {
                    Application.logMessageReceived -= Log;
                    Application.logMessageReceived += Log;
                }

                Debug.unityLogger.logEnabled = doShow;

#if UNITY_EDITOR
                Debug.unityLogger.logEnabled = true;
#endif






                _LKeyHeldTime = 3;
            }
        }

        if (Input.GetKeyUp(KeyCode.L))
        {
            _LKeyHeldTime = 0;
        }


        //        if (Input.GetKeyDown(KeyCode.L))
        //        {
        //            doShow = !doShow;

        //            if (doShow)
        //            {
        //                Application.logMessageReceived -= Log;
        //                Application.logMessageReceived += Log;
        //            }

        //            Debug.unityLogger.logEnabled = doShow;

        //#if UNITY_EDITOR
        //            Debug.unityLogger.logEnabled = true;
        //#endif
        //        }
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        // for onscreen...
        //myLog = myLog + "\n" + logString;
        //if (myLog.Length > kChars) { myLog = myLog.Substring(myLog.Length - kChars); }

        //_myLogList.Add(logString);
        _myLogList.Insert(0, logString);

        if (_myLogList.Count > kLines) _myLogList.RemoveAt(_myLogList.Count - 1);

        myLog = "";
        foreach (string s in _myLogList)
        {
            myLog = myLog + "\n" + s;
        }


        // for the file ...
        //if (filename == "")
        //{
        //    string d = System.Environment.GetFolderPath(
        //       System.Environment.SpecialFolder.Desktop) + "/SPACE_WACKOS_LOGS";
        //    System.IO.Directory.CreateDirectory(d);
        //    string r = Random.Range(1000, 9999).ToString();
        //    filename = d + "/log-" + r + ".txt";
        //}
        //try { System.IO.File.AppendAllText(filename, logString + "\n"); }
        //catch { }
    }

    void OnGUI()
    {
        if (!doShow) { return; }
        //GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        GUI.TextArea(new Rect(10, 10, 540, 1060), myLog);
    }
}