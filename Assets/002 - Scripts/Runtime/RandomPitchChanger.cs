using UnityEngine;

public class RandomPitchChanger : MonoBehaviour
{
    [Range(0.01f, 10f)]
    public float pitchRandomMultiplier = 1f;


    [SerializeField] AudioSource _audioSource;

    private void OnEnable()
    {

    }
}
