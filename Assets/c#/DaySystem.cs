using UnityEngine;
using UnityEngine.Events;
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

public class DaySystem : MonoBehaviour
{
    [System.Serializable]
    private struct DayActionPointSetting
    {
        public int day;
        public int actionPoints;
    }

    [Header("Day Settings")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private List<DayActionPointSetting> dayActionPointSettings = new List<DayActionPointSetting>
    {
        new DayActionPointSetting { day = 1, actionPoints = 20 }
    };

    [Header("Day State")]
    public bool nextDayCommand;
    [SerializeField] private bool autoStartNextDayWhenReady = true;

    [Header("Day Events")]
    [SerializeField] private DayNumberEvent onDayStarted = new DayNumberEvent();
    [SerializeField] private DayNumberEvent onDayEnded = new DayNumberEvent();
    [SerializeField] private DayPhaseEvent onDayPhaseChanged = new DayPhaseEvent();

    [Header("External Systems")]
    [SerializeField] private ActionPointSystem actionPointSystem;
    [SerializeField] private DialogManager dialogManager;

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
    private int defaultActionPoints;
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

        defaultActionPoints = actionPointSystem != null ? actionPointSystem.MaxActionPoints : 0;
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

        if (!autoStartNextDayWhenReady || (dialogManager != null && dialogManager.IsDialogActive))
        {
            return;
        }

        CompleteDayTransition();
    }

    public void AdvanceDay()
    {
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
        defaultActionPoints = actionPointSystem != null ? actionPointSystem.MaxActionPoints : 0;
        ApplyActionPointSettingsForCurrentDay();
    }

    public void SetDialogManager(DialogManager manager)
    {
        dialogManager = manager;
    }

    public void RequestEndDay()
    {
        if (isWaitingForDaySummary)
        {
            return;
        }

        isWaitingForDaySummary = true;
        SetPhase(DayPhase.Night);
        InvokeDayEnded();
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

        CompleteDayTransition();
    }

    private void CompleteDayTransition()
    {
        isWaitingForDaySummary = false;

        AdvanceDay();
    }

    private void SetPhase(DayPhase phase)
    {
        if (currentPhase == phase)
        {
            return;
        }

        currentPhase = phase;
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

        int actionPoints = GetActionPointsForDay(currentDay);
        actionPointSystem.SetMaxActionPoints(actionPoints);
        actionPointSystem.ResetActionPoints();
    }

    private int GetActionPointsForDay(int day)
    {
        if (dayActionPointSettings != null)
        {
            for (int i = 0; i < dayActionPointSettings.Count; i++)
            {
                DayActionPointSetting setting = dayActionPointSettings[i];

                if (setting.day == day)
                {
                    return Mathf.Max(0, setting.actionPoints);
                }
            }
        }

        return defaultActionPoints;
    }
}
