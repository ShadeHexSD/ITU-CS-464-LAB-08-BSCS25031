# CRATER CLASH

## Lab 08 — Audio, Events and Loading

**Developer:** Tayyab Ahmed Minhas  
**Engine:** Unity 6.3 (`6000.3.24f1`)  
**Reference resolution:** 1080 × 1920  
**Project:** `ITU-CS-464-LAB-08-BSCS25031`  
**Scenes:** `Menus`, `Gameplay`  
**Demo video / download:** [Google Drive](https://drive.google.com/file/d/18OmsAE9klMI9_oIdS5t7ypFK8OOo0ct4/view?usp=sharing)

This README documents the Lab 08 additions built on top of the Lab 07 menu project:
a real asynchronous loading screen, a single audio mixer with persistent volume controls,
reusable UI/gameplay audio, managed music transitions, and a C# event system with independent
listeners. The ten Lab 07 menu screens, their navigation, tweens and responsive mobile layout
are preserved.

---

## 1. Setup and how to run

1. Open the project in **Unity 6.3**.
2. In **File → Build Profiles → Scene List**, confirm:
   1. `Assets/Scenes/Menus.unity`
   2. `Assets/Scenes/Gameplay.unity`
3. Press **Play** from `Menus.unity`.
4. Navigate normally: Title → Main Menu → Mode Select → Level Select → Start Match.
5. Volume and mute controls are on **Menus → Settings**.
6. Audio attribution is shown on **Menus → Settings → Credits**.

**Project assets used by this lab**

| Asset | Location |
|---|---|
| Audio mixer | `Assets/Audio/Lab08Mixer.mixer` |
| Sound clips | `Assets/Audio/Clips/` |
| Runtime code | `Assets/Scripts/` |

---

## 2. Game loading flow

`LevelLoader` lives on the menu canvas and owns the transition from menus to gameplay.

```text
Main Menu / Level Select
        ↓  Play / Start Match  →  LevelLoader.LoadGameplay()
LoadingPanel (inactive by default)
        ↓  LoadSceneAsync("Gameplay")
Real progress: op.progress / 0.9  →  Slider + "NN%"
        ↓  100%
Gameplay scene activates
```

- The scene is loaded with `SceneManager.LoadSceneAsync(...)`.
- `allowSceneActivation` stays **false** while loading, so the player cannot enter gameplay early.
- Unity reports loading progress up to `0.9`; the bar and percentage divide by `0.9`, so they
  genuinely reach **100%** instead of stopping at 90%.
- The panel stays up for a minimum display time so it does not flash on a fast load, then holds
  briefly at 100% before activation.
- A 30-second timeout fails safely with a Console error instead of hanging forever.
- Double-clicks are ignored while a load is in progress.
- Before loading, the loader resets `Time.timeScale = 1f` and `AudioListener.pause = false`, so a
  restart from Pause never leaves the next scene frozen or silent.

---

## 3. Audio mixer groups and routing

There is exactly **one** Audio Mixer: `Lab08Mixer`.

```text
Master
 ├── Music
 ├── SFX
 └── UI
```

- `Master` is the parent group; `Music`, `SFX` and `UI` are its children.
- Every `AudioSource` in the project is routed to the correct group.
- The four exposed parameters are named:

| Group | Exposed parameter |
|---|---|
| Master | `MasterVolume` |
| Music | `MusicVolume` |
| SFX | `SfxVolume` |
| UI | `UiVolume` |

### UI sounds

A single reusable `UIAudio` manager plays all interface sounds. There is **no `AudioSource` on
every button**. `ButtonFeedback` and `NavButton` call the shared manager, and the sounds are
routed through the dedicated **UI** group.

Covered interactions: button hover/selection, button press, back/cancel, panel open, panel close,
pause, resume, success and error.

### Gameplay sounds

`SfxPlayer` plays short effects with `PlayOneShot`, so rapid events overlap instead of cutting
each other off. Repeated effects use small pitch and volume variation. Gameplay effects include
pickup, damage, death, attack, jump, boost, win and lose. All are routed to the **SFX** group;
music is routed to **Music**.

### Pause policy

`PauseController` sets `Time.timeScale = 0` and `AudioListener.pause = true`. This is intentional:

- **Gameplay SFX stop** — `SfxPlayer` does not ignore listener pause.
- **UI sounds continue** — `UIAudio` sets `ignoreListenerPause = true`.
- **Music continues** — `MusicPlayer` sets `ignoreListenerPause = true`.
- UI tweens use unscaled time, so Pause and Results animate correctly while gameplay is frozen.

An optional paused mix snapshot may apply a low-pass filter, but it must not change the exposed
volume parameters.

---

## 4. Volume mapping and persistence

`AudioPrefs` converts each linear slider value into decibels using a logarithmic mapping:

```text
dB = 20 × log10(linear)
```

Assigning a linear `0–1` value directly to an Audio Mixer volume would sound wrong, so sliders
never do that. The value is clamped to a small positive minimum before the logarithm, which makes
mute safe — `log10(0)` is never evaluated.

- Mute is stored separately from volume, so unmuting restores the previous level.
- Volume and mute are written to `PlayerPrefs` immediately and reapplied on startup by
  `AudioBootstrap`.
- Settings therefore survive quitting and restarting the game.

---

## 5. Music transitions

- **Menus are silent.** No menu background clip is assigned and no `SceneMusic` exists there.
- Entering **Gameplay** starts the looping `music.ogg` immediately. `SceneMusic` plus
  `MusicCues.Start()` request the track on load, so music is audible before the match's first
  `GameStarted` event.
- Reaching **Results** crossfades from the gameplay loop to a one-shot stinger:
  `win.ogg` on victory, `lose.ogg` on defeat. `MusicPlayer.CrossfadeTo(clip, false)` makes them
  play once rather than looping.
- The demonstrated state change is **menu → gameplay → results**. Only `music.ogg` loops; the
  stingers do not repeat, and `ResultFlowListener` ignores repeated outcome events.

---

## 6. C# events: publishers and subscribers

Core gameplay systems **raise events** instead of calling UI, audio and camera code directly.

### Publishers

| Publisher | Event | Meaning |
|---|---|---|
| `Health` | `Damaged(int current, int max)` | Damage landed |
| `Health` | `Healed(int current, int max)` | Health restored |
| `Health` | `Died()` | Health reached zero |
| `GameEvents` | `CoinCollected(int value)` | Pickup collected |
| `GameEvents` | `GameStarted()` | Match began |
| `GameEvents` | `GameWon()` / `GameLost()` | Match outcome |

The required code-driven system uses C# events:

```csharp
public event Action<int, int> Damaged;

void OnEnable()  { health.Damaged += OnChanged; }
void OnDisable() { health.Damaged -= OnChanged; }

Damaged?.Invoke(current, max);
```

Subscribers use `+=` in `OnEnable` and `-=` in `OnDisable`; publishers invoke with `?.Invoke(...)`.
`Health.onDied` is the optional **UnityEvent** kept for Inspector-authored designer wiring only.

### Independent subscribers

| Listener | Responds to | Effect |
|---|---|---|
| `HealthBarListener` | `Damaged`, `Healed` | Eases the health bar and updates `n / max` |
| `HurtSound` | `Damaged`, `Died` | One-shot hurt / death SFX |
| `HitSparks` | `Damaged` | Particle burst at the player |
| `ShakeOnHit` | `Damaged` | Camera shake trauma |
| `HitFlash` | `PlayerDamaged` | Red screen flash |
| `CoinCollector` | `CoinCollected` | Updates score and wallet |
| `MusicCues` | `GameStarted`, `GameWon`, `GameLost` | Music / crossfade changes |
| `ResultFlowListener` | `GameWon`, `GameLost` | Shows Results with success/error cue |

---

## 7. Integration and quality checks

- All ten Lab 07 screens remain: Title, Main Menu, Mode Select, Level Select, Loadout, Shop,
  Settings, Pause, Results and Credits.
- Screen history/back navigation, Push/Fade/Modal transitions and the in-house tween helpers
  still work. Tweens use unscaled time where UI must keep animating while paused.
- The responsive layout is unchanged: `MenuCanvas`, 1080 × 1920 reference, `Scale With Screen
  Size`, TextMeshPro, and a `SafeArea` component on each screen's `Safe` child so nothing sits
  under a phone notch.

```text
launch → menus → loading (bar reaches 100%) → gameplay
       → pause/settings → resume → result → replay or main menu
```

- The Console must stay free of errors.
- Pause opens with Esc / Android Back. The Results panel provides **Next Map** and **Home**.
- Win/lose stingers play once; rapid pickups overlap through `SfxPlayer`.

---

## 8. Asset credits and licences

All third-party audio used in this project was supplied by **Kenney** and released under
**CC0 1.0 Universal** (public domain dedication):

- Creator: Kenney — <https://kenney.nl/assets>
- Licence: CC0 1.0 — <https://creativecommons.org/publicdomain/zero/1.0/>

One row per unique source file (a reused file is credited once):

| File | Used for | Creator | Licence | Source |
|---|---|---|---|---|
| `back_001.ogg` | UI back / cancel | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `click.ogg` | UI button press / selection | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `close_001.ogg` | UI panel close | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `error_008.ogg` | UI error feedback | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `hit.ogg` | Gameplay damage | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `hum1.ogg` | Credited only — not played; Menus are silent | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `hum2.ogg` | UI pause | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `hover.ogg` | UI hover / selection | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `lose.ogg` | Gameplay death and defeat stinger | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `maximize_001.ogg` | UI resume and gameplay jump | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `music.ogg` | Looping gameplay music | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `open_001.ogg` | UI panel open and gameplay boost | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `pickup.ogg` | Gameplay pickup and heal | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `pluck_002.ogg` | Gameplay attack | Kenney | CC0 1.0 | https://kenney.nl/assets |
| `win.ogg` | UI success and victory stinger | Kenney | CC0 1.0 | https://kenney.nl/assets |

