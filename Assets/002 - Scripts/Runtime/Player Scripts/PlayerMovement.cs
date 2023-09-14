using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=f473C43s8nE&ab_channel=Dave%2FGameDevelopment

public class PlayerMovement : MonoBehaviour
{
    public bool isGrounded
    {
        get
        {
            return _isGrounded = _groundCheckScript.isGrounded;
        }
    }
    public Rewired.Player rewiredPlayer { get { if (_rewiredPlayer == null) _rewiredPlayer = _playerController.rewiredPlayer; return _rewiredPlayer; } }

    public bool onSlope
    {
        get
        {
            if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, _playerHeight * 0.5f + 0.5f, _slopeLayerMask))
            {
                _slopeHitTransform = _slopeHit.transform;
                _slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                _onSlope = _slopeAngle < _maxSlopeAngle && _slopeAngle != 0;
                return _onSlope;
            }
            else
            {
                _onSlope = false;
                _slopeAngle = 0;
                _slopeHitTransform = null;
            }
            return false;
        }
    }






    Vector3 SlopeMoveDirection
    {
        get
        {
            try
            {
                _slopeHitTransform = _slopeHit.transform;
            }
            catch { }
            return Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;
        }
    }




    [SerializeField] GroundCheck _groundCheckScript;

    [SerializeField] float _playerHeight, _moveSpeed, _groundDrag, _jumpForce;

    [SerializeField] Transform _orientation;

    [SerializeField] float _horizontalInput, _verticalInput;

    [SerializeField] float _maxSlopeAngle, _slopeAngle;
    [SerializeField] Transform _slopeHitTransform;
    [SerializeField] LayerMask _slopeLayerMask;

    RaycastHit _slopeHit;

    [SerializeField] bool _isGrounded, _onSlope;

    Vector3 _moveDirection;

    Rigidbody _rb;

    PlayerController _playerController;

    Rewired.Player _rewiredPlayer;




    // Start is called before the first frame update
    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        Input();
        DragCheck();
        SpeedCheck();
        Jump();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void Input()
    {
        _horizontalInput = rewiredPlayer.GetAxis("Move Horizontal");
        _verticalInput = rewiredPlayer.GetAxis("Move Vertical");

        if (_playerController.activeControllerType == Rewired.ControllerType.Keyboard
            || _playerController.activeControllerType == Rewired.ControllerType.Mouse)
        {

            if (rewiredPlayer.GetAxis("Move Horizontal") != 0)
                _horizontalInput = Mathf.Clamp(3 * rewiredPlayer.GetAxis("Move Horizontal"), -1, 1);
            if (rewiredPlayer.GetAxis("Move Vertical") != 0)
                _verticalInput = Mathf.Clamp(3 * rewiredPlayer.GetAxis("Move Vertical"), -1, 1);


            //if (rewiredPlayer.GetAxis("Move Horizontal") != 0)
            //    _horizontalInput = 1 * Mathf.Sign(rewiredPlayer.GetAxis("Move Horizontal"));
            //if (rewiredPlayer.GetAxis("Move Vertical") != 0)
            //    _verticalInput = 1 * Mathf.Sign(rewiredPlayer.GetAxis("Move Vertical"));
        }
    }

    void MovePlayer()
    {
        try
        {
            if (isGrounded)
            {
                _moveDirection = _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;

                if (onSlope)
                    _moveDirection = SlopeMoveDirection;

                _rb.AddForce(_moveDirection.normalized * _moveSpeed * 10f, ForceMode.Force);
            }
        }
        catch { }
    }

    void DragCheck()
    {
        if (isGrounded)
            _rb.drag = _groundDrag;
        else
            _rb.drag = 0;
    }

    void SpeedCheck()
    {
        Vector3 flatVelVect = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

        // limit velocity if needed
        if (flatVelVect.magnitude > _moveSpeed)
        {
            Vector3 limitedVelVect = flatVelVect.normalized * _moveSpeed;
            _rb.velocity = new Vector3(limitedVelVect.x, _rb.velocity.y, limitedVelVect.z);
        }
    }

    void Jump()
    {
        if (isGrounded && rewiredPlayer.GetButtonDown("Jump"))
        {
            // reset y velocity
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

            _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
        }
    }
}
