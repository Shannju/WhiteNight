using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum CameraViewType
{
    None,
    Mate,
    Windows,
    Teacher,
    Book
}

public enum DialogTriggerMode
{
    Random,
    ActionPoint,
    Sequence
}

public class DialogManager : MonoBehaviour
{
    [Header("Dialog Controllers")]
    [SerializeField] private RandomDialogController randomDialogController;
    [SerializeField] private ActionPointDialogController actionPointDialogController;
    [SerializeField] private SequenceDialogController sequenceDialogController;

    [Header("External Systems")]
    [SerializeField] private DaySystem daySystem;
    [SerializeField] private ActionPointSystem actionPointSystem;

    [Header("Camera View Detection")]
    [SerializeField] private CinemachineVirtualCamera camUp;
    [SerializeField] private CinemachineVirtualCamera camDown;
    [SerializeField] private CinemachineVirtualCamera camLeft;
    [SerializeField] private CinemachineVirtualCamera camRight;
    [SerializeField] private int activePriority = 10;

    [Header("Camera View Types")]
    [SerializeField] private CameraViewType camUpType = CameraViewType.Windows;
    [SerializeField] private CameraViewType camDownType = CameraViewType.Book;
    [SerializeField] private CameraViewType camLeftType = CameraViewType.Teacher;
    [SerializeField] private CameraViewType camRightType = CameraViewType.Mate;

    private void Awake()
    {
        ResolveDialogControllers();
    }

    public DialogEntry GetDialogForCharacter(string characterId, DialogTriggerMode triggerMode)
    {
        switch (triggerMode)
        {
            case DialogTriggerMode.Random:
                return GetRandomDialogForCharacter(characterId);
            case DialogTriggerMode.ActionPoint:
                return GetActionPointDialogForCharacter(characterId);
            case DialogTriggerMode.Sequence:
                return GetSequenceDialogForCharacter(characterId);
            default:
                Debug.LogWarning($"Unsupported dialog trigger mode: {triggerMode}", this);
                return null;
        }
    }

    public DialogEntry GetRandomDialogForCharacter(string characterId)
    {
        if (randomDialogController == null)
        {
            Debug.LogWarning("Random dialog controller is not assigned.", this);
            return null;
        }

        return randomDialogController.GetDialogForCharacter(characterId);
    }

    public DialogEntry GetActionPointDialogForCharacter(string characterId, int currentActionPoints)
    {
        if (actionPointDialogController == null)
        {
            Debug.LogWarning("Action point dialog controller is not assigned.", this);
            return null;
        }

        return actionPointDialogController.GetDialogForCharacter(characterId, currentActionPoints);
    }

    public DialogEntry GetActionPointDialogForCharacter(string characterId)
    {
        int currentActionPoints = actionPointSystem != null ? actionPointSystem.CurrentActionPoints : 0;
        return GetActionPointDialogForCharacter(characterId, currentActionPoints);
    }

    public DialogEntry GetSequenceDialogForCharacter(string characterId)
    {
        if (sequenceDialogController == null)
        {
            Debug.LogWarning("Sequence dialog controller is not assigned.", this);
            return null;
        }

        return sequenceDialogController.GetDialogForCharacter(characterId);
    }

    public List<DialogLine> GetDialogLines(string characterId, DialogTriggerMode triggerMode)
    {
        DialogEntry dialog = GetDialogForCharacter(characterId, triggerMode);
        return dialog != null ? dialog.lines : null;
    }

    public List<DialogLine> GetRandomDialogLines(string characterId)
    {
        DialogEntry dialog = GetRandomDialogForCharacter(characterId);
        return dialog != null ? dialog.lines : null;
    }

    public List<DialogLine> GetActionPointDialogLines(string characterId, int currentActionPoints)
    {
        DialogEntry dialog = GetActionPointDialogForCharacter(characterId, currentActionPoints);
        return dialog != null ? dialog.lines : null;
    }

    public List<DialogLine> GetActionPointDialogLines(string characterId)
    {
        DialogEntry dialog = GetActionPointDialogForCharacter(characterId);
        return dialog != null ? dialog.lines : null;
    }

    public List<DialogLine> GetSequenceDialogLines(string characterId)
    {
        DialogEntry dialog = GetSequenceDialogForCharacter(characterId);
        return dialog != null ? dialog.lines : null;
    }

    public void SetDaySystem(DaySystem system)
    {
        daySystem = system;
    }

    public void SetActionPointSystem(ActionPointSystem system)
    {
        actionPointSystem = system;
    }

    public void SetRandomDialogController(RandomDialogController controller)
    {
        randomDialogController = controller;
    }

    public void SetActionPointDialogController(ActionPointDialogController controller)
    {
        actionPointDialogController = controller;
    }

    public void SetSequenceDialogController(SequenceDialogController controller)
    {
        sequenceDialogController = controller;
    }

    public int GetCurrentDay()
    {
        return daySystem != null ? daySystem.CurrentDay : 0;
    }

    public int GetCurrentActionPoints()
    {
        return actionPointSystem != null ? actionPointSystem.CurrentActionPoints : 0;
    }

    public CameraViewType GetCurrentCameraViewType()
    {
        if (camUp != null && camUp.Priority == activePriority)
            return camUpType;

        if (camDown != null && camDown.Priority == activePriority)
            return camDownType;

        if (camLeft != null && camLeft.Priority == activePriority)
            return camLeftType;

        if (camRight != null && camRight.Priority == activePriority)
            return camRightType;

        return CameraViewType.None;
    }

    public void ResetSequenceProgress(string characterId)
    {
        if (sequenceDialogController != null)
        {
            sequenceDialogController.ResetSequenceProgress(characterId);
        }
    }

    private void ResolveDialogControllers()
    {
        if (randomDialogController == null)
        {
            randomDialogController = FindObjectOfType<RandomDialogController>();
        }

        if (actionPointDialogController == null)
        {
            actionPointDialogController = FindObjectOfType<ActionPointDialogController>();
        }

        if (sequenceDialogController == null)
        {
            sequenceDialogController = FindObjectOfType<SequenceDialogController>();
        }
    }
}
