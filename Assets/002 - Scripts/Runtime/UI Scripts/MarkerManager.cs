using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MarkerManager : MonoBehaviour
{
    public static MarkerManager instance;


    [SerializeField] Marker _marker, _markerEnSpot;
    [SerializeField] List<Marker> _markers = new List<Marker>(), _markersEnSpot = new List<Marker>();



    // called zero
    private void Awake()
    {
        instance = this;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;


        Log.Print("MarkerManager OnSceneLoaded > 0");
        instance._markers = new List<Marker>();
        instance._markersEnSpot = new List<Marker>();

        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * 10; i++)
        {
            GameObject obj = Instantiate(_marker.gameObject, transform.position, transform.rotation);
            obj.SetActive(false);
            instance._markers.Add(obj.GetComponent<Marker>());
            obj.transform.parent = gameObject.transform;


            obj = Instantiate(_markerEnSpot.gameObject, transform.position, transform.rotation);
            obj.SetActive(false);
            instance._markersEnSpot.Add(obj.GetComponent<Marker>());
            obj.transform.parent = gameObject.transform;
        }

        CurrentRoomManager.instance.AddSpawnedMappAddOn(null);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
    }

    // called first
    private void OnEnable()
    {

    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //if (instance == this)
        //{
        //    Log.Print("MarkerManager OnSceneLoaded instance");
        //    if (scene.buildIndex > 0)
        //    {
        //        Log.Print("MarkerManager OnSceneLoaded > 0");
        //        instance._markers = new List<Marker>();
        //        instance._markersEnSpot = new List<Marker>();

        //        for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * 10; i++)
        //        {
        //            GameObject obj = Instantiate(_marker.gameObject, transform.position, transform.rotation);
        //            obj.SetActive(false);
        //            instance._markers.Add(obj.GetComponent<Marker>());
        //            obj.transform.parent = gameObject.transform;


        //            obj = Instantiate(_markerEnSpot.gameObject, transform.position, transform.rotation);
        //            obj.SetActive(false);
        //            instance._markersEnSpot.Add(obj.GetComponent<Marker>());
        //            obj.transform.parent = gameObject.transform;
        //        }
        //    }
        //    else
        //    {
        //        Log.Print("MarkerManager OnSceneLoaded 0");
        //        if (instance._markers.Count > 0)
        //            for (int i = instance._markers.Count; i-- > 0;)
        //                if (instance._markers[i] != null)
        //                    Destroy(instance._markers[i].gameObject);

        //        if (instance._markersEnSpot.Count > 0)
        //            for (int i = instance._markersEnSpot.Count; i-- > 0;)
        //                if (instance._markersEnSpot[i] != null)
        //                    Destroy(instance._markersEnSpot[i].gameObject);

        //        instance._markers.Clear(); instance._markersEnSpot.Clear();
        //    }
        //}
    }

    // called third
    private void Start()
    {

    }


    public void SpawnNormalMarker(Vector3 pos, int playerPhotonView)
    {
        Log.Print($"SpawnNormalMarker {playerPhotonView}");
        foreach (Marker obj in instance._markers)
            if (!obj.gameObject.activeSelf)
            {
                obj.transform.position = pos;
                obj.targetPlayer = GameManager.GetPlayerWithPhotonView(playerPhotonView);
                ChangeLayer(obj.gameObject, GameManager.GetPlayerWithPhotonView(playerPhotonView).rid);
                obj.gameObject.SetActive(true);
                StartCoroutine(DisableObjectAfterTime(obj.gameObject, 4));
                break;
            }
    }


    public void SpawnEnnSpotMarker(Vector3 pos, int player_id)
    {
        foreach (Marker obj in instance._markersEnSpot)
            if (!obj.gameObject.activeSelf)
            {
                obj.transform.position = pos;
                obj.targetPlayer = GameManager.GetPlayerWithPhotonView(player_id);
                Log.Print($"SpawnEnnSpotMarker {obj.gameObject}");
                Log.Print($"SpawnEnnSpotMarker {player_id}");
                ChangeLayer(obj.gameObject, GameManager.GetPlayerWithPhotonView(player_id).rid);
                obj.gameObject.SetActive(true);
                StartCoroutine(DisableObjectAfterTime(obj.gameObject, 4));
                break;
            }
    }


    void ChangeLayer(GameObject obj, int playerRewiredId)
    {
        if (playerRewiredId == 0)
            GameManager.SetLayerRecursively(obj, 20);
        else if (playerRewiredId == 1)
            GameManager.SetLayerRecursively(obj, 21);
        else if (playerRewiredId == 2)
            GameManager.SetLayerRecursively(obj, 22);
        else if (playerRewiredId == 3)
            GameManager.SetLayerRecursively(obj, 23);
    }





    public IEnumerator DisableObjectAfterTime(GameObject obj, float time = 1)
    {
        yield return new WaitForSeconds(time);
        if (!CurrentRoomManager.instance.gameOver && obj.activeSelf)
        {
            if (SceneManager.GetActiveScene().buildIndex > 0)
                obj.SetActive(false);
        }
    }
}
