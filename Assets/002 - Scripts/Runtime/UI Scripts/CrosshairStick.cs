using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairStick : MonoBehaviour
{
    public enum BloomBehaviour { position, scale }
    public enum BloomDirection { left, right, up, down }

    [SerializeField] WeaponProperties _weaponProperties;
    [SerializeField] BloomBehaviour _bloomBehaviour;
    [SerializeField] BloomDirection _bloomDirection;

    Vector3 _originalPosition, _originalScale;

    float _modBloom;

    // Start is called before the first frame update
    void Start()
    {
        _originalPosition = transform.localPosition;
        _originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        _modBloom = _weaponProperties.bloom;
        if (_bloomBehaviour == BloomBehaviour.position)
        {
            _modBloom *= 4;
            if (_bloomDirection == BloomDirection.left)
            {
                try
                {
                    transform.localPosition = new Vector3(_originalPosition.x - _modBloom, 0, 0);
                }
                catch { }
            }

            if (_bloomDirection == BloomDirection.right)
            {
                try
                {
                    transform.localPosition = new Vector3(_originalPosition.x + _modBloom, 0, 0);
                }
                catch { }
            }

            if (_bloomDirection == BloomDirection.up)
            {
                try
                {
                    transform.localPosition = new Vector3(0, _originalPosition.y + _modBloom, 0);
                }
                catch { }
            }

            if (_bloomDirection == BloomDirection.down)
            {
                try
                {
                    transform.localPosition = new Vector3(0, _originalPosition.y - _modBloom, 0);
                }
                catch { }
            }
        }
        else if (_bloomBehaviour == BloomBehaviour.scale)
        {
            _modBloom *= 2;

            try
            {
                transform.localScale = new Vector3(_originalScale.x + _modBloom, _originalScale.y + _modBloom, _originalScale.z + _modBloom);
            }
            catch { }
        }
    }
}
