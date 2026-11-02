using KinematicCharacterController;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private bool _canJump;
    private float _coyoteTime = 0.1f;

    //Input System
    private InputSystem_Player _input;

    private InputAction _move;
    private InputAction _look;
    private InputAction _jump;
    private InputAction _sprint;

    [Header("Debug")]
    private PlayerInput _playerInputs;
    public Vector2 m_movebutton;
    public Vector2 m_lookRot;
    public bool m_jumpbutton;

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


    private void Update()
    {
        InputStructDebug();
      
    }

    //Visualize the struct on inspector, can remove when script is completed
    private void InputStructDebug()
    {
        m_movebutton = _playerInputs.MoveButton;
        m_jumpbutton = _playerInputs.JumpButton;
        m_lookRot = _playerInputs.CameraRotation;
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
        _jump.canceled += OnJump;

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
        _jump.canceled -= OnJump;

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
       // if(_playerInputs.DashButton == true)

        float speed = _isRun ? _runSpeed : _walkSpeed;

        //Retarget the player input to the right movement axis
        Vector3 targetVel = new Vector3(_playerInputs.MoveButton.y * speed,0 , _playerInputs.MoveButton.x * speed * -1);


        //ground speed limit
        Vector3 maxXZspeed = Vector3.ClampMagnitude(currentVelocity, _maxSpeed);
         currentVelocity = new Vector3(maxXZspeed.x, currentVelocity.y, maxXZspeed.z);

        currentVelocity = Vector3.Lerp(currentVelocity, targetVel, deltaTime * _groundAcceleration);
    }

    //Add part where player move with platform


    //How player move on Air
    private void AirVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //only overwrite speed when player is controlling 
        if (_playerInputs.MoveButton.sqrMagnitude > 0)
        {
            //Set speed
            float speed = _airAddSpeed;

            //Retarget the player input to the right movement axis and add the velocity on existing velocity
            Vector3 targetVel = new Vector3(_playerInputs.MoveButton.y * speed, 0, _playerInputs.MoveButton.x * speed * -1);

            //Clamp air speed limit is add as a quick fix of continuous jump will boost jump distance by a lot
            Vector3 addVelocity = Vector3.Lerp(Vector3.zero, Vector3.ClampMagnitude(targetVel, _airMaxSpeed) , deltaTime * _airAcceleration);

            //not overwrite gravity
            currentVelocity = currentVelocity + new Vector3(addVelocity.x, 0, addVelocity.z);
        }

        //apply gravity
        currentVelocity += _gravity * deltaTime;
    }

    private void Jump(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_playerInputs.JumpButton && _motor.GroundingStatus.IsStableOnGround)
        { 
          _motor.ForceUnground();

            Vector3 jumpForce = _motor.CharacterUp * _jumpForce;

            currentVelocity += jumpForce;
        }
    }

    //new Input System

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        
        _playerInputs.MoveButton = input;
    }

    private void OnLookAround(InputAction.CallbackContext ctx)
    {

    }

    //why you are not working on pressed???????????????????? you work just like OnMove()
    private void OnJump(InputAction.CallbackContext ctx)
    {
        bool jumpPressed = ctx.ReadValueAsButton();
        Debug.Log(jumpPressed);

        _playerInputs.JumpButton = jumpPressed;


    }

    private void OnSprint(InputAction.CallbackContext ctx)
    {
        bool running = ctx.ReadValueAsButton();
        Debug.Log(running);

        _playerInputs.DashButton = running;
        _isRun = running;


    }

    
}
