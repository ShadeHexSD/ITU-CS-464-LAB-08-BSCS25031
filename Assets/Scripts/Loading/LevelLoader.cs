using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The loading screen. Loads a scene asynchronously behind a panel with a progress bar and a
/// percentage, and does NOT let the player into the new scene until loading is complete
/// (allowSceneActivation stays false until the scene is fully loaded).
///
/// Unity reports progress 0 -> 0.9 while loading; at 0.9 the scene is loaded and waiting to be
/// switched in. The bar shows progress / 0.9 so it reaches 100%, not 90%.
///
/// Put it on the Canvas with the loading panel. Menu buttons call LoadGameplay() / LoadMenu() /
/// Load(name). Every scene it loads must be in File > Build Profiles > Scene List.
/// </summary>
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    [Header("Loading panel")]
    [SerializeField] GameObject loadingPanel;           // inactive by default
    [SerializeField] Slider progressBar;                // min 0, max 1
    [SerializeField] TMP_Text percentText;              // optional

    [Header("Scenes")]
    [SerializeField] string menuSceneName = "Menus";
    [SerializeField] string gameplaySceneName = "Gameplay";

    [Header("Feel")]
    [Tooltip("Keep the screen up at least this long so it doesn't flash on fast loads.")]
    [SerializeField] float minimumSeconds = 0.75f;
    [SerializeField] float holdAtFullSeconds = 0.2f;
    [SerializeField, Tooltip("Safety net: never hang on a broken scene load.")] float timeoutSeconds = 30f;

    public bool IsLoading { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Only matters if this loader is kept alive with DontDestroyOnLoad: hide the panel again.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsLoading = false;
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    // ---- public API (wire these to buttons) -----------------------------------------------

    public void LoadGameplay() => Load(gameplaySceneName);
    public void LoadMenu() => Load(menuSceneName);
    public void Reload() => Load(SceneManager.GetActiveScene().buildIndex);

    public void Load(string sceneName) => Begin(sceneName, () => SceneManager.LoadSceneAsync(sceneName));
    public void Load(int buildIndex) => Begin("#" + buildIndex, () => SceneManager.LoadSceneAsync(buildIndex));

    // ---- internals ------------------------------------------------------------------------

    void Begin(string label, Func<AsyncOperation> start)
    {
        if (IsLoading) return;                           // ignore double-clicks
        StartCoroutine(Run(label, start));
    }

    IEnumerator Run(string label, Func<AsyncOperation> start)
    {
        IsLoading = true;

        // A restart from the pause menu must not leave the next scene frozen or silent.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (loadingPanel != null) loadingPanel.SetActive(true);
        SetProgress(0f);

        AsyncOperation op = start();
        if (op == null)                                  // scene not in the Scene List (Unity logs why)
        {
            IsLoading = false;
            if (loadingPanel != null) loadingPanel.SetActive(false);
            yield break;
        }

        op.allowSceneActivation = false;                 // hold the player out until we say so

        float elapsed = 0f;
        while ((op.progress < 0.9f || elapsed < minimumSeconds) && elapsed < timeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float real = Mathf.Clamp01(op.progress / 0.9f);
            float timed = minimumSeconds > 0f ? Mathf.Clamp01(elapsed / minimumSeconds) : 1f;
            SetProgress(Mathf.Min(real, timed));         // never ahead of the real progress
            yield return null;
        }

        if (elapsed >= timeoutSeconds)
        {
            Debug.LogError($"LevelLoader: timed out loading '{label}'. Check the Scene List.", this);
            IsLoading = false;
            if (loadingPanel != null) loadingPanel.SetActive(false);
            yield break;
        }

        SetProgress(1f);
        yield return new WaitForSecondsRealtime(holdAtFullSeconds);

        op.allowSceneActivation = true;                  // loaded: now the scene may switch in
        while (!op.isDone) yield return null;

        IsLoading = false;
    }

    void SetProgress(float p)
    {
        if (progressBar != null) progressBar.value = p;
        if (percentText != null) percentText.SetText("{0:0}%", p * 100f);
    }
}
