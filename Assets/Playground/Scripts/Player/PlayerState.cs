using UnityEngine;

public enum PlayerMovementState
{
    Idling,
    Walking,
    Running,
    Jumping,
    Falling,
    Dashing,
    Sliding,
    Crouching,
    WalkAndCrouch,
    Landing,
}

public enum PlayerFallLevel
{
    None,   
    Low,
    High,
    VeryHigh,
    ExtremeHigh
}

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public PlayerMovementState CurrPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
    [field: SerializeField] public PlayerFallLevel CurrPlayerFallLevel { get; private set; } = PlayerFallLevel.None;

    public void Set(PlayerMovementState playerMovementState)
    {
        CurrPlayerMovementState = playerMovementState;
    }
    public void Set(PlayerFallLevel playerFallLevel)
    {
        CurrPlayerFallLevel = playerFallLevel;
    }

    public bool InGroundedState()
    {
        return CurrPlayerMovementState == PlayerMovementState.Idling ||
            CurrPlayerMovementState == PlayerMovementState.Walking ||
            CurrPlayerMovementState == PlayerMovementState.Running ||
            CurrPlayerMovementState == PlayerMovementState.Sliding ||
            CurrPlayerMovementState == PlayerMovementState.Crouching ||
            CurrPlayerMovementState == PlayerMovementState.WalkAndCrouch;
    }

    public bool IsRunningState()
    {
        return CurrPlayerMovementState == PlayerMovementState.Running;
    }

    public bool IsCrouchingState()
    {
        return CurrPlayerMovementState == PlayerMovementState.Crouching;
    }

    public bool IsWalkAndCrouchState()
    {
        return CurrPlayerMovementState == PlayerMovementState.WalkAndCrouch;
    }

    public bool IsDashingState()
    {
        return CurrPlayerMovementState == PlayerMovementState.Dashing;
    }

    public bool IsSlidingState()
    {
        return CurrPlayerMovementState == PlayerMovementState.Sliding;
    }

    public bool IsNormalMoveState()
    {
        return CurrPlayerMovementState == PlayerMovementState.Walking ||
            CurrPlayerMovementState == PlayerMovementState.Running ||
            CurrPlayerMovementState == PlayerMovementState.WalkAndCrouch;
    }

    public bool IsFallingState()
    {
        return CurrPlayerMovementState == PlayerMovementState.Falling;
    }

    public bool IsLandingState()
    {
        return CurrPlayerMovementState == PlayerMovementState.Landing;
    }

    public bool InStateGroundedState(PlayerMovementState movementState)
    {
        return movementState == PlayerMovementState.Idling ||
            movementState == PlayerMovementState.Walking ||
            movementState == PlayerMovementState.Running ||
            movementState == PlayerMovementState.Sliding ||
            movementState == PlayerMovementState.Crouching ||
            movementState == PlayerMovementState.WalkAndCrouch;
    }
}
