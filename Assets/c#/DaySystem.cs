using UnityEngine;

public class DaySystem : MonoBehaviour
{
    [Header("Day Settings")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int currentDay = 1;

    [Header("Day State")]
    public bool nextDayCommand;

    [Header("External Systems")]
    [SerializeField] private ActionPointSystem actionPointSystem;

    public int StartDay => startDay;
    public int CurrentDay => currentDay;

    private bool isWaitingForDaySummary;

    private void Awake()
    {
        startDay = Mathf.Max(1, startDay);
        currentDay = Mathf.Max(1, currentDay);

        if (actionPointSystem == null)
        {
            actionPointSystem = FindObjectOfType<ActionPointSystem>();
        }
    }

    private void Update()
    {
        if (nextDayCommand)
        {
            isWaitingForDaySummary = true;
            return;
        }

        if (!isWaitingForDaySummary)
        {
            return;
        }

        CompleteDayTransition();
    }

    public void AdvanceDay()
    {
        currentDay++;
    }

    public void ResetDay()
    {
        currentDay = startDay;
    }

    public void SetCurrentDay(int day)
    {
        currentDay = Mathf.Max(1, day);
    }

    public void SetStartDay(int day)
    {
        startDay = Mathf.Max(1, day);

        if (currentDay < startDay)
        {
            currentDay = startDay;
        }
    }

    public void SetActionPointSystem(ActionPointSystem system)
    {
        actionPointSystem = system;
    }

    private void CompleteDayTransition()
    {
        isWaitingForDaySummary = false;

        if (actionPointSystem != null)
        {
            actionPointSystem.ResetActionPoints();
        }

        AdvanceDay();
    }
}
