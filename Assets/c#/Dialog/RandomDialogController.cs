using System.Collections.Generic;
using UnityEngine;

public class RandomDialogController : MonoBehaviour
{
    [Header("Random Dialog Data")]
    [SerializeField] private TextAsset dialogJsonFile;

    [Header("External Systems")]
    [SerializeField] private DaySystem daySystem;

    private DialogDatabase dialogDatabase;
    private readonly Dictionary<string, HashSet<string>> playedDialogIdsByDay = new Dictionary<string, HashSet<string>>();

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
        playedDialogIdsByDay.Clear();

        if (dialogJsonFile == null)
        {
            dialogDatabase = null;
            return;
        }

        dialogDatabase = JsonUtility.FromJson<DialogDatabase>(dialogJsonFile.text);

        if (dialogDatabase == null || dialogDatabase.characters == null)
        {
            Debug.LogError("Failed to parse random dialog json data.", this);
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
        return GetDialogForCharacter(characterId, true);
    }

    public DialogEntry PeekDialogForCharacter(string characterId)
    {
        return GetDialogForCharacter(characterId, false);
    }

    private DialogEntry GetDialogForCharacter(string characterId, bool markAsPlayed)
    {
        CharacterDialogConfig character = GetCharacterConfig(characterId);

        if (character == null || character.dialogs == null || character.dialogs.Count == 0)
        {
            Debug.LogWarning($"No random dialog found for characterId: {characterId}", this);
            return null;
        }

        int currentDay = GetCurrentDay();
        List<DialogEntry> availableDialogs = GetAvailableDialogs(character, currentDay);

        if (availableDialogs.Count == 0)
        {
            return null;
        }

        string progressKey = GetProgressKey(character.characterId, currentDay);

        if (!playedDialogIdsByDay.TryGetValue(progressKey, out HashSet<string> playedDialogIds))
        {
            playedDialogIds = new HashSet<string>();
            playedDialogIdsByDay[progressKey] = playedDialogIds;
        }

        List<DialogEntry> candidates = new List<DialogEntry>();

        foreach (DialogEntry dialog in availableDialogs)
        {
            if (!playedDialogIds.Contains(dialog.dialogId))
            {
                candidates.Add(dialog);
            }
        }

        if (candidates.Count == 0)
        {
            playedDialogIds.Clear();
            candidates.AddRange(availableDialogs);
        }

        DialogEntry selectedDialog = candidates[Random.Range(0, candidates.Count)];

        if (markAsPlayed)
        {
            playedDialogIds.Add(selectedDialog.dialogId);
        }

        PrepareDialogForPlayback(selectedDialog);
        return selectedDialog;
    }

    public DialogEntry GetDialogForCharacterByDialogId(string characterId, string dialogId, bool markAsPlayed = true)
    {
        if (string.IsNullOrEmpty(dialogId))
        {
            Debug.LogWarning("Random dialog id is empty.", this);
            return null;
        }

        CharacterDialogConfig character = GetCharacterConfig(characterId);

        if (character == null || character.dialogs == null || character.dialogs.Count == 0)
        {
            Debug.LogWarning($"No random dialog found for characterId: {characterId}", this);
            return null;
        }

        int currentDay = GetCurrentDay();
        List<DialogEntry> availableDialogs = GetAvailableDialogs(character, currentDay);
        DialogEntry selectedDialog = availableDialogs.Find(dialog => dialog.dialogId == dialogId);

        if (selectedDialog == null)
        {
            Debug.LogWarning($"No available random dialog matched dialogId: {dialogId}", this);
            return null;
        }

        if (markAsPlayed)
        {
            MarkDialogAsPlayed(character.characterId, currentDay, selectedDialog.dialogId);
        }

        PrepareDialogForPlayback(selectedDialog);
        return selectedDialog;
    }

    public List<DialogLine> GetDialogLines(string characterId)
    {
        DialogEntry dialog = GetDialogForCharacter(characterId);
        return dialog != null ? dialog.lines : null;
    }

    public void ResetRandomProgress(string characterId)
    {
        int currentDay = GetCurrentDay();
        string progressKey = GetProgressKey(characterId, currentDay);
        playedDialogIdsByDay.Remove(progressKey);
    }

    public void ResetRandomProgress(string characterId, int day)
    {
        string progressKey = GetProgressKey(characterId, day);
        playedDialogIdsByDay.Remove(progressKey);
    }

    public void SetDaySystem(DaySystem system)
    {
        daySystem = system;
    }

    public int GetCurrentDay()
    {
        return daySystem != null ? daySystem.CurrentDay : 1;
    }

    private List<DialogEntry> GetAvailableDialogs(CharacterDialogConfig character, int currentDay)
    {
        List<DialogEntry> dialogs = new List<DialogEntry>();

        foreach (DialogEntry dialog in character.dialogs)
        {
            if (dialog.day == 0 || dialog.day == currentDay)
            {
                dialogs.Add(dialog);
            }
        }

        return dialogs;
    }

    private void PrepareDialogForPlayback(DialogEntry dialog)
    {
        if (dialog?.lines == null)
        {
            return;
        }

        dialog.lines.Sort((left, right) => left.triggerOrder.CompareTo(right.triggerOrder));
    }

    private void MarkDialogAsPlayed(string characterId, int day, string dialogId)
    {
        string progressKey = GetProgressKey(characterId, day);

        if (!playedDialogIdsByDay.TryGetValue(progressKey, out HashSet<string> playedDialogIds))
        {
            playedDialogIds = new HashSet<string>();
            playedDialogIdsByDay[progressKey] = playedDialogIds;
        }

        playedDialogIds.Add(dialogId);
    }

    private string GetProgressKey(string characterId, int day)
    {
        return $"{characterId}:{day}";
    }
}
