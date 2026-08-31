using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeController : Singleton<GlobalVolumeController>
{
    [Header("Refs")]
    [SerializeField] private Volume volume;

    [Header("Smoothing")]
    [SerializeField] private float depthOfFieldSmooth = 14f;
    [SerializeField] private float toneSmooth = 6f;

    private VolumeProfile _profile;

    private DepthOfField _depthOfField;
    private float _dofFocus_D;
    private float _dofAperture_D;
    private float _dofLength_D;

    private float _dof_PT;
    private float _dofPD;

    private float _dofFocus_T;
    private float _dofAperture_T;
    private float _dofLength_T;

    private ColorAdjustments _colorAdjustments;
    private Color _cp_D;
    private float _cp_PT;
    private Color _cp_T;

    private void Awake()
    {
        if (volume == null)
            volume = GetComponent<Volume>();
        _profile = volume.profile;
        
        _profile.TryGet(out _depthOfField);
        _profile.TryGet(out _colorAdjustments);
        if (_depthOfField != null)
        {
            _depthOfField.mode.value = DepthOfFieldMode.Bokeh;

            _depthOfField.active = true;

            _dofFocus_D = _depthOfField.focusDistance.value;
            _dofAperture_D = _depthOfField.aperture.value;
            _dofLength_D = _depthOfField.focalLength.value;
        }
        if (_colorAdjustments)
        {
            _colorAdjustments.active = true;
            _cp_D = _colorAdjustments.colorFilter.value;
            _cp_PT = 0f;
        }

        ResetTone();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        UpdateTimer(dt);

        UpdateBokehDof();
        UpdateTone();
    }

    private void UpdateTimer(float dt)
    {
        if (_dof_PT > 0) _dof_PT -= dt;
    }

    private void UpdateBokehDof()
    {
        if (_depthOfField == null) return;

        bool pulsing = _dof_PT > 0f;

        float targetFocus = pulsing ? _dofFocus_T : _dofFocus_D;
        float targetAperture = pulsing ? _dofAperture_T : _dofAperture_D;
        float targetFocal = pulsing ? _dofLength_T : _dofLength_D;

        _depthOfField.focusDistance.value = Mathf.Lerp(_depthOfField.focusDistance.value, targetFocus, depthOfFieldSmooth * Time.deltaTime);
        _depthOfField.aperture.value = Mathf.Lerp(_depthOfField.aperture.value, targetAperture, depthOfFieldSmooth * Time.deltaTime);
        _depthOfField.focalLength.value = Mathf.Lerp(_depthOfField.focalLength.value, targetFocal, depthOfFieldSmooth * Time.deltaTime);
    }

    private void UpdateTone()
    {
        if (_colorAdjustments == null) return;

        if (_cp_PT >= 1f) return;

        _cp_PT = Mathf.MoveTowards(_cp_PT, 1f, toneSmooth * Time.deltaTime);

        Color blended = Color.Lerp(_cp_D, _cp_T, _cp_PT);
        _colorAdjustments.colorFilter.value = blended;
    }

    /// <summary>
    /// ทำเอฟเฟกต์เบลอทั้งจอแบบ Bokeh DOF แล้วค่อยๆหาย
    /// duration: ระยะเวลา (เช่น 0.15-0.35)
    /// blurStrength: 0..1 (ยิ่งมากยิ่งเบลอ) -> แปลงเป็น aperture ต่ำลง + focalLength สูงขึ้น
    /// focusDistance: ระยะโฟกัส (เมตร) ถ้าอยากให้ทั้งจอเบลอ ให้ตั้งไกล/แปลกๆ หรือใช้ aperture ต่ำมาก
    /// </summary>
    public void PulseBokehBlur(
        float duration,
        float blurStrength = 1f,
        float focusDistance = 0.25f)
    {
        if (_depthOfField == null) return;

        _depthOfField.active = true;

        blurStrength = Mathf.Clamp01(blurStrength);

        float aperture = Mathf.Lerp(16f, 1.2f, blurStrength);
        float focalLen = Mathf.Lerp(_dofLength_D, 80f, blurStrength);

        _dofFocus_T = Mathf.Max(0.05f, focusDistance);
        _dofAperture_T = aperture;
        _dofLength_T = focalLen;

        _dofPD = duration;
        _dof_PT = Mathf.Max(_dof_PT, duration);
    }

    public void ToneChange(Color targetColor)
    {
        if (_colorAdjustments == null) return;

        _cp_T = targetColor;
        _cp_PT = 0f;
    }

    public void ResetTone()
    {
        if (_colorAdjustments == null) return;

        _cp_T = _cp_D;
        _cp_PT = 0f;
    }
}
