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

        HideAllPictures();
        targetEntry.target.SetActive(true);
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

        List<GameObject> targets = new List<GameObject>();

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null)
            {
                continue;
            }

            if (normalizedPictureIds.Contains(NormalizePictureId(entry.pictureId)))
            {
                targets.Add(entry.target);
            }
        }

        if (targets.Count == 0)
        {
            return false;
        }

        HideAllPictures();

        foreach (GameObject target in targets)
        {
            target.SetActive(true);
        }

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

        List<GameObject> targets = new List<GameObject>();

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
                    targets.Add(entry.target);
                    break;
                }
            }
        }

        if (targets.Count == 0)
        {
            return false;
        }

        HideAllPictures();

        foreach (GameObject target in targets)
        {
            target.SetActive(true);
        }

        return true;
    }

    public bool ActivateDefaultPicture(DialogTriggerMode triggerMode)
    {
        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return false;
        }

        DialogPictureEntry defaultEntry = null;

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry != null && NormalizePictureId(entry.pictureId) == DefaultPictureId)
            {
                defaultEntry = entry;
                break;
            }
        }

        if (defaultEntry == null || defaultEntry.target == null)
        {
            return false;
        }

        HideAllPictures();
        defaultEntry.target.SetActive(true);
        return true;
    }

    public void ActivateAllDefaultPictures()
    {
        ActivateDefaultPicture(DialogTriggerMode.ActionPoint);
    }

    public void HidePictures(DialogTriggerMode triggerMode)
    {
        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return;
        }

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target != null)
            {
                entry.target.SetActive(false);
            }
        }
    }

    public void HideAllPictures()
    {
        HidePictures(DialogTriggerMode.Sequence);
        HidePictures(DialogTriggerMode.ActionPoint);
        HidePictures(DialogTriggerMode.Random);
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
