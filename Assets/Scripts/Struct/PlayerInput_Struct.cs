using UnityEngine;

public struct PlayerInput
{
    [SerializeField] private Vector2 _moveButton;
    [SerializeField] private Quaternion _camRoration;
    [SerializeField] private bool _jumpButton;
}