using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MarkerManager : MonoBehaviour
{
    public static MarkerManager instance;


    [SerializeField] Marker _marker, _markerEnSpot;
    [SerializeField] List<Marker> _markers, _markersEnSpot;



    // called zero
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called first
    private void OnEnable()
    {

    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        print("MarkerManager OnSceneLoaded");
        if (instance)
        {
            print("MarkerManager OnSceneLoaded");
            if (scene.buildIndex > 0)
            {
                for (int i = 0; i < CurrentRoomManager.instance.expectedNbPlayers * 10; i++)
                {
                    GameObject obj = Instantiate(_marker.gameObject, transform.position, transform.rotation);
                    obj.SetActive(false);
                    _markers.Add(obj.GetComponent<Marker>());
                    obj.transform.parent = gameObject.transform;


                    obj = Instantiate(_markerEnSpot.gameObject, transform.position, transform.rotation);
                    obj.SetActive(false);
                    _markersEnSpot.Add(obj.GetComponent<Marker>());
                    obj.transform.parent = gameObject.transform;
                }
            }
            else
            {
                for (int i = _markers.Count; i-- > 0;)
                {
                    Destroy(_markers[i].gameObject);
                }

                for (int i = _markersEnSpot.Count; i-- > 0;)
                {
                    Destroy(_markersEnSpot[i].gameObject);
                }
            }
        }
    }

    // called third
    private void Start()
    {

    }


    public void SpawnNormalMarker(Vector3 pos, int player_id)
    {
        foreach (Marker obj in _markers)
            if (!obj.gameObject.activeSelf)
            {
                obj.transform.position = pos;
                obj.lookAtThisTrans = GameManager.GetPlayerWithId(player_id).transform;
                ChangeLayer(obj.gameObject, GameManager.GetPlayerWithId(player_id).rid);
                obj.gameObject.SetActive(true);
                StartCoroutine(DisableObjectAfterTime(obj.gameObject, 4));
                break;
            }
    }


    public void SpawnEnnSpotMarker(Vector3 pos, int player_id)
    {
        foreach (Marker obj in _markersEnSpot)
            if (!obj.gameObject.activeSelf)
            {
                obj.transform.position = pos;
                obj.lookAtThisTrans = GameManager.GetPlayerWithId(player_id).transform;
                ChangeLayer(obj.gameObject, GameManager.GetPlayerWithId(player_id).rid);
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
        obj.SetActive(false);
    }
}
