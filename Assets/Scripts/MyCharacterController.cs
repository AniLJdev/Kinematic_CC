using KinematicCharacterController;
using UnityEngine;

public class MyCharacterController : MonoBehaviour, ICharacterController
{
    
    private KinematicCharacterMotor Motor;

    private void Start()
    {
        AddKinematicCC();
    }

    private void AddKinematicCC()
    {
        Motor = GetComponent<KinematicCharacterMotor>();
        Motor.CharacterController = this;
    }

    void ICharacterController.AfterCharacterUpdate(float deltaTime)
    {
        
    }

    void ICharacterController.BeforeCharacterUpdate(float deltaTime)
    {
       
    }

    bool ICharacterController.IsColliderValidForCollisions(Collider coll)
    {
       return false;
    }

    void ICharacterController.OnDiscreteCollisionDetected(Collider hitCollider)
    {
       
    }

    void ICharacterController.OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    void ICharacterController.OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    void ICharacterController.PostGroundingUpdate(float deltaTime)
    {
       
    }

    void ICharacterController.ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    void ICharacterController.UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        
    }

    void ICharacterController.UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
       
    }
}
