using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Lab08EditorUtil;

/// <summary>
/// Single place where the user-supplied audio files are mapped to Lab 08 cues.
/// The old builders generated procedural WAVs; these now load the real clips added
/// under Assets/Audio/Clips. Reused files are intentional where a cue has no dedicated clip.
/// </summary>
public static class Lab08AudioMap
{
    public static readonly Dictionary<string, string> Files = new Dictionary<string, string>
    {
        // UI - Task 02
        { "UI_Hover",   "hover.ogg" },
        { "UI_Click",   "click.ogg" },
        { "UI_Back",    "back_001.ogg" },
        { "UI_Open",    "open_001.ogg" },
        { "UI_Close",   "close_001.ogg" },
        { "UI_Pause",   "hum2.ogg" },
        { "UI_Resume",  "maximize_001.ogg" },
        { "UI_Success", "win.ogg" },
        { "UI_Error",   "error_008.ogg" },

        // Music - Task 04
        { "Music_Menu",     "hum1.ogg" },
        { "Music_Gameplay", "music.ogg" },
        { "Music_Win",      "win.ogg" },
        { "Music_Lose",     "lose.ogg" },

        // Gameplay SFX - Tasks 05, 06, 07, 09, 10
        { "Sfx_Attack", "pluck_002.ogg" },
        { "Sfx_Jump",   "maximize_001.ogg" },
        { "Sfx_Boost",  "open_001.ogg" },
        { "Sfx_Heal",   "pickup.ogg" },
        { "Sfx_Pickup", "pickup.ogg" },
        { "Sfx_Hurt",   "hit.ogg" },
        { "Sfx_Death",  "lose.ogg" },
        { "Sfx_Win",    "win.ogg" },
        { "Sfx_Lose",   "lose.ogg" }
    };

    /// <summary>Loads the real clip for a cue and warns clearly if it is missing.</summary>
    public static AudioClip Load(string cue)
    {
        string file;
        if (!Files.TryGetValue(cue, out file))
        {
            Debug.LogWarning("[Lab08] No audio file mapped for cue: " + cue);
            return null;
        }

        string path = ClipRoot + "/" + file;
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogWarning("[Lab08] Missing audio clip '" + path + "' for cue '" + cue + "'.");
        return clip;
    }

    /// <summary>One credit per unique source file, not one per reused cue.</summary>
    public static IEnumerable<Credit> Credits()
    {
        return Files.Values
            .Distinct()
            .OrderBy(f => f)
            .Select(f => new Credit(
                Path.GetFileNameWithoutExtension(f),
                "Kenney",
                "CC0 1.0",
                "https://kenney.nl/assets"));
    }
}
