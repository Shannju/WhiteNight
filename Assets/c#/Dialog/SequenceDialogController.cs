using System.Collections.Generic;
using UnityEngine;

public class SequenceDialogController : MonoBehaviour
{
    [Header("Sequence Dialog Data")]
    [SerializeField] private TextAsset dialogJsonFile;

    [Header("External Systems")]
    [SerializeField] private DaySystem daySystem;

    private DialogDatabase dialogDatabase;
    private readonly Dictionary<string, int> sequenceProgress = new Dictionary<string, int>();

    private void Awake()
    {
        if (daySystem == null)
        {
            daySystem = FindObjectOfType<DaySystem>();
        }

        LoadDialogData();
    }

    public void LoadDialogData()
    {
        sequenceProgress.Clear();

        if (dialogJsonFile == null)
        {
            dialogDatabase = null;
            return;
        }

        dialogDatabase = JsonUtility.FromJson<DialogDatabase>(dialogJsonFile.text);

        if (dialogDatabase == null || dialogDatabase.characters == null)
        {
            Debug.LogError("Failed to parse sequence dialog json data.", this);
        }
    }

    public CharacterDialogConfig GetCharacterConfig(string characterId)
    {
        if (dialogDatabase == null || dialogDatabase.characters == null)
        {
            return null;
        }

        return dialogDatabase.characters.Find(character => character.characterId == characterId);
    }

    public DialogEntry GetDialogForCharacter(string characterId)
    {
        CharacterDialogConfig character = GetCharacterConfig(characterId);

        if (character == null || character.dialogs == null || character.dialogs.Count == 0)
        {
            Debug.LogWarning($"No sequence dialog found for characterId: {characterId}", this);
            return null;
        }

        int currentDay = GetCurrentDay();
        List<DialogEntry> currentDayDialogs = GetDialogsForDay(character, currentDay);

        if (currentDayDialogs.Count == 0)
        {
            Debug.LogWarning($"No sequence dialog found for characterId: {characterId} on day: {currentDay}", this);
            return null;
        }

        string progressKey = GetProgressKey(character.characterId, currentDay);

        if (!sequenceProgress.ContainsKey(progressKey))
        {
            sequenceProgress[progressKey] = 0;
        }

        int index = sequenceProgress[progressKey];

        if (index < 0 || index >= currentDayDialogs.Count)
        {
            return null;
        }

        DialogEntry dialog = currentDayDialogs[index];
        PrepareDialogForPlayback(dialog);

        sequenceProgress[progressKey]++;

        return dialog;
    }

    public List<DialogLine> GetDialogLines(string characterId)
    {
        DialogEntry dialog = GetDialogForCharacter(characterId);
        return dialog != null ? dialog.lines : null;
    }

    public void ResetSequenceProgress(string characterId)
    {
        int currentDay = GetCurrentDay();
        string progressKey = GetProgressKey(characterId, currentDay);

        if (sequenceProgress.ContainsKey(progressKey))
        {
            sequenceProgress[progressKey] = 0;
        }
    }

    public void ResetSequenceProgress(string characterId, int day)
    {
        string progressKey = GetProgressKey(characterId, day);

        if (sequenceProgress.ContainsKey(progressKey))
        {
            sequenceProgress[progressKey] = 0;
        }
    }

    public void SetDaySystem(DaySystem system)
    {
        daySystem = system;
    }

    public int GetCurrentDay()
    {
        return daySystem != null ? daySystem.CurrentDay : 1;
    }

    private List<DialogEntry> GetDialogsForDay(CharacterDialogConfig character, int day)
    {
        List<DialogEntry> dialogs = new List<DialogEntry>();

        foreach (DialogEntry dialog in character.dialogs)
        {
            if (dialog.day == day)
            {
                dialogs.Add(dialog);
            }
        }

        dialogs.Sort((left, right) => left.sequenceOrder.CompareTo(right.sequenceOrder));
        return dialogs;
    }

    private void PrepareDialogForPlayback(DialogEntry dialog)
    {
        if (dialog.lines == null)
        {
            return;
        }

        dialog.lines.Sort((left, right) => left.triggerOrder.CompareTo(right.triggerOrder));
    }

    private string GetProgressKey(string characterId, int day)
    {
        return $"{characterId}:{day}";
    }
}
