using System;
using System.Linq;
using UnityEngine;
using Rewired;

namespace FPSControllerLPFP
{
    /// Manages a first person character
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(AudioSource))]
    public class FpsControllerLPFP : MonoBehaviour
    {
#pragma warning disable 649
		[Header("Arms")]
        [Tooltip("The transform component that holds the gun camera."), SerializeField]
        public Transform arms;

        [Tooltip("The position of the arms and gun camera relative to the fps controller GameObject."), SerializeField]
        public Vector3 armPosition;

		[Header("Audio Clips")]
        [Tooltip("The audio clip that is played while walking."), SerializeField]
        public AudioClip walkingSound;

        [Tooltip("The audio clip that is played while running."), SerializeField]
        public AudioClip runningSound;

		[Header("Movement Settings")]
        [Tooltip("How fast the player moves while walking and strafing."), SerializeField]
        public float walkingSpeed = 5f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        public float runningSpeed = 9f;

        [Tooltip("Approximately the amount of time it will take for the player to reach maximum running or walking speed."), SerializeField]
        public float movementSmoothness = 0.125f;

        [Tooltip("Amount of force applied to the player when jumping."), SerializeField]
        public float jumpForce = 35f;

		[Header("Look Settings")]
        [Tooltip("Rotation speed of the fps controller."), SerializeField]
        public float mouseSensitivity = 7f;

        [Tooltip("Approximately the amount of time it will take for the fps controller to reach maximum rotation speed."), SerializeField]
        public float rotationSmoothness = 0.05f;

        [Tooltip("Minimum rotation of the arms and camera on the x axis."),
         SerializeField]
        public float minVerticalAngle = -90f;

        [Tooltip("Maximum rotation of the arms and camera on the axis."),
         SerializeField]
        public float maxVerticalAngle = 90f;

        [Tooltip("The names of the axes and buttons for Unity's Input Manager."), SerializeField]
        public FpsInput input;
#pragma warning restore 649

        public Rigidbody _rigidbody;
        public CapsuleCollider _collider;
        public AudioSource _audioSource;
        public SmoothRotation _rotationX;
        public SmoothRotation _rotationY;
        public SmoothVelocity _velocityX;
        public SmoothVelocity _velocityZ;
        public bool _isGrounded;
        public bool notMyIsWalking = false;

        [Header("Directions")]
        public bool Forward = false;
        public bool Backwards = false;
        public bool Left = false;
        public bool Right = false;
        [Space(10)]
        public bool Forward_Left = false;
        public bool Forward_Right = false;
        public bool Backwards_Left = false;
        public bool Backwards_Right = false;

        public int directionIndicator = 0;
        float calulated_xValue = 0f;
        float calculated_zValue = 0f;

        public readonly RaycastHit[] _groundCastResults = new RaycastHit[8];
        public readonly RaycastHit[] _wallCastResults = new RaycastHit[8];

        public int playerRewiredID;

        /// Initializes the FpsController on start.
        public void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _collider = GetComponent<CapsuleCollider>();
            _audioSource = GetComponent<AudioSource>();
			arms = AssignCharactersCamera();
            _audioSource.clip = walkingSound;
            _audioSource.loop = true;
            _rotationX = new SmoothRotation(RotationXRaw);
            _rotationY = new SmoothRotation(RotationYRaw);
            _velocityX = new SmoothVelocity();
            _velocityZ = new SmoothVelocity();
            Cursor.lockState = CursorLockMode.Locked;
            ValidateRotationRestriction();
            
        }
        
        public void SetPlayerIDInInput()
        {            
            input.playerID = playerRewiredID;
            input.player = ReInput.players.GetPlayer(input.playerID);
        }

        public Transform AssignCharactersCamera()
        {
            var t = transform;
			arms.SetPositionAndRotation(t.position, t.rotation);
			return arms;
        }
        
        /// Clamps <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> to valid values and
        /// ensures that <see cref="minVerticalAngle"/> is less than <see cref="maxVerticalAngle"/>.
        public void ValidateRotationRestriction()
        {
            minVerticalAngle = ClampRotationRestriction(minVerticalAngle, -80, 80);
            maxVerticalAngle = ClampRotationRestriction(maxVerticalAngle, -80, 80);
            if (maxVerticalAngle >= minVerticalAngle) return;
            Debug.LogWarning("maxVerticalAngle should be greater than minVerticalAngle.");
            var min = minVerticalAngle;
            minVerticalAngle = maxVerticalAngle;
            maxVerticalAngle = min;
        }

        public static float ClampRotationRestriction(float rotationRestriction, float min, float max)
        {
            if (rotationRestriction >= min && rotationRestriction <= max) return rotationRestriction;
            var message = string.Format("Rotation restrictions should be between {0} and {1} degrees.", min, max);
            Debug.LogWarning(message);
            return Mathf.Clamp(rotationRestriction, min, max);
        }
			
        /// Checks if the character is on the ground.
        public void OnCollisionStay()
        {
            var bounds = _collider.bounds;
            var extents = bounds.extents;
            var radius = extents.x - 0.01f;
            Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
                _groundCastResults, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
            if (!_groundCastResults.Any(hit => hit.collider != null && hit.collider != _collider)) return;
            for (var i = 0; i < _groundCastResults.Length; i++)
            {
                _groundCastResults[i] = new RaycastHit();
            }

            _isGrounded = true;
        }
			
        /// Processes the character movement and the camera rotation every fixed framerate frame.
        public void FixedUpdate()
        {
            // FixedUpdate is used instead of Update because this code is dealing with physics and smoothing.
            RotateCameraAndCharacter();
            MoveCharacter();
            _isGrounded = false;
        }
			
        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        public void Update()
        {
			arms.position = transform.position + transform.TransformVector(armPosition);
            Jump();
            PlayFootstepSounds();
            
        }

        public void RotateCameraAndCharacter()
        {/*
            var rotationX = _rotationX.Update(RotationXRaw, rotationSmoothness);
            var rotationY = _rotationY.Update(RotationYRaw, rotationSmoothness);
            var clampedY = RestrictVerticalRotation(rotationY);
            _rotationY.Current = clampedY;
			var worldUp = arms.InverseTransformDirection(Vector3.up);
			var rotation = arms.rotation * Quaternion.AngleAxis(rotationX, worldUp) * Quaternion.AngleAxis(clampedY, Vector3.left);
            transform.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
			arms.rotation = rotation;
            */
        }
			
        /// Returns the target rotation of the camera around the y axis with no smoothing.
        public float RotationXRaw
        {
            get { return input.RotateX * mouseSensitivity; }
        }
			
        /// Returns the target rotation of the camera around the x axis with no smoothing.
        public float RotationYRaw
        {
            get { return input.RotateY * mouseSensitivity; }
        }
			
        /// Clamps the rotation of the camera around the x axis
        /// between the <see cref="minVerticalAngle"/> and <see cref="maxVerticalAngle"/> values.
        public float RestrictVerticalRotation(float mouseY)
        {
			var currentAngle = NormalizeAngle(arms.eulerAngles.x);
            var minY = minVerticalAngle + currentAngle;
            var maxY = maxVerticalAngle + currentAngle;
            return Mathf.Clamp(mouseY, minY + 0.01f, maxY - 0.01f);
        }
			
        /// Normalize an angle between -180 and 180 degrees.
        /// <param name="angleDegrees">angle to normalize</param>
        /// <returns>normalized angle</returns>
        public static float NormalizeAngle(float angleDegrees)
        {
            while (angleDegrees > 180f)
            {
                angleDegrees -= 360f;
            }

            while (angleDegrees <= -180f)
            {
                angleDegrees += 360f;
            }

            return angleDegrees;
        }

        public void MoveCharacter()
        {
            var direction = new Vector3(input.Move, 0f, input.Strafe).normalized;

            CheckDirection(direction.x, direction.z);

            var worldDirection = transform.TransformDirection(direction);
            var velocity = worldDirection * (/*input.Run ? runningSpeed :*/ walkingSpeed);
            Debug.Log(velocity);
            //Checks for collisions so that the character does not stuck when jumping against walls.
            var intersectsWall = CheckCollisionsWithWalls(velocity);
            if (intersectsWall)
            {
                //_velocityX.Current = _velocityZ.Current = 0f;
                return;
            }

            var smoothX = _velocityX.Update(velocity.x, movementSmoothness);
            var smoothZ = _velocityZ.Update(velocity.z, movementSmoothness);
            var rigidbodyVelocity = _rigidbody.velocity;
            var force = new Vector3(smoothX - rigidbodyVelocity.x, 0f, smoothZ - rigidbodyVelocity.z);
            _rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        int CheckDirection(float xValue, float zValue)
        {
            if(xValue == -1 && zValue == 0)
            {
                directionIndicator = 1;
            }
            else if(xValue == 0 && zValue == 1)
            {
                directionIndicator = 3;
            }
            else if (xValue == 1 && zValue == 0)
            {
                directionIndicator = 5;
            }
            else if (xValue == 0 && zValue == -1)
            {
                directionIndicator = 7;
            }

            else if(zValue > 0)
            {
                if (xValue < 0) //Second Quarter of Cartesian Map
                {
                    if (zValue <= -0.5 * xValue)
                    {
                        directionIndicator = 1;
                    }
                    else if (zValue > -0.5 * xValue && zValue < -2 * xValue)
                    {
                        directionIndicator = 2;
                    }
                    else if (zValue >= -2 * xValue)
                    {
                        directionIndicator = 3;
                    }
                }
                else if (xValue > 0) //First Quarter of Cartesian Map
                {

                    if (zValue >= 2 * xValue)
                    {
                        directionIndicator = 3;
                    }
                    else if (zValue > 0.5 * xValue && zValue < 2 * xValue)
                    {
                        directionIndicator = 4;
                    }
                    else if (zValue <= 0.5 * xValue)
                    {
                        directionIndicator = 5;
                    }
                }

            }
            if(zValue < 0)
            {
                if (xValue < 0) //Third Quarter of Cartesian Map
                {
                    if (zValue >= 0.5 * xValue)
                    {
                        directionIndicator = 1;
                    }
                    else if (zValue < 0.5 * xValue && zValue > 2 * xValue)
                    {
                        directionIndicator = 8;
                    }
                    else if (zValue <= 2 * xValue)
                    {
                        directionIndicator = 7;
                    }
                }
                else if (xValue > 0) //Fourth Quarter of Cartesian Map
                {

                    if (zValue <= -2 * xValue)
                    {
                        directionIndicator = 7;
                    }
                    else if (zValue < -0.5 * xValue && zValue > -2 * xValue)
                    {
                        directionIndicator = 6;
                    }
                    else if (zValue >= -0.5 * xValue)
                    {
                        directionIndicator = 5;
                    }
                }
            }
            else if(zValue == 0 && xValue == 0)
            {
                directionIndicator = 0;
            }
            

            return directionIndicator;
        }

        public bool CheckCollisionsWithWalls(Vector3 velocity)
        {
            if (_isGrounded) return false;
            var bounds = _collider.bounds;
            var radius = _collider.radius;
            var halfHeight = _collider.height * 0.5f - radius * 1.0f;
            var point1 = bounds.center;
            point1.y += halfHeight;
            var point2 = bounds.center;
            point2.y -= halfHeight;
            Physics.CapsuleCastNonAlloc(point1, point2, radius, velocity.normalized, _wallCastResults,
                radius * 0.04f, ~0, QueryTriggerInteraction.Ignore);
            var collides = _wallCastResults.Any(hit => hit.collider != null && hit.collider != _collider);
            if (!collides) return false;
            for (var i = 0; i < _wallCastResults.Length; i++)
            {
                _wallCastResults[i] = new RaycastHit();
            }

            return true;
        }

        public void Jump()
        {
            if (!_isGrounded || !input.Jump) return;
            _isGrounded = false;
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        public void PlayFootstepSounds()
        {
            if (_isGrounded && _rigidbody.velocity.sqrMagnitude > 0.1f)
            {
                _audioSource.clip = input.Run ? runningSound : walkingSound;
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                    notMyIsWalking = true;
                }
            }
            else
            {
                if (_audioSource.isPlaying)
                {
                    _audioSource.Pause();
                    notMyIsWalking = false;
                }
            }
        }
			
        /// A helper for assistance with smoothing the camera rotation.
        public class SmoothRotation
        {
            public float _current;
            public float _currentVelocity;

            public SmoothRotation(float startAngle)
            {
                _current = startAngle;
            }
				
            /// Returns the smoothed rotation.
            public float Update(float target, float smoothTime)
            {
                return _current = Mathf.SmoothDampAngle(_current, target, ref _currentVelocity, smoothTime);
            }

            public float Current
            {
                set { _current = value; }
            }
        }
			
        /// A helper for assistance with smoothing the movement.
        public class SmoothVelocity
        {
            public float _current;
            public float _currentVelocity;

            /// Returns the smoothed velocity.
            public float Update(float target, float smoothTime)
            {
                return _current = Mathf.SmoothDamp(_current, target, ref _currentVelocity, smoothTime);
            }

            public float Current
            {
                set { _current = value; }
            }
        }

        
			
        /// Input mappings
        [Serializable]
        public class FpsInput
        {
            [Tooltip("The name of the virtual axis mapped to rotate the camera around the y axis."),
             SerializeField]
            public string rotateX = "Mouse X";

            [Tooltip("The name of the virtual axis mapped to rotate the camera around the x axis."),
             SerializeField]
            public string rotateY = "Mouse Y";

            [Tooltip("The name of the virtual axis mapped to move the character back and forth."),
             SerializeField]
            public string move = "Move Horizontal";

            [Tooltip("The name of the virtual axis mapped to move the character left and right."),
             SerializeField]
            public string strafe = "Move Vertical";

            [Tooltip("The name of the virtual button mapped to run."),
             SerializeField]
            public string run = "Fire3";

            [Tooltip("The name of the virtual button mapped to jump."),
             SerializeField]
            public string jump = "Jump";

            public int playerID ;
            public Rewired.Player player;

            

            /// Returns the value of the virtual axis mapped to rotate the camera around the y axis.
            public float RotateX
            {
                get { return player.GetAxis(rotateX); }
            }
				         
            /// Returns the value of the virtual axis mapped to rotate the camera around the x axis.        
            public float RotateY
            {
                get { return player.GetAxis(rotateY); }
            }
				        
            /// Returns the value of the virtual axis mapped to move the character back and forth.        
            public float Move
            {
                get { return player.GetAxis(move); }
            }

            
				       
            /// Returns the value of the virtual axis mapped to move the character left and right.         
            public float Strafe
            {
                get { return player.GetAxis(strafe); }
            }
				    
            /// Returns true while the virtual button mapped to run is held down.          
            public bool Run
            {
                get { return Input.GetButton(run); }
            }
				     
            /// Returns true during the frame the user pressed down the virtual button mapped to jump.          
            public bool Jump
            {
                get { return player.GetButtonDown(jump); }
            }


            
        }
    }

    
}