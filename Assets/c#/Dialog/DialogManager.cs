using System.Collections.Generic;
using System.Collections;
using Cinemachine;
using TMPro;
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
    [SerializeField] private InputEventDispatcher inputEventDispatcher;

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

    [Header("Dialog Playback")]
    [SerializeField] private KeyCode interactKey = KeyCode.Space;
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private PlayerPictureDisplay playerPictureDisplay;
    [SerializeField] private DialogPictureController dialogPictureController;
    [SerializeField] private SpeakerColorPalette speakerColorPalette;
    [SerializeField] private string mateCharacterId = "mate";
    [SerializeField] private string teacherCharacterId = "teacher";
    [SerializeField] private string windowsCharacterId = "windows";
    [SerializeField] private bool showSpeakerName = true;
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private float autoAdvanceDelay = 1.5f;
    [SerializeField] private bool clearTextWhenDialogEnds = true;

    private DialogEntry activeDialog;
    private int activeLineIndex;
    private Coroutine typingCoroutine;
    private Coroutine autoAdvanceCoroutine;
    private string fullLineText;
    private bool isTyping;
    private bool isWaitingForAdvance;
    private bool shouldEndDialogAfterWait;
    private DialogTriggerMode activeDialogTriggerMode;
    private DialogEntry pendingRandomDialog;
    private int lastRandomRefreshActionPoints = -1;

    private void Awake()
    {
        ResolveDialogControllers();
        ClearDialogText();
        HidePlayerPicture();
        RefreshPendingRandomDialog(forceRefresh: true);
    }

    private void Update()
    {
        RefreshPendingRandomDialog();

        if (Input.GetKeyDown(interactKey))
        {
            AdvanceCurrentDialog();
        }
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

    public DialogEntry GetActionPointDialogForCharacterBySpentActionPoints(string characterId, int spentActionPoints)
    {
        if (actionPointDialogController == null)
        {
            Debug.LogWarning("Action point dialog controller is not assigned.", this);
            return null;
        }

        return actionPointDialogController.GetDialogForCharacterBySpentActionPoints(characterId, spentActionPoints);
    }

    public DialogEntry GetActionPointDialogForCharacterBySpentActionPoints(string characterId)
    {
        int spentActionPoints = actionPointSystem != null ? actionPointSystem.SpentActionPoints : 0;
        return GetActionPointDialogForCharacterBySpentActionPoints(characterId, spentActionPoints);
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

        if (randomDialogController != null)
        {
            randomDialogController.SetDaySystem(system);
        }

        if (sequenceDialogController != null)
        {
            sequenceDialogController.SetDaySystem(system);
        }
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

    public int GetSpentActionPoints()
    {
        return actionPointSystem != null ? actionPointSystem.SpentActionPoints : 0;
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

    public void AdvanceCurrentDialog()
    {
        if (isTyping)
        {
            CompleteCurrentLine();
            return;
        }

        if (isWaitingForAdvance)
        {
            AdvanceFromWaitingState();
            return;
        }

        if (activeDialog == null)
        {
            StartDialogForCurrentCameraView();
            return;
        }

        ShowNextActiveDialogLine();
    }

    public void StartDialogForCurrentCameraView()
    {
        CameraViewType currentViewType = GetCurrentCameraViewType();

        switch (currentViewType)
        {
            case CameraViewType.Mate:
                StartMateSequenceDialog();
                break;
            case CameraViewType.Teacher:
                StartTeacherActionPointDialog();
                break;
            case CameraViewType.Windows:
                StartWindowsRandomDialog();
                break;
            default:
                Debug.LogWarning($"No dialog playback is configured for camera view type: {currentViewType}", this);
                break;
        }
    }

    public void StartMateSequenceDialog()
    {
        StopAutoAdvanceCountdown();

        DialogEntry dialog = GetSequenceDialogForCharacter(mateCharacterId);

        if (dialog == null)
        {
            return;
        }

        if (dialog.lines == null || dialog.lines.Count == 0)
        {
            Debug.LogWarning($"No playable mate sequence dialog found for characterId: {mateCharacterId}", this);
            return;
        }

        activeDialog = dialog;
        activeLineIndex = 0;
        activeDialogTriggerMode = DialogTriggerMode.Sequence;
        SetCameraSwitchingEnabled(false);
        TriggerStartActionCommand();
        ShowNextActiveDialogLine();
    }

    public void StartTeacherActionPointDialog()
    {
        StopAutoAdvanceCountdown();

        if (actionPointSystem == null)
        {
            Debug.LogWarning("Action point system is not assigned.", this);
            return;
        }

        if (!actionPointSystem.TryStartAction())
        {
            return;
        }

        DialogEntry dialog = GetActionPointDialogForCharacterBySpentActionPoints(teacherCharacterId);

        if (dialog == null)
        {
            return;
        }

        if (dialog.lines == null || dialog.lines.Count == 0)
        {
            Debug.LogWarning($"No playable teacher action point dialog found for characterId: {teacherCharacterId}", this);
            return;
        }

        activeDialog = dialog;
        activeLineIndex = 0;
        activeDialogTriggerMode = DialogTriggerMode.ActionPoint;
        SetCameraSwitchingEnabled(false);
        ShowNextActiveDialogLine();
    }

    public void StartWindowsRandomDialog()
    {
        StopAutoAdvanceCountdown();

        DialogEntry dialog = pendingRandomDialog;

        if (dialog == null)
        {
            return;
        }

        if (dialog.lines == null || dialog.lines.Count == 0)
        {
            Debug.LogWarning($"No playable windows random dialog found for characterId: {windowsCharacterId}", this);
            return;
        }

        if (actionPointSystem == null)
        {
            Debug.LogWarning("Action point system is not assigned.", this);
            return;
        }

        if (!actionPointSystem.TryStartAction())
        {
            return;
        }

        activeDialog = dialog;
        activeLineIndex = 0;
        activeDialogTriggerMode = DialogTriggerMode.Random;
        pendingRandomDialog = null;
        RefreshPendingRandomDialog(forceRefresh: true, updatePicture: false);
        SetCameraSwitchingEnabled(false);
        ShowNextActiveDialogLine();
    }

    public void EndCurrentDialog()
    {
        DialogTriggerMode endedTriggerMode = activeDialogTriggerMode;

        StopTyping();
        StopAutoAdvanceCountdown();
        activeDialog = null;
        activeLineIndex = 0;
        shouldEndDialogAfterWait = false;
        activeDialogTriggerMode = DialogTriggerMode.Sequence;
        SetCameraSwitchingEnabled(true);

        if (clearTextWhenDialogEnds)
        {
            ClearDialogText();
        }

        HidePlayerPicture();
        NotifyDialogPictureEnded(endedTriggerMode);
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

        if (sequenceDialogController != null && daySystem != null)
        {
            sequenceDialogController.SetDaySystem(daySystem);
        }

        if (randomDialogController != null && daySystem != null)
        {
            randomDialogController.SetDaySystem(daySystem);
        }

        if (inputEventDispatcher == null)
        {
            inputEventDispatcher = FindObjectOfType<InputEventDispatcher>();
        }

        if (playerPictureDisplay == null)
        {
            playerPictureDisplay = FindObjectOfType<PlayerPictureDisplay>();
        }

        if (dialogPictureController == null)
        {
            dialogPictureController = FindObjectOfType<DialogPictureController>();
        }

        if (speakerColorPalette == null)
        {
            speakerColorPalette = FindObjectOfType<SpeakerColorPalette>();
        }
    }

    private void ShowNextActiveDialogLine()
    {
        if (activeDialog == null || activeDialog.lines == null)
        {
            EndCurrentDialog();
            return;
        }

        if (activeLineIndex >= activeDialog.lines.Count)
        {
            EndCurrentDialog();
            return;
        }

        DialogLine line = activeDialog.lines[activeLineIndex];
        activeLineIndex++;
        shouldEndDialogAfterWait = activeLineIndex >= activeDialog.lines.Count;
        SetDialogText(line);
    }

    private void SetDialogText(DialogLine line)
    {
        if (dialogText == null)
        {
            Debug.LogWarning("Dialog TMP text is not assigned.", this);
            return;
        }

        if (line == null)
        {
            dialogText.text = string.Empty;
            return;
        }

        string prefix = showSpeakerName && !string.IsNullOrEmpty(line.characterName)
            ? $"{line.characterName}: "
            : string.Empty;
        string text = line.text ?? string.Empty;

        ApplySpeakerColor(line);
        UpdatePlayerPicture(line);
        UpdateDialogPictureState(line);
        StartTypingLine(prefix, text);
    }

    private void StartTypingLine(string prefix, string text)
    {
        StopTyping();
        StopAutoAdvanceCountdown();

        fullLineText = prefix + text;
        dialogText.text = prefix;
        typingCoroutine = StartCoroutine(TypeLine(prefix, text));
    }

    private IEnumerator TypeLine(string prefix, string text)
    {
        isTyping = true;

        if (charactersPerSecond <= 0f)
        {
            dialogText.text = fullLineText;
            isTyping = false;
            typingCoroutine = null;
            BeginAutoAdvanceCountdown();
            yield break;
        }

        float delay = 1f / charactersPerSecond;

        for (int index = 0; index < text.Length; index++)
        {
            dialogText.text = prefix + text.Substring(0, index + 1);
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
        typingCoroutine = null;
        BeginAutoAdvanceCountdown();
    }

    private void CompleteCurrentLine()
    {
        StopTyping();

        if (dialogText != null)
        {
            dialogText.text = fullLineText;
        }

        BeginAutoAdvanceCountdown();
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    private void TriggerStartActionCommand()
    {
        if (actionPointSystem != null)
        {
            actionPointSystem.ReceiveStartActionCommand();
        }
    }

    private void BeginAutoAdvanceCountdown()
    {
        StopAutoAdvanceCountdown();

        if (activeDialog == null)
        {
            return;
        }

        isWaitingForAdvance = true;
        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay());
    }

    private void StopAutoAdvanceCountdown()
    {
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        isWaitingForAdvance = false;
    }

    private IEnumerator AutoAdvanceAfterDelay()
    {
        if (autoAdvanceDelay > 0f)
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
        }

        autoAdvanceCoroutine = null;
        isWaitingForAdvance = false;
        AdvanceAfterDelay();
    }

    private void AdvanceFromWaitingState()
    {
        StopAutoAdvanceCountdown();

        if (shouldEndDialogAfterWait)
        {
            EndCurrentDialog();
            return;
        }

        ShowNextActiveDialogLine();
    }

    private void AdvanceAfterDelay()
    {
        if (shouldEndDialogAfterWait)
        {
            EndCurrentDialog();
            return;
        }

        ShowNextActiveDialogLine();
    }

    private void ClearDialogText()
    {
        if (dialogText != null)
        {
            dialogText.text = string.Empty;
        }
    }

    private void UpdatePlayerPicture(DialogLine line)
    {
        if (playerPictureDisplay == null)
        {
            return;
        }

        if (line == null || string.IsNullOrEmpty(line.playerPicture))
        {
            playerPictureDisplay.Hide();
            return;
        }

        playerPictureDisplay.Show(line.playerPicture);
    }

    private void HidePlayerPicture()
    {
        if (playerPictureDisplay != null)
        {
            playerPictureDisplay.Hide();
        }
    }

    private void UpdateDialogPictureState(DialogLine line)
    {
        if (dialogPictureController == null)
        {
            return;
        }

        dialogPictureController.OnDialogLineChanged(activeDialogTriggerMode, line);
    }

    private void NotifyDialogPictureEnded(DialogTriggerMode endedTriggerMode)
    {
        if (dialogPictureController != null)
        {
            dialogPictureController.OnDialogEnded(endedTriggerMode);

            if (endedTriggerMode == DialogTriggerMode.Random)
            {
                dialogPictureController.OnPendingRandomDialogChanged(pendingRandomDialog);
            }
        }
    }

    private void RefreshPendingRandomDialog(bool forceRefresh = false, bool updatePicture = true)
    {
        if (randomDialogController == null || dialogPictureController == null)
        {
            return;
        }

        int currentActionPoints = actionPointSystem != null ? actionPointSystem.CurrentActionPoints : 0;

        if (!forceRefresh && currentActionPoints == lastRandomRefreshActionPoints)
        {
            return;
        }

        lastRandomRefreshActionPoints = currentActionPoints;
        pendingRandomDialog = GetRandomDialogForCharacter(windowsCharacterId);

        if (updatePicture)
        {
            dialogPictureController.OnPendingRandomDialogChanged(pendingRandomDialog);
        }
    }

    private void ApplySpeakerColor(DialogLine line)
    {
        if (dialogText == null)
        {
            return;
        }

        if (speakerColorPalette == null || line == null)
        {
            dialogText.color = Color.white;
            return;
        }

        dialogText.color = speakerColorPalette.GetColor(line.speakerId);
    }

    private void SetCameraSwitchingEnabled(bool isEnabled)
    {
        if (inputEventDispatcher != null)
        {
            inputEventDispatcher.enabled = isEnabled;
        }
    }
}
