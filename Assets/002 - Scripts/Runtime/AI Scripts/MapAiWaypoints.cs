using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAiWaypoints : MonoBehaviour
{
    public List<Transform> waypoints { get { return _waypoint; } }

    [SerializeField] List<Transform> _waypoint;
    [SerializeField] List<WaypointAssignment> _assignments = new List<WaypointAssignment>();


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
}
