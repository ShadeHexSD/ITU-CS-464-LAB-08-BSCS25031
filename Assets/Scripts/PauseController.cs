using UnityEngine;

// Owns pausing. Put it on an empty object called Controllers.
public class PauseController : MonoBehaviour
{
    [SerializeField] UIScreen matchScreen;
    [SerializeField] UIScreen pauseScreen;
    [SerializeField] UIScreen homeScreen;
    [SerializeField] GameplayStub gameplay;

    UIScreen previousScreen;

    void OnEnable()
    {
        ScreenManager.ScreenChanged += OnScreenChanged;
        var manager = ScreenManager.Instance;
        if (manager != null) previousScreen = manager.Current;
    }

    void OnDisable()
    {
        ScreenManager.ScreenChanged -= OnScreenChanged;
        Time.timeScale = 1f;   // never leave the editor frozen
        AudioBootstrap.SetPaused(false);
    }

    void Update()
    {
        // Esc / Android back during a match opens Pause instead of leaving the match.
        var manager = ScreenManager.Instance;
        if (BackInput.WasPressedThisFrame() && manager != null && manager.Current == matchScreen) Pause();
    }

    // Safety net: whenever a screen finishes opening, time runs only if it is not Pause.
    // Audio follows the same rule: SFX pause with the game, UI and music intentionally continue.
    void OnScreenChanged(UIScreen screen)
    {
        bool wasPaused = previousScreen == pauseScreen;
        bool isPaused = screen == pauseScreen;

        if (isPaused)
        {
            AudioBootstrap.SetPaused(true);
        }
        else if (wasPaused)
        {
            AudioBootstrap.SetPaused(false);
            UIAudio.PlayResume();
        }

        Time.timeScale = isPaused ? 0f : 1f;
        previousScreen = screen;
    }

    public void Pause()
    {
        Time.timeScale = 0f;   // stop the game right away, not after the animation
        AudioBootstrap.SetPaused(true);
        UIAudio.PlayPause();
        ScreenManager.Instance?.Show(pauseScreen, Transition.Modal);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        ScreenManager.Instance?.Back();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        AudioBootstrap.SetPaused(false);

        // The real gameplay scene owns a LevelLoader. The placeholder match only has GameplayStub.
        if (LevelLoader.Instance != null) LevelLoader.Instance.Reload();
        else
        {
            gameplay?.ResetTimer();
            Resume();
        }
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        AudioBootstrap.SetPaused(false);

        // In the gameplay scene there is no menu Screen to return to: load Menus.unity instead.
        if (LevelLoader.Instance != null) LevelLoader.Instance.LoadMenu();
        else if (homeScreen != null) ScreenManager.Instance?.GoHome(homeScreen, Transition.Fade);
    }
}
