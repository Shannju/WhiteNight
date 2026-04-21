using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogPictureEntry
{
    public string pictureId;
    public GameObject target;
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

    public void ShowPicture(DialogTriggerMode triggerMode, string pictureId)
    {
        pictureId = NormalizePictureId(pictureId);

        if (string.IsNullOrEmpty(pictureId))
        {
            return;
        }

        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return;
        }

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null)
            {
                continue;
            }

            bool shouldShow = !string.IsNullOrEmpty(pictureId) && NormalizePictureId(entry.pictureId) == pictureId;
            entry.target.SetActive(shouldShow);
        }
    }

    public void ActivateDefaultPicture(DialogTriggerMode triggerMode)
    {
        List<DialogPictureEntry> pictures = GetPictures(triggerMode);

        if (pictures == null)
        {
            return;
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
            HidePictures(triggerMode);
            return;
        }

        foreach (DialogPictureEntry entry in pictures)
        {
            if (entry?.target == null)
            {
                continue;
            }

            entry.target.SetActive(entry == defaultEntry);
        }
    }

    public void ActivateAllDefaultPictures()
    {
        ActivateDefaultPicture(DialogTriggerMode.Sequence);
        ActivateDefaultPicture(DialogTriggerMode.ActionPoint);
        ActivateDefaultPicture(DialogTriggerMode.Random);
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
}
