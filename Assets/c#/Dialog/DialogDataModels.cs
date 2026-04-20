using System;
using System.Collections.Generic;

[Serializable]
public class DialogDatabase
{
    public List<CharacterDialogConfig> characters;
}

[Serializable]
public class CharacterDialogConfig
{
    public string characterId;
    public string characterName;
    public string mode;
    public List<DialogEntry> dialogs;
}

[Serializable]
public class DialogEntry
{
    public string dialogId;
    public int actionPointMin;
    public int actionPointMax;
    public List<DialogLine> lines;
}

[Serializable]
public class DialogLine
{
    public string speakerId;
    public string text;
}
