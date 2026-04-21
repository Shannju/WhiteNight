using System.Collections.Generic;
using UnityEngine;

public class ActionPointDialogController : MonoBehaviour
{
    [Header("Action Point Dialog Data")]
    [SerializeField] private TextAsset dialogJsonFile;

    private DialogDatabase dialogDatabase;

    private void Awake()
    {
        LoadDialogData();
    }

    public void LoadDialogData()
    {
        if (dialogJsonFile == null)
        {
            dialogDatabase = null;
            return;
        }

        dialogDatabase = JsonUtility.FromJson<DialogDatabase>(dialogJsonFile.text);

        if (dialogDatabase == null || dialogDatabase.characters == null)
        {
            Debug.LogError("Failed to parse action point dialog json data.", this);
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

    public DialogEntry GetDialogForCharacter(string characterId, int currentActionPoints)
    {
        return GetDialogForCharacterByRange(
            characterId,
            currentActionPoints,
            $"No action point dialog matched for {{0}} with action points: {currentActionPoints}");
    }

    public DialogEntry GetDialogForCharacterBySpentActionPoints(string characterId, int spentActionPoints)
    {
        return GetDialogForCharacterByRange(
            characterId,
            spentActionPoints,
            $"No action point dialog matched for {{0}} with spent action points: {spentActionPoints}");
    }

    public List<DialogLine> GetDialogLines(string characterId, int currentActionPoints)
    {
        DialogEntry dialog = GetDialogForCharacter(characterId, currentActionPoints);
        return dialog != null ? dialog.lines : null;
    }

    public List<DialogLine> GetDialogLinesBySpentActionPoints(string characterId, int spentActionPoints)
    {
        DialogEntry dialog = GetDialogForCharacterBySpentActionPoints(characterId, spentActionPoints);
        return dialog != null ? dialog.lines : null;
    }

    private DialogEntry GetDialogForCharacterByRange(string characterId, int matchValue, string noMatchMessageFormat)
    {
        CharacterDialogConfig character = GetCharacterConfig(characterId);

        if (character == null || character.dialogs == null || character.dialogs.Count == 0)
        {
            Debug.LogWarning($"No action point dialog found for characterId: {characterId}", this);
            return null;
        }

        foreach (DialogEntry dialog in character.dialogs)
        {
            if (matchValue >= dialog.actionPointMin && matchValue <= dialog.actionPointMax)
            {
                PrepareDialogForPlayback(dialog);
                return dialog;
            }
        }

        Debug.LogWarning(string.Format(noMatchMessageFormat, character.characterId), this);
        return null;
    }

    private void PrepareDialogForPlayback(DialogEntry dialog)
    {
        if (dialog?.lines == null)
        {
            return;
        }

        dialog.lines.Sort((left, right) => left.triggerOrder.CompareTo(right.triggerOrder));
    }
}
