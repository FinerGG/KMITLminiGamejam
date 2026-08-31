using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Needed to change scenes
using System.Collections;

public class SwitchSceneButton : CoolButton
{
    [Header("Scene Settings")]
    [Tooltip("Enter the exact name of the scene you want to load")]
    [SerializeField] private string sceneName;

    [Header("Optional Settings")]
    [Tooltip("Play a sound before changing scene (optional)")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float soundVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (clickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    protected override void Start()
    {
        base.Start();
        button.onClick.AddListener(LoadScene);
    }

    // This function can be called from the Button OnClick() event
    public void LoadScene()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, soundVolume);
            // Wait for the sound to finish before loading scene
            StartCoroutine(LoadAfterSound());
        }
        else
        {
            SceneController.Instance.LoadScene(sceneName);
        }
    }

    private IEnumerator LoadAfterSound()
    {
        yield return new WaitForSeconds(clickSound.length);
        SceneController.Instance.LoadScene(sceneName);
    }
}
