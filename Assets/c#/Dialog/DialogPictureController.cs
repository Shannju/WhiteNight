using UnityEngine;

public class DialogPictureController : MonoBehaviour
{
    [SerializeField] private DialogPictureRegistry dialogPictureRegistry;
    [SerializeField] private ActionPointSystem actionPointSystem;
    [SerializeField] private ActionPointDialogController actionPointDialogController;
    [SerializeField] private string teacherCharacterId = "teacher";

    private int lastSpentActionPoints = -1;
    private int lastActionPointDay = -1;

    private void Start()
    {
        ResolveReferences();

        if (dialogPictureRegistry != null)
        {
            dialogPictureRegistry.ActivateAllDefaultPictures();
        }

        lastSpentActionPoints = actionPointSystem != null ? actionPointSystem.SpentActionPoints : -1;
        lastActionPointDay = actionPointDialogController != null ? actionPointDialogController.GetCurrentDay() : -1;
    }

    private void Update()
    {
        RefreshActionPointPictureState();
    }

    public void OnDialogLineChanged(DialogTriggerMode triggerMode, DialogLine line)
    {
        if (dialogPictureRegistry == null)
        {
            return;
        }

        if (triggerMode == DialogTriggerMode.ActionPoint)
        {
            return;
        }

        if (triggerMode == DialogTriggerMode.Sequence)
        {
            if (line == null || string.IsNullOrEmpty(line.pictureId))
            {
                dialogPictureRegistry.ActivateDefaultPicture(DialogTriggerMode.Sequence);
                return;
            }

            dialogPictureRegistry.ShowPicture(triggerMode, line.pictureId);
            return;
        }

        if (line == null || string.IsNullOrEmpty(line.pictureId))
        {
            return;
        }

        dialogPictureRegistry.ShowPicture(triggerMode, line.pictureId);
    }

    public void OnDialogEnded(DialogTriggerMode triggerMode)
    {
        // Keep the last visible picture after a dialog ends.
        // The picture will change later when a new state explicitly updates it.
    }

    public void OnPendingRandomDialogChanged(DialogEntry dialog)
    {
        if (dialogPictureRegistry == null)
        {
            return;
        }

        string pictureId = GetFirstPictureId(dialog);

        if (string.IsNullOrEmpty(pictureId))
        {
            return;
        }

        dialogPictureRegistry.ShowPicture(DialogTriggerMode.Random, pictureId);
    }

    public void RefreshActionPointPictureState(bool forceRefresh = false)
    {
        if (actionPointSystem == null || dialogPictureRegistry == null || actionPointDialogController == null)
        {
            return;
        }

        int spentActionPoints = actionPointSystem.SpentActionPoints;
        int currentDay = actionPointDialogController.GetCurrentDay();

        if (!forceRefresh &&
            spentActionPoints == lastSpentActionPoints &&
            currentDay == lastActionPointDay)
        {
            return;
        }

        lastSpentActionPoints = spentActionPoints;
        lastActionPointDay = currentDay;

        if (spentActionPoints <= 0)
        {
            dialogPictureRegistry.ActivateDefaultPicture(DialogTriggerMode.ActionPoint);
            return;
        }

        DialogEntry dialog = actionPointDialogController.FindDialogForCharacterBySpentActionPoints(
            teacherCharacterId,
            spentActionPoints);

        if (dialog == null)
        {
            dialogPictureRegistry.ActivateDefaultPicture(DialogTriggerMode.ActionPoint);
            return;
        }

        string pictureId = GetFirstPictureId(dialog);

        if (string.IsNullOrEmpty(pictureId))
        {
            dialogPictureRegistry.ActivateDefaultPicture(DialogTriggerMode.ActionPoint);
            return;
        }

        dialogPictureRegistry.ShowPicture(DialogTriggerMode.ActionPoint, pictureId);
    }

    private void ResolveReferences()
    {
        if (dialogPictureRegistry == null)
        {
            dialogPictureRegistry = FindObjectOfType<DialogPictureRegistry>();
        }

        if (actionPointSystem == null)
        {
            actionPointSystem = FindObjectOfType<ActionPointSystem>();
        }

        if (actionPointDialogController == null)
        {
            actionPointDialogController = FindObjectOfType<ActionPointDialogController>();
        }
    }

    private string GetFirstPictureId(DialogEntry dialog)
    {
        if (dialog?.lines == null)
        {
            return string.Empty;
        }

        foreach (DialogLine line in dialog.lines)
        {
            if (line != null && !string.IsNullOrEmpty(line.pictureId))
            {
                return line.pictureId;
            }
        }

        return string.Empty;
    }
}
