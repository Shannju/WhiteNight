using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DialogCsvJsonImporter
{
    private const string CsvFolderPath = "Assets/c#/Dialog/csv";

    static DialogCsvJsonImporter()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Tools/Dialog/Generate JSON From CSV")]
    public static void GenerateAllJson()
    {
        if (!Directory.Exists(CsvFolderPath))
        {
            Debug.LogWarning($"Dialog CSV folder was not found: {CsvFolderPath}");
            return;
        }

        string[] csvPaths = Directory.GetFiles(CsvFolderPath, "*.csv", SearchOption.TopDirectoryOnly);
        int generatedCount = 0;

        foreach (string csvPath in csvPaths)
        {
            if (GenerateJsonForCsv(csvPath))
            {
                generatedCount++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Generated dialog json files from {generatedCount} csv file(s).");
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            GenerateAllJson();
        }
    }

    private static bool GenerateJsonForCsv(string csvPath)
    {
        string csvText = File.ReadAllText(csvPath, Encoding.UTF8);
        List<Dictionary<string, string>> rows = ParseCsv(csvText);

        if (rows.Count == 0)
        {
            Debug.LogWarning($"Skipped empty csv file: {csvPath}");
            return false;
        }

        string fileName = Path.GetFileNameWithoutExtension(csvPath);
        if (!IsSupportedFile(fileName))
        {
            return false;
        }

        CharacterDialogConfig character = BuildCharacterConfig(fileName, rows);

        if (character == null)
        {
            Debug.LogWarning($"Failed to build dialog config from csv: {csvPath}");
            return false;
        }

        DialogDatabase database = new DialogDatabase
        {
            characters = new List<CharacterDialogConfig> { character }
        };

        string jsonPath = Path.ChangeExtension(csvPath, ".json");
        string jsonText = JsonUtility.ToJson(database, true) + Environment.NewLine;

        if (File.Exists(jsonPath))
        {
            string existingText = File.ReadAllText(jsonPath, Encoding.UTF8);

            if (existingText == jsonText)
            {
                return true;
            }
        }

        File.WriteAllText(jsonPath, jsonText, new UTF8Encoding(false));
        return true;
    }

    private static CharacterDialogConfig BuildCharacterConfig(string fileName, List<Dictionary<string, string>> rows)
    {
        string mode = GetValue(rows[0], "mode");

        if (!IsSupportedMode(mode))
        {
            return null;
        }

        string characterId = fileName;
        string characterName = GetTopLevelCharacterName(rows, characterId, fileName);

        CharacterDialogConfig character = new CharacterDialogConfig
        {
            characterId = characterId,
            characterName = characterName,
            mode = mode,
            dialogs = BuildDialogEntries(rows, mode)
        };

        return character;
    }

    private static string GetTopLevelCharacterName(
        List<Dictionary<string, string>> rows,
        string topLevelCharacterId,
        string fallbackName)
    {
        foreach (Dictionary<string, string> row in rows)
        {
            string rowCharacterId = GetValue(row, "characterId");
            string rowCharacterName = GetValue(row, "characterName");

            if (string.Equals(rowCharacterId, topLevelCharacterId, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(rowCharacterName))
            {
                return rowCharacterName;
            }
        }

        foreach (Dictionary<string, string> row in rows)
        {
            string rowCharacterName = GetValue(row, "characterName");

            if (!string.IsNullOrEmpty(rowCharacterName))
            {
                return rowCharacterName;
            }
        }

        return fallbackName;
    }

    private static List<DialogEntry> BuildDialogEntries(List<Dictionary<string, string>> rows, string mode)
    {
        List<DialogEntry> dialogs = new List<DialogEntry>();
        Dictionary<string, DialogEntry> dialogMap = new Dictionary<string, DialogEntry>();

        foreach (Dictionary<string, string> row in rows)
        {
            string dialogId = GetValue(row, "dialogID");

            if (string.IsNullOrEmpty(dialogId))
            {
                continue;
            }

            int day = ParseInt(GetValue(row, "day"));
            int sequenceOrder = ParseInt(GetValue(row, "sequenceOrder"));
            int triggerOrder = ParseInt(GetValue(row, "triggerOrder"));
            int actionPointOrder = ParseInt(GetValue(row, "actionpointOrder"));
            string dialogKey = BuildDialogKey(mode, dialogId, day, sequenceOrder, actionPointOrder);

            if (!dialogMap.TryGetValue(dialogKey, out DialogEntry dialog))
            {
                dialog = new DialogEntry
                {
                    dialogId = dialogId,
                    day = day,
                    sequenceOrder = sequenceOrder,
                    actionPointMin = actionPointOrder,
                    actionPointMax = actionPointOrder,
                    lines = new List<DialogLine>()
                };

                dialogMap.Add(dialogKey, dialog);
                dialogs.Add(dialog);
            }

            dialog.lines.Add(new DialogLine
            {
                speakerId = GetValue(row, "characterId"),
                characterName = GetValue(row, "characterName"),
                triggerOrder = triggerOrder,
                text = GetValue(row, "text"),
                pictureId = GetValue(row, "pictureId"),
                playerPicture = GetValue(row, "playerpicture")
            });
        }

        foreach (DialogEntry dialog in dialogs)
        {
            dialog.lines.Sort((left, right) => left.triggerOrder.CompareTo(right.triggerOrder));
        }

        if (string.Equals(mode, "sequence", StringComparison.OrdinalIgnoreCase))
        {
            dialogs.Sort((left, right) =>
            {
                int dayCompare = left.day.CompareTo(right.day);
                return dayCompare != 0 ? dayCompare : left.sequenceOrder.CompareTo(right.sequenceOrder);
            });
        }
        else if (string.Equals(mode, "actionpoint", StringComparison.OrdinalIgnoreCase))
        {
            dialogs.Sort((left, right) => left.actionPointMin.CompareTo(right.actionPointMin));
        }
        else
        {
            dialogs.Sort((left, right) => string.CompareOrdinal(left.dialogId, right.dialogId));
        }

        return dialogs;
    }

    private static bool IsSupportedFile(string fileName)
    {
        return string.Equals(fileName, "mate", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(fileName, "teacher", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSupportedMode(string mode)
    {
        return string.Equals(mode, "sequence", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(mode, "actionpoint", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildDialogKey(
        string mode,
        string dialogId,
        int day,
        int sequenceOrder,
        int actionPointOrder)
    {
        if (string.Equals(mode, "sequence", StringComparison.OrdinalIgnoreCase))
        {
            return $"{day}:{sequenceOrder}:{dialogId}";
        }

        if (string.Equals(mode, "actionpoint", StringComparison.OrdinalIgnoreCase))
        {
            return $"{day}:{actionPointOrder}:{dialogId}";
        }

        return dialogId;
    }

    private static List<Dictionary<string, string>> ParseCsv(string csvText)
    {
        List<List<string>> records = ParseCsvRecords(csvText);
        List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();

        if (records.Count <= 1)
        {
            return rows;
        }

        List<string> headers = records[0];

        for (int rowIndex = 1; rowIndex < records.Count; rowIndex++)
        {
            List<string> record = records[rowIndex];
            bool hasValue = false;
            Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int columnIndex = 0; columnIndex < headers.Count; columnIndex++)
            {
                string header = headers[columnIndex];
                string value = columnIndex < record.Count ? record[columnIndex] : string.Empty;

                if (!string.IsNullOrEmpty(value))
                {
                    hasValue = true;
                }

                row[header] = value;
            }

            if (hasValue)
            {
                rows.Add(row);
            }
        }

        return rows;
    }

    private static List<List<string>> ParseCsvRecords(string csvText)
    {
        List<List<string>> records = new List<List<string>>();
        List<string> currentRecord = new List<string>();
        StringBuilder currentField = new StringBuilder();
        bool inQuotes = false;

        for (int index = 0; index < csvText.Length; index++)
        {
            char currentChar = csvText[index];

            if (currentChar == '"')
            {
                if (inQuotes && index + 1 < csvText.Length && csvText[index + 1] == '"')
                {
                    currentField.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (currentChar == ',' && !inQuotes)
            {
                currentRecord.Add(CleanField(currentField.ToString()));
                currentField.Clear();
                continue;
            }

            if ((currentChar == '\n' || currentChar == '\r') && !inQuotes)
            {
                if (currentChar == '\r' && index + 1 < csvText.Length && csvText[index + 1] == '\n')
                {
                    index++;
                }

                currentRecord.Add(CleanField(currentField.ToString()));
                currentField.Clear();

                if (currentRecord.Count > 1 || !string.IsNullOrEmpty(currentRecord[0]))
                {
                    records.Add(currentRecord);
                }

                currentRecord = new List<string>();
                continue;
            }

            currentField.Append(currentChar);
        }

        if (currentField.Length > 0 || currentRecord.Count > 0)
        {
            currentRecord.Add(CleanField(currentField.ToString()));
            if (currentRecord.Count > 1 || !string.IsNullOrEmpty(currentRecord[0]))
            {
                records.Add(currentRecord);
            }
        }

        return records;
    }

    private static string CleanField(string value)
    {
        return value.Trim().TrimStart('\uFEFF');
    }

    private static string GetValue(Dictionary<string, string> row, string key)
    {
        return row.TryGetValue(key, out string value) ? value : string.Empty;
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value, out int parsedValue) ? parsedValue : 0;
    }
}
