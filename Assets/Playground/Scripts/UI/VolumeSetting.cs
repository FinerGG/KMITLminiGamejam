using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider BGMSlider;
    [SerializeField] private Slider SFXSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("BGMVolume")
            && PlayerPrefs.HasKey("SFXVolume"))
        {
            LoadVolume();
            return;
        }

        SetBGMVolume();
        SetSFXVolume();
    }

    public void SetBGMVolume()
    {
        float volume = BGMSlider.value;
        mixer.SetFloat("BGM", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        mixer.SetFloat("SFX", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void LoadVolume()
    {
        BGMSlider.value = PlayerPrefs.GetFloat("BGMVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");

        SetBGMVolume();
        SetSFXVolume();
    }
}