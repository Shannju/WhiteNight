using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpeakerColorEntry
{
    public string characterId;
    public Color color = Color.white;
}

public class SpeakerColorPalette : MonoBehaviour
{
    [SerializeField] private List<SpeakerColorEntry> speakerColors = new List<SpeakerColorEntry>();

    public Color GetColor(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return Color.white;
        }

        foreach (SpeakerColorEntry entry in speakerColors)
        {
            if (entry != null && entry.characterId == characterId)
            {
                return entry.color;
            }
        }

        return Color.white;
    }

    public bool TryGetColor(string characterId, out Color color)
    {
        if (!string.IsNullOrEmpty(characterId))
        {
            foreach (SpeakerColorEntry entry in speakerColors)
            {
                if (entry != null && entry.characterId == characterId)
                {
                    color = entry.color;
                    return true;
                }
            }
        }

        color = Color.white;
        return false;
    }
}
