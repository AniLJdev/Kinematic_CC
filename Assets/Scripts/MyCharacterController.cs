using KinematicCharacterController;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyCharacterController : MonoBehaviour, ICharacterController
{   
    private KinematicCharacterMotor _motor;
    private CinemachineCamera _Camera;

    [Header("Ground Movement")]
    [SerializeField] private float _groundAcceleration = 10f;
    [SerializeField] private float _groundSpeedMax = 15f;
    [SerializeField] private float _groundControlRate = 1f;

    [Header("Air Movement")]
    [SerializeField] private float _airAcceleration = 5f;
    [SerializeField] private float _airSpeedMax = 15f;
    [SerializeField] private float _airControlRate = 1f;
    [SerializeField] private float _gravity = 9.7f;

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

    private void EnableInputSystem()
    {
        _input.Player.Enable();

        _move.performed += OnMove;
        _move.canceled += OnMove;

        _look.performed += OnLookAround;
        _look.canceled += OnLookAround;

        _jump.started += OnJump;
    }



    private void OnDisable()
    {
        DisableInputSystem();
    }
    private void DisableInputSystem()
    {
        _input.Player.Enable();

        _move.performed -= OnMove;
        _move.canceled -= OnMove;

        _look.performed -= OnLookAround;
        _look.canceled -= OnLookAround;

        _jump.started -= OnJump;
    }

    private void Update()
    {
        
    }


    private void AddKinematicCC()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _motor.CharacterController = this;
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
       
    }

    //Input System

    private void OnMove(InputAction.CallbackContext ctx)
    {
        
    }

    private void OnLookAround(InputAction.CallbackContext ctx)
    {

    }

    private void OnJump(InputAction.CallbackContext ctx)
    {

    }
}
