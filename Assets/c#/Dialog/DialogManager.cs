using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [Header("Dialog Data")]
    [SerializeField] private TextAsset dialogJsonFile;

    [Header("External Systems")]
    [SerializeField] private DaySystem daySystem;
    [SerializeField] private ActionPointSystem actionPointSystem;

    private DialogDatabase dialogDatabase;
    private readonly Dictionary<string, int> sequenceProgress = new Dictionary<string, int>();

    private void Awake()
    {
        LoadDialogData();
    }

    public void LoadDialogData()
    {
        if (dialogJsonFile == null)
        {
            Debug.LogError("Dialog json file is not assigned.", this);
            return;
        }

        dialogDatabase = JsonUtility.FromJson<DialogDatabase>(dialogJsonFile.text);

        if (dialogDatabase == null || dialogDatabase.characters == null)
        {
            Debug.LogError("Failed to parse dialog json data.", this);
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

    public DialogEntry GetDialogForCharacter(string characterId, int currentActionPoints = 0)
    {
        CharacterDialogConfig character = GetCharacterConfig(characterId);

        if (character == null || character.dialogs == null || character.dialogs.Count == 0)
        {
            Debug.LogWarning($"No dialog found for characterId: {characterId}", this);
            return null;
        }

        switch (character.mode)
        {
            case "random":
                return GetRandomDialog(character);
            case "actionPoint":
                return GetActionPointDialog(character, currentActionPoints);
            case "sequence":
                return GetSequenceDialog(character);
            default:
                Debug.LogWarning($"Unsupported dialog mode: {character.mode}", this);
                return null;
        }
    }

    public DialogEntry GetDialogForCharacter(string characterId)
    {
        int currentActionPoints = actionPointSystem != null ? actionPointSystem.CurrentActionPoints : 0;
        return GetDialogForCharacter(characterId, currentActionPoints);
    }

    public List<DialogLine> GetDialogLines(string characterId, int currentActionPoints = 0)
    {
        DialogEntry dialog = GetDialogForCharacter(characterId, currentActionPoints);
        return dialog != null ? dialog.lines : null;
    }

    public List<DialogLine> GetDialogLines(string characterId)
    {
        DialogEntry dialog = GetDialogForCharacter(characterId);
        return dialog != null ? dialog.lines : null;
    }

    public void SetDaySystem(DaySystem system)
    {
        daySystem = system;
    }

    public void SetActionPointSystem(ActionPointSystem system)
    {
        actionPointSystem = system;
    }

    public int GetCurrentDay()
    {
        return daySystem != null ? daySystem.CurrentDay : 0;
    }

    public int GetCurrentActionPoints()
    {
        return actionPointSystem != null ? actionPointSystem.CurrentActionPoints : 0;
    }

    public void ResetSequenceProgress(string characterId)
    {
        if (sequenceProgress.ContainsKey(characterId))
        {
            sequenceProgress[characterId] = 0;
        }
    }

    private DialogEntry GetRandomDialog(CharacterDialogConfig character)
    {
        int index = Random.Range(0, character.dialogs.Count);
        return character.dialogs[index];
    }

    private DialogEntry GetActionPointDialog(CharacterDialogConfig character, int currentActionPoints)
    {
        foreach (DialogEntry dialog in character.dialogs)
        {
            if (currentActionPoints >= dialog.actionPointMin && currentActionPoints <= dialog.actionPointMax)
            {
                return dialog;
            }
        }

        Debug.LogWarning(
            $"No action point dialog matched for {character.characterId} with action points: {currentActionPoints}",
            this);
        return null;
    }

    private DialogEntry GetSequenceDialog(CharacterDialogConfig character)
    {
        if (!sequenceProgress.ContainsKey(character.characterId))
        {
            sequenceProgress[character.characterId] = 0;
        }

        int index = sequenceProgress[character.characterId];
        index = Mathf.Clamp(index, 0, character.dialogs.Count - 1);

        DialogEntry dialog = character.dialogs[index];

        if (sequenceProgress[character.characterId] < character.dialogs.Count - 1)
        {
            sequenceProgress[character.characterId]++;
        }

        return dialog;
    }
}
