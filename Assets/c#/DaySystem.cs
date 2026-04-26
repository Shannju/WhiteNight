using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public enum DayPhase
{
    Day,
    Night
}

[System.Serializable]
public class DayNumberEvent : UnityEvent<int>
{
}

[System.Serializable]
public class DayPhaseEvent : UnityEvent<DayPhase>
{
}

public class DaySystem : MonoBehaviour
{
    private const int DailyActionPoints = 16;

    [Header("Day Settings")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int currentDay = 1;

    [Header("Day State")]
    public bool nextDayCommand;
    [SerializeField] private bool autoStartNextDayWhenReady = true;
    [SerializeField] private bool switchToDeskBeforeDayEnd = true;
    [SerializeField] private float deskCameraSwitchDelay = 0.5f;

    [Header("Day Events")]
    [SerializeField] private DayNumberEvent onDayStarted = new DayNumberEvent();
    [SerializeField] private DayNumberEvent onDayEnded = new DayNumberEvent();
    [SerializeField] private DayPhaseEvent onDayPhaseChanged = new DayPhaseEvent();

    [Header("Transition")]
    [SerializeField] private DayNightFadeTransition fadeTransition;
    [SerializeField] private bool useFadeTransition = true;

    [Header("External Systems")]
    [SerializeField] private ActionPointSystem actionPointSystem;
    [SerializeField] private DialogManager dialogManager;
    [SerializeField] private FourDirectionCameraSwitcher cameraSwitcher;

    public int StartDay => startDay;
    public int CurrentDay => currentDay;
    public DayPhase CurrentPhase => currentPhase;
    public DayNumberEvent OnDayStartedEvent => onDayStarted;
    public DayNumberEvent OnDayEndedEvent => onDayEnded;
    public DayPhaseEvent OnDayPhaseChangedEvent => onDayPhaseChanged;
    public bool IsWaitingForDaySummary => isWaitingForDaySummary;

    public event System.Action<int> DayStarted;
    public event System.Action<int> DayEnded;
    public event System.Action<DayPhase> DayPhaseChanged;

    private bool isWaitingForDaySummary;
    private bool hasEnteredNightForPendingEndDay;
    private bool hasPreparedEndDayCamera;
    private bool isTransitioningPhase;
    private float endDayCameraReadyTime;
    private DayPhase currentPhase = DayPhase.Day;

    private void Awake()
    {
        startDay = Mathf.Max(1, startDay);
        currentDay = Mathf.Max(1, currentDay);

        if (actionPointSystem == null)
        {
            actionPointSystem = FindObjectOfType<ActionPointSystem>();
        }

        if (dialogManager == null)
        {
            dialogManager = FindObjectOfType<DialogManager>();
        }

        if (cameraSwitcher == null)
        {
            cameraSwitcher = FindObjectOfType<FourDirectionCameraSwitcher>();
        }

        if (fadeTransition == null)
        {
            fadeTransition = FindObjectOfType<DayNightFadeTransition>(true);
        }

    }

    private void Start()
    {
        ApplyActionPointSettingsForCurrentDay();
        InvokeDayStarted();
    }

    private void Update()
    {
        if (nextDayCommand)
        {
            RequestEndDay();
            nextDayCommand = false;
        }

        if (!isWaitingForDaySummary)
        {
            return;
        }

        if (isTransitioningPhase)
        {
            return;
        }

        if (!hasEnteredNightForPendingEndDay && !TryBeginNightTransition())
        {
            return;
        }

        if (!autoStartNextDayWhenReady)
        {
            return;
        }

        CompleteDayTransition();
    }

    public void AdvanceDay()
    {
        if (ShouldUseFadeTransition() && currentPhase == DayPhase.Night && !isTransitioningPhase)
        {
            StartCoroutine(RunTransitionWithFade(AdvanceDayInstant));
            return;
        }

        AdvanceDayInstant();
    }

    private void AdvanceDayInstant()
    {
        ClearPendingEndDayState();
        currentDay++;
        ApplyActionPointSettingsForCurrentDay();
        SetPhase(DayPhase.Day);
        InvokeDayStarted();
    }

    public void ResetDay()
    {
        currentDay = startDay;
        ApplyActionPointSettingsForCurrentDay();
        SetPhase(DayPhase.Day);
        InvokeDayStarted();
    }

    public void SetCurrentDay(int day)
    {
        currentDay = Mathf.Max(1, day);
        ApplyActionPointSettingsForCurrentDay();
        SetPhase(DayPhase.Day);
        InvokeDayStarted();
    }

    public void SetStartDay(int day)
    {
        startDay = Mathf.Max(1, day);

        if (currentDay < startDay)
        {
            currentDay = startDay;
            ApplyActionPointSettingsForCurrentDay();
        }
    }

    public void SetActionPointSystem(ActionPointSystem system)
    {
        actionPointSystem = system;
        ApplyActionPointSettingsForCurrentDay();
    }

    public void SetDialogManager(DialogManager manager)
    {
        dialogManager = manager;
    }

    public void SetCameraSwitcher(FourDirectionCameraSwitcher switcher)
    {
        cameraSwitcher = switcher;
    }

    public void RequestEndDay()
    {
        if (isWaitingForDaySummary)
        {
            return;
        }

        isWaitingForDaySummary = true;
        TryBeginNightTransition();
    }

    public void ReceiveEndDayCommand()
    {
        RequestEndDay();
    }

    public void ReceiveStartDayCommand()
    {
        if (!isWaitingForDaySummary)
        {
            SetPhase(DayPhase.Day);
            InvokeDayStarted();
            return;
        }

        if (!hasEnteredNightForPendingEndDay && !TryBeginNightTransition())
        {
            return;
        }

        CompleteDayTransition();
    }

    private void CompleteDayTransition()
    {
        if (isTransitioningPhase)
        {
            return;
        }

        if (ShouldUseFadeTransition())
        {
            StartCoroutine(RunTransitionWithFade(CompleteDayTransitionInstant));
            return;
        }

        CompleteDayTransitionInstant();
    }

    private void CompleteDayTransitionInstant()
    {
        AdvanceDayInstant();
    }

    private void ClearPendingEndDayState()
    {
        isWaitingForDaySummary = false;
        hasEnteredNightForPendingEndDay = false;
        hasPreparedEndDayCamera = false;
        endDayCameraReadyTime = 0f;
    }

    private bool TryBeginNightTransition()
    {
        if (dialogManager != null && dialogManager.IsDialogActive)
        {
            return false;
        }

        if (!TryPrepareEndDayCamera())
        {
            return false;
        }

        hasEnteredNightForPendingEndDay = true;

        if (ShouldUseFadeTransition())
        {
            StartCoroutine(RunTransitionWithFade(() =>
            {
                SetPhase(DayPhase.Night);
                InvokeDayEnded();
            }));
        }
        else
        {
            SetPhase(DayPhase.Night);
            InvokeDayEnded();
        }

        return true;
    }

    private IEnumerator RunTransitionWithFade(System.Action midpointAction)
    {
        isTransitioningPhase = true;

        fadeTransition.FadeIn();
        yield return new WaitWhile(() => fadeTransition != null && fadeTransition.IsFading);

        midpointAction?.Invoke();

        fadeTransition.FadeOut();
        yield return new WaitWhile(() => fadeTransition != null && fadeTransition.IsFading);

        isTransitioningPhase = false;
    }

    private bool ShouldUseFadeTransition()
    {
        return useFadeTransition && fadeTransition != null;
    }

    private bool TryPrepareEndDayCamera()
    {
        if (!switchToDeskBeforeDayEnd)
        {
            return true;
        }

        if (!hasPreparedEndDayCamera)
        {
            hasPreparedEndDayCamera = true;
            endDayCameraReadyTime = Time.time + Mathf.Max(0f, deskCameraSwitchDelay);

            if (cameraSwitcher != null)
            {
                cameraSwitcher.SwitchToDown();
            }
        }

        return Time.time >= endDayCameraReadyTime;
    }

    private void SetPhase(DayPhase phase)
    {
        if (currentPhase == phase)
        {
            return;
        }

        currentPhase = phase;
        if (cameraSwitcher != null)
        {
            if (currentPhase == DayPhase.Night)
            {
                cameraSwitcher.HideVirtualCameraChildren();
            }
        }

        DayPhaseChanged?.Invoke(currentPhase);
        onDayPhaseChanged.Invoke(currentPhase);
    }

    private void InvokeDayStarted()
    {
        DayStarted?.Invoke(currentDay);
        onDayStarted.Invoke(currentDay);
    }

    private void InvokeDayEnded()
    {
        DayEnded?.Invoke(currentDay);
        onDayEnded.Invoke(currentDay);
    }

    private void ApplyActionPointSettingsForCurrentDay()
    {
        if (actionPointSystem == null)
        {
            return;
        }

        actionPointSystem.SetMaxActionPoints(DailyActionPoints);
        actionPointSystem.ResetActionPoints();
    }
}
