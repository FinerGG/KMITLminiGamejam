using UnityEngine;

namespace MGJ
{
    public class CameraController : Singleton<CameraController>
    {
        [Header("Components")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerState state;
        [SerializeField] private Camera startCamera;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 120f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;
        [SerializeField] private float defaultYawLimit = 0f; // จำกัดการหันซ้าย-ขวาของกล้องอิสระ (0 = หมุนได้รอบตัว)

        [Header("Fov")]
        [SerializeField] private bool enableFovKick = true;
        [SerializeField] private float baseFov = 70f;
        [SerializeField] private float fovSmooth = 10f;

        [Header("Camera Shake")]
        [SerializeField] private float shakePositionScale = 1f;
        [SerializeField] private float shakeRotationScale = 1f;
        [SerializeField] private float shakeReturnSmooth = 18f;
        [SerializeField] private float cameraTransitionDuration = 0.5f;

        private Camera _camera;
        public Camera Camera => _camera;

        private Vector3 _cameraOriginalPosition;
        private Quaternion _cameraOriginalRotation;
        private float _pitch;
        private float _yaw;
        private float _yawLimit;
        private bool _cameraFollowsPlayer;
        private bool _cameraTurnEnable = true;
        private Coroutine _cameraTransition;

        private bool _isCameraTransitioning;
        public bool IsCameraTransitioning => _isCameraTransitioning;

        private Vector3 _transitionPosition;
        private Quaternion _transitionRotation;

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

        private Vector3 _offsetRotAxes = Vector3.zero;

        private void Awake()
        {
            if (startCamera == null)
            {
                startCamera = Camera.main;
            }
        }

        void Start()
        {
            SetCamera(startCamera,0f);
        }

        void Update()
        {
            HandleMouseLook();
            HandleShake();
            UpdateFov();
            UpdateShake();
        }

        private void LateUpdate()
        {
            ApplyCameraPose();
        }

        public void SetCamera(Camera newCamera)
        {
            SetCamera(newCamera, cameraTransitionDuration);
        }

        public void SetCamera(Camera newCamera, float duration)
        {
            if (newCamera == null)
            {
                Debug.LogWarning("CameraController.SetCamera received a null camera.");
                return;
            }

            if (_camera == newCamera && !_isCameraTransitioning)
                return;

            if (_cameraTransition != null)
            {
                StopCoroutine(_cameraTransition);
                _cameraTransition = null;
                _isCameraTransitioning = false;
            }

            duration = Mathf.Max(0f, duration);

            if (_camera == null || duration <= 0f)
            {
                SwitchCamera(newCamera);
                return;
            }

            newCamera.enabled = false;
            _cameraTransition = StartCoroutine(SmoothSetCamera(newCamera, duration));
        }

        private void SwitchCamera(Camera newCamera)
        {
            if (_camera != null)
            {
                _camera.transform.localPosition = _cameraOriginalPosition;
                _camera.transform.localRotation = _cameraOriginalRotation;
                _camera.tag = "Untagged";
                _camera.enabled = false;
            }

            _camera = newCamera;
            _camera.enabled = true;
            _camera.tag = "MainCamera";

            _cameraOriginalPosition = newCamera.transform.localPosition;
            _cameraOriginalRotation = newCamera.transform.localRotation;

            // กล้องที่เป็นลูกของ player จะได้ yaw จากการหมุนตัว player อยู่แล้ว
            // กล้องอิสระ (เช่นกล้องประตู) ต้องเก็บ yaw ไว้บนตัวกล้องเอง
            _cameraFollowsPlayer = playerTransform != null && newCamera.transform.IsChildOf(playerTransform);

            _pitch = 0f;
            _yaw = 0f;
        }

        private System.Collections.IEnumerator SmoothSetCamera(Camera newCamera, float duration)
        {
            Camera previousCamera = _camera;
            Vector3 fromPosition = previousCamera.transform.position;
            Quaternion fromRotation = previousCamera.transform.rotation;
            Vector3 toPosition = newCamera.transform.position;
            Quaternion toRotation = newCamera.transform.rotation;

            _isCameraTransitioning = true;
            _transitionPosition = fromPosition;
            _transitionRotation = fromRotation;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);

                _transitionPosition = Vector3.LerpUnclamped(fromPosition, toPosition, t);
                _transitionRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }

            _isCameraTransitioning = false;
            _cameraTransition = null;
            SwitchCamera(newCamera);
        }

        public void ReCamera()
        {
            SetCamera(startCamera);
            _offsetRotAxes = Vector3.zero;
            _cameraTurnEnable = true;
            _yawLimit = defaultYawLimit;
            ApplyCameraPose();
        }

        public void SetCameraTurnEnable(bool turnEnable)
        {
            _cameraTurnEnable = turnEnable;
        }

        public void SetCameraTurnEnable(bool turnEnable, Vector3 offsetRot)
        {
            SetCameraTurnEnable(turnEnable);
            _offsetRotAxes = offsetRot;
            _pitch = 0f;
        }

        /// <summary>
        /// เปิด mouse look พร้อมจำกัดการหันซ้าย-ขวาของกล้องอิสระ
        /// yawLimit เป็นองศาจากมุมตั้งต้นของกล้อง (0 = หมุนได้รอบตัว)
        /// </summary>
        public void SetCameraTurnEnable(bool turnEnable, float yawLimit)
        {
            SetCameraTurnEnable(turnEnable);
            _yawLimit = Mathf.Max(0f, yawLimit);
            _pitch = 0f;
            _yaw = 0f;
        }

        private void HandleMouseLook()
        {
            if (!_cameraTurnEnable)
                return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            if (_cameraFollowsPlayer)
            {
                if (playerTransform != null)
                    playerTransform.Rotate(Vector3.up * mouseX);
            }
            else
            {
                _yaw += mouseX;

                if (_yawLimit > 0f)
                    _yaw = Mathf.Clamp(_yaw, -_yawLimit, _yawLimit);
            }

            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        private void UpdateFov()
        {
            if (_camera == null || !enableFovKick) return;

            float target = baseFov;
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, target, fovSmooth * Time.deltaTime);
        }

        private void HandleShake()
        {

        }

        private void ApplyCameraPose()
        {
            if (_camera == null)
                return;

            if (_isCameraTransitioning)
            {
                _camera.transform.SetPositionAndRotation(_transitionPosition, _transitionRotation);
                return;
            }

            //base + shake
            _camera.transform.localPosition = _cameraOriginalPosition + _shakePosOffset;

            //pitch + yaw + shakeRotation
            Quaternion baseRot = Quaternion.Euler(_pitch, _yaw, 0f);
            Quaternion shakeRot = Quaternion.Euler(_shakeRotOffset);
            Quaternion offsetRot = Quaternion.Euler(_offsetRotAxes);

            // กล้องอิสระเริ่มหันจากมุมที่จัดไว้ใน Scene แล้วค่อยบวก mouse look
            Quaternion restRot = _cameraFollowsPlayer ? Quaternion.identity : _cameraOriginalRotation;

            _camera.transform.localRotation = restRot * baseRot * shakeRot * offsetRot;
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

}
