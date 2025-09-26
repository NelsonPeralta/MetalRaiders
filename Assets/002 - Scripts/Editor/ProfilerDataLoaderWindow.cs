using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Profiling;

public class ProfilerDataLoaderWindow : EditorWindow
{
    static List<string> s_cachedFilePaths;
    static int s_chosenIndex = -1;

    [MenuItem("Window/ProfilerDataLoader")]
    static void Init()
    {
        ProfilerDataLoaderWindow window = (ProfilerDataLoaderWindow)EditorWindow.GetWindow(typeof(ProfilerDataLoaderWindow));
        window.Show();

        ReadProfilerDataFiles();
    }

    static void ReadProfilerDataFiles()
    {
        // make sure the profiler releases the file handle
        // to any of the files we're about to load in
        Profiler.logFile = "";

        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "profilerLog*");

        s_cachedFilePaths = new List<string>();

        // we want to ignore all of the binary
        // files that end in .data. The Profiler
        // will figure that part out
        Regex test = new Regex(".data$");

        for (int i = 0; i < filePaths.Length; i++)
        {
            string thisPath = filePaths[i];

            Match match = test.Match(thisPath);

            if (!match.Success)
            {
                // not a binary file, add it to the list
                Log.Print(() =>"Found file: " + thisPath);
                s_cachedFilePaths.Add(thisPath);
            }
        }

        s_chosenIndex = -1;
    }

    void OnGUI()
    {
        if (GUILayout.Button("Find Files"))
        {
            ReadProfilerDataFiles();
        }

        if (s_cachedFilePaths == null)
            return;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Files");

        EditorGUILayout.BeginHorizontal();

        // create some styles to organize the buttons, and show
        // the most recently-selected button with red text
        GUIStyle defaultStyle = new GUIStyle(GUI.skin.button);
        defaultStyle.fixedWidth = 40f;

        GUIStyle highlightedStyle = new GUIStyle(defaultStyle);
        highlightedStyle.normal.textColor = Color.red;

        for (int i = 0; i < s_cachedFilePaths.Count; ++i)
        {

            // list 5 items per row
            if (i % 5 == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }

            GUIStyle thisStyle = null;

            if (s_chosenIndex == i)
            {
                thisStyle = highlightedStyle;
            }
            else
            {
                thisStyle = defaultStyle;
            }

            if (GUILayout.Button("" + i, thisStyle))
            {
                Profiler.AddFramesFromFile(s_cachedFilePaths[i]);

                s_chosenIndex = i;
            }
        }

        EditorGUILayout.EndHorizontal();
    }
}
