using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notes : MonoBehaviour
{
    [TextArea(0, 100)]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string notes = "Write Notes Here";
}
