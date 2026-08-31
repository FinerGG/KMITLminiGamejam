using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneController : MonoSingleton<SceneController>
{
    [Header("Loading UI")]
    [SerializeField] private GameObject loadingScreen;

    [Tooltip("Optional progress bar (0–1)")]
    [SerializeField] private Slider progressBar;

    [Tooltip("Optional text for percentage display")]
    [SerializeField] private TMP_Text progressText;

    [Tooltip("Extra delay after load completes")]
    [SerializeField] private float postLoadDelay = 1f;

    private bool isLoading;
    public bool IsLoading => isLoading;

    protected override void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            Destroy(loadingScreen);
        }
        base.Awake();
    }

    protected override void OnInitializing()
    {
        DontDestroyOnLoad(gameObject);

        if (loadingScreen != null)
        {
            DontDestroyOnLoad(loadingScreen);
            loadingScreen.SetActive(false);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (!isLoading)
            StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void LoadSceneNotWait(string sceneName) => UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        // Activate loading UI
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Start async loading
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);

            // Update UI
            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = $"{progress * 100f:0}%";

            // When scene is loaded (almost 0.9f)
            if (async.progress >= 0.9f)
            {
                yield return new WaitForSeconds(postLoadDelay);

                async.allowSceneActivation = true;
            }

            yield return null;
        }

        isLoading = false;

        // Fade out loading UI (optional)
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
}