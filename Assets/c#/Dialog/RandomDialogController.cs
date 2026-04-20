using System.Collections.Generic;
using UnityEngine;

public class RandomDialogController : MonoBehaviour
{
    [Header("Random Dialog Data")]
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
        CharacterDialogConfig character = GetCharacterConfig(characterId);

        if (character == null || character.dialogs == null || character.dialogs.Count == 0)
        {
            Debug.LogWarning($"No random dialog found for characterId: {characterId}", this);
            return null;
        }

        int index = Random.Range(0, character.dialogs.Count);
        return character.dialogs[index];
    }

    public List<DialogLine> GetDialogLines(string characterId)
    {
        DialogEntry dialog = GetDialogForCharacter(characterId);
        return dialog != null ? dialog.lines : null;
    }
}
