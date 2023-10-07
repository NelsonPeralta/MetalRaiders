using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapAiWaypoints : MonoBehaviour
{
    public List<Transform> waypoints { get { return _waypoints; } }

    [SerializeField] List<Transform> _waypoints;
    [SerializeField] List<WaypointAssignment> _assignments = new List<WaypointAssignment>();


    int randomNumber;

    private void Awake()
    {
        randomNumber = Random.Range(0, waypoints.Count);
        _waypoints = GetComponentsInChildren<Transform>().ToList();
    }

    public struct WaypointAssignment
    {
        public GameObject user;
        public Transform waypoint;

        public WaypointAssignment(GameObject g, Transform v)
        {
            this.user = g;
            this.waypoint = v;
        }
    }


    public WaypointAssignment AssignObjectToWaypoint(GameObject u, Transform w)
    {
         WaypointAssignment wa = new WaypointAssignment(u, w);

        _assignments.Add(wa);

        return wa;
    }


    public Transform GetRandomWaypoint()
    {
        randomNumber = Random.Range(0, waypoints.Count);

        return _waypoints[randomNumber];
    }
}
