using KinematicCharacterController;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyCharacterController : MonoBehaviour, ICharacterController
{   
    private KinematicCharacterMotor _motor;
    private CinemachineCamera _Camera;
    [SerializeField] private Transform _CameraTransform;

    [Header("Ground Movement")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _maxSpeed = 15f;
    [SerializeField] private bool _isRun;

    [Header("Ground Movement")]

    [SerializeField] private float _groundControlRate = 1f;

    [Header("Air Movement")]

    [SerializeField] private float _airControlRate = 1f;
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

    [Header("Debug")]
    private PlayerInput _playerInputs;
    public Vector2 m_movebutton;
    public Vector2 m_lookRot;
    public bool m_jumpbutton;

    private void Awake()
    {
        AddKinematicCC();
        AddInput();
    }

    private void Start()
    {
        
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
        // Debug.Log(_playerInputs.JumpButton);
        InputStructDebug();
        PressJump();
    }

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

        _jump.started += Jump;
        _jump.canceled += Jump;
    }

    private void DisableInputSystem()
    {
        _input.Player.Disable();

        _move.performed -= OnMove;
        _move.canceled -= OnMove;

        _look.performed -= OnLookAround;
        _look.canceled -= OnLookAround;

        _jump.started -= Jump;
        _jump.canceled -= Jump;
    }


    private void AddInput()
    {
        _input = new InputSystem_Player();
        _move = _input.Player.Move;
        _look = _input.Player.Look;
        _jump = _input.Player.Jump;
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
       return false;
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
        //Remove this after ground check
        GroundVelocity(ref currentVelocity, deltaTime);

        if (_motor.GroundingStatus.IsStableOnGround)
        {
            GroundVelocity(ref currentVelocity, deltaTime);
        }
        else
        {
            AirVelocity(ref currentVelocity, deltaTime);
        }
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

    //How player move on ground
    private void GroundVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        float speed = _isRun ? _runSpeed : _walkSpeed;

        Vector3 playerDirection = GetCameraPlanarRotation(_playerInputs.MoveButton);
        
        //Retarget the player input to the right movement axis
        Vector3 targetVel = new Vector3(playerDirection.x * speed, 0, playerDirection.x * speed);

        Debug.Log(targetVel);
        currentVelocity = Vector3.Lerp(currentVelocity, targetVel, deltaTime * speed);
    }

    //How player move on Air
    private void AirVelocity(ref Vector3 currentVelocity, float deltaTime)
    { 
     

        //apply gravity
       // currentVelocity = _gravity * deltaTime;
    }


    //new Input System
    private void PlayerInputs()
    { 
      
    }


    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        
        _playerInputs.MoveButton = input;
    }

    private void OnLookAround(InputAction.CallbackContext ctx)
    {

    }

    //why you are not working on pressed???????????????????? you work just like OnMove()
    private void Jump(InputAction.CallbackContext ctx)
    {
        bool jumpPressed = ctx.ReadValueAsButton();
        Debug.Log(jumpPressed);

        _playerInputs.JumpButton = jumpPressed;


    }

    private void PressJump()
    {
        if (_jump.IsPressed())
        {
            _playerInputs.JumpButton = true;
        }
        else 
        {
            _playerInputs.JumpButton = false;
        }

    }
}
