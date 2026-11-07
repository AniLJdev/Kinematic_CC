using KinematicCharacterController;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;

public class MyCharacterController : MonoBehaviour, ICharacterController
{
    private KinematicCharacterMotor _motor;
    private CinemachineCamera _Camera;
    [SerializeField] private Transform _CameraTransform;
    [SerializeField] private Transform _PlayerModelTransform;

    [Header("Ground Movement")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _groundAcceleration = 5f;
    [SerializeField] private bool _isRun;
    [SerializeField] private float _maxSpeed = 22f;

    [Header("Air Movement")]
    [SerializeField] private float _airAddSpeed = 15f;
    [SerializeField] private float _airMaxSpeed = 4f;
    [SerializeField] private float _airAcceleration = 2f;
    [SerializeField] private Vector3 _gravity = new Vector3(0, -9.7f, 0);

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 10f;
    //[SerializeField] private int _jumpCount = 2;
    private bool _jumpIsPressed;
    private bool _canJump;
    private bool _jumpCoyoteEnded = false;
    private int _airAction;

    [SerializeField] private bool _canAirJump;
    [SerializeField] private int _maxAirAction = 1;
    [SerializeField] private float _coyoteTime = 0.2f;
    [SerializeField] private float _offGroundTime = 0.2f;

    [SerializeField] private float _cooldownTime = 0.25f;

    [SerializeField] private float _airJumpTimer = 0;

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

        AddInput();
    }

    private void Start()
    {
        AddKinematicCC();
    }

    private void OnEnable()
    {
        EnableInputSystem();
    }

    private void OnDisable()
    {
        DisableInputSystem();
    }



    private void AddKinematicCC()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _motor.CharacterController = this;
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

    private Vector3 GetCameraPlanarRotation(Vector2 _playerInputs)
    {
        Vector3 cameraForward = Vector3.ProjectOnPlane(_CameraTransform.forward, Vector3.up);
        Vector3 cameraRight = Vector3.ProjectOnPlane(_CameraTransform.right, Vector3.up);

        if (cameraForward.sqrMagnitude > 0.001f)
            cameraForward.Normalize();
        if (cameraRight.sqrMagnitude > 0.001f)
            cameraForward.Normalize();

        Vector3 direction = cameraForward * _playerInputs.y + cameraRight * _playerInputs.x;
        return Vector3.ClampMagnitude(direction, 1f);
    }

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
        else if(_airJumpTimer >= 0.25f && _playerInputs.JumpButton && !_motor.GroundingStatus.IsStableOnGround && _airAction >= 1)
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

        Vector3 playerDirection = GetCameraPlanarRotation(_moveConvert);

        //Retarget the player input to the right movement axis
        Vector3 targetVel = _moveConvert * speed;

        _PlayerModelTransform.forward = _moveConvert.normalized;

        //ground speed limit
        Vector3 maxXZspeed = Vector3.ClampMagnitude(currentVelocity, _maxSpeed);
        currentVelocity = new Vector3(maxXZspeed.x, currentVelocity.y, maxXZspeed.z);

        currentVelocity = Vector3.Lerp(currentVelocity, targetVel, deltaTime * _groundAcceleration);

        //Ground coyote
        _canJump = true;
        
        _jumpCoyoteEnded = false;
        _canAirJump = false;

        if(_airJumpTimer > 0)
        {
            _airJumpTimer = 0;
        }

        if(_canJump && !_canAirJump)
        {
            _airAction = _maxAirAction;
        }
    }

    //Add part where player move with platform


    //How player move on Air
    private void AirVelocity(ref Vector3 currentVelocity, float deltaTime)
    {

        LeaveGroundCoyote();

        CanAirJump();
        
        if(_airJumpTimer < .25f)
        {
            _airJumpTimer += Time.deltaTime;
        }
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
        
            _motor.ForceUnground();

            Vector3 jumpForce = _motor.CharacterUp * _jumpForce;

        currentVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

            currentVelocity += jumpForce;
            _canJump = false;

       
    }
    private void Jump2(ref Vector3 currentVelocity, float deltaTime)
    {

        _motor.ForceUnground();

        Vector3 jumpForce = _motor.CharacterUp * _jumpForce;

        Vector3 newJumpDirection = new Vector3(_moveConvert.x * Mathf.Abs(currentVelocity.x), 0, _moveConvert.z * Mathf.Abs(currentVelocity.z));
        currentVelocity = new Vector3(newJumpDirection.x, 0, newJumpDirection.z);

        currentVelocity += jumpForce;
        _canJump = false;


    }

    private async void CanAirJump()
    {
        await Awaitable.WaitForSecondsAsync(_cooldownTime);

        _canAirJump = true;
    }


    private async void CoyoteTime()
    {
        Debug.Log("CoyoteTime: please don't spawn this message x time per press");

        _jumpIsPressed = true;
        Debug.Log("jump pressed" + _jumpIsPressed);
        await Awaitable.WaitForSecondsAsync(_coyoteTime);

        _jumpIsPressed = false;
        Debug.Log("jump pressed" + _jumpIsPressed);
        _jumpCoyoteEnded = true;
    }

    private async void LeaveGroundCoyote()
    {

        await Awaitable.WaitForSecondsAsync(_offGroundTime);

        _canJump = false;
        _jumpCoyoteEnded = true;
    }



    Vector3 ConvertToCameraSpace(Vector3 input)
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
    //handle Move input in Vector2
    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        _playerInputs.MoveButton = input;

        Vector3 cameraRelativeMovement = ConvertToCameraSpace(_playerInputs.MoveButton);

        //Convert input into Vector3
        _moveConvert = cameraRelativeMovement;
    }



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
        //Debug.Log(running);

        _playerInputs.DashButton = running;
        _isRun = running;


    }


}
