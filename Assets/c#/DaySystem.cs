using UnityEngine;
using System.Collections.Generic;

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

    [Header("External Systems")]
    [SerializeField] private ActionPointSystem actionPointSystem;
    [SerializeField] private DialogManager dialogManager;

    public int StartDay => startDay;
    public int CurrentDay => currentDay;

    private bool isWaitingForDaySummary;
    private int defaultActionPoints;

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
    }

    private void Update()
    {
        if (nextDayCommand)
        {
            isWaitingForDaySummary = true;
            nextDayCommand = false;
        }

        if (!isWaitingForDaySummary)
        {
            return;
        }

        if (dialogManager != null && dialogManager.IsDialogActive)
        {
            return;
        }

        CompleteDayTransition();
    }

    public void AdvanceDay()
    {
        currentDay++;
        ApplyActionPointSettingsForCurrentDay();
    }

    public void ResetDay()
    {
        currentDay = startDay;
        ApplyActionPointSettingsForCurrentDay();
    }

    public void SetCurrentDay(int day)
    {
        currentDay = Mathf.Max(1, day);
        ApplyActionPointSettingsForCurrentDay();
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

    private void CompleteDayTransition()
    {
        isWaitingForDaySummary = false;

        AdvanceDay();
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
