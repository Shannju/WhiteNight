using TMPro;
using UnityEngine;

public class ActionStatsTextUpdater : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private DaySystem daySystem;
    [SerializeField] private ActionPointSystem actionPointSystem;

    [Header("UI References")]
    [SerializeField] private TMP_Text targetText;

    [Header("Display Labels")]
    [SerializeField] private string dayLabel = "Day";
    [SerializeField] private string teacherLabel = "Teacher";
    [SerializeField] private string mateLabel = "Mate";

    private int lastDay = int.MinValue;
    private int lastTeacherSpent = int.MinValue;
    private int lastMateSpent = int.MinValue;

    private void Awake()
    {
        ResolveReferences();

        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        ResolveReferences();
        RefreshTextIfChanged();
    }

    public void Refresh()
    {
        lastDay = int.MinValue;
        RefreshTextIfChanged();
    }

    private void ResolveReferences()
    {
        if (daySystem == null)
        {
            daySystem = FindObjectOfType<DaySystem>();
        }

        if (actionPointSystem == null)
        {
            actionPointSystem = FindObjectOfType<ActionPointSystem>();
        }
    }

    private void RefreshTextIfChanged()
    {
        if (targetText == null)
        {
            return;
        }

        int currentDay = daySystem != null ? daySystem.CurrentDay : 0;
        int teacherSpent = actionPointSystem != null ? actionPointSystem.TeacherSpentActionPoints : 0;
        int mateSpent = actionPointSystem != null ? actionPointSystem.MateSpentActionPoints : 0;

        if (currentDay == lastDay &&
            teacherSpent == lastTeacherSpent &&
            mateSpent == lastMateSpent)
        {
            return;
        }

        lastDay = currentDay;
        lastTeacherSpent = teacherSpent;
        lastMateSpent = mateSpent;

        targetText.text =
            $"{dayLabel}: {currentDay}\n" +
            $"{teacherLabel}: {teacherSpent}\n" +
            $"{mateLabel}: {mateSpent}";
    }
}
