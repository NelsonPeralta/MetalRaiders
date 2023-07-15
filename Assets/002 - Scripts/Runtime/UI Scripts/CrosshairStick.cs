using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairStick : MonoBehaviour
{
    public enum BloomBehaviour { position, scale }
    public enum BloomDirection { left, right, up, down, lu, ru, ld, rd }

    [SerializeField] WeaponProperties _weaponProperties;
    [SerializeField] BloomBehaviour _bloomBehaviour;
    [SerializeField] BloomDirection _bloomDirection;
    [SerializeField] int _bloomFactor;

    Vector3 _originalPosition, _originalScale;

    // Start is called before the first frame update
    void Start()
    {
        _bloomFactor = 1;
        _originalPosition = transform.localPosition;
        _originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (_bloomBehaviour == BloomBehaviour.position)
        {
            if (_bloomDirection == BloomDirection.left)
                try
                {
                    transform.localPosition = new Vector3(_originalPosition.x - (_weaponProperties.bloom * _bloomFactor), 0, 0);
                }
                catch { }

            if (_bloomDirection == BloomDirection.right)
            {
                try
                {
                    transform.localPosition = new Vector3(_originalPosition.x + (_weaponProperties.bloom * _bloomFactor), 0, 0);
                }
                catch { }
            }

            if (_bloomDirection == BloomDirection.up)
            {
                try
                {
                    transform.localPosition = new Vector3(0, _originalPosition.y + (_weaponProperties.bloom * _bloomFactor), 0);
                }
                catch { }
            }

            if (_bloomDirection == BloomDirection.down)
            {
                try
                {
                    transform.localPosition = new Vector3(0, _originalPosition.y - (_weaponProperties.bloom * _bloomFactor), 0);
                }
                catch { }
            }









            if (_bloomDirection == BloomDirection.lu)
                try
                {
                    transform.localPosition = new Vector3(_originalPosition.x - (_weaponProperties.bloom * _bloomFactor * 0.7071f),
                        _originalPosition.y + (_weaponProperties.bloom * _bloomFactor * .7071f), 0);
                }
                catch { }

            if (_bloomDirection == BloomDirection.ru)
                try
                {
                    transform.localPosition = new Vector3(_originalPosition.x + (_weaponProperties.bloom * _bloomFactor * 0.7071f),
                        _originalPosition.y + (_weaponProperties.bloom * _bloomFactor * .7071f), 0);
                }
                catch { }

            if (_bloomDirection == BloomDirection.ld)
                try
                {
                    transform.localPosition = new Vector3(_originalPosition.x - (_weaponProperties.bloom * _bloomFactor * 0.7071f),
                        _originalPosition.y - (_weaponProperties.bloom * _bloomFactor * .7071f), 0);
                }
                catch { }

            if (_bloomDirection == BloomDirection.rd)
                try
                {
                    transform.localPosition = new Vector3(_originalPosition.x + (_weaponProperties.bloom * _bloomFactor * 0.7071f),
                        _originalPosition.y - (_weaponProperties.bloom * _bloomFactor * .7071f), 0);
                }
                catch { }





        }
        else if (_bloomBehaviour == BloomBehaviour.scale)
        {
            try
            {
                transform.localScale = new Vector3(_originalScale.x + (_weaponProperties.bloom * _bloomFactor), _originalScale.y + (_weaponProperties.bloom * _bloomFactor), _originalScale.z + (_weaponProperties.bloom * _bloomFactor));
            }
            catch { }
        }
    }
}
