using System.Collections.Generic;
using UnityEngine;

public class SequenceDialogController : MonoBehaviour
{
    [Header("Sequence Dialog Data")]
    [SerializeField] private TextAsset dialogJsonFile;

    private DialogDatabase dialogDatabase;
    private readonly Dictionary<string, int> sequenceProgress = new Dictionary<string, int>();

    private void Awake()
    {
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

    public List<DialogLine> GetDialogLines(string characterId)
    {
        DialogEntry dialog = GetDialogForCharacter(characterId);
        return dialog != null ? dialog.lines : null;
    }

    public void ResetSequenceProgress(string characterId)
    {
        if (sequenceProgress.ContainsKey(characterId))
        {
            sequenceProgress[characterId] = 0;
        }
    }
}
