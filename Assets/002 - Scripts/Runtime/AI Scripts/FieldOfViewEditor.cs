using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        Transform or = fov.GetComponent<Actor>().losSpawn;

        Handles.DrawWireArc(or.position, Vector3.up, Vector3.forward, 360, fov.radius);

        Vector3 viewAngle01 = DirectionFromAngle(or.eulerAngles.y, -fov.angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(or.eulerAngles.y, fov.angle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(or.position, or.position + viewAngle01 * fov.radius);
        Handles.DrawLine(or.position, or.position + viewAngle02 * fov.radius);

        if (fov.canSeePlayer)
        {
            Handles.color = Color.green;
            Handles.DrawLine(or.position, fov.playerRef.transform.position);
        }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}