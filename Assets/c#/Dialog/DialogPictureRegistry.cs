using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogPictureEntry
{
    public string characterId;
    public string pictureId;
    public GameObject target;
}

public class DialogPictureRequest
{
    public string characterId;
    public string pictureId;
}

public class DialogPictureRegistry : MonoBehaviour
{
    private const string DefaultPictureId = "0";

    [Header("Sequence Pictures")]
    [SerializeField] private List<DialogPictureEntry> sequencePictures = new List<DialogPictureEntry>();

    [Header("Action Point Pictures")]
    [SerializeField] private List<DialogPictureEntry> actionPointPictures = new List<DialogPictureEntry>();

    [Header("Random Pictures")]
    [SerializeField] private List<DialogPictureEntry> randomPictures = new List<DialogPictureEntry>();

    private void Awake()
    {
        BackfillActionPointPictures();
        ActivateAllDefaultPictures();
    }

    public bool ShowPicture(DialogTriggerMode triggerMode, string pictureId)
    {
        pictureId = NormalizePictureId(pictureId);

        if (string.IsNullOrEmpty(pictureId))
        {
            return false;
        }

        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return false;
        }

        DialogPictureEntry targetEntry = null;

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null)
            {
                continue;
            }

            if (NormalizePictureId(entry.pictureId) == pictureId)
            {
                targetEntry = entry;
                break;
            }
        }

        if (targetEntry?.target == null)
        {
            return false;
        }

        ApplyPictureEntries(triggerMode, new[] { targetEntry });
        NormalizePictureGroups(triggerMode);
        return true;
    }

    public bool ShowPictures(DialogTriggerMode triggerMode, IEnumerable<string> pictureIds)
    {
        if (pictureIds == null)
        {
            return false;
        }

        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return false;
        }

        HashSet<string> normalizedPictureIds = new HashSet<string>();

        foreach (string pictureId in pictureIds)
        {
            string normalizedPictureId = NormalizePictureId(pictureId);

            if (!string.IsNullOrEmpty(normalizedPictureId))
            {
                normalizedPictureIds.Add(normalizedPictureId);
            }
        }

        if (normalizedPictureIds.Count == 0)
        {
            return false;
        }

        List<DialogPictureEntry> targetEntries = new List<DialogPictureEntry>();

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null)
            {
                continue;
            }

            if (normalizedPictureIds.Contains(NormalizePictureId(entry.pictureId)))
            {
                targetEntries.Add(entry);
            }
        }

        if (targetEntries.Count == 0)
        {
            return false;
        }

        ApplyPictureEntries(triggerMode, targetEntries);
        NormalizePictureGroups(triggerMode);

        return true;
    }

    public bool ShowPictures(DialogTriggerMode triggerMode, IEnumerable<DialogPictureRequest> pictureRequests)
    {
        if (pictureRequests == null)
        {
            return false;
        }

        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return false;
        }

        List<DialogPictureRequest> normalizedRequests = new List<DialogPictureRequest>();

        foreach (DialogPictureRequest request in pictureRequests)
        {
            if (request == null)
            {
                continue;
            }

            string pictureId = NormalizePictureId(request.pictureId);

            if (string.IsNullOrEmpty(pictureId))
            {
                continue;
            }

            normalizedRequests.Add(new DialogPictureRequest
            {
                characterId = NormalizeCharacterId(request.characterId),
                pictureId = pictureId
            });
        }

        if (normalizedRequests.Count == 0)
        {
            return false;
        }

        List<DialogPictureEntry> targetEntries = new List<DialogPictureEntry>();

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null)
            {
                continue;
            }

            foreach (DialogPictureRequest request in normalizedRequests)
            {
                if (DoesEntryMatchRequest(entry, request))
                {
                    targetEntries.Add(entry);
                    break;
                }
            }
        }

        if (targetEntries.Count == 0)
        {
            return false;
        }

        ApplyPictureEntriesForRequests(triggerMode, targetEntries, normalizedRequests);
        NormalizePictureGroups(triggerMode);

        return true;
    }

    public bool ActivateDefaultPicture(DialogTriggerMode triggerMode)
    {
        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return false;
        }

        List<DialogPictureEntry> defaultEntries = new List<DialogPictureEntry>();

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target != null && NormalizePictureId(entry.pictureId) == DefaultPictureId)
            {
                defaultEntries.Add(entry);
            }
        }

        if (defaultEntries.Count == 0)
        {
            return false;
        }

        ApplyPictureEntries(triggerMode, defaultEntries);
        NormalizePictureGroups(triggerMode);
        return true;
    }

    public void ActivateAllDefaultPictures()
    {
        BackfillActionPointPictures();
        ActivateDefaultPicture(DialogTriggerMode.ActionPoint);
        NormalizePictureGroups(DialogTriggerMode.ActionPoint);
    }

    public void HidePictures(DialogTriggerMode triggerMode)
    {
        // Pictures should never be fully cleared during normal play; they only swap
        // when a new line asks for a different picture.
    }

    public void HideAllPictures()
    {
        // Intentionally left blank for the same reason as HidePictures.
    }

    private void ApplyPictureEntries(
        DialogTriggerMode triggerMode,
        IEnumerable<DialogPictureEntry> activeEntries)
    {
        HashSet<GameObject> activeTargetSet = new HashSet<GameObject>();
        HashSet<string> updatedCharacterIds = new HashSet<string>();
        bool updatesAllCharacters = false;

        foreach (DialogPictureEntry entry in activeEntries)
        {
            if (entry?.target == null)
            {
                continue;
            }

            activeTargetSet.Add(entry.target);

            string characterId = NormalizeCharacterId(entry.characterId);

            if (string.IsNullOrEmpty(characterId))
            {
                updatesAllCharacters = true;
                continue;
            }

            updatedCharacterIds.Add(characterId);
        }

        ApplyPictureVisibilityForCharacterGroups(triggerMode, activeTargetSet, updatedCharacterIds, updatesAllCharacters);
    }

    private void BackfillActionPointPictures()
    {
        BackfillActionPointPicturesForCharacter("mate", sequencePictures);
        BackfillActionPointPicturesForCharacter("windows", randomPictures);
    }

    private void BackfillActionPointPicturesForCharacter(
        string characterId,
        IEnumerable<DialogPictureEntry> fallbackPictures)
    {
        if (string.IsNullOrEmpty(characterId) || fallbackPictures == null)
        {
            return;
        }

        string normalizedCharacterId = NormalizeCharacterId(characterId);

        foreach (DialogPictureEntry fallbackEntry in fallbackPictures)
        {
            if (fallbackEntry?.target == null)
            {
                continue;
            }

            string pictureId = NormalizePictureId(fallbackEntry.pictureId);

            if (string.IsNullOrEmpty(pictureId) ||
                HasActionPointPicture(normalizedCharacterId, pictureId))
            {
                continue;
            }

            actionPointPictures.Add(new DialogPictureEntry
            {
                characterId = characterId,
                pictureId = pictureId,
                target = fallbackEntry.target
            });
        }
    }

    private bool HasActionPointPicture(string normalizedCharacterId, string pictureId)
    {
        foreach (DialogPictureEntry entry in actionPointPictures)
        {
            if (entry?.target == null)
            {
                continue;
            }

            if (NormalizeCharacterId(entry.characterId) == normalizedCharacterId &&
                NormalizePictureId(entry.pictureId) == pictureId)
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyPictureEntriesForRequests(
        DialogTriggerMode triggerMode,
        IEnumerable<DialogPictureEntry> activeEntries,
        IEnumerable<DialogPictureRequest> requests)
    {
        HashSet<GameObject> activeTargetSet = new HashSet<GameObject>();
        HashSet<string> matchedCharacterIds = new HashSet<string>();
        HashSet<string> requestedCharacterIds = new HashSet<string>();
        bool updatesAllCharacters = false;

        foreach (DialogPictureEntry entry in activeEntries)
        {
            if (entry?.target == null)
            {
                continue;
            }

            activeTargetSet.Add(entry.target);

            string characterId = NormalizeCharacterId(entry.characterId);

            if (string.IsNullOrEmpty(characterId))
            {
                updatesAllCharacters = true;
                continue;
            }

            matchedCharacterIds.Add(characterId);
        }

        foreach (DialogPictureRequest request in requests)
        {
            if (request == null || string.IsNullOrEmpty(request.pictureId))
            {
                continue;
            }

            if (string.IsNullOrEmpty(request.characterId))
            {
                updatesAllCharacters = true;
                continue;
            }

            requestedCharacterIds.Add(request.characterId);
        }

        if (!updatesAllCharacters)
        {
            matchedCharacterIds.IntersectWith(requestedCharacterIds);
        }

        ApplyPictureVisibilityForCharacterGroups(triggerMode, activeTargetSet, matchedCharacterIds, updatesAllCharacters);
    }

    private void ApplyPictureVisibilityForCharacterGroups(
        DialogTriggerMode triggerMode,
        HashSet<GameObject> activeTargetSet,
        HashSet<string> updatedCharacterIds,
        bool updatesAllCharacters)
    {
        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return;
        }

        foreach (GameObject target in activeTargetSet)
        {
            SetActiveIfChanged(target, true);
        }

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null)
            {
                continue;
            }

            string entryCharacterId = NormalizeCharacterId(entry.characterId);

            if (!updatesAllCharacters &&
                string.IsNullOrEmpty(entryCharacterId))
            {
                continue;
            }

            if (!updatesAllCharacters &&
                !updatedCharacterIds.Contains(entryCharacterId))
            {
                continue;
            }

            if (!activeTargetSet.Contains(entry.target))
            {
                SetActiveIfChanged(entry.target, false);
            }
        }

        NormalizePictureGroups(triggerMode, activeTargetSet);
    }

    private void SetActiveIfChanged(GameObject target, bool isActive)
    {
        if (target.activeSelf != isActive)
        {
            target.SetActive(isActive);
        }
    }

    private void NormalizePictureGroups(DialogTriggerMode triggerMode, HashSet<GameObject> preferredTargetSet = null)
    {
        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return;
        }

        HashSet<string> characterIds = new HashSet<string>();

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null)
            {
                continue;
            }

            characterIds.Add(NormalizeCharacterId(entry.characterId));
        }

        foreach (string characterId in characterIds)
        {
            DialogPictureEntry activeEntry = GetActivePictureForCharacter(pictures, characterId, preferredTargetSet);
            DialogPictureEntry fallbackEntry = activeEntry ?? GetFallbackPictureForCharacter(pictures, characterId);

            if (fallbackEntry?.target != null)
            {
                SetActiveIfChanged(fallbackEntry.target, true);
            }

            CloseOtherPicturesForCharacter(pictures, characterId, fallbackEntry);
        }
    }

    private DialogPictureEntry GetActivePictureForCharacter(
        List<DialogPictureEntry> pictures,
        string characterId,
        HashSet<GameObject> preferredTargetSet)
    {
        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target != null &&
                NormalizeCharacterId(entry.characterId) == characterId &&
                preferredTargetSet != null &&
                preferredTargetSet.Contains(entry.target))
            {
                return entry;
            }
        }

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target != null &&
                NormalizeCharacterId(entry.characterId) == characterId &&
                entry.target.activeSelf)
            {
                return entry;
            }
        }

        return null;
    }

    private void CloseOtherPicturesForCharacter(
        List<DialogPictureEntry> pictures,
        string characterId,
        DialogPictureEntry activeEntry)
    {
        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null || NormalizeCharacterId(entry.characterId) != characterId)
            {
                continue;
            }

            if (entry != activeEntry)
            {
                SetActiveIfChanged(entry.target, false);
            }
        }
    }

    private DialogPictureEntry GetFallbackPictureForCharacter(List<DialogPictureEntry> pictures, string characterId)
    {
        DialogPictureEntry firstEntry = null;

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null || NormalizeCharacterId(entry.characterId) != characterId)
            {
                continue;
            }

            if (firstEntry == null)
            {
                firstEntry = entry;
            }

            if (NormalizePictureId(entry.pictureId) == DefaultPictureId)
            {
                return entry;
            }
        }

        return firstEntry;
    }

    private List<DialogPictureEntry> GetPictures(DialogTriggerMode triggerMode)
    {
        switch (triggerMode)
        {
            case DialogTriggerMode.Sequence:
                return sequencePictures;
            case DialogTriggerMode.ActionPoint:
                return actionPointPictures;
            case DialogTriggerMode.Random:
                return randomPictures;
            default:
                return null;
        }
    }

    private string NormalizePictureId(string pictureId)
    {
        return string.IsNullOrEmpty(pictureId) ? string.Empty : pictureId.Trim();
    }

    private string NormalizeCharacterId(string characterId)
    {
        return string.IsNullOrEmpty(characterId) ? string.Empty : characterId.Trim().ToLowerInvariant();
    }

    private bool DoesEntryMatchRequest(DialogPictureEntry entry, DialogPictureRequest request)
    {
        if (NormalizePictureId(entry.pictureId) != request.pictureId)
        {
            return false;
        }

        string entryCharacterId = NormalizeCharacterId(entry.characterId);

        return string.IsNullOrEmpty(entryCharacterId) ||
               string.IsNullOrEmpty(request.characterId) ||
               entryCharacterId == request.characterId;
    }
}
