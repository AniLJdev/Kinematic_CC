using Unity.VisualScripting;
using UnityEngine;

public struct PlayerInput
{
    [SerializeField] private Vector2 _moveButton;
    [SerializeField] private Vector2 _cameraRotation;
    [SerializeField] private float _jumpButton;

    public Vector2 MoveButton => _moveButton;
    public Vector2 CameraRotation => _cameraRotation;
    public float JumpButton => _jumpButton;
    public PlayerInput(Vector2 move, Vector2 rotation, float jump)
    { 
        this._moveButton = move;
        this._cameraRotation = rotation;   
        this._jumpButton = jump;
    }
}