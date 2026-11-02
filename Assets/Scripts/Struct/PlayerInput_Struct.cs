using Unity.VisualScripting;
using UnityEngine;

public struct PlayerInput
{
    [SerializeField] private Vector2 _moveButton;
    [SerializeField] private Vector2 _cameraRotation;
    [SerializeField] private bool _jumpButton;

    public Vector2 MoveButton { get { return _moveButton; } set { _moveButton = value; } }
    public Vector2 CameraRotation { get { return _cameraRotation; } set { _cameraRotation = value; } }
    public bool JumpButton { get { return _jumpButton; } set { _jumpButton = value; } }
    public PlayerInput(Vector2 move, Vector2 rotation, bool jump)
    { 
        this._moveButton = move;
        this._cameraRotation = rotation;   
        this._jumpButton = jump;
    }
}