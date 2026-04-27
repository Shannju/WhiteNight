using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

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

[System.Serializable]
public class SpecificDayStartedEvent
{
    [SerializeField] private int day = 1;
    [SerializeField] private UnityEvent onStarted = new UnityEvent();

    public int Day => day;
    public UnityEvent OnStarted => onStarted;

    public void Normalize()
    {
        day = Mathf.Max(1, day);
    }
}

public class DaySystem : MonoBehaviour
{
    private const int DailyActionPoints = 16;

    [Header("Day Settings")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int finalDay = 8;

    [Header("Day State")]
    public bool nextDayCommand;
    [SerializeField] private bool autoStartNextDayWhenReady = true;
    [SerializeField] private bool switchToDeskBeforeDayEnd = true;
    [SerializeField] private float deskCameraSwitchDelay = 0.5f;

    [Header("Day Events")]
    [SerializeField] private DayNumberEvent onDayStarted = new DayNumberEvent();
    [SerializeField] private DayNumberEvent onDayEnded = new DayNumberEvent();
    [SerializeField] private DayPhaseEvent onDayPhaseChanged = new DayPhaseEvent();
    [SerializeField] private List<SpecificDayStartedEvent> specificDayStartedEvents = new List<SpecificDayStartedEvent>();

    [Header("Transition")]
    [SerializeField] private DayNightFadeTransition fadeTransition;
    [SerializeField] private bool useFadeTransition = true;

    [Header("External Systems")]
    [SerializeField] private ActionPointSystem actionPointSystem;
    [SerializeField] private DialogManager dialogManager;
    [SerializeField] private FourDirectionCameraSwitcher cameraSwitcher;

    public int StartDay => startDay;
    public int CurrentDay => currentDay;
    public int FinalDay => finalDay;
    public DayPhase CurrentPhase => currentPhase;
    public DayNumberEvent OnDayStartedEvent => onDayStarted;
    public DayNumberEvent OnDayEndedEvent => onDayEnded;
    public DayPhaseEvent OnDayPhaseChangedEvent => onDayPhaseChanged;
    public IReadOnlyList<SpecificDayStartedEvent> SpecificDayStartedEvents => specificDayStartedEvents;
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
        finalDay = Mathf.Max(startDay, finalDay);
        NormalizeSpecificDayStartedEvents();

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

    private void OnValidate()
    {
        startDay = Mathf.Max(1, startDay);
        currentDay = Mathf.Max(1, currentDay);
        finalDay = Mathf.Max(startDay, finalDay);
        NormalizeSpecificDayStartedEvents();
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

    public void SetFinalDay(int day)
    {
        finalDay = Mathf.Max(startDay, day);
    }

    public void SetStartDay(int day)
    {
        startDay = Mathf.Max(1, day);
        finalDay = Mathf.Max(startDay, finalDay);

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

    public void TestSkipDayNight()
    {
        SkipDayNightForTest();
    }

    public void SkipDayNightForTest()
    {
        if (isTransitioningPhase)
        {
            return;
        }

        if (currentPhase == DayPhase.Day)
        {
            if (ShouldUseFadeTransition())
            {
                StartCoroutine(RunTransitionWithFade(SkipToNightInstantForTest));
                return;
            }

            SkipToNightInstantForTest();
            return;
        }

        AdvanceDay();
    }

    public void TestSkipToFinalDay()
    {
        SkipToFinalDayForTest();
    }

    public void SkipToFinalDayForTest()
    {
        if (isTransitioningPhase)
        {
            return;
        }

        if (ShouldUseFadeTransition())
        {
            StartCoroutine(RunTransitionWithFade(SkipToFinalDayInstantForTest));
            return;
        }

        SkipToFinalDayInstantForTest();
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

    private void SkipToNightInstantForTest()
    {
        ClearPendingEndDayState();
        SetPhase(DayPhase.Night);
        InvokeDayEnded();
    }

    private void SkipToFinalDayInstantForTest()
    {
        ClearPendingEndDayState();

        int targetDay = Mathf.Max(1, finalDay);
        if (currentDay >= targetDay)
        {
            currentDay = targetDay;
            ApplyActionPointSettingsForCurrentDay();
            SetPhase(DayPhase.Day);
            InvokeDayStarted();
            return;
        }

        if (currentPhase == DayPhase.Day)
        {
            SetPhase(DayPhase.Night);
            InvokeDayEnded();
        }

        while (currentDay < targetDay)
        {
            currentDay++;
            ApplyActionPointSettingsForCurrentDay();
            SetPhase(DayPhase.Day);
            InvokeDayStarted();

            if (currentDay < targetDay)
            {
                SetPhase(DayPhase.Night);
                InvokeDayEnded();
            }
        }

        SetPhase(DayPhase.Day);
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
        InvokeSpecificDayStartedEvents(currentDay);
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

    private void InvokeSpecificDayStartedEvents(int day)
    {
        if (specificDayStartedEvents == null)
        {
            return;
        }

        foreach (SpecificDayStartedEvent dayStartedEvent in specificDayStartedEvents)
        {
            if (dayStartedEvent == null || dayStartedEvent.Day != day)
            {
                continue;
            }

            dayStartedEvent.OnStarted.Invoke();
        }
    }

    private void NormalizeSpecificDayStartedEvents()
    {
        if (specificDayStartedEvents == null)
        {
            return;
        }

        foreach (SpecificDayStartedEvent dayStartedEvent in specificDayStartedEvents)
        {
            dayStartedEvent?.Normalize();
        }
    }
}
