using UnityEngine;

namespace Playground
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : Singleton<PlayerController>
    {
        #region Class Veriables
        [Header("Components")]
        [SerializeField] private CharacterController _controller;
        [SerializeField] private CameraController _camera;

        [Header("Camera")]
        [SerializeField] private Transform cameraHolder;

        [Header("Movement")]
        [SerializeField] private float movingThreshold = 0.01f;

        [Header("Move (Ground)")]
        [SerializeField] private float inGroundDrag = 8f;
        [SerializeField] private float walkAcceleration = 35f;
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runAcceleration = 50f;
        [SerializeField] private float runSpeed = 7f;

        [Header("Move (Air)")]
        [SerializeField] private float airSpeed = 7f;
        [SerializeField] private float inAirAcceleration = 18f;
        [SerializeField] private float inAirDrag = 1.5f;

        [Header("Jump")]
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float terminalVelo = 50f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        [SerializeField] private float fallGravityMultiplier = 1.35f;
        [SerializeField] private float lowJumpMultiplier = 1.15f;

        [Header("Multi Jump")]
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private float extraJumpHeightMultiplier = 0.95f;

        [Header("Slide")]
        [SerializeField] private KeyCode slideKey = KeyCode.C;
        [SerializeField] private float slideSpeed = 10f;
        [SerializeField] private float slideDuration = 0.75f;
        [SerializeField] private float slideHeightMultiplier = 0.55f;
        [SerializeField] private float slideExitBoost = 0.25f;
        [SerializeField] private float runSlideSpeedMultiplier = 1.25f;

        [Header("Slide -> Jump Combo")]
        [SerializeField] private float slideJumpHeightMultiplier = 1.35f;
        [SerializeField] private float slideJumpForwardBoost = 4.5f;
        [SerializeField] private float slideJumpMinForwardSpeed = 9f;

        [Header("Dash")]
        [SerializeField] private KeyCode dashKey = KeyCode.Q;
        [SerializeField] private float dashSpeed = 16f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 0.6f;
        [SerializeField] private bool allowAirDash = true;
        [SerializeField] private int maxAirDashes = 1;

        [Header("Dash -> Slide Combo")]
        [SerializeField] private float powerSlideSpeed = 18f;
        [SerializeField] private float powerSlideDuration = 0.85f;
        [SerializeField] private float dashSlideWindow = 0.35f;

        [Header("Step-Up")]
        [SerializeField] private bool enableStepUp = true;
        [SerializeField] private float stepOffset = -0.65f;
        [SerializeField] private float stepUpHeight = 0.35f;
        [SerializeField] private float stepUpForwardCheck = 0.45f;
        [SerializeField] private float stepUpMinMoveSpeed = 0.1f;
        [SerializeField] private float stepDistanceSkip = 0.33f;

        [Header("Crouch")]
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        [SerializeField] private bool crouchToggle = false;
        [SerializeField] private float crouchHeightMultiplier = 0.6f;
        [SerializeField] private float crouchSpeedMultiplier = 0.75f;
        [SerializeField] private float crouchTransitionSpeed = 16f;
        [SerializeField] private float crouchHeadClearanceCheck = 0.2f;

        [Header("Stats")]
        [SerializeField] private PlayerState State;

        [Header("Ground Check")]
        [SerializeField] private LayerMask _groundLayers;
        [SerializeField] private float _groundOffset = -0.5f;

        private bool _jumpPressed;
        private bool _jumpHeld;
        private bool _runHeld;
        private bool _slidePressed;
        private bool _dashPressed;
        private bool _crouchPressed;

        private float coyoteCounter;
        private float jumpBufferCounter;
        private int _jumpsUsed;

        private Vector3 _moveInput;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;

        private float _stepOffset;

        private bool _sliding;
        private float _slideCounter;
        private float _originalHeight;
        private Vector3 _originalCenter;

        private bool _dashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private Vector3 _dashDir;
        private int _airDashUsed;
        private float _dashSlideTimer;

        private bool _crouching;
        private float _targetHeight;
        private Vector3 _targetCenter;
        private bool _forceUncrouchUntilKeyUp;

        private float _fallLevel = 0;
        private PlayerMovementState _lastMovementState = PlayerMovementState.Falling;
        #endregion

        #region Normal
        private void Awake()
        {
            if (_controller == null)
                _controller = GetComponent<CharacterController>();

            _originalHeight = _controller.height;
            _originalCenter = _controller.center;

            _targetHeight = _originalHeight;
            _targetCenter = _originalCenter;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        private void Update()
        {
            bool ground = IsGrounded();
            HandleMovementInput();
            UpdateState(ground);

            if (ground)
                _airDashUsed = 0;

            UpdateTimer(ground);

            UpdateCrouch(ground);

            TryStartDash(ground);
            UpdateDash();

            if (!_sliding && _slidePressed && _dashSlideTimer > 0f && ground)
                StartSlide(true);

            if (!_sliding && ground && _slidePressed && _horizontalVelocity.magnitude > 1.0f)
                StartSlide(false);

            if (_sliding)
                UpdateSlide(ground);

            VerticalMovement(ground);
            LateralMovement(ground);

            TryStepUp();

            Vector3 finalMove = !State.InGroundedState() ? SteepWalls(_horizontalVelocity) : _horizontalVelocity;
            finalMove.y = _verticalVelocity;
            _controller.Move(finalMove * Time.deltaTime);

            ConsumeEdgeInputs();
        }

        private void OnDrawGizmos()
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y + _groundOffset, transform.position.z);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pos, _controller.radius);

            if (!enableStepUp || _sliding) return;
            if (_horizontalVelocity.magnitude < stepUpMinMoveSpeed) return;

            Vector3 dir = new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z);
            if (dir.sqrMagnitude < 0.001f) return;
            dir.Normalize();

            Vector3 origin = transform.position + Vector3.up * stepOffset;
            Vector3 originUp = (transform.position + Vector3.up * stepOffset) + Vector3.up * stepUpHeight;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(origin, dir * stepUpForwardCheck);
            Gizmos.DrawRay(originUp, dir * stepUpForwardCheck);
        }
        #endregion

        #region Helpers
        private void ConsumeEdgeInputs()
        {
            _jumpPressed = false;
            _slidePressed = false;
            _dashPressed = false;
            _crouchPressed = false;
        }

        private void UpdateTimer(bool ground)
        {
            coyoteCounter = ground ? coyoteTime : Mathf.Max(0f, coyoteCounter - Time.deltaTime);
            jumpBufferCounter = (_jumpPressed || (ground && _jumpHeld)) ? jumpBufferTime : Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);

            if (_slideCounter > 0f) _slideCounter -= Time.deltaTime;

            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= Time.deltaTime;
            if (_dashSlideTimer > 0f) _dashSlideTimer -= Time.deltaTime;
        }

        private Vector3 SteepWalls(Vector3 velo)
        {
            Vector3 normal = CharacterControllerUnils.GetNormalWithSphereCast(_controller, _groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle < _controller.slopeLimit;

            if (!validAngle && _verticalVelocity < 0f)
            {
                velo = Vector3.ProjectOnPlane(velo, normal);
            }

            return velo;
        }

        private Vector3 GetCameraRelativeDir()
        {
            Vector3 camForward = cameraHolder.forward;
            Vector3 camRight = cameraHolder.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 dir = camRight * _moveInput.x + camForward * _moveInput.y;
            if (dir.sqrMagnitude > 0f) dir.Normalize();
            return dir;
        }
        #endregion

        #region Check
        private bool IsMovinglaterally()
        {
            Vector3 lateralVelo = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);

            return lateralVelo.sqrMagnitude > movingThreshold * movingThreshold;
        }

        private bool IsGrounded()
        {
            bool grounded = State.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
            return grounded;
        }

        private bool IsGroundedWhileGrounded()
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y + _groundOffset, transform.position.z);
            bool grounded = Physics.CheckSphere(pos, _controller.radius, _groundLayers, QueryTriggerInteraction.Ignore);
            return grounded;
        }

        private bool IsGroundedWhileAirborne()
        {
            Vector3 normal = CharacterControllerUnils.GetNormalWithSphereCast(_controller, _groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle < _controller.slopeLimit;

            return _controller.isGrounded && validAngle;
        }
        #endregion

        #region Input
        private void HandleMovementInput()
        {
            _moveInput.x = Input.GetAxisRaw("Horizontal");
            _moveInput.y = Input.GetAxisRaw("Vertical");
            _moveInput = Vector2.ClampMagnitude(_moveInput, 1f);

            _runHeld = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.Space))
                _jumpPressed = true;
            _jumpHeld = Input.GetKey(KeyCode.Space);

            if (Input.GetKeyDown(slideKey))
                _slidePressed = true;

            if (Input.GetKeyDown(dashKey))
                _dashPressed = true;

            if (crouchToggle)
            {
                if (Input.GetKeyDown(crouchKey))
                    _crouchPressed = true;
            }
            else
                _crouchPressed = Input.GetKey(crouchKey);
        }

        #endregion

        #region State
        private void UpdateState(bool ground)
        {
            _lastMovementState = State.CurrPlayerMovementState;

            bool isMovementInput = (_moveInput.x != 0f || _moveInput.z != 0f); //order
            bool isMovinglaterally = IsMovinglaterally(); //matter
            bool isRunning = _runHeld && isMovinglaterally; //order matters

            if (_dashing)
            {
                State.Set(PlayerMovementState.Dashing);
            }
            else if ((!ground || _jumpPressed) && _controller.velocity.y > 0f)
            {
                State.Set(PlayerMovementState.Jumping);
                _jumpPressed = false;
                _controller.stepOffset = 0f;
            }
            else if ((!ground || _jumpPressed) && _controller.velocity.y <= 0f)
            {
                State.Set(PlayerMovementState.Falling);
                _fallLevel += Mathf.Abs(_controller.velocity.y * Time.deltaTime);
                _jumpPressed = false;
                _controller.stepOffset = 0f;
            }
            else if (ground && _lastMovementState == PlayerMovementState.Falling)
            {
                State.Set(PlayerMovementState.Landing);

                if (_fallLevel <= (_controller.height * 1.5f))
                    State.Set(PlayerFallLevel.Low);
                else if (_fallLevel <= (_controller.height *  2.25f))
                    State.Set(PlayerFallLevel.High);
                else if (_fallLevel <= (_controller.height * 4.55f))
                    State.Set(PlayerFallLevel.VeryHigh);
                else
                    State.Set(PlayerFallLevel.ExtremeHigh);

                _jumpsUsed = 0;
                _fallLevel = 0;
            }
            else if (_crouching && ground)
            {
                PlayerMovementState lateralState = (isMovinglaterally || isMovementInput) ? PlayerMovementState.WalkAndCrouch : PlayerMovementState.Crouching;
                State.Set(lateralState);
                _controller.stepOffset = _stepOffset;
                _fallLevel = 0;
            }
            else
            {
                PlayerMovementState lateralState = _sliding ? PlayerMovementState.Sliding :
                    (isRunning ? PlayerMovementState.Running : (isMovinglaterally || isMovementInput ? PlayerMovementState.Walking : PlayerMovementState.Idling));
                State.Set(lateralState);
                _controller.stepOffset = _stepOffset;
                _fallLevel = 0;
            }
        }
        #endregion

        #region Vertical
        private void VerticalMovement(bool ground)
        {
            if (jumpBufferCounter > 0f)
            {
                if (coyoteCounter > 0f)
                {
                    DoJump(false);
                    coyoteCounter = 0f;
                    jumpBufferCounter = 0f;
                }
                else if (_jumpsUsed + 1 <= maxJumps)
                {
                    DoJump(true);
                    jumpBufferCounter = 0f;
                }
            }

            if (ground && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            float g = gravity;
            if (_verticalVelocity < 0f) g *= fallGravityMultiplier;
            else if (!_jumpHeld) g *= lowJumpMultiplier;

            _verticalVelocity += g * Time.deltaTime;
            _verticalVelocity = Mathf.Max(_verticalVelocity, -terminalVelo);
        }

        private void DoJump(bool isExtraJump)
        {
            if (_crouching)
                _forceUncrouchUntilKeyUp = true;

            bool slideJump = _sliding;

            float height = jumpHeight * (isExtraJump ? extraJumpHeightMultiplier : 1f);

            if (slideJump)
                height *= slideJumpHeightMultiplier;

            _verticalVelocity = Mathf.Sqrt(height * -2f * gravity);

            _jumpsUsed = Mathf.Clamp(_jumpsUsed + 1, 0, maxJumps);

            if (slideJump)
            {
                Vector3 hv = new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z);

                Vector3 dir = hv.sqrMagnitude > 0.2f
                    ? hv.normalized
                    : Vector3.ProjectOnPlane(cameraHolder.forward, Vector3.up).normalized;

                float newSpeed = Mathf.Max(hv.magnitude + slideJumpForwardBoost, slideJumpMinForwardSpeed);
                _horizontalVelocity = dir * newSpeed;

                EndSlide();
            }
            else if (_sliding) EndSlide();
        }
        #endregion

        #region Lateral
        private void LateralMovement(bool ground)
        {
            if (_sliding || _dashing) return;

            bool isRunning = State.IsRunningState();

            float acceleration = !ground ? inAirAcceleration :
                isRunning ? runAcceleration : walkAcceleration;
            float speed = isRunning || !ground ? runSpeed : walkSpeed;

            if (_crouching)
                speed *= crouchSpeedMultiplier;

            Vector3 moveDir = GetCameraRelativeDir();
            Vector3 targetVelocity = moveDir * speed;


            // accelerate toward target
            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity,
                targetVelocity,
                acceleration * Time.deltaTime
            );

            float drag = ground ? inGroundDrag : inAirDrag;
            if (moveDir.sqrMagnitude < 0.0001f)
            {
                _horizontalVelocity = Vector3.MoveTowards(
                    _horizontalVelocity,
                    Vector3.zero,
                    drag * Time.deltaTime
                );
            }
        }
        #endregion

        #region Slide
        private void StartSlide(bool powerSlide)
        {
            _sliding = true;
            _slideCounter = powerSlide ? powerSlideDuration : slideDuration;

            _controller.height = _originalHeight * slideHeightMultiplier;
            _controller.center = _originalCenter * slideHeightMultiplier;

            Vector3 forward = Vector3.ProjectOnPlane(cameraHolder.forward, Vector3.up).normalized;
            Vector3 dir = forward;

            Vector3 hv = new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z);
            if (hv.sqrMagnitude > 0.2f) dir = hv.normalized;

            float spd = powerSlide ? powerSlideSpeed :
                slideSpeed * (State.IsRunningState() ? runSlideSpeedMultiplier : 1f);

            float currentSpeed = hv.magnitude;
            float finalSpeed = Mathf.Max(currentSpeed, spd);

            _horizontalVelocity = dir * finalSpeed;
            _dashing = false;
        }

        private void UpdateSlide(bool grounded)
        {
            if (_slideCounter <= 0f || !grounded)
            {
                EndSlide();
                return;
            }

            float friction = inGroundDrag * 1.2f;
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, Vector3.zero, friction * Time.deltaTime);
        }

        private void EndSlide()
        {
            _sliding = false;
            _controller.height = _targetHeight;
            _controller.center = _targetCenter;

            _horizontalVelocity *= (1f + slideExitBoost);
        }
        #endregion

        #region Dash
        private void TryStartDash(bool grounded)
        {
            if (!_dashPressed || _dashCooldownTimer > 0f) return;
            if (!grounded && (!allowAirDash || _airDashUsed >= maxAirDashes)) return;

            Vector3 camForward = Vector3.ProjectOnPlane(cameraHolder.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cameraHolder.right, Vector3.up).normalized;
            
            Vector3 inputDir = (camRight * _moveInput.x + camForward * _moveInput.y);
            if (inputDir.sqrMagnitude > 0.001f) inputDir.Normalize();
            else inputDir = camForward;

            _dashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;
            _dashDir = inputDir;

            _dashSlideTimer = dashSlideWindow;

            if (!grounded)
            {
                _airDashUsed++;
                _verticalVelocity = Mathf.Max(_verticalVelocity, -2f);
            }

            if (_sliding) EndSlide();

            _horizontalVelocity = _dashDir * dashSpeed;
        }

        private void UpdateDash()
        {
            if (!_dashing) return;

            _dashTimer -= Time.deltaTime;

            _horizontalVelocity = _dashDir * dashSpeed;

            _verticalVelocity = Mathf.Max(_verticalVelocity, -6f);

            if (_dashTimer <= 0f)
            {
                _dashing = false;
                _horizontalVelocity *= 0.85f;
            }
        }
        #endregion

        #region Crouch
        private void UpdateCrouch(bool grounded)
        {
            if (_forceUncrouchUntilKeyUp)
            {
                TryUncrouch();

                if (!Input.GetKey(crouchKey))
                    _forceUncrouchUntilKeyUp = false;
            }
            else
            {
                if (crouchToggle)
                {
                    if (_crouchPressed)
                    {
                        if (_crouching) TryUncrouch();
                        else StartCrouch();
                    }
                }
                else
                {
                    if (_crouchPressed) StartCrouch();
                    else TryUncrouch();
                }
            }

            float desiredHeight = _crouching ? _originalHeight * crouchHeightMultiplier : _originalHeight;
            Vector3 desiredCenter = _crouching ? _originalCenter * crouchHeightMultiplier : _originalCenter;

            _targetHeight = desiredHeight;
            _targetCenter = desiredCenter;

            _controller.height = Mathf.Lerp(_controller.height, _targetHeight, crouchTransitionSpeed * Time.deltaTime);
            _controller.center = Vector3.Lerp(_controller.center, _targetCenter, crouchTransitionSpeed * Time.deltaTime);

            if (_sliding)
            {
                _controller.height = _originalHeight * slideHeightMultiplier;
                _controller.center = _originalCenter * slideHeightMultiplier;
            }
        }

        private void StartCrouch()
        {
            if (_sliding) return;
            _crouching = true;
        }

        private void TryUncrouch()
        {
            if (!_crouching || _sliding) return;

            float currentHeight = _controller.height;
            float wantHeight = _originalHeight;

            float extra = (wantHeight - currentHeight) + crouchHeadClearanceCheck;
            if (extra <= 0f)
            {
                _crouching = false;
                return;
            }

            Vector3 origin = transform.position + Vector3.up * (currentHeight * 0.5f);
            bool blocked = Physics.SphereCast(origin, _controller.radius * 0.95f, Vector3.up, out _, extra, _groundLayers, QueryTriggerInteraction.Ignore);

            if (!blocked)
                _crouching = false;
        }
        #endregion

        #region Auto Step
        private void TryStepUp()
        {
            if (!enableStepUp || _sliding) return;
            if (_horizontalVelocity.magnitude < stepUpMinMoveSpeed) return;

            Vector3 dir = new Vector3(_horizontalVelocity.x, 0f,_horizontalVelocity.z);
            if (dir.sqrMagnitude < 0.001f) return;
            dir.Normalize();

            Vector3 origin = transform.position + Vector3.up * stepOffset;
            Vector3 originUp = (transform.position + Vector3.up * stepOffset) + Vector3.up * stepUpHeight;

            bool hitLow = Physics.Raycast(origin, dir, out RaycastHit lowHit, stepUpForwardCheck, _groundLayers, QueryTriggerInteraction.Ignore);
            bool hitHigh = Physics.Raycast(originUp, dir, stepUpForwardCheck, _groundLayers, QueryTriggerInteraction.Ignore);

            if (hitLow && !hitHigh)
            {
                Vector3 pos = originUp + dir * (_controller.radius + 0.1f);
                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, _controller.radius, _groundLayers, QueryTriggerInteraction.Ignore))
                {
                    float controllerBottom = _controller.bounds.min.y;
                    float stepHeight = hit.point.y - controllerBottom;
                    if (stepHeight > 0f && stepHeight <= stepUpHeight)
                        _controller.Move(Vector3.up * stepHeight);

                    if (hit.distance > stepDistanceSkip)
                    {
                        //Debug.Log("Skip");
                        //_controller.Move(Vector3.up * stepUpSmooth * Time.deltaTime);
                    }
                }

                if (_verticalVelocity < 0f) _verticalVelocity = -2f;
            }
        }
        #endregion
    }
}