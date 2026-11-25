using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[Serializable]
public class AudioCreditEntry
{
    public string title;     // e.g. "Coin pickup"
    public string author;    // e.g. "Kenney"
    public string license;   // e.g. "CC0 1.0"
    public string source;    // URL or "Self-made"
}

/// <summary>
/// Fills a TMP_Text with every third-party audio asset, its author, licence and source.
/// Put it on a credits screen (a normal UIScreen) and fill the list in the Inspector.
/// </summary>
public class AudioCredits : MonoBehaviour
{
    [SerializeField] TMP_Text target;
    [SerializeField] List<AudioCreditEntry> entries = new List<AudioCreditEntry>();

    void OnEnable() => Build();

    void Build()
    {
        if (target == null) return;

        var sb = new StringBuilder();
        foreach (AudioCreditEntry e in entries)
        {
            sb.Append("<b>").Append(e.title).Append("</b>");
            if (!string.IsNullOrEmpty(e.author)) sb.Append(" - ").Append(e.author);
            sb.Append('\n');
            if (!string.IsNullOrEmpty(e.license)) sb.Append("Licence: ").Append(e.license).Append('\n');
            if (!string.IsNullOrEmpty(e.source)) sb.Append("Source: ").Append(e.source).Append('\n');
            sb.Append('\n');
        }
        target.SetText(sb);
    }
}
