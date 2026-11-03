using KinematicCharacterController;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;

public class MyCharacterController : MonoBehaviour, ICharacterController
{   
    private KinematicCharacterMotor _motor;
    private CinemachineCamera _Camera;

    [Header("Ground Movement")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _groundAcceleration = 5f;
    [SerializeField] private bool  _isRun;
    [SerializeField] private float _maxSpeed = 22f;

    [Header("Air Movement")]
    [SerializeField] private float _airAddSpeed = 15f;
    [SerializeField] private float _airMaxSpeed = 4f;
    [SerializeField] private float _airAcceleration = 2f;
    [SerializeField] private Vector3 _gravity = new Vector3(0,-9.7f,0);

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 10f;
    //[SerializeField] private int _jumpCount = 2;
    private bool _jumpIsPressed;
    private bool _canJump;
    [SerializeField] private float _coyoteTime = 0.2f;
    [SerializeField] private float _offGroundTime = 0.2f;

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
        Jump(ref currentVelocity, deltaTime);
    }

    //How player move on ground
    private void GroundVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //Set speed
        float speed = _isRun ? _runSpeed : _walkSpeed;

        //Retarget the player input to the right movement axis
        Vector3 targetVel = _moveConvert * speed;

        //ground speed limit
        Vector3 maxXZspeed = Vector3.ClampMagnitude(currentVelocity, _maxSpeed);
         currentVelocity = new Vector3(maxXZspeed.x, currentVelocity.y, maxXZspeed.z);

        currentVelocity = Vector3.Lerp(currentVelocity, targetVel, deltaTime * _groundAcceleration);

        //Ground coyote
        _canJump = true;
    }

    //Add part where player move with platform


    //How player move on Air
    private void AirVelocity(ref Vector3 currentVelocity, float deltaTime)
    {

        LeaveGroundCoyote();

        //only overwrite speed when player is controlling 
        if (_playerInputs.MoveButton.sqrMagnitude > 0)
        {
            //Set speed
            float speed = _airAddSpeed;

            //Retarget the player input to the right movement axis and add the velocity on existing velocity
            Vector3 targetVel = _moveConvert * speed;

            //Clamp air speed limit is add as a quick fix of continuous jump will boost jump distance by a lot
            Vector3 addVelocity = Vector3.Lerp(Vector3.zero, Vector3.ClampMagnitude(targetVel, _airMaxSpeed) , deltaTime * _airAcceleration);

            //not overwrite gravity
            currentVelocity = currentVelocity + new Vector3(addVelocity.x, 0, addVelocity.z);
        }

        //apply gravity
        currentVelocity += _gravity * deltaTime;



    }

    //Raw jump function that pull player up with jump force
    private void Jump(ref Vector3 currentVelocity, float deltaTime)
    {
        if (/* _playerInputs.JumpButton && */ _jumpIsPressed && _canJump)
        { 
          _motor.ForceUnground();

            Vector3 jumpForce = _motor.CharacterUp * _jumpForce;

            currentVelocity += jumpForce;
            _canJump = false;
        }
    }



    private async void CoyoteTime()
    {
        Debug.Log("CoyoteTime: please don't spawn this message x time per press");

        _jumpIsPressed = true;
        Debug.Log("jump pressed" + _jumpIsPressed);
        await Awaitable.WaitForSecondsAsync(_coyoteTime);

        _jumpIsPressed = false;
        Debug.Log("jump pressed" + _jumpIsPressed);
    }

    private async void LeaveGroundCoyote()
    {

        await Awaitable.WaitForSecondsAsync(_offGroundTime);

        _canJump = false;
    }



    //new Input System
    //handle Move input in Vector2
    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
   
        _playerInputs.MoveButton = input;

        //Convert input into Vector3
        _moveConvert = new Vector3(_playerInputs.MoveButton.x, 0, _playerInputs.MoveButton.y);
    }



    private void OnLookAround(InputAction.CallbackContext ctx)
    {

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
