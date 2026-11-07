using KinematicCharacterController;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;
using UnityEngine.Experimental.GlobalIllumination;

public class MyCharacterController : MonoBehaviour, ICharacterController
{
    //ref
    private KinematicCharacterMotor _motor;
    [SerializeField] private Transform _CameraTransform;
    [SerializeField] private Transform _PlayerModelTransform;

    [Header("Ground Movement")]
    //Speeds
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _runSpeed = 8f;
    //Decide how fast it can goes up and also how slippery.
    [SerializeField] private float _groundAcceleration = 8f;
    [SerializeField] private bool _isRun;
    //Clamp max speed on ground
    [SerializeField] private float _maxSpeed = 22f;

    [Header("Air Movement")]
    //Air speed also add controlability, but also make it goes too fast.
    [SerializeField] private float _airAddSpeed = 8f;
    // Max air speed
    [SerializeField] private float _airMaxSpeed = 4f;
    [SerializeField] private float _airAcceleration = 2f;

    [SerializeField] private float _jumpSpeedConservation = 0.75f;
    //Direct down force
    [SerializeField] private Vector3 _gravity = new Vector3(0, -9.7f, 0);

    [Header("Jump")]
    //Jump height
    [SerializeField] private float _jumpForce = 10f;
    private bool _jumpIsPressed;
    private bool _canJump;
    //Air Jump
    private int _airAction;
    [SerializeField] private bool _canAirJump;
    [SerializeField] private int _maxAirAction = 1;
    //Coyote
    [SerializeField] private float _coyoteTime = 0.2f;
    [SerializeField] private float _offGroundTime = 0.1f;
    [SerializeField] private float _cooldownTime = 0.25f;


    //Input System
    private InputSystem_Player _input;

    private InputAction _move;
    private InputAction _look;
    private InputAction _jump;
    private InputAction _sprint;


    [Header("Parameters")]
    private PlayerInput _playerInputs;
    private Vector3 _moveConvert;


    private void Awake()
    {
        Initializing();
        AddInput();
    }

    private void OnEnable()
    {
        EnableInputSystem();
    }

    private void OnDisable()
    {
        DisableInputSystem();
    }



    private void Initializing()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _motor.CharacterController = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void EnableInputSystem()
    {
        _input.Player.Enable();

        _move.performed += OnMove;
        _move.canceled += OnMove;

        _look.performed += OnLookAround;
        _look.canceled += OnLookAround;

        _jump.performed += OnJump;

        _sprint.performed += OnSprint;
        _sprint.canceled += OnSprint;
    }

    private void DisableInputSystem()
    {
        _input.Player.Disable();

        _move.performed -= OnMove;
        _move.canceled -= OnMove;

        _look.performed -= OnLookAround;
        _look.canceled -= OnLookAround;

        _jump.performed -= OnJump;

        _sprint.performed -= OnSprint;
        _sprint.canceled -= OnSprint;
    }


    private void AddInput()
    {
        _input = new InputSystem_Player();
        _move = _input.Player.Move;
        _look = _input.Player.Look;
        _jump = _input.Player.Jump;
        _sprint = _input.Player.Sprint;
    }


    //Main Functions

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //Change acceleration module base on ground check
        if (_motor.GroundingStatus.IsStableOnGround)
        {
            GroundVelocity(ref currentVelocity, deltaTime);
        }
        else
        {
            AirVelocity(ref currentVelocity, deltaTime);
        }



        //Jump module
        if (_jumpIsPressed && _canJump)
        {
            Jump(ref currentVelocity, deltaTime);
        }
        //Double Jump
        else if(_playerInputs.JumpButton && !_motor.GroundingStatus.IsStableOnGround && _airAction >= 1)
        {
            Jump2(ref currentVelocity, deltaTime);
            _airAction -= 1;
        }
    }

    //How player move on ground
    private void GroundVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //Set speed
        float speed = _isRun ? _runSpeed : _walkSpeed;

        //Retarget the player input to the right movement axis
        Vector3 targetVel = _moveConvert * speed;

        //Align Player model with the moving direction
        _PlayerModelTransform.forward += _moveConvert.normalized;

        //ground speed limit
        Vector3 maxXZspeed = Vector3.ClampMagnitude(currentVelocity, _maxSpeed);
        currentVelocity = new Vector3(maxXZspeed.x, currentVelocity.y, maxXZspeed.z);

        //Move Player
        currentVelocity = Vector3.Lerp(currentVelocity, targetVel, deltaTime * _groundAcceleration);

        //Ground coyote
        _canJump = true;
        
        _canAirJump = false;

        
        if(_canJump && !_canAirJump)
        {
            _airAction = _maxAirAction;
        }
    }

    //Add part where player move with platform


    //How player move on Air
    private void AirVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //Ground detection stay shortly after leaving the ground
        LeaveGroundCoyote();
        //Align Player model with the moving direction
        _PlayerModelTransform.forward += _moveConvert.normalized;
        //only overwrite speed when player is controlling 
        if (_playerInputs.MoveButton.sqrMagnitude > 0)
        {
            //Set speed
            float speed = _airAddSpeed;

            //Retarget the player input to the right movement axis and add the velocity on existing velocity
            Vector3 targetVel = _moveConvert * speed;

            //Clamp air speed limit is add as a quick fix of continuous jump will boost jump distance by a lot
            Vector3 addVelocity = Vector3.Lerp(Vector3.zero, Vector3.ClampMagnitude(targetVel, _airMaxSpeed), deltaTime * _airAcceleration);

            //not overwrite gravity
            currentVelocity = currentVelocity + new Vector3(addVelocity.x, 0, addVelocity.z);
        }

        //apply gravity
        currentVelocity += _gravity * deltaTime;



    }

    //Raw jump function that pull player up with jump force
    private void Jump(ref Vector3 currentVelocity, float deltaTime)
    {
        //Apply jump force and leave ground.
            _motor.ForceUnground();

        Vector3 jumpForce = _motor.CharacterUp * _jumpForce;

        //Reset velocity to make double jump more controllable base on player input
        Vector2 velocity = new Vector2(Mathf.Abs(currentVelocity.x), Mathf.Abs(currentVelocity.z));

        float momentum = velocity.magnitude;

        currentVelocity = new Vector3(((_moveConvert.x * momentum) + currentVelocity.x) * _jumpSpeedConservation / 2, jumpForce.y, ((_moveConvert.z * momentum) + currentVelocity.z) * _jumpSpeedConservation / 2);
        _canJump = false;
        _playerInputs.JumpButton = false;


    }

    //Air jump
    private void Jump2(ref Vector3 currentVelocity, float deltaTime)
    {
        //Jump height
        Vector3 jumpForce = _motor.CharacterUp * _jumpForce;

        //Reset velocity to make double jump more controllable base on player input
        Vector2 velocity = new Vector2(Mathf.Abs(currentVelocity.x), Mathf.Abs(currentVelocity.z));

        float momentum = velocity.magnitude;

        currentVelocity = new Vector3(((_moveConvert.x * momentum) + currentVelocity.x) * _jumpSpeedConservation / 2, jumpForce.y, ((_moveConvert.z * momentum) + currentVelocity.z) * _jumpSpeedConservation / 2);
        _canJump = false;


    }

    

    //When Jump is pressed, it will stay active in the _coyote time
    private async void CoyoteTime()
    {
        _jumpIsPressed = true;
        
        await Awaitable.WaitForSecondsAsync(_coyoteTime);

        _jumpIsPressed = false;
        _playerInputs.JumpButton = false;
    }

    //When player leave the ground, the detection will stay until off Gound time is time up
    private async void LeaveGroundCoyote()
    {
        await Awaitable.WaitForSecondsAsync(_offGroundTime);

        _canJump = false;
    }


    // Camera Planar, make player input is relative to Camera
  private Vector3 ConvertToCameraSpace(Vector3 input)
    {
        Vector3 cameraForward = _CameraTransform.forward;
        Vector3 cameraRight = _CameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraForwardZProduct = input.y * cameraForward;
        Vector3 cameraRightXProduct = input.x * cameraRight;

        Vector3 vectorRotatedToCameraSpace = cameraForwardZProduct + cameraRightXProduct;
        return vectorRotatedToCameraSpace;
    }


    //new Input System
    //handle Move input in Vector2 and convert it in Vector3
    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        _playerInputs.MoveButton = input;

        //Relative to Camera when pressing Direction
        Vector3 cameraRelativeMovement = ConvertToCameraSpace(_playerInputs.MoveButton);

        //Convert input into Vector3
        _moveConvert = cameraRelativeMovement;
    }


    //When look around, update the player direction to camera
    private void OnLookAround(InputAction.CallbackContext ctx)
    {
        Vector3 cameraRelativeMovement = ConvertToCameraSpace(_playerInputs.MoveButton);
        _moveConvert = cameraRelativeMovement;
    }

    //Handle Jump input only
    private void OnJump(InputAction.CallbackContext ctx)
    {
        bool jumpPressed = ctx.ReadValueAsButton();
        // Debug.Log(jumpPressed);

        _playerInputs.JumpButton = jumpPressed;

        CoyoteTime();


    }

    //handle sprint input and bool
    private void OnSprint(InputAction.CallbackContext ctx)
    {
        bool running = ctx.ReadValueAsButton(); 

        _playerInputs.DashButton = running;
        _isRun = running;


    }


    // Unused Functions
    public void AfterCharacterUpdate(float deltaTime)
    {

    }

    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {

    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {

    }
}
