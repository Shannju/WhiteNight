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
        CharacterDialogConfig character = GetCharacterConfig(characterId);

        if (character == null || character.dialogs == null || character.dialogs.Count == 0)
        {
            Debug.LogWarning($"No action point dialog found for characterId: {characterId}", this);
            return null;
        }

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

    public List<DialogLine> GetDialogLines(string characterId, int currentActionPoints)
    {
        DialogEntry dialog = GetDialogForCharacter(characterId, currentActionPoints);
        return dialog != null ? dialog.lines : null;
    }
}
