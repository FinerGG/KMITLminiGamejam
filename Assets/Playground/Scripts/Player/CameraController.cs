using UnityEngine;

[System.Serializable]
struct MultiplierHighLevel
{
    public float Low;
    public float High;
    public float VeryHigh;
    public float ExtremeHigh;
}

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerState State;
    [SerializeField] private Camera _camera;
    [SerializeField] private GlobalVolumeController _volumeController;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 120f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("HeadBob")]
    [SerializeField, Range(0, 0.02f)] private float amountHeadBob = 0.002f;
    [SerializeField, Range(0, 30f)] private float frequencyHeadBob = 10f;
    [SerializeField, Range(0, 100f)] private float smoothHeadBob = 14f;
    [SerializeField] private float amountHeadBobRunMultiplier = 1.25f;
    [SerializeField] private float amountHeadBobCrouchMultiplier = 0.85f;

    [Header("HeadBob Speed Scaling")]
    [SerializeField] private float minBobSpeed = 0.15f; // below this -> no bob
    [SerializeField] private float maxBobSpeed = 7f;    // speed at which bob reaches full strength
    [SerializeField] private float bobFadeIn = 18f;
    [SerializeField] private float bobFadeOut = 18f;

    [Header("Fov")]
    [SerializeField] private bool enableFovKick = true;
    [SerializeField] private float baseFov = 70f;
    [SerializeField] private float runFov = 78f;
    [SerializeField] private float crouchFov = 62f;
    [SerializeField] private float dashFov = 90f;
    [SerializeField] private float slideFov = 84f;
    [SerializeField] private float fovSmooth = 10f;

    [Header("Tilt")]
    [SerializeField] private bool enableTilt = true;
    [SerializeField] private float tiltAmount = 2.5f; // degrees
    [SerializeField] private float tiltSmooth = 10f;

    [Header("Camera Shake")]
    [field: SerializeField] private MultiplierHighLevel MHL;
    [SerializeField] private float shakePositionScale = 1f;
    [SerializeField] private float shakeRotationScale = 1f;
    [SerializeField] private float shakeReturnSmooth = 18f;

    private Vector3 _cameraOriginalPosition;
    private float _pitch;
    private float _bobWeight;
    private float _bobTime;
    private Vector3 _bobOffset;
    private float _currentTilt;

    private Vector3 _shakePosOffset;
    private Vector3 _shakeRotOffset;

    private float _shakeTimeLeft;
    private float _shakeDuration;
    private float _shakeStrength;
    private float _shakeFrequency;
    private float _shakeDamping;
    private Vector3 _shakePosAxes;
    private Vector3 _shakeRotAxes;
    private bool _shakeUseUnscaled;
    private float _shakeSeed;


    private void Awake()
    {
        _cameraOriginalPosition = transform.localPosition;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleShake();

        float speed01 = GetMoveSpeed01();
        bool shouldBob = State != null && State.IsNormalMoveState() && speed01 > 0.001f;

        UpdateHeadBob(shouldBob, speed01);
        UpdateFov();
        UpdateTilt();
        UpdateShake();
    }

    private void LateUpdate()
    {
        ApplyCameraPose();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        if (playerTransform != null)
            playerTransform.Rotate(Vector3.up * mouseX);

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        //transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void HandleShake()
    {
        if (State.IsLandingState() && 
            State.CurrPlayerFallLevel != PlayerFallLevel.None)
        {
            float muit = State.CurrPlayerFallLevel == PlayerFallLevel.Low ? MHL.Low :
                (State.CurrPlayerFallLevel == PlayerFallLevel.High ? MHL.High :
                (State.CurrPlayerFallLevel == PlayerFallLevel.VeryHigh ? MHL.VeryHigh :
                MHL.ExtremeHigh));
            Shake(
                0.25f * muit,
                0.12f * muit, 
                28f * muit,
                1f * muit, 
                new Vector3(0.3f, 1f, 0.2f) * muit, 
                new Vector3(1f, 0.4f, 0.8f) * muit
                );

            State.Set(PlayerFallLevel.None);
        }

        if (State.IsDashingState())
        {
            Shake(0.35f, 0.12f, 35f, 1.2f,
                new Vector3(0.15f, 0.15f, 0.15f),
                new Vector3(0.28f, 0.12f, 0.18f));
        }

        if (State.IsSlidingState())
        {
            Shake(0.3f, 0.1f, 25f, 1f,
                new Vector3(0.1f, 0.1f, 0.1f),
                new Vector3(0.12f, 0.12f, 0.12f));
        }
    }

    private float GetMoveSpeed01()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float inputMag = new Vector2(h, v).magnitude;

        float approxSpeed = inputMag * (State != null && State.IsRunningState() ? maxBobSpeed : maxBobSpeed * 0.65f);

        if (approxSpeed < minBobSpeed) return 0f;
        return Mathf.Clamp01(approxSpeed / maxBobSpeed);
    }

    private void ApplyCameraPose()
    {
        //base + headbob + shake
        transform.localPosition = _cameraOriginalPosition + _bobOffset + _shakePosOffset;

        //pitch + tilt + shakeRotation
        Quaternion baseRot = Quaternion.Euler(_pitch, 0f, _currentTilt);
        Quaternion shakeRot = Quaternion.Euler(_shakeRotOffset);
        transform.localRotation = baseRot * shakeRot;
    }


    private void UpdateHeadBob(bool shouldBob, float speed01)
    {
        float target = shouldBob ? 1f : 0f;
        float fade = shouldBob ? bobFadeIn : bobFadeOut;
        _bobWeight = Mathf.MoveTowards(_bobWeight, target, fade * Time.deltaTime);

        float stateMult = 1f;
        if (State != null)
        {
            if (State.IsRunningState()) stateMult = amountHeadBobRunMultiplier;
            else if (State.IsWalkAndCrouchState()) stateMult = amountHeadBobCrouchMultiplier;
        }

        if (_bobWeight > 0.001f)
            _bobTime += Time.deltaTime * frequencyHeadBob * Mathf.Lerp(0.6f, 1.25f, speed01);

        Vector3 raw = Vector3.zero;
        raw.y = Mathf.Sin(_bobTime) * amountHeadBob * 1.4f * stateMult;
        raw.x = Mathf.Cos(_bobTime * 0.5f) * amountHeadBob * 1.6f * stateMult;

        raw *= _bobWeight;
        raw *= Mathf.Lerp(0.25f, 1f, speed01);

        _bobOffset = Vector3.Lerp(_bobOffset, raw, smoothHeadBob * Time.deltaTime);

        //transform.localPosition = _cameraOriginalPosition + _bobOffset;
    }

    private void UpdateFov()
    {
        if (_camera == null || !enableFovKick) return;

        float target = State.IsDashingState() ? dashFov : 
            (State.IsWalkAndCrouchState() ? crouchFov : 
            (State.IsSlidingState() ? slideFov : 
            (State.IsRunningState() ? runFov : baseFov)));

        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, target, fovSmooth * Time.deltaTime);
    }

    private void UpdateTilt()
    {
        if (!enableTilt) return;

        float h = Input.GetAxisRaw("Horizontal");
        float targetTilt = -h * tiltAmount;

        _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, tiltSmooth * Time.deltaTime);

        //transform.localRotation = Quaternion.Euler(_pitch, 0f, _currentTilt);
    }

    /// <summary>
    /// strength: ความแรง
    /// duration: ระยะเวลา
    /// frequency: ความถี่การสั่น
    /// damping: 0..1 (1 = fade out จนจบ, 0 = ไม่ fade)
    /// posAxes/rotAxes: เลือกสั่นเฉพาะแกน (0 หรือ 1)
    /// useUnscaledTime: ใช้เวลาแบบไม่โดน timeScale
    /// </summary>
    public void Shake(
        float strength,
        float duration,
        float frequency = 25f,
        float damping = 1f,
        Vector3? posAxes = null,
        Vector3? rotAxes = null,
        bool useUnscaledTime = false)
    {
        _shakeStrength = Mathf.Max(_shakeStrength, strength);
        _shakeDuration = Mathf.Max(_shakeDuration, duration);
        _shakeTimeLeft = Mathf.Max(_shakeTimeLeft, duration);

        _shakeFrequency = Mathf.Max(0.01f, frequency);
        _shakeDamping = Mathf.Clamp01(damping);

        _shakePosAxes = posAxes ?? new Vector3(1f, 1f, 1f);
        _shakeRotAxes = rotAxes ?? new Vector3(1f, 1f, 1f);

        _shakeUseUnscaled = useUnscaledTime;

        _shakeSeed = Random.value * 999f;
    }

    private void UpdateShake()
    {
        float dt = _shakeUseUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;

        if (_shakeTimeLeft > 0f)
        {
            _shakeTimeLeft -= dt;

            float t = 1f - Mathf.Clamp01(_shakeTimeLeft / Mathf.Max(0.0001f, _shakeDuration));
            float fade = (_shakeDamping <= 0f) ? 1f : (1f - t);

            float time = (_shakeSeed + Time.time) * _shakeFrequency;

            float px = (Mathf.PerlinNoise(time, 0.13f) * 2f - 1f) * _shakeStrength * fade;
            float py = (Mathf.PerlinNoise(time, 0.47f) * 2f - 1f) * _shakeStrength * fade;
            float pz = (Mathf.PerlinNoise(time, 0.91f) * 2f - 1f) * _shakeStrength * fade;

            float rx = (Mathf.PerlinNoise(time, 1.13f) * 2f - 1f) * _shakeStrength * fade;
            float ry = (Mathf.PerlinNoise(time, 1.47f) * 2f - 1f) * _shakeStrength * fade;
            float rz = (Mathf.PerlinNoise(time, 1.91f) * 2f - 1f) * _shakeStrength * fade;

            _shakePosOffset = Vector3.Scale(new Vector3(px, py, pz) * shakePositionScale, _shakePosAxes);
            _shakeRotOffset = Vector3.Scale(new Vector3(rx, ry, rz) * shakeRotationScale, _shakeRotAxes);
        }
        else
        {
            _shakePosOffset = Vector3.Lerp(_shakePosOffset, Vector3.zero, shakeReturnSmooth * Time.deltaTime);
            _shakeRotOffset = Vector3.Lerp(_shakeRotOffset, Vector3.zero, shakeReturnSmooth * Time.deltaTime);

            _shakeStrength = 0f;
            _shakeDuration = 0f;
        }
    }

}
