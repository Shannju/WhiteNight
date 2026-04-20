using UnityEngine;

public class DaySystem : MonoBehaviour
{
    [Header("Day Settings")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int currentDay = 1;

    [Header("Day State")]
    public bool nextDayCommand;

    public int StartDay => startDay;
    public int CurrentDay => currentDay;

    private void Awake()
    {
        startDay = Mathf.Max(1, startDay);
        currentDay = Mathf.Max(1, currentDay);
    }

    private void Update()
    {
        if (!nextDayCommand)
        {
            return;
        }

        AdvanceDay();
        nextDayCommand = false;
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
}
